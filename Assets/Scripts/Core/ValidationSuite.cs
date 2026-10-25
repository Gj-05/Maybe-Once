using System;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public static class ValidationSuite
    {
        public static List<BenchmarkResult> Run(string path)
        {
            var all=new List<BenchmarkResult>(); for(int i=1;i<=10;i++) all.AddRange(Run(i,path)); all.AddRange(MissionBudgetChecks.GuideSanity()); return all;
        }
        public static List<BenchmarkResult> Run(int check,string path)
        {
            var b=new List<BenchmarkResult>(); double au=Constants.AU,mu=Constants.SolarMu;
            if(check==1)
            {
                double period=KeplerOrbit.Period(au,mu+Constants.G); var r=new SimulationRunner(KeplerOrbit.Binary(au),period/20000); var p=r.State.Bodies[1].Position; r.RunSteps(20000);
                b.Add(new BenchmarkResult("X1","Circular full-period return",(r.State.Bodies[1].Position-p).Magnitude/au,0,1e-9,"AU",dt:r.Dt));
                double ratio=Math.Pow(KeplerOrbit.Period(2.7*au,mu)/KeplerOrbit.Period(au,mu),2);
                b.Add(new BenchmarkResult("X2","Kepler period squared ratio",ratio,Math.Pow(2.7,3),1e-12,"dimensionless"));
            }
            if(check==2)
            {
                var s=new SystemState(new Body("Sun",1000,new Vector3d(-10,0,0),new Vector3d()),new Body("Rocket",100,new Vector3d(1,2,0),new Vector3d()));
                var l=new Ledger(s); var drive=new EjectaDrive(1,10,100,new Vector3d(1,0,0),false); drive.Kick(s,l,1); l.Check(s);
                double expected=100*Math.Log(100/99.9);
                b.Add(new BenchmarkResult("X5.rocket","Finite burn rocket equation",s.Bodies[1].Velocity.X,expected,expected*1e-9,"m/s"));
                b.Add(new BenchmarkResult("X5.momentum","Rocket booked momentum closure",l.MaxMomentumError,0,1e-9,"relative"));
                b.Add(new BenchmarkResult("X5.energy","Rocket energy closure",l.MaxEnergyError,0,1e-9,"relative"));
                b.Add(new BenchmarkResult("X5.reaction","Rocket impulse reaction closure",l.MaxReactionClosure,0,1e-9,"relative"));
                var open=new SystemState(new Body("Sun",1000,new Vector3d(-10,0,0),new Vector3d()),new Body("Earth",100,new Vector3d(1,2,0),new Vector3d(4,5,0)));
                var ledger=new Ledger(open); var supplied=new EjectaDrive(1,10,100,new Vector3d(1,0,0)); supplied.Kick(open,ledger,1); ledger.Check(open);
                b.Add(new BenchmarkResult("X5.sourceP","Replenished source/ejecta P closure",ledger.MaxMomentumError,0,1e-9,"relative"));
                b.Add(new BenchmarkResult("X5.sourceE","Replenished energy input closure",ledger.MaxEnergyError,0,1e-9,"relative"));
                b.Add(new BenchmarkResult("X5.sourceL","Replenished angular closure",ledger.MaxAngularMomentumError,0,1e-9,"relative"));
                b.Add(new BenchmarkResult("X5.force","Applied force/body impulse closure",ledger.MaxForceClosure,0,1e-9,"relative"));
            }
            if(check==3)
            {
                b.AddRange(Convergence(new Yoshida4(),4,0.2)); b.AddRange(Convergence(new VelocityVerlet(),2,0.12));
            }
            if(check==4)
            {
                var el=new OrbitalElements{A=au}; var encke=new Encke(el,mu); var free=new Encke(el,mu);
                var direct=new SimulationRunner(KeplerOrbit.Binary(au),60); var baseRun=new SimulationRunner(KeplerOrbit.Binary(au),60); direct.Forces.Add(new ProgradeAcceleration(1,0,1e-8));
                for(int k=0;k<1440;k++) { encke.Step(60,s=>s.Velocity.Normalized*1e-8); free.Step(60,s=>new Vector3d()); direct.Step(); baseRun.Step(); }
                var difference=direct.State.Bodies[1].Position-baseRun.State.Bodies[1].Position;
                double error=(difference-encke.DeltaPosition).Magnitude;
                b.Add(new BenchmarkResult("X7","Encke versus double push displacement",error,0,0.05,"m","60s RK4 deviation vs double split Yoshida; independent gravity representations",60));
                b.Add(new BenchmarkResult("X7.reference","Unforced Encke deviation",free.DeltaPosition.Magnitude,0,1e-20,"m",dt:60));
                double floatDifference=(FloatOrbit(true)-FloatOrbit(false)).Magnitude;
                b.Add(new BenchmarkResult("X8.float","Float loses day-long tiny push",floatDifference,0,0,"m","Float push/control updates identical at this dt",60));
                b.Add(new BenchmarkResult("X8.double","Double retains tiny push",difference.Magnitude,0,0,"m","This row expects nonzero displacement; pass is computed below",60));
                var last=b[b.Count-1]; last.expected=encke.DeltaPosition.Magnitude; last.error=Math.Abs(last.measured-last.expected); last.tolerance=0.05; last.passed=last.measured>1 && last.error<last.tolerance;
            }
            if(check==5)
            {
                b.AddRange(MissionBudgetChecks.Analytic());
            }
            if(check==6)
            {
                var r=new SimulationRunner(InitialConditions.SolarSystem(path),3600); bool bound=true; double maxDistance=0;
                for(int k=0;k<87660;k++) { r.Step(); bound &= InitialConditions.MoonBound(r.State,out _,out _); var e=r.State.Bodies[r.State.Index("Earth")]; var m=r.State.Bodies[r.State.Index("Moon")]; maxDistance=Math.Max(maxDistance,(m.Position-e.Position).Magnitude); }
                b.Add(new BenchmarkResult("X10","Moon bound over 10yr including planets",bound?1:0,1,0,"boolean","maximum lunar distance m="+maxDistance.ToString("R",System.Globalization.CultureInfo.InvariantCulture),r.Dt));
            }
            if(check==7)
            {
                var a=new SimulationRunner(InitialConditions.SolarSystem(path)); var c=new SimulationRunner(InitialConditions.SolarSystem(path)); a.RunSteps(100); c.RunSteps(100); bool same=Equal(a.State,c.State); a.Reset(); a.RunSteps(100); same &=Equal(a.State,c.State);
                b.Add(new BenchmarkResult("X11","Reset and bitwise determinism",same?1:0,1,0,"boolean"));
                var toy=new SimulationRunner(new SystemState(new Body("Sun",1000,new Vector3d(-10,0,0),new Vector3d()),new Body("Earth",10,new Vector3d(10,0,0),new Vector3d())),0.1);
                var drive=new EjectaDrive(1,1,100,new Vector3d(1,0,0)); toy.Forces.Add(drive); toy.RunSteps(10); var end=toy.State.Clone(); toy.Reset(); bool reset=drive.TotalEjected==0 && toy.Ledger.Lines.Count==0; toy.RunSteps(10);
                b.Add(new BenchmarkResult("X11.forceReset","Stateful force reset and determinism",reset && Equal(end,toy.State)?1:0,1,0,"boolean"));
            }
            if(check==8)
            {
                var el=new OrbitalElements{A=au,E=0.3,I=0.4,Node=1.1,Periapsis=0.7,MeanAnomaly=2.0}; var s=KeplerOrbit.FromElements(el,mu); var returned=KeplerOrbit.FromElements(KeplerOrbit.ToElements(s.Position,s.Velocity,mu),mu);
                b.Add(new BenchmarkResult("X.kepler","Elements-state roundtrip",(returned.Position-s.Position).Magnitude/au,0,1e-12,"AU"));
            }
            if(check==9)
            {
                var clock=new SimulationClock(); int n=0; clock.Start(); clock.Advance(1,7200,3600,1,()=>n++); double pending=clock.PendingSeconds; clock.Pause(); clock.Advance(100,7200,3600,10,()=>n++); clock.Resume(); clock.Advance(0,7200,3600,10,()=>n++); clock.Reset();
                b.Add(new BenchmarkResult("X.clock","Scheduler cap pause resume reset",n==2 && pending==3600 && clock.PendingSeconds==0 && !clock.Running?1:0,1,0,"boolean"));
                var softened=new SimulationRunner(KeplerOrbit.Binary(au),3600,gravity:new GravityModel(0.1*au)); softened.RunSteps(240);
                b.Add(new BenchmarkResult("X.softening","Softened-force consistent energy ledger",softened.Ledger.MaxEnergyError,0,1e-9,"relative","Plummer potential uses same softening as gravity",3600));
            }
            if(check==10)
            {
                // Force partner bookkeeping is verified without assuming body-only conservation.
                var s=KeplerOrbit.Binary(au); var l=new Ledger(s); var momentum=new Vector3d(1,2,3); PhotonSink.Book(l,new Vector3d(2,0,0),momentum,10);
                b.Add(new BenchmarkResult("X.photon","Photon sink angular booking",(l.Lines["PhotonSink"].AngularMomentum-Vector3d.Cross(new Vector3d(2,0,0),momentum)).Magnitude,0,1e-15,"kg m2/s"));
            }
            return b;
        }
        public static List<BenchmarkResult> Slow(string path,double years=10000,double dt=3600,string outputFolder=null)
        {
            var r=new SimulationRunner(InitialConditions.SolarSystem(path,false),dt);
            if(outputFolder==null) r.RunSteps((long)Math.Round(years*Constants.JulianYear/dt));
            else r.WriteRun(outputFolder,"baseline_"+years.ToString("0",System.Globalization.CultureInfo.InvariantCulture), (long)Math.Round(years*Constants.JulianYear/dt),10000);
            return new List<BenchmarkResult>{new BenchmarkResult("X3.slow","Sun Earth Moon long baseline energy",r.Ledger.MaxEnergyError,0,1e-9,"relative","years="+years,dt),new BenchmarkResult("X3.P","Long baseline momentum",r.Ledger.MaxMomentumError,0,1e-9,"relative",dt:dt),new BenchmarkResult("X3.L","Long baseline angular momentum",r.Ledger.MaxAngularMomentumError,0,1e-9,"relative",dt:dt)};
        }
        public static List<BenchmarkResult> Convergence(IIntegrator prototype,double expected,double tolerance)
        {
            double period=KeplerOrbit.Period(Constants.AU,Constants.SolarMu+Constants.G); double sx=0,sy=0,sxx=0,sxy=0; var rows=new List<BenchmarkResult>(); Vector3d previous=new Vector3d();
            for(int k=0;k<4;k++)
            {
                int n=100*(1<<k); double dt=period/n; var r=new SimulationRunner(KeplerOrbit.Binary(Constants.AU,0.3),dt,prototype.Name=="Yoshida4"?(IIntegrator)new Yoshida4():new VelocityVerlet());
                var start=r.State.Bodies[1].Position-r.State.Bodies[0].Position; r.RunSteps(n); double error=((r.State.Bodies[1].Position-r.State.Bodies[0].Position)-start).Magnitude/Constants.AU;
                double x=Math.Log(dt),y=Math.Log(error); sx+=x; sy+=y; sxx+=x*x; sxy+=x*y;
                rows.Add(new BenchmarkResult("X6."+prototype.Name+".h"+n,"Analytic endpoint error",error,0,1,"AU","Order fit sample, not a precision acceptance bound",dt));
                var endpoint=r.State.Bodies[1].Position-r.State.Bodies[0].Position;
                if(k>0) rows.Add(new BenchmarkResult("X6."+prototype.Name+".difference"+n,"h versus h/2 endpoint difference",(endpoint-previous).Magnitude/Constants.AU,0,1,"AU","Convergence sample",dt));
                previous=endpoint;
            }
            double order=(4*sxy-sx*sy)/(4*sxx-sx*sx); rows.Add(new BenchmarkResult("X6."+prototype.Name,"Observed log-log order",order,expected,tolerance,"order")); return rows;
        }
        static bool Equal(SystemState a,SystemState b)
        {
            if(BitConverter.DoubleToInt64Bits(a.Time)!=BitConverter.DoubleToInt64Bits(b.Time)) return false;
            for(int k=0;k<a.Bodies.Length;k++)
            {
                var x=a.Bodies[k]; var y=b.Bodies[k]; double[] v={x.Position.X,x.Position.Y,x.Position.Z,x.Velocity.X,x.Velocity.Y,x.Velocity.Z,x.Mass}; double[] w={y.Position.X,y.Position.Y,y.Position.Z,y.Velocity.X,y.Velocity.Y,y.Velocity.Z,y.Mass};
                for(int j=0;j<v.Length;j++) if(BitConverter.DoubleToInt64Bits(v[j])!=BitConverter.DoubleToInt64Bits(w[j])) return false;
            } return true;
        }
        static Vector3d FloatOrbit(bool push)
        {
            float x=(float)Constants.AU,y=0,vx=0,vy=(float)Math.Sqrt(Constants.SolarMu/Constants.AU),dt=60;
            for(int k=0;k<1440;k++)
            {
                float radius=(float)Math.Sqrt((double)x*x+(double)y*y); float factor=(float)(-Constants.SolarMu/((double)radius*radius*radius));
                vx+=factor*x*dt/2; vy+=factor*y*dt/2;
                if(push) { float speed=(float)Math.Sqrt((double)vx*vx+(double)vy*vy); vx+=(float)1e-8*vx/speed*dt/2; vy+=(float)1e-8*vy/speed*dt/2; }
                x+=vx*dt; y+=vy*dt; radius=(float)Math.Sqrt((double)x*x+(double)y*y); factor=(float)(-Constants.SolarMu/((double)radius*radius*radius)); vx+=factor*x*dt/2; vy+=factor*y*dt/2;
                if(push) { float speed=(float)Math.Sqrt((double)vx*vx+(double)vy*vy); vx+=(float)1e-8*vx/speed*dt/2; vy+=(float)1e-8*vy/speed*dt/2; }
            } return new Vector3d(x,y,0);
        }
    }
}
