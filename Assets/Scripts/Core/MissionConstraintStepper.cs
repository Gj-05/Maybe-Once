using System;
namespace OctoEarth.Core
{
    public sealed class MissionConstraintStepper
    {
        readonly IIntegrator integrator=new Yoshida4();
        readonly GravityModel gravity=new GravityModel();
        readonly SystemState scratch;
        readonly MissionTargets targets;
        readonly int[] controlled;
        readonly EjectaDrive[] drives;
        readonly Vector3d[] oldKick,previousKick,guess;
        readonly MissionConfiguration config;
        int history;
        bool departureComplete;
        public bool DepartureComplete => departureComplete;
        public double MaxPositionResidual,MaxImpulseError;
        KahanSum jetEnergy;
        public double JetEnergy => jetEnergy.Value;
        public double StepJetEnergy {get;private set;}
        public double Ejected { get { double m=0; foreach(var d in drives) m+=d.TotalEjected; return m; } }
        public Vector3d TractorThrust;
        public readonly Vector3d[] LastThrust=new Vector3d[3];
        public EjectaDrive[] Drives => drives;
        public void SetResourceCheck(Action<double> check) { foreach(var drive in drives) drive.BeforeEject=check; }
        public MissionConstraintStepper(SystemState s,MissionConfiguration c,MissionTargets targets)
        {
            config=c; this.targets=targets; controlled=targets.Controlled; scratch=s.Clone(); drives=new EjectaDrive[controlled.Length];
            oldKick=new Vector3d[controlled.Length]; previousKick=new Vector3d[controlled.Length]; guess=new Vector3d[controlled.Length];
            for(int k=0;k<drives.Length;k++) drives[k]=new EjectaDrive(controlled[k],0,c.ExhaustSpeed,new Vector3d(1,0,0)){EnergySourceName="SolarArrayJetEnergySource"};
        }
        void Kick(SystemState s,Ledger ledger,int k,Vector3d dv,double duration)
        {
            double force=dv.Magnitude*s.Bodies[controlled[k]].Mass/duration; if(force==0) return;
            var earth=s.Bodies[s.Index("Earth")]; var sun=s.Bodies[s.Index("Sun")]; var normal=Vector3d.Cross(earth.Position-sun.Position,earth.Velocity-sun.Velocity).Normalized;
            var axis=dv.Normalized; normal=(normal-axis*Vector3d.Dot(normal,axis)).Normalized;
            if(normal.Magnitude<0.1) normal=Vector3d.Cross(axis,new Vector3d(1,0,0)).Normalized;
            if(normal.Magnitude<0.1) normal=Vector3d.Cross(axis,new Vector3d(0,1,0)).Normalized;
            var body=s.Bodies[controlled[k]]; var before=body.Velocity;
            if(config.SteerPlumes) normal=PlumeSteering.Select(s,controlled[k],axis,normal,config);
            drives[k].Thrust=force; drives[k].KickCanted(s,ledger,duration,axis,normal,Units.Radians(config.CantDegrees));
            double energy=drives[k].LastEjectedMass*config.ExhaustSpeed*config.ExhaustSpeed/2; jetEnergy.Add(energy); StepJetEnergy+=energy;
            MaxImpulseError=Math.Max(MaxImpulseError,((body.Velocity-before)*body.Mass-dv*body.Mass).Magnitude/(dv.Magnitude*body.Mass));
            if(k==0) TractorThrust=axis*force;
            LastThrust[k]=axis*force;
        }
        public void Step(SystemState s,Ledger ledger,MissionRadiation radiation,double dt)
        {
            StepJetEnergy=0; targets.Begin(s); radiation.Kick(s,ledger,dt/2);
            bool freeTractor=targets.TractorUnconstrained;
            for(int k=0;k<guess.Length;k++) guess[k]=k==0 && freeTractor?new Vector3d():history==0?new Vector3d():history==1?previousKick[k]:2*previousKick[k]-oldKick[k];
            if(freeTractor) DepartureKick(s,ledger,dt/2);
            double residual=double.PositiveInfinity;
            for(int iteration=0;iteration<16;iteration++)
            {
                scratch.Time=s.Time; for(int i=0;i<s.Bodies.Length;i++) { scratch.Bodies[i].Position=s.Bodies[i].Position; scratch.Bodies[i].Velocity=s.Bodies[i].Velocity; scratch.Bodies[i].Mass=s.Bodies[i].Mass; }
                for(int k=0;k<guess.Length;k++) scratch.Bodies[controlled[k]].Velocity+=guess[k];
                integrator.Step(scratch,gravity,dt); residual=0;
                for(int k=0;k<guess.Length;k++)
                {
                    if(k==0 && freeTractor) continue;
                    targets.Get(scratch,controlled[k],out var r,out _); var error=r-scratch.Bodies[controlled[k]].Position; residual=Math.Max(residual,error.Magnitude);
                    if(error.Magnitude>0.01) guess[k]+=error/dt;
                }
                if(residual<=0.01) break;
            }
            if(residual>0.01) throw new InvalidOperationException("Mission prescribed solver residual m="+residual);
            for(int k=0;k<guess.Length;k++) { oldKick[k]=previousKick[k]; previousKick[k]=guess[k]; if(k!=0 || !freeTractor) Kick(s,ledger,k,guess[k],dt/2); }
            history++; integrator.Step(s,gravity,dt); radiation.Kick(s,ledger,dt/2);
            for(int k=0;k<guess.Length;k++)
            {
                if(k==0 && freeTractor) { DepartureKick(s,ledger,dt/2); continue; }
                targets.Get(s,controlled[k],out var r,out var v); MaxPositionResidual=Math.Max(MaxPositionResidual,(s.Bodies[controlled[k]].Position-r).Magnitude);
                Kick(s,ledger,k,v-s.Bodies[controlled[k]].Velocity,dt/2);
            }
            targets.Finish(s); ledger.Check(s);
        }
        void DepartureKick(SystemState s,Ledger ledger,double duration)
        {
            TractorThrust=new Vector3d(); LastThrust[0]=new Vector3d(); drives[0].Thrust=0;
            if(config.PostTargetPolicy!="Depart" || departureComplete) return;
            var e=s.Bodies[s.Index("Earth")]; var t=s.Bodies[controlled[0]]; var sun=s.Bodies[s.Index("Sun")];
            var r=t.Position-e.Position; var v=t.Velocity-e.Velocity;
            double energy=v.SqrMagnitude/2-Constants.G*(e.Mass+t.Mass)/r.Magnitude;
            if(energy>=5e5 && Vector3d.Dot(r,v)>0 && r.Magnitude>=2*config.Distance) {departureComplete=true; return;}
            var normal=Vector3d.Cross(e.Position-sun.Position,e.Velocity-sun.Velocity).Normalized;
            var direction=(r.Normalized+normal*Math.Tan(Units.Radians(5))).Normalized;
            double force=new MissionBudget(Constants.AU,2.7*Constants.AU,Constants.PairMass,10000*Constants.JulianYear).Force*config.ThrustMargin*1.02;
            Kick(s,ledger,0,direction*(force*duration/t.Mass),duration);
        }
    }
}
