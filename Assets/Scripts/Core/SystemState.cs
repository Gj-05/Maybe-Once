using System;
namespace OctoEarth.Core
{
    public sealed class Body
    {
        public string Name;
        public double Mass;
        public Vector3d Position, Velocity;
        public Body(string name,double mass,Vector3d position,Vector3d velocity)
        {
            if (!(mass>0) || double.IsInfinity(mass)) throw new ArgumentOutOfRangeException(nameof(mass));
            Name=name; Mass=mass; Position=position; Velocity=velocity;
        }
        public Body Clone() => new Body(Name,Mass,Position,Velocity);
    }
    public sealed class SystemState
    {
        public Body[] Bodies { get; }
        public double Time;
        public SystemState(params Body[] bodies) { Bodies=bodies; }
        public SystemState Clone() { var b=new Body[Bodies.Length]; for(int i=0;i<b.Length;i++) b[i]=Bodies[i].Clone(); return new SystemState(b){Time=Time}; }
        public int Index(string name) { for(int i=0;i<Bodies.Length;i++) if(Bodies[i].Name==name) return i; throw new ArgumentException(name); }
        public void Barycenter()
        {
            var p=new KahanVector(); var r=new KahanVector(); var m=new KahanSum();
            foreach(var b in Bodies) { m.Add(b.Mass); p.Add(b.Velocity*b.Mass); r.Add(b.Position*b.Mass); }
            var center=r.Value/m.Value; var speed=p.Value/m.Value;
            foreach(var b in Bodies) { b.Position-=center; b.Velocity-=speed; }
        }
    }
    public interface IForceModel
    {
        void AddAccelerations(SystemState state,Vector3d[] accel);
        // Called by positive-time external kicks, never by negative Yoshida gravity substeps.
        void BookReactions(Ledger ledger,double dt);
    }
    public sealed class GravityModel
    {
        public double Softening { get; }
        KahanVector[] sums;
        public GravityModel(double softening=0) { if(softening<0) throw new ArgumentOutOfRangeException(nameof(softening)); Softening=softening; }
        public void Accelerations(SystemState s,Vector3d[] a)
        {
            if(sums==null || sums.Length!=a.Length) sums=new KahanVector[a.Length];
            Array.Clear(sums,0,sums.Length);
            for(int i=0;i<s.Bodies.Length;i++) for(int j=i+1;j<s.Bodies.Length;j++)
            {
                var d=s.Bodies[j].Position-s.Bodies[i].Position; double r2=d.SqrMagnitude+Softening*Softening;
                if(r2==0) throw new InvalidOperationException("Coincident bodies with zero softening");
                var factor=d*(Constants.G/(r2*Math.Sqrt(r2)));
                sums[i].Add(factor*s.Bodies[j].Mass); sums[j].Add(factor*(-s.Bodies[i].Mass));
            }
            for(int i=0;i<a.Length;i++) a[i]=sums[i].Value;
        }
    }
}
