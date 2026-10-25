using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text;
namespace OctoEarth.Core
{
    public sealed class SimulationRunner
    {
        readonly SystemState initial;
        public SystemState State {get;private set;}
        public Ledger Ledger {get;private set;}
        public IIntegrator Integrator {get;}
        public GravityModel Gravity {get;}
        public double Dt {get;}
        public List<IExternalKick> Forces {get;}=new List<IExternalKick>();
        public List<Action<SystemState,Ledger>> Monitors {get;}=new List<Action<SystemState,Ledger>>();
        public Action<SystemState,Ledger> Logger;
        public SimulationRunner(SystemState state,double dt=3600,IIntegrator integrator=null,GravityModel gravity=null)
        {
            if(!(dt>0) || double.IsInfinity(dt)) throw new ArgumentOutOfRangeException(nameof(dt));
            initial=state.Clone(); State=state.Clone(); Dt=dt; Integrator=integrator??new Yoshida4(); Gravity=gravity??new GravityModel(); Ledger=new Ledger(State,Gravity.Softening);
        }
        public void Reset()
        {
            State=initial.Clone(); Ledger=new Ledger(State,Gravity.Softening);
            foreach(var f in Forces) if(f is EjectaDrive drive) drive.ResetBookkeeping();
        }
        public void Step()
        {
            // Symmetric external half-kicks, conservative step, external half-kicks.
            // Gravity remains order 4 for Yoshida; velocity-dependent/irreversible forcing is order 2.
            foreach(var f in Forces) f.Kick(State,Ledger,Dt/2);
            Integrator.Step(State,Gravity,Dt);
            foreach(var f in Forces) f.Kick(State,Ledger,Dt/2);
            Ledger.Check(State); foreach(var monitor in Monitors) monitor(State,Ledger); Logger?.Invoke(State,Ledger);
        }
        public void RunSteps(long count) { for(long i=0;i<count;i++) Step(); }
        public void WriteRun(string folder,string id,long steps,int sampleEvery=1000,string gitHash="unavailable",string benchmarkStatus="not read")
        {
            Directory.CreateDirectory(folder); string stem=Path.Combine(folder,"run_"+id);
            using(var file=new StreamWriter(stem+".csv",false,Encoding.UTF8))
            {
                file.WriteLine("time_s,energy_J,relative_energy_error,relative_momentum_error,relative_angular_error,a_m,e");
                for(long k=0;k<=steps;k++)
                {
                    if(k%sampleEvery==0 || k==steps)
                    {
                        int earth=State.Index("Earth"),sun=State.Index("Sun"); var r=State.Bodies[earth].Position-State.Bodies[sun].Position; var v=State.Bodies[earth].Velocity-State.Bodies[sun].Velocity;
                        double mu=Constants.G*(State.Bodies[earth].Mass+State.Bodies[sun].Mass);
                        file.WriteLine(string.Join(",",new[]{State.Time,OctoEarth.Core.Ledger.Energy(State,Gravity.Softening),Ledger.MaxEnergyError,Ledger.MaxMomentumError,Ledger.MaxAngularMomentumError,KeplerOrbit.SemiMajorAxis(r,v,mu),KeplerOrbit.EccentricityVector(r,v,mu).Magnitude}.SelectInvariant()));
                    }
                    if(k<steps) Step();
                }
            }
            File.WriteAllText(stem+".json","{\"schema\":1,\"dt\":"+Dt.ToString("R",CultureInfo.InvariantCulture)+",\"steps\":"+steps+",\"integrator\":\""+Integrator.Name+"\",\"forceSplittingOrder\":2,\"initialConditions\":\"approximate J2000\",\"gitHash\":\""+gitHash+"\",\"benchmarkStatus\":\""+benchmarkStatus+"\",\"ledger\":\""+Json.Escape(Ledger.Report())+"\"}");
        }
    }
    public static class Json
    {
        public static string Escape(string text) => text.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r","\\r").Replace("\n","\\n");
        public static IEnumerable<string> SelectInvariant(this IEnumerable<double> values) { foreach(double v in values) yield return v.ToString("R",CultureInfo.InvariantCulture); }
    }
}
