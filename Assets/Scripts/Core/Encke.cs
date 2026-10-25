using System;
namespace OctoEarth.Core
{
    // Classical Encke deviation equation about an analytic Kepler ellipse, integrated with RK4.
    // For long/high-thrust runs reference rectification would be needed; not used for M2 N-body runs.
    public sealed class Encke
    {
        readonly OrbitalElements reference;
        readonly double mu;
        public Vector3d DeltaPosition {get;private set;}
        public Vector3d DeltaVelocity {get;private set;}
        public double Time {get;private set;}
        public Encke(OrbitalElements reference,double mu) { this.reference=reference; this.mu=mu; }
        public OrbitalState State { get { var r=KeplerOrbit.FromElements(reference,mu,Time); return new OrbitalState(r.Position+DeltaPosition,r.Velocity+DeltaVelocity,Time); } }
        Vector3d Accel(double time,Vector3d d,Vector3d velocity,Func<OrbitalState,Vector3d> push)
        {
            var r=KeplerOrbit.FromElements(reference,mu,time); double r2=r.Position.SqrMagnitude;
            double q=Vector3d.Dot(d,2*r.Position+d)/r2; double w=Math.Pow(1+q,1.5);
            double f=q*(3+3*q+q*q)/(w*(w+1));
            return (d-(r.Position+d)*f)*(-mu/(r2*Math.Sqrt(r2)))+push(new OrbitalState(r.Position+d,r.Velocity+velocity,time));
        }
        public void Step(double dt,Func<OrbitalState,Vector3d> push)
        {
            var x=DeltaPosition; var v=DeltaVelocity; double t=Time;
            var a=Accel(t,x,v,push); var vx=v+a*(dt/2); var xx=x+v*(dt/2);
            var b=Accel(t+dt/2,xx,vx,push); var vy=v+b*(dt/2); var xy=x+vx*(dt/2);
            var c=Accel(t+dt/2,xy,vy,push); var vz=v+c*dt; var xz=x+vy*dt;
            var d=Accel(t+dt,xz,vz,push);
            DeltaPosition=x+(v+2*vx+2*vy+vz)*(dt/6); DeltaVelocity=v+(a+2*b+2*c+d)*(dt/6); Time=t+dt;
        }
    }
}
