using System;
namespace OctoEarth.Core
{
    public sealed class MissionMonitors
    {
        readonly MissionConfiguration c;
        readonly MissionThresholds limits;
        readonly int earth,moon,sun,tractor,array,mirror;
        readonly int[] emitters,plumeTargets;
        readonly string[] boundCodes,inwardCodes,heatingCodes;
        readonly string[,] intersectionCodes;
        readonly double tractorRadius;
        public MissionFlags Flags {get;}=new MissionFlags();
        public double A,E,TideRatio,MinSafetyFactor=double.PositiveInfinity,MinPlumeMiss=double.PositiveInfinity,MaxPlumeIrradiance,MaxShade;
        public double MinTemperature=double.PositiveInfinity,MaxTemperature;
        public double MoonBinding,HillRadius,MoonDistance;
        public double MaxEarthShade,MaxMirrorShade,MaxTideRatio,MaxFlux,EarthPlumeClearance=1e300,MoonPlumeClearance=1e300,MaxArrayJetPower,MaxMirrorJetPower,MaxRockStress,MaxArrayStress,MaxMirrorStress;
        public bool AllMoonBound=true;
        public MissionMonitors(SystemState s,MissionConfiguration c,MissionThresholds limits)
        {
            this.c=c; this.limits=limits; earth=s.Index("Earth"); moon=s.Index("Moon"); sun=s.Index("Sun"); tractor=s.Index("Tractor"); array=s.Index("Array"); mirror=s.Index("Mirror");
            emitters=new[]{tractor,array,mirror}; plumeTargets=new[]{earth,moon,tractor,array,mirror}; boundCodes=new string[3]; inwardCodes=new string[3]; heatingCodes=new string[3]; intersectionCodes=new string[3,5];
            for(int k=0;k<3;k++) { string name=s.Bodies[emitters[k]].Name; boundCodes[k]="BoundEjecta."+name; inwardCodes[k]="InitiallyInwardEjecta."+name; heatingCodes[k]="PlumeHeating."+name; for(int j=0;j<5;j++) intersectionCodes[k,j]="PlumeIntersection."+name+"."+s.Bodies[plumeTargets[j]].Name; }
            tractorRadius=Math.Pow(3*s.Bodies[tractor].Mass/(4*Math.PI*c.RockDensity),1.0/3);
            if(c.EnableMirror) Flags.Add(0,"FocusingOpticsUnresolved","Giant focusing optics remain outside E1-E3 pending organiser confirmation.");
            Flags.Add(0,"MaterialStrengthUnvalidated","Rock and frame strength inputs are illustrative engineering assumptions, not validated components.");
            Flags.Add(0,"ThermalStructuralModelIncomplete","Local beam/column checks omit coupled thermal deformation and global optical stability.");
        }
        public void Check(SystemState s,Ledger ledger,MissionConstraintStepper stepper,MissionRadiation rad,EnergyBalanceClimate climate,double materials)
        {
            var e=s.Bodies[earth]; var m=s.Bodies[moon]; var t=s.Bodies[tractor]; var sol=s.Bodies[sun]; var orbit=InitialConditions.EarthMoonBarycenter(s);
            A=KeplerOrbit.SemiMajorAxis(orbit.Position,orbit.Velocity,Constants.G*(sol.Mass+e.Mass+m.Mass)); E=KeplerOrbit.EccentricityVector(orbit.Position,orbit.Velocity,Constants.G*(sol.Mass+e.Mass+m.Mass)).Magnitude;
            MoonDistance=(m.Position-e.Position).Magnitude; bool bound=InitialConditions.MoonBound(s,out MoonBinding,out HillRadius); AllMoonBound &= bound;
            if(!bound) Flags.Add(s.Time,"MoonUnbound","Lunar binding or Hill criterion failed.");
            if(MoonDistance<limits.moonDistanceMinimum.value || MoonDistance>limits.moonDistanceMaximum.value) Flags.Add(s.Time,"MoonDistance","Lunar separation outside placeholder interval.");
            double d=(t.Position-e.Position).Magnitude;
            TideRatio=(t.Mass/m.Mass)*Math.Pow(MoonDistance/d,3);
            MaxTideRatio=Math.Max(MaxTideRatio,TideRatio); MaxFlux=Math.Max(MaxFlux,rad.EarthFlux);
            MaxEarthShade=Math.Max(MaxEarthShade,rad.ArrayShadeEarth); MaxMirrorShade=Math.Max(MaxMirrorShade,rad.ArrayShadeMirror);
            if(TideRatio>limits.tideRatioMaximum.value) Flags.Add(s.Time,"TractorTides","Tractor tide gradient exceeds placeholder Moon-relative threshold.");
            if(E>limits.eccentricityMaximum.value) Flags.Add(s.Time,"Eccentricity","Heliocentric eccentricity exceeds threshold.");
            MinTemperature=Math.Min(MinTemperature,climate.Temperature); MaxTemperature=Math.Max(MaxTemperature,climate.Temperature);
            if(climate.Temperature<limits.temperatureMinimum.value || climate.Temperature>limits.temperatureMaximum.value) Flags.Add(s.Time,"Temperature","Global EBM temperature outside placeholder habitability interval.");
            if(climate.Snowball) Flags.Add(s.Time,"Snowball","Ice-albedo EBM entered the snowball branch.");
            if((climate.LiquidOcean?1:0)<limits.liquidFractionMinimum.value) Flags.Add(s.Time,"LiquidOcean","Global mean proxy fails placeholder liquid-ocean criterion.");
            MaxShade=Math.Max(MaxShade,Math.Max(rad.ArrayShadeEarth,rad.ArrayShadeMirror));
            if(MaxShade>limits.shadingMaximum.value) Flags.Add(s.Time,"ArrayShading","Array overlaps Sun as seen from Earth or mirror beyond threshold.");
            if(rad.EarthFlux>limits.mirrorFluxMaximum.value) Flags.Add(s.Time,"FluxCap","Received visible plus array waste-heat flux exceeds placeholder limit.");
            if(materials>c.MaterialCap) Flags.Add(s.Time,"MaterialCap","E1 delivery mass cap exceeded.",true);
            if(ledger.MaxEnergyError>limits.ledgerMaximum.value || ledger.MaxMomentumError>limits.ledgerMaximum.value || ledger.MaxAngularMomentumError>limits.ledgerMaximum.value || stepper.MaxImpulseError>limits.ledgerMaximum.value || ledger.MaxReactionClosure>limits.ledgerMaximum.value) Flags.Add(s.Time,"Ledger","A recorded conservation metric exceeds tolerance.");
            Structural(s,stepper,rad);
            for(int k=0;k<3;k++)
            {
                var drive=stepper.Drives[k]; if(drive.Thrust==0) continue;
                double power=drive.Thrust*c.ExhaustSpeed/(2*Math.Cos(Units.Radians(c.CantDegrees)));
                if(k==1) MaxArrayJetPower=Math.Max(MaxArrayJetPower,power); if(k==2) MaxMirrorJetPower=Math.Max(MaxMirrorJetPower,power);
                var source=s.Bodies[emitters[k]];
                for(int j=0;j<2;j++)
                {
                    var direction=j==0?drive.ExhaustAxisPlus:drive.ExhaustAxisMinus;
                    var relativeV=(j==0?drive.LastEjectaPlus:drive.LastEjectaMinus)-sol.Velocity; var r=source.Position-sol.Position;
                    if(relativeV.SqrMagnitude/2-Constants.SolarMu/r.Magnitude<0) Flags.Add(s.Time,boundCodes[k],"At least one exported stream is bound to the Sun.");
                    if(Vector3d.Dot(relativeV,r)<=0) Flags.Add(s.Time,inwardCodes[k],"An unintegrated parcel is not initially radially outbound; ballistic future escape is not assumed.");
                    for(int targetIndex=0;targetIndex<plumeTargets.Length;targetIndex++)
                    {
                        int index=plumeTargets[targetIndex];
                        if(index==emitters[k]) continue;
                        double along; double miss=MissionGeometry.RayDistance(source.Position,direction,s.Bodies[index].Position,out along); if(along<=0) continue;
                        double radius=index==earth?Constants.EarthRadius:index==moon?MissionGeometry.MoonRadius:index==tractor?tractorRadius:Math.Sqrt((index==array?rad.ArrayArea:rad.MirrorArea)/Math.PI);
                        double cone=along*Math.Tan(Units.Radians(c.PlumeHalfAngleDegrees));
                        if(index==earth) EarthPlumeClearance=Math.Min(EarthPlumeClearance,miss-Math.Max(radius+cone,limits.plumeExclusionRadii.value*Constants.EarthRadius));
                        if(index==moon) MoonPlumeClearance=Math.Min(MoonPlumeClearance,miss-radius-cone);
                        if(index==earth) MinPlumeMiss=Math.Min(MinPlumeMiss,miss);
                        if(miss<radius+cone || (index==earth && miss<limits.plumeExclusionRadii.value*Constants.EarthRadius)) Flags.Add(s.Time,intersectionCodes[k,targetIndex],"Plume cone or exclusion zone intersects target.");
                        if(index==earth)
                        {
                            double distance=(e.Position-source.Position).Magnitude;
                            double angle=Math.Atan2(miss,along); double theta=Math.Max(0,angle-Math.Asin(Math.Min(1,Constants.EarthRadius/distance)));
                            double sigma=Units.Radians(c.PlumeHalfAngleDegrees)/2;
                            double jet=drive.Thrust*c.ExhaustSpeed/(2*Math.Cos(Units.Radians(c.CantDegrees)));
                            double irradiance=(jet/2)/(2*Math.PI*sigma*sigma*along*along)*Math.Exp(-theta*theta/(2*sigma*sigma));
                            MaxPlumeIrradiance=Math.Max(MaxPlumeIrradiance,irradiance);
                            if(irradiance>limits.plumeIrradianceMaximum.value) Flags.Add(s.Time,heatingCodes[k],"Gaussian plume irradiance on Earth's near limb exceeds threshold.");
                        }
                    }
                }
            }
        }
        void Structural(SystemState s,MissionConstraintStepper stepper,MissionRadiation rad)
        {
            var t=s.Bodies[tractor]; double radius=tractorRadius;
            double acceleration=stepper.TractorThrust.Magnitude/t.Mass;
            double stress=c.RockDensity*(acceleration*2*radius+Constants.G*t.Mass/radius);
            MaxRockStress=Math.Max(MaxRockStress,stress);
            double safety=Beam.SafetyFactor(c.RockAllowableStress,stress);
            double inertia=Math.PI*Math.Pow(radius,4)/4;
            double buckling=Beam.SafetyFactor(Beam.EulerBuckling(50e9,inertia,1,2*radius),stepper.TractorThrust.Magnitude); // Illustrative rock E=50GPa.
            safety=Math.Min(safety,buckling);
            for(int k=1;k<3;k++)
            {
                double area=k==1?rad.ArrayArea:rad.MirrorArea; if(area<=0) continue;
                double pressure=(k==1?rad.ArrayForce:rad.MirrorForce).Magnitude/area;
                double density=k==1?c.ArrayDensity:c.MirrorDensity;
                double control=stepper.LastThrust[k].Magnitude/s.Bodies[k==1?array:mirror].Mass;
                // Explicit illustrative 100m local panel module; global optical/thermal structure remains flagged.
                double q=(pressure+density*control)*100;
                double moment=Beam.RootMoment(q,100); double sigma=Beam.Stress(0,1e-4,moment,0.005,Math.Pow(0.01,4)/12);
                if(k==1) MaxArrayStress=Math.Max(MaxArrayStress,sigma); else MaxMirrorStress=Math.Max(MaxMirrorStress,sigma);
                safety=Math.Min(safety,Beam.SafetyFactor(200e6,sigma));
            }
            MinSafetyFactor=Math.Min(MinSafetyFactor,safety);
            if(safety<limits.minimumSafetyFactor.value) Flags.Add(s.Time,"StructuralSafety","A modeled component safety factor is below 1.5; rock/frame assumptions do not imply unlimited strength.");
        }
    }
}
