using System;
namespace OctoEarth.Core
{
    public sealed class MissionRadiation
    {
        readonly MissionConfiguration c;
        readonly int earth,sun,array,mirror;
        readonly FocusingMirror optics=new FocusingMirror();
        public double ArrayArea,MirrorArea,ArrayShadeEarth,ArrayShadeMirror,ArrayFlux,EarthFlux,MirrorRequiredArea;
        public Vector3d EarthForce,ArrayForce,MirrorForce;
        public double OpticalIncidenceCos,MirrorIntercept,ActiveMirrorArea,ArrayWasteHeat,InfraredFlux,VisibleFlux,DirectFlux,ReflectedFlux;
        double infraredTime=double.NegativeInfinity,infraredArea;
        Vector3d infraredDirection;
        public MissionRadiation(SystemState s,MissionConfiguration config,double arrayArea,double mirrorArea)
        { c=config; earth=s.Index("Earth"); sun=s.Index("Sun"); array=s.Index("Array"); mirror=s.Index("Mirror"); ArrayArea=arrayArea; MirrorArea=mirrorArea; }
        public void Evaluate(SystemState s)
        {
            var ep=s.Bodies[earth].Position; var sp=s.Bodies[sun].Position; var ap=s.Bodies[array].Position; var mp=s.Bodies[mirror].Position;
            ArrayShadeEarth=MissionGeometry.Shading(ep,sp,ap,ArrayArea);
            ArrayShadeMirror=MissionGeometry.Shading(mp,sp,ap,ArrayArea);
            ArrayFlux=Insolation.Flux((ap-sp).Magnitude);
            optics.Evaluate(sp,ep,mp,c.MirrorReflectivity,c.OpticalErrorRadians,ArrayShadeMirror,ArrayShadeEarth,c.EnableMirror?MirrorArea:0);
            ActiveMirrorArea=c.EnableMirror?Math.Min(MirrorArea,optics.RequiredArea):0;
            VisibleFlux=optics.EffectiveFlux; DirectFlux=optics.DirectFlux; ReflectedFlux=optics.ReflectedPower/(Math.PI*Constants.EarthRadius*Constants.EarthRadius);
            EarthFlux=VisibleFlux; MirrorRequiredArea=c.EnableMirror?optics.RequiredArea:0; OpticalIncidenceCos=optics.IncidenceCos; MirrorIntercept=optics.Intercept;
            ArrayWasteHeat=Math.Max(0,1-c.ArrayReflectivity-c.ArrayEfficiency)*ArrayFlux*ArrayArea;
            if(s.Time-infraredTime>=6*Units.Day/24 || ArrayArea!=infraredArea)
            {
                InfraredFlux=ExtendedDiscInfrared(ep,ap,(ap-sp).Normalized,ArrayArea,ArrayWasteHeat,out infraredDirection);
                infraredTime=s.Time; infraredArea=ArrayArea;
            }
            EarthFlux+=InfraredFlux;
            ArrayForce=(ap-sp).Normalized*(ArrayFlux*ArrayArea*(1+c.ArrayReflectivity)/Constants.C);
            MirrorForce=c.EnableMirror?optics.RadiationForce(ActiveMirrorArea,c.MirrorReflectivity):new Vector3d();
            EarthForce=(ep-sp).Normalized*(optics.DirectFlux*Math.PI*Constants.EarthRadius*Constants.EarthRadius/Constants.C)+optics.Outgoing*(optics.ReflectedPower/Constants.C);
            EarthForce+=infraredDirection*(InfraredFlux*Math.PI*Constants.EarthRadius*Constants.EarthRadius/Constants.C);
        }
        public static double ExtendedDiscInfrared(Vector3d observer,Vector3d center,Vector3d normal,double area,double heat,out Vector3d direction)
        { return FiniteDiscRadiation.Irradiance(observer,center,normal,area,heat,out direction); }
        public static double LegacyDiscInfrared(Vector3d observer,Vector3d center,Vector3d normal,double area,double heat,out Vector3d direction)
        {
            direction=(observer-center).Normalized; if(area<=0 || heat<=0) return 0;
            var reference=Math.Abs(normal.Z)<0.9?new Vector3d(0,0,1):new Vector3d(1,0,0);
            var x=Vector3d.Cross(normal,reference).Normalized; var y=Vector3d.Cross(normal,x);
            double radius=Math.Sqrt(area/Math.PI),sum=0; var rays=new KahanVector();
            // Midpoint quadrature of the emitting finite disc, Lambertian emission from both faces.
            // Sampling is refreshed every six simulated hours; this approximation is reported.
            for(int i=0;i<8;i++) for(int j=0;j<16;j++)
            {
                double r=radius*(i+0.5)/8,phi=2*Math.PI*(j+0.5)/16;
                var point=center+(x*Math.Cos(phi)+y*Math.Sin(phi))*r; var ray=observer-point;
                double length=ray.Magnitude,weight=Math.Abs(Vector3d.Dot(normal,ray))/(length*length*length)*r*(radius/8)*(2*Math.PI/16);
                sum+=weight; rays.Add(ray.Normalized*weight);
            }
            direction=rays.Value.Normalized; return heat/(2*Math.PI*area)*sum;
        }
        void Apply(SystemState s,Ledger ledger,int index,Vector3d force,double dt)
        {
            var body=s.Bodies[index]; var dv=force*(dt/body.Mass); double work=body.Mass*Vector3d.Dot(body.Velocity+dv/2,dv); body.Velocity+=dv;
            PhotonSink.Book(ledger,body.Position,force*(-dt),-work);
        }
        public void Kick(SystemState s,Ledger ledger,double dt)
        { Evaluate(s); Apply(s,ledger,earth,EarthForce,dt); Apply(s,ledger,array,ArrayForce,dt); Apply(s,ledger,mirror,MirrorForce,dt); }
    }
}
