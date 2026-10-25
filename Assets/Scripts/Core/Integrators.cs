using System;
namespace OctoEarth.Core
{
    public interface IIntegrator { string Name { get; } void Step(SystemState state,GravityModel gravity,double dt); }
    public sealed class VelocityVerlet : IIntegrator
    {
        public string Name => "VelocityVerlet";
        Vector3d[] a;
        public void Step(SystemState s,GravityModel gravity,double dt)
        {
            if(a==null || a.Length!=s.Bodies.Length) a=new Vector3d[s.Bodies.Length];
            gravity.Accelerations(s,a);
            for(int i=0;i<a.Length;i++) { s.Bodies[i].Velocity+=a[i]*(dt/2); s.Bodies[i].Position+=s.Bodies[i].Velocity*dt; }
            gravity.Accelerations(s,a);
            for(int i=0;i<a.Length;i++) s.Bodies[i].Velocity+=a[i]*(dt/2);
            s.Time+=dt;
        }
    }
    public sealed class Yoshida4 : IIntegrator
    {
        // Yoshida 1990, Phys. Lett. A 150:262, triple-jump symmetric composition.
        // Negative middle substep applies ONLY to conservative gravity.
        static readonly double W1=1/(2-Math.Pow(2,1.0/3)), W0=-Math.Pow(2,1.0/3)/(2-Math.Pow(2,1.0/3));
        readonly VelocityVerlet verlet=new VelocityVerlet();
        public string Name => "Yoshida4";
        public void Step(SystemState s,GravityModel g,double dt)
        {
            double t=s.Time; verlet.Step(s,g,W1*dt); verlet.Step(s,g,W0*dt); verlet.Step(s,g,W1*dt); s.Time=t+dt;
        }
    }
    public sealed class SimulationClock
    {
        public bool Started {get;private set;}
        public bool Running {get;private set;}
        public double PendingSeconds {get;private set;}
        public void Start() { if(!Started) { Started=true; Running=true; } }
        public void Pause() { Running=false; }
        public void Resume() { if(Started) Running=true; }
        public void Reset() { Started=Running=false; PendingSeconds=0; }
        public int Advance(double wallSeconds,double rate,double dt,int cap,Action step)
        {
            if(wallSeconds<0 || rate<0 || !(dt>0) || cap<1) throw new ArgumentOutOfRangeException();
            if(!Running) return 0; PendingSeconds+=wallSeconds*rate; int n=0;
            while(PendingSeconds>=dt && n<cap) { step(); PendingSeconds-=dt; n++; } return n;
        }
    }
}
