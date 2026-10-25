using System;
namespace OctoEarth.Core
{
    public sealed class MissionConfiguration
    {
        // Engineering assumptions/configurable limits supplied by the M3 prompt; not hardware validation.
        public double Years=10000,Dt=3600,ExhaustSpeed=300000,CantDegrees=25,Distance=5e7,ThrustMargin=1.15;
        public double ThrusterEfficiency=1,ArrayEfficiency=0.3,ArrayDensity=0.01,ArrayReflectivity=0.1;
        public double ArrayRadiusAU=1,ArrayInclinationDegrees=10,ArrayPhaseDegrees=0;
        public bool ArrayCoMoving=false;
        public bool ArrayTracksEarthLongitude=false,SteerPlumes=false;
        public double TractorSunwardBiasDegrees=0;
        public string PostTargetPolicy="Parking"; // Parking, Coast, Depart. Raw coast is tested, not assumed safe.
        public bool RecordEveryStep=false;
        public double MaterialCap=3e23,DeliveryRate=1e13,PowerReserve=0.05;
        public double MaxArrayArea=1e21,MaxMirrorArea=1e17; // Configurable engineering area caps, not enablers.
        public double MirrorDensity=0.001,MirrorReflectivity=0.95,MirrorDeflectionDegrees=30,MirrorDistance=1e9,OpticalErrorRadians=1e-3;
        public double InitialTemperature=288,MixedLayerDepth=100,PlumeHalfAngleDegrees=1,ExclusionRadii=3;
        public double RockDensity=3000,RockAllowableStress=5e6; // Explicit illustrative rock assumptions, not measured asteroid properties.
        public bool EnableMirror=true,StopOnSafetyFlag=false;
        public TractorParameters Tractor() => new TractorParameters{Distance=Distance,ExhaustSpeed=ExhaustSpeed,CantDegrees=CantDegrees,ThrustMargin=ThrustMargin,SunwardBiasDegrees=TractorSunwardBiasDegrees};
        public void Validate()
        {
            Tractor().Validate();
            if(ArrayReflectivity<0 || ArrayReflectivity+ArrayEfficiency>1) throw new ArgumentOutOfRangeException("Array optical plus electrical fractions cannot exceed incident power");
            if(!(Dt>0) || Years<=0 || ArrayEfficiency<=0 || ArrayEfficiency>1 || ThrusterEfficiency<=0 || ThrusterEfficiency>1 || MaterialCap<=0 || DeliveryRate<=0 || ArrayRadiusAU<=0 || MirrorDistance<=Constants.EarthRadius || MirrorReflectivity<=0 || MirrorReflectivity>1 || OpticalErrorRadians<0 || MixedLayerDepth<=0 || ArrayDensity<=0 || MirrorDensity<=0) throw new ArgumentOutOfRangeException("Mission configuration");
        }
    }
    public static class MissionGeometry
    {
        public const double SolarRadius=6.957e8; // IAU 2015 B3, https://www.iau.org/static/resolutions/IAU2015_English.pdf
        public const double MoonRadius=1737400; // NASA Moon Fact Sheet, volumetric mean radius.
        public static double DiscOverlap(double r1,double r2,double separation)
        {
            if(r1<=0 || r2<=0 || separation>=r1+r2) return 0;
            if(separation<=Math.Abs(r1-r2)) return Math.PI*Math.Pow(Math.Min(r1,r2),2);
            double a=Math.Acos(Clamp((separation*separation+r1*r1-r2*r2)/(2*separation*r1)));
            double b=Math.Acos(Clamp((separation*separation+r2*r2-r1*r1)/(2*separation*r2)));
            double q=(-separation+r1+r2)*(separation+r1-r2)*(separation-r1+r2)*(separation+r1+r2);
            return r1*r1*a+r2*r2*b-0.5*Math.Sqrt(Math.Max(0,q));
        }
        public static double Shading(Vector3d observer,Vector3d sun,Vector3d occultor,double area)
        {
            var toSun=sun-observer; var toObject=occultor-observer;
            if(area<=0 || toObject.Magnitude>=toSun.Magnitude || Vector3d.Dot(toSun,toObject)<=0) return 0;
            double rs=Math.Asin(Math.Min(1,SolarRadius/toSun.Magnitude));
            double ro=Math.Atan2(Math.Sqrt(area/Math.PI),toObject.Magnitude);
            double angle=Math.Acos(Clamp(Vector3d.Dot(toSun.Normalized,toObject.Normalized)));
            return Math.Min(1,DiscOverlap(rs,ro,angle)/(Math.PI*rs*rs));
        }
        public static double RayDistance(Vector3d source,Vector3d direction,Vector3d target,out double along)
        { var delta=target-source; along=Vector3d.Dot(delta,direction.Normalized); return along>0 ? (delta-direction.Normalized*along).Magnitude : delta.Magnitude; }
        public static double Clamp(double x) => Math.Max(-1,Math.Min(1,x));
    }
    public sealed class EnergyBalanceClimate
    {
        // User-supplied Budyko/North-type illustrative parameterization; no carbon-cycle model.
        const double A=203.3,B=2.09,TransitionK=233.15;
        readonly double capacity;
        readonly double emissivity=(A-40*B)/(Constants.StefanBoltzmann*TransitionK*TransitionK*TransitionK*TransitionK);
        public double Temperature {get;private set;}
        public double IceFraction => Temperature>=273.15?0:Temperature<=263.15?1:(273.15-Temperature)/10;
        public bool LiquidOcean => Temperature>=273.15 && Temperature<373.15;
        public bool Snowball => Temperature<=263.15;
        public double Albedo => 0.30+0.32*IceFraction;
        public EnergyBalanceClimate(double temperature,double depth) { Temperature=temperature; capacity=1000*4186*depth; }
        public double Outgoing(double temperature) => temperature>=TransitionK ? A+B*(temperature-273.15) : emissivity*Constants.StefanBoltzmann*Math.Pow(temperature,4);
        double Rate(double temperature,double flux,double infrared)
        {
            double ice=temperature>=273.15?0:temperature<=263.15?1:(273.15-temperature)/10;
            return (flux*(1-(0.30+0.32*ice))/4+infrared/4-Outgoing(temperature))/capacity;
        }
        public void Step(double seconds,double flux)
        { StepRadiation(seconds,flux,0); }
        public void StepRadiation(double seconds,double visibleFlux,double infraredFlux)
        {
            int n=Math.Max(1,(int)Math.Ceiling(seconds/86400)); double dt=seconds/n;
            for(int i=0;i<n;i++) { double t=Temperature,k1=Rate(t,visibleFlux,infraredFlux),k2=Rate(t+k1*dt/2,visibleFlux,infraredFlux),k3=Rate(t+k2*dt/2,visibleFlux,infraredFlux),k4=Rate(t+k3*dt,visibleFlux,infraredFlux); Temperature=t+dt*(k1+2*k2+2*k3+k4)/6; }
        }
    }
    public sealed class FocusingMirror
    {
        public Vector3d Incoming,Outgoing,Normal;
        public double IncidenceCos,Intercept,Flux,RequiredArea,DirectFlux,ReflectedPower,EffectiveFlux;
        public Vector3d RadiationForce(double area,double reflectivity)
        { double power=Flux*area*IncidenceCos; return (Incoming-Outgoing*reflectivity)*(power/Constants.C); }
        public void Evaluate(Vector3d sun,Vector3d earth,Vector3d mirror,double reflectivity,double opticalError,double sunShade,double earthShade,double area)
        {
            Incoming=(mirror-sun).Normalized; Outgoing=(earth-mirror).Normalized;
            Normal=(Incoming-Outgoing).Normalized; IncidenceCos=Math.Abs(Vector3d.Dot(Incoming,Normal));
            double angularSun=2*Math.Asin(MissionGeometry.SolarRadius/(mirror-sun).Magnitude);
            double spot=(earth-mirror).Magnitude*Math.Sqrt(angularSun*angularSun+opticalError*opticalError);
            Intercept=Math.Min(1,Math.Pow(2*Constants.EarthRadius/spot,2));
            Flux=Insolation.Flux((mirror-sun).Magnitude)*(1-sunShade);
            DirectFlux=Insolation.Flux((earth-sun).Magnitude)*(1-earthShade);
            double missing=Math.Max(0,Insolation.Flux(Constants.AU)-DirectFlux);
            RequiredArea=Flux*reflectivity*IncidenceCos*Intercept>0 ? missing*Math.PI*Constants.EarthRadius*Constants.EarthRadius/(Flux*reflectivity*IncidenceCos*Intercept) : double.PositiveInfinity;
            ReflectedPower=Flux*Math.Min(area,RequiredArea)*reflectivity*IncidenceCos*Intercept;
            EffectiveFlux=DirectFlux+ReflectedPower/(Math.PI*Constants.EarthRadius*Constants.EarthRadius);
        }
    }
}
