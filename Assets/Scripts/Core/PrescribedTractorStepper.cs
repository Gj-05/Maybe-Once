using System;
namespace OctoEarth.Core
{
    // Inverse discrete hover: solve an initial physical impulse so conservative propagation ends
    // at the prescribed position, then book the ending velocity impulse. Never teleport a body.
    public sealed class PrescribedTractorStepper
    {
        readonly IIntegrator integrator;
        readonly GravityModel gravity=new GravityModel();
        readonly TractorHoverDrive drive;
        readonly int tractor;
        Vector3d previousKick,olderKick;
        int history;
        double previousDt;
        readonly SystemState scratch;
        public double MaxPositionResidual {get;private set;}
        public TractorHoverDrive Drive => drive;
        public PrescribedTractorStepper(SystemState s,TractorParameters config,IIntegrator integrator=null)
        { tractor=s.Index("Tractor"); drive=new TractorHoverDrive(s,config); this.integrator=integrator??new Yoshida4(); scratch=s.Clone(); }
        public void Step(SystemState s,Ledger ledger,double dt)
        {
            if(!(dt>0)) throw new ArgumentOutOfRangeException(nameof(dt));
            var dv=history==0 || previousDt!=dt ? new Vector3d() : history==1 ? previousKick : previousKick*2-olderKick;
            double residual=double.PositiveInfinity; SystemState trial=null;
            // Fixed point iteration uses the local drift response d(position)/d(initial velocity)≈dt.
            // The 1cm solver residual is a physical-length stopping rule; gate tolerances stay unchanged.
            for(int k=0;k<12;k++)
            {
                trial=scratch; trial.Time=s.Time;
                for(int i=0;i<s.Bodies.Length;i++) { trial.Bodies[i].Position=s.Bodies[i].Position; trial.Bodies[i].Velocity=s.Bodies[i].Velocity; trial.Bodies[i].Mass=s.Bodies[i].Mass; }
                trial.Bodies[tractor].Velocity+=dv; integrator.Step(trial,gravity,dt);
                drive.Target(trial,out var r,out _,out _,out _);
                var error=r-trial.Bodies[tractor].Position; residual=error.Magnitude;
                if(residual<=0.01) break;
                dv+=error/dt;
            }
            if(residual>0.01) throw new InvalidOperationException("Prescribed hover solver did not converge: residual m="+residual);
            olderKick=previousKick; previousKick=dv; history++; previousDt=dt;
            drive.ApplyVelocityChange(s,ledger,dv,dt/2);
            integrator.Step(s,gravity,dt);
            drive.Target(s,out var target,out var velocity,out _,out _);
            MaxPositionResidual=Math.Max(MaxPositionResidual,(s.Bodies[tractor].Position-target).Magnitude);
            drive.ApplyVelocityChange(s,ledger,velocity-s.Bodies[tractor].Velocity,dt/2);
            ledger.Check(s);
        }
    }
}
