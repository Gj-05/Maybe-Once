using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
namespace OctoEarth.Core
{
    public sealed class LedgerLine
    {
        public string Name;
        KahanSum mass,energy;
        KahanVector momentum,angularMomentum;
        public double Mass => mass.Value;
        public double Energy => energy.Value;
        public Vector3d Momentum => momentum.Value;
        public Vector3d AngularMomentum => angularMomentum.Value;
        public void Add(double dm,Vector3d dp,Vector3d dl,double de) { mass.Add(dm); momentum.Add(dp); angularMomentum.Add(dl); energy.Add(de); }
    }
    public sealed class Ledger
    {
        readonly Dictionary<string,LedgerLine> lines=new Dictionary<string,LedgerLine>();
        readonly Vector3d initialP,initialL;
        readonly double initialE;
        readonly double softening;
        readonly double momentumScale,angularScale;
        KahanVector appliedImpulse,partnerImpulse;
        public Vector3d AppliedImpulse => appliedImpulse.Value;
        public Vector3d PartnerImpulse => partnerImpulse.Value;
        public double MaxMomentumError,MaxAngularMomentumError,MaxEnergyError,MaxForceClosure,MaxReactionClosure;
        public IReadOnlyDictionary<string,LedgerLine> Lines => lines;
        public Ledger(SystemState state,double softening=0,IEnumerable<LedgerLine> initialTransfers=null)
        {
            this.softening=softening;
            initialP=Momentum(state); initialL=AngularMomentum(state); initialE=Energy(state,softening);
            if(initialTransfers!=null)
            {
                var p0=new KahanVector(); p0.Add(initialP); var l0=new KahanVector(); l0.Add(initialL); var e0=new KahanSum(); e0.Add(initialE);
                foreach(var line in initialTransfers) { lines.Add(line.Name,line); p0.Add(line.Momentum); l0.Add(line.AngularMomentum); e0.Add(line.Energy); }
                initialP=p0.Value; initialL=l0.Value; initialE=e0.Value;
            }
            var p=new KahanSum(); var l=new KahanSum(); foreach(var b in state.Bodies) { p.Add(b.Mass*b.Velocity.Magnitude); l.Add(Vector3d.Cross(b.Position,b.Velocity*b.Mass).Magnitude); }
            momentumScale=Math.Max(1,p.Value); angularScale=Math.Max(1,l.Value);
        }
        public void Book(string name,double mass,Vector3d momentum,Vector3d angular,double energy)
        {
            if(!lines.TryGetValue(name,out var line)) { line=new LedgerLine{Name=name}; lines.Add(name,line); }
            line.Add(mass,momentum,angular,energy);
        }
        public void CheckImpulse(Vector3d deltaBody,Vector3d applied,Vector3d partner)
        {
            appliedImpulse.Add(applied); partnerImpulse.Add(partner); double scale=applied.Magnitude;
            if(scale>0) { MaxForceClosure=Math.Max(MaxForceClosure,(deltaBody-applied).Magnitude/scale); MaxReactionClosure=Math.Max(MaxReactionClosure,(applied+partner).Magnitude/scale); }
        }
        public void Check(SystemState s)
        {
            var p=new KahanVector(); p.Add(Momentum(s)); var l=new KahanVector(); l.Add(AngularMomentum(s)); var e=new KahanSum(); e.Add(Energy(s,softening));
            foreach(var line in lines.Values) { p.Add(line.Momentum); l.Add(line.AngularMomentum); e.Add(line.Energy); }
            MaxMomentumError=Math.Max(MaxMomentumError,(p.Value-initialP).Magnitude/momentumScale);
            MaxAngularMomentumError=Math.Max(MaxAngularMomentumError,(l.Value-initialL).Magnitude/angularScale);
            MaxEnergyError=Math.Max(MaxEnergyError,Math.Abs(e.Value-initialE)/Math.Max(1,Math.Abs(initialE)));
        }
        public static Vector3d Momentum(SystemState s) { var p=new KahanVector(); foreach(var b in s.Bodies) p.Add(b.Velocity*b.Mass); return p.Value; }
        public static Vector3d AngularMomentum(SystemState s) { var p=new KahanVector(); foreach(var b in s.Bodies) p.Add(Vector3d.Cross(b.Position,b.Velocity*b.Mass)); return p.Value; }
        public static double Energy(SystemState s,double softening=0)
        {
            var e=new KahanSum(); for(int i=0;i<s.Bodies.Length;i++)
            {
                var b=s.Bodies[i]; e.Add(b.Mass*b.Velocity.SqrMagnitude/2);
                for(int j=i+1;j<s.Bodies.Length;j++) e.Add(-Constants.G*b.Mass*s.Bodies[j].Mass/Math.Sqrt((b.Position-s.Bodies[j].Position).SqrMagnitude+softening*softening));
            } return e.Value;
        }
        public static double ParcelPotential(SystemState s,int emitting,double parcelMass)
        {
            var sum=new KahanSum(); for(int j=0;j<s.Bodies.Length;j++) if(j!=emitting) sum.Add(-Constants.G*parcelMass*s.Bodies[j].Mass/(s.Bodies[j].Position-s.Bodies[emitting].Position).Magnitude); return sum.Value;
        }
        public string Report()
        {
            var b=new StringBuilder("Ledger (SI; relative errors normalized by initial gross body scales)\nName | mass kg | px,py,pz kg m/s | Lx,Ly,Lz kg m²/s | energy J\n");
            foreach(var x in lines.Values) b.AppendFormat(CultureInfo.InvariantCulture,"{0} | {1:R} | {2:R},{3:R},{4:R} | {5:R},{6:R},{7:R} | {8:R}\n",x.Name,x.Mass,x.Momentum.X,x.Momentum.Y,x.Momentum.Z,x.AngularMomentum.X,x.AngularMomentum.Y,x.AngularMomentum.Z,x.Energy);
            b.AppendLine("net momentum gained by Earth system vs momentum carried by ejecta");
            b.AppendFormat(CultureInfo.InvariantCulture,"Applied impulse magnitude {0:R}; reaction impulse magnitude {1:R}\nMax relative P={2:R}; L={3:R}; E={4:R}; force closure={5:R}; reaction closure={6:R}\n",AppliedImpulse.Magnitude,PartnerImpulse.Magnitude,MaxMomentumError,MaxAngularMomentumError,MaxEnergyError,MaxForceClosure,MaxReactionClosure); return b.ToString();
        }
    }
    public static class PhotonSink
    {
        public static void Book(Ledger ledger,Vector3d position,Vector3d photonMomentum,double energy) => ledger.Book("PhotonSink",0,photonMomentum,Vector3d.Cross(position,photonMomentum),energy);
    }
}
