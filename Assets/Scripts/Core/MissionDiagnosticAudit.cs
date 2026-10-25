using System;
namespace OctoEarth.Core
{
    public sealed class InitialMissionAudit
    {
        public int bodyCount;
        public double tractorMassKg,distanceMetres,aEmbAU,exhaustSpeedMetresPerSecond,dtSeconds,configuredYears,durationSeconds;
        public double aheadCosine,earthAccelerationAlongVelocity,earthTractorForceN,tractorEarthGravityMetresPerSecondSquared;
        public double reciprocalForceRelativeError,feedforwardThrustN,feedforwardOutwardCosine,feedforwardResidualAcceleration;
        public double initialArrayArea,initialSolarElectricalPowerW,feedforwardJetPowerW,materialCapKg,targetAU,targetMetres,prescribedTargetMovementOneStepMetres;
        public bool cutoffAtStart;
    }
    public static class MissionDiagnosticAudit
    {
        public static InitialMissionAudit Initial(string path,MissionConfiguration c,MissionThresholds limits)
        {
            var state=new MissionSimulation(path,c,limits).DiagnosticState;
            var e=state.Bodies[state.Index("Earth")]; var t=state.Bodies[state.Index("Tractor")]; var sun=state.Bodies[state.Index("Sun")]; var moon=state.Bodies[state.Index("Moon")]; var array=state.Bodies[state.Index("Array")];
            var delta=t.Position-e.Position; var u=InitialConditions.EarthMoonBarycenter(state).Velocity.Normalized;
            var pair=new SystemState(e.Clone(),t.Clone()); var pairAcc=new Vector3d[2]; new GravityModel().Accelerations(pair,pairAcc);
            var g=new Vector3d[state.Bodies.Length]; new GravityModel().Accelerations(state,g);
            var drive=new TractorHoverDrive(state,c.Tractor()); drive.Target(state,out var target,out _,out var desired,out _);
            var command=(desired-g[state.Index("Tractor")])*t.Mass;
            var emb=InitialConditions.EarthMoonBarycenter(state); double a=KeplerOrbit.SemiMajorAxis(emb.Position,emb.Velocity,Constants.G*(sun.Mass+e.Mass+moon.Mass));
            var targets=new MissionTargets(state,c); targets.Begin(state); var future=state.Clone(); new Yoshida4().Step(future,new GravityModel(),c.Dt); targets.Get(future,state.Index("Tractor"),out var moved,out _);
            double force=(pairAcc[0]*e.Mass).Magnitude;
            return new InitialMissionAudit{bodyCount=state.Bodies.Length,tractorMassKg=t.Mass,distanceMetres=delta.Magnitude,aEmbAU=a/Constants.AU,exhaustSpeedMetresPerSecond=c.ExhaustSpeed,dtSeconds=c.Dt,configuredYears=c.Years,durationSeconds=c.Years*Constants.JulianYear,
                aheadCosine=Vector3d.Dot(delta.Normalized,u),earthAccelerationAlongVelocity=Vector3d.Dot(pairAcc[0],(e.Velocity-sun.Velocity).Normalized),earthTractorForceN=force,tractorEarthGravityMetresPerSecondSquared=pairAcc[1].Magnitude,
                reciprocalForceRelativeError=(pairAcc[0]*e.Mass+pairAcc[1]*t.Mass).Magnitude/force,feedforwardThrustN=command.Magnitude,feedforwardOutwardCosine=Vector3d.Dot(command.Normalized,delta.Normalized),feedforwardResidualAcceleration=(g[state.Index("Tractor")]+command/t.Mass-desired).Magnitude,
                initialArrayArea=array.Mass/c.ArrayDensity,initialSolarElectricalPowerW=Insolation.Flux((array.Position-sun.Position).Magnitude)*(array.Mass/c.ArrayDensity)*c.ArrayEfficiency,feedforwardJetPowerW=command.Magnitude*c.ExhaustSpeed/(2*Math.Cos(Units.Radians(c.CantDegrees))*c.ThrusterEfficiency),materialCapKg=c.MaterialCap,targetAU=2.7,targetMetres=2.7*Constants.AU,cutoffAtStart=a>=2.7*Constants.AU,prescribedTargetMovementOneStepMetres=(moved-target).Magnitude};
        }
    }
}
