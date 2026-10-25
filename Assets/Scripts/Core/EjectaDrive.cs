using System;
namespace OctoEarth.Core
{
    public interface IExternalKick : IForceModel { void Kick(SystemState state,Ledger ledger,double dt); }
    public sealed class EjectaDrive : IExternalKick
    {
        public int BodyIndex;
        public double Thrust,ExhaustSpeed;
        public Vector3d Direction;
        public bool Replenished;
        public string EnergySourceName="DriveEnergySource";
        public Vector3d LastEjectaPlus,LastEjectaMinus;
        public Vector3d ExhaustAxisPlus,ExhaustAxisMinus;
        public double LastEjectedMass;
        public Action<double> BeforeEject;
        public bool BoundEjecta {get;private set;}
        KahanSum ejectedMass;
        public double TotalEjected => ejectedMass.Value;
        public void ResetBookkeeping() { BoundEjecta=false; ejectedMass=new KahanSum(); }
        public EjectaDrive(int bodyIndex,double thrust,double exhaustSpeed,Vector3d direction,bool replenished=true)
        {
            if(thrust<0 || !(exhaustSpeed>0) || direction.Magnitude==0) throw new ArgumentOutOfRangeException();
            BodyIndex=bodyIndex; Thrust=thrust; ExhaustSpeed=exhaustSpeed; Direction=direction.Normalized; Replenished=replenished;
        }
        public void AddAccelerations(SystemState s,Vector3d[] a) { a[BodyIndex]+=Direction*(Thrust/s.Bodies[BodyIndex].Mass); }
        public void BookReactions(Ledger ledger,double dt) { /* Booking is atomic with Kick; no double booking. */ }
        public void Kick(SystemState s,Ledger ledger,double dt)
        {
            if(dt<0) throw new ArgumentOutOfRangeException(nameof(dt));
            var b=s.Bodies[BodyIndex]; double dm=Thrust/ExhaustSpeed*dt; LastEjectedMass=dm; if(dm==0) return;
            if(!Replenished && dm>=b.Mass) throw new InvalidOperationException("Propellant exhausted");
            BeforeEject?.Invoke(dm);
            double m0=b.Mass; var v0=b.Velocity; var dir=Direction.Normalized; double m1=Replenished ? m0 : m0-dm;
            double dv=Replenished ? Thrust*dt/m0 : ExhaustSpeed*Math.Log(m0/m1);
            var delta=dir*dv; var midpoint=v0+delta/2; b.Velocity=v0+delta; b.Mass=m1;
            Vector3d incoming=Replenished ? midpoint*dm : new Vector3d();
            // Exact finite-interval body momentum balance; variable-mass mode obeys rocket equation.
            Vector3d bodyChange=Replenished ? delta*m0 : delta*m1-v0*dm;
            Vector3d exhaust=Replenished ? incoming-dir*(Thrust*dt) : new Vector3d();
            double potential=Ledger.ParcelPotential(s,BodyIndex,dm);
            double ejectaK;
            if(Replenished) ejectaK=dm*(midpoint-dir*ExhaustSpeed).SqrMagnitude/2;
            else
            {
                // Eight-point Gauss-Legendre quadrature of parcel KE during exact finite rocket burn.
                double[] nodes={-0.9602898564975363,-0.7966664774136267,-0.525532409916329,-0.1834346424956498,0.1834346424956498,0.525532409916329,0.7966664774136267,0.9602898564975363};
                double[] weights={0.1012285362903763,0.2223810344533745,0.3137066458778873,0.362683783378362,0.362683783378362,0.3137066458778873,0.2223810344533745,0.1012285362903763};
                ejectaK=0; for(int k=0;k<8;k++) { double mass=m0-dm*(nodes[k]+1)/2; var vel=v0+dir*(ExhaustSpeed*(Math.Log(m0/mass)-1)); ejectaK+=weights[k]*dm/2*vel.SqrMagnitude/2; exhaust+=vel*(weights[k]*dm/2); }
            }
            ledger.Book("EjectaSink",dm,exhaust,Vector3d.Cross(b.Position,exhaust),ejectaK+potential);
            double incomingK=Replenished ? dm*midpoint.SqrMagnitude/2 : 0;
            ledger.Book("EnablerSource",Replenished ? -dm : 0,-1*incoming,Vector3d.Cross(b.Position,-1*incoming),-incomingK-(Replenished ? potential : 0));
            // An explicitly declared energy input, not a conservation claim for bodies alone.
            ledger.Book(EnergySourceName,0,new Vector3d(),new Vector3d(),-dm*ExhaustSpeed*ExhaustSpeed/2);
            var partner=exhaust-incoming;
            ledger.CheckImpulse(bodyChange,Replenished ? dir*(Thrust*dt) : bodyChange,partner);
            ejectedMass.Add(dm);
            int sun=-1; for(int i=0;i<s.Bodies.Length;i++) if(s.Bodies[i].Name=="Sun") sun=i;
            if(sun>=0 && sun!=BodyIndex)
            {
                var veject=exhaust/dm-s.Bodies[sun].Velocity;
                double r=(b.Position-s.Bodies[sun].Position).Magnitude;
                BoundEjecta |= veject.SqrMagnitude/2-Constants.G*s.Bodies[sun].Mass/r<0;
            }
        }
        public void KickCanted(SystemState s,Ledger ledger,double duration,Vector3d axis,Vector3d normal,double cantRadians)
        {
            if(!Replenished || !(duration>0) || cantRadians<0 || cantRadians>=Math.PI/2) throw new ArgumentOutOfRangeException();
            if(Thrust==0) return;
            axis=axis.Normalized; normal=(normal-axis*Vector3d.Dot(normal,axis)).Normalized;
            if(normal.Magnitude==0) throw new InvalidOperationException("Cant normal parallel to thrust");
            double c=Math.Cos(cantRadians),sn=Math.Sin(cantRadians),dm=Thrust*duration/(ExhaustSpeed*c);
            LastEjectedMass=dm;
            BeforeEject?.Invoke(dm);
            var body=s.Bodies[BodyIndex]; var dv=axis*(Thrust*duration/body.Mass); var midpoint=body.Velocity+dv/2;
            body.Velocity+=dv;
            var a=midpoint-(axis*c+normal*sn)*ExhaustSpeed; var b=midpoint-(axis*c-normal*sn)*ExhaustSpeed;
            ExhaustAxisPlus=-(1.0)*(axis*c+normal*sn); ExhaustAxisMinus=-(1.0)*(axis*c-normal*sn);
            LastEjectaPlus=a; LastEjectaMinus=b;
            double potential=Ledger.ParcelPotential(s,BodyIndex,dm);
            var pa=a*(dm/2); var pb=b*(dm/2);
            ledger.Book("EjectaSink",dm/2,pa,Vector3d.Cross(body.Position,pa),dm*a.SqrMagnitude/4+potential/2);
            ledger.Book("EjectaSink",dm/2,pb,Vector3d.Cross(body.Position,pb),dm*b.SqrMagnitude/4+potential/2);
            var supplied=midpoint*dm;
            ledger.Book("EnablerSource",-dm,-1*supplied,Vector3d.Cross(body.Position,-1*supplied),-dm*midpoint.SqrMagnitude/2-potential);
            ledger.Book(EnergySourceName,0,new Vector3d(),new Vector3d(),-dm*ExhaustSpeed*ExhaustSpeed/2);
            var impulse=dv*body.Mass;
            ledger.CheckImpulse(impulse,axis*(Thrust*duration),pa+pb-supplied);
            ejectedMass.Add(dm);
            int sun=-1; for(int k=0;k<s.Bodies.Length;k++) if(s.Bodies[k].Name=="Sun") sun=k;
            if(sun>=0 && sun!=BodyIndex)
            {
                double r=(body.Position-s.Bodies[sun].Position).Magnitude;
                BoundEjecta |= (a-s.Bodies[sun].Velocity).SqrMagnitude/2-Constants.G*s.Bodies[sun].Mass/r<0;
                BoundEjecta |= (b-s.Bodies[sun].Velocity).SqrMagnitude/2-Constants.G*s.Bodies[sun].Mass/r<0;
            }
        }
    }
    public sealed class ProgradeAcceleration : IExternalKick
    {
        readonly int bodyIndex,centerIndex;
        readonly double acceleration;
        public ProgradeAcceleration(int bodyIndex,int centerIndex,double acceleration) { this.bodyIndex=bodyIndex; this.centerIndex=centerIndex; this.acceleration=acceleration; }
        public void AddAccelerations(SystemState s,Vector3d[] a) { a[bodyIndex]+=(s.Bodies[bodyIndex].Velocity-s.Bodies[centerIndex].Velocity).Normalized*acceleration; }
        public void BookReactions(Ledger ledger,double dt) { }
        public void Kick(SystemState s,Ledger l,double dt)
        {
            var b=s.Bodies[bodyIndex]; var dv=(b.Velocity-s.Bodies[centerIndex].Velocity).Normalized*(acceleration*dt); var dp=dv*b.Mass;
            double work=b.Mass*Vector3d.Dot(b.Velocity+dv/2,dv); b.Velocity+=dv;
            l.Book("BenchmarkExternalPartner",0,-1*dp,Vector3d.Cross(b.Position,-1*dp),-work); l.CheckImpulse(dp,dp,-1*dp);
        }
    }
}
