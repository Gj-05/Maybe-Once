using System;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public sealed class TractorParameters
    {
        // Engineering/controller assumptions from the user's M3 specification, not measured capabilities.
        public double Distance=5e7,ExhaustSpeed=300000,CantDegrees=25,ThrustMargin=1.15;
        public double ControlFrequency=2e-4,Damping=1;
        public double SunwardBiasDegrees=0;
        public void Validate()
        {
            if(!(Distance>Constants.EarthRadius) || !(ExhaustSpeed>0) || !(ControlFrequency>0) || !(Damping>0) || !(ThrustMargin>0) || CantDegrees<0 || CantDegrees>=89) throw new ArgumentOutOfRangeException("Tractor configuration");
        }
    }
    public static class GravityTractor
    {
        public static double SizeMass(SystemState s,Vector3d position,Vector3d prograde,double desiredForce)
        {
            // Use actual force vectors projected along prograde; sums of magnitudes would overstate useful pull.
            var sum=new KahanVector(); foreach(var b in s.Bodies) if(b.Name=="Earth" || b.Name=="Moon")
            { var d=position-b.Position; sum.Add(d*(Constants.G*b.Mass/Math.Pow(d.Magnitude,3))); }
            double useful=Vector3d.Dot(sum.Value,prograde); if(!(useful>0)) throw new InvalidOperationException("Tractor gives no prograde pull"); return desiredForce/useful;
        }
        public static SystemState Toy(TractorParameters config)
        {
            config.Validate(); var binary=KeplerOrbit.Binary(Constants.AU,0,Constants.SolarMu,Constants.EarthMu/Constants.G);
            var earth=binary.Bodies[1]; var sun=binary.Bodies[0]; var u=(earth.Velocity-sun.Velocity).Normalized;
            var target=earth.Position+u*config.Distance;
            var budget=new MissionBudget(Constants.AU,2.7*Constants.AU,earth.Mass,10000*Constants.JulianYear);
            double mass=SizeMass(binary,target,u,config.ThrustMargin*budget.Force);
            var acc=new Vector3d[2]; new GravityModel().Accelerations(binary,acc);
            var relative=acc[1]-acc[0]; var udot=(relative-u*Vector3d.Dot(relative,u))/(earth.Velocity-sun.Velocity).Magnitude;
            var bodies=new List<Body>(binary.Bodies); bodies.Add(new Body("Tractor",mass,target,earth.Velocity+udot*config.Distance));
            var s=new SystemState(bodies.ToArray()); s.Barycenter(); return s;
        }
        public static SystemState Planetary(string path,TractorParameters config)
        {
            var s=InitialConditions.SolarSystem(path); var e=s.Bodies[s.Index("Earth")]; var bary=InitialConditions.EarthMoonBarycenter(s); var u=bary.Velocity.Normalized;
            var radial=(e.Position-s.Bodies[s.Index("Sun")].Position).Normalized;
            var direction=(u*Math.Cos(Units.Radians(config.SunwardBiasDegrees))-radial*Math.Sin(Units.Radians(config.SunwardBiasDegrees))).Normalized;
            var position=e.Position+direction*config.Distance;
            var budget=new MissionBudget(Constants.AU,2.7*Constants.AU,Constants.PairMass,10000*Constants.JulianYear);
            double mass=SizeMass(s,position,u,config.ThrustMargin*budget.Force);
            var bodies=new List<Body>(s.Bodies); bodies.Add(new Body("Tractor",mass,position,e.Velocity));
            var result=new SystemState(bodies.ToArray()); var drive=new TractorHoverDrive(result,config);
            drive.Target(result,out _,out var velocity,out _,out _); result.Bodies[result.Index("Tractor")].Velocity=velocity;
            result.Barycenter(); return result;
        }
    }
    public sealed class TractorHoverDrive : IExternalKick
    {
        readonly TractorParameters config;
        readonly GravityModel gravity=new GravityModel();
        readonly int earth,sun,tractor,moon;
        readonly Vector3d[] accelerations;
        readonly EjectaDrive drive;
        public Vector3d ThrustVector {get;private set;}
        public Vector3d ExhaustPlus {get;private set;}
        public Vector3d ExhaustMinus {get;private set;}
        public double MaxKickImpulseError {get;private set;}
        public double Propellant => drive.TotalEjected;
        public TractorHoverDrive(SystemState s,TractorParameters config)
        {
            config.Validate(); this.config=config; earth=s.Index("Earth"); sun=s.Index("Sun"); tractor=s.Index("Tractor"); moon=-1;
            for(int k=0;k<s.Bodies.Length;k++) if(s.Bodies[k].Name=="Moon") moon=k;
            accelerations=new Vector3d[s.Bodies.Length];
            drive=new EjectaDrive(tractor,0,config.ExhaustSpeed,new Vector3d(1,0,0));
        }
        public void Target(SystemState s,out Vector3d r,out Vector3d v,out Vector3d a,out Vector3d normal)
        {
            gravity.Accelerations(s,accelerations); var e=s.Bodies[earth]; var t=s.Bodies[tractor]; var sol=s.Bodies[sun];
            var baryV=e.Velocity-sol.Velocity; var baryR=e.Position-sol.Position; var baryA=accelerations[earth]-accelerations[sun];
            if(moon>=0)
            {
                var lunar=s.Bodies[moon]; double m=e.Mass+lunar.Mass;
                baryV=(e.Velocity*e.Mass+lunar.Velocity*lunar.Mass)/m-sol.Velocity;
                baryR=(e.Position*e.Mass+lunar.Position*lunar.Mass)/m-sol.Position;
                baryA=(accelerations[earth]*e.Mass+accelerations[moon]*lunar.Mass)/m-accelerations[sun];
            }
            var u=baryV.Normalized; var udot=(baryA-u*Vector3d.Dot(baryA,u))/baryV.Magnitude;
            if(config.SunwardBiasDegrees!=0)
            {
                var radial=(e.Position-sol.Position).Normalized; var rv=e.Velocity-sol.Velocity;
                var radialDot=(rv-radial*Vector3d.Dot(rv,radial))/(e.Position-sol.Position).Magnitude;
                double beta=Units.Radians(config.SunwardBiasDegrees);
                var q=u*Math.Cos(beta)-radial*Math.Sin(beta); var derivative=udot*Math.Cos(beta)-radialDot*Math.Sin(beta);
                u=q.Normalized; udot=(derivative-u*Vector3d.Dot(derivative,u))/q.Magnitude;
            }
            r=e.Position+config.Distance*u; v=e.Velocity+config.Distance*udot;
            // Circular/slow-spiral feedforward from the supplied M3 approximation; PD handles residuals.
            a=accelerations[earth]-config.Distance*udot.SqrMagnitude*u;
            normal=Vector3d.Cross(baryR,baryV).Normalized;
        }
        Vector3d Compute(SystemState s,out Vector3d normal)
        {
            Target(s,out var r,out var v,out var feedforward,out normal); var t=s.Bodies[tractor];
            var command=feedforward+(r-t.Position)*(config.ControlFrequency*config.ControlFrequency)+(v-t.Velocity)*(2*config.Damping*config.ControlFrequency);
            return (command-accelerations[tractor])*t.Mass;
        }
        public void AddAccelerations(SystemState s,Vector3d[] a) { var f=Compute(s,out _); a[tractor]+=f/s.Bodies[tractor].Mass; }
        public void BookReactions(Ledger ledger,double dt) { /* Atomic with the actual positive-time exhaust kicks. */ }
        public void Kick(SystemState s,Ledger ledger,double dt)
        {
            if(!(dt>0)) throw new ArgumentOutOfRangeException(nameof(dt));
            var force=Compute(s,out var normal);
            ApplyForce(s,ledger,dt,force,normal);
        }
        public void ApplyVelocityChange(SystemState s,Ledger ledger,Vector3d dv,double duration)
        {
            Target(s,out _,out _,out _,out var normal);
            ApplyForce(s,ledger,duration,dv*(s.Bodies[tractor].Mass/duration),normal);
        }
        void ApplyForce(SystemState s,Ledger ledger,double dt,Vector3d force,Vector3d normal)
        {
            ThrustVector=force; double net=force.Magnitude; if(net==0) return;
            var axis=ThrustVector/net;
            normal=(normal-axis*Vector3d.Dot(normal,axis)).Normalized;
            double alpha=Units.Radians(config.CantDegrees),c=Math.Cos(alpha),sn=Math.Sin(alpha);
            var a=axis*c+normal*sn; var b=axis*c-normal*sn;
            ExhaustPlus=-1*a; ExhaustMinus=-1*b;
            drive.Thrust=net;
            var body=s.Bodies[tractor]; var before=body.Velocity;
            drive.KickCanted(s,ledger,dt,axis,normal,alpha);
            var actual=(body.Velocity-before)*body.Mass; var expected=ThrustVector*dt;
            MaxKickImpulseError=Math.Max(MaxKickImpulseError,(actual-expected).Magnitude/expected.Magnitude);
        }
    }
}
