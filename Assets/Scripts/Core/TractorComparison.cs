using System;
using System.IO;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public sealed class TractorRunMeasurement
    {
        public double Years,Dt,DeliveredDeltaV,A,E,MaxHoverError,EnergyError,MomentumError,AngularError,ImpulseError,ReactionError;
        public bool MoonBound=true;
        public string LedgerReport;
    }
    public static class TractorComparison
    {
        public static TractorRunMeasurement Run(string dataPath,bool prescribed,double years,double dt,string output=null,Action<string> progress=null)
        {
            var config=new TractorParameters(); var initial=GravityTractor.Planetary(dataPath,config);
            var runner=new SimulationRunner(initial,dt); var stepper=prescribed?new PrescribedTractorStepper(runner.State,config):null;
            var drive=prescribed?stepper.Drive:new TractorHoverDrive(runner.State,config); if(!prescribed) runner.Forces.Add(drive);
            var result=new TractorRunMeasurement{Years=years,Dt=dt}; var dv=new KahanSum();
            long count=(long)Math.Round(years*Constants.JulianYear/dt); int yearly=Math.Max(1,(int)Math.Round(Constants.JulianYear/dt));
            using(var file=output==null?null:new StreamWriter(output))
            {
                file?.WriteLine("time_s,delivered_prograde_dv_m_s,a_m,e,max_hover_error,max_relative_P,max_relative_L,max_relative_E,moon_bound");
                for(long k=0;k<count;k++)
                {
                    double before=DeliveredAcceleration(runner.State);
                    if(prescribed) stepper.Step(runner.State,runner.Ledger,dt); else runner.Step();
                    dv.Add((before+DeliveredAcceleration(runner.State))*dt/2);
                    var s=runner.State; var e=s.Bodies[s.Index("Earth")]; var t=s.Bodies[s.Index("Tractor")];
                    result.MaxHoverError=Math.Max(result.MaxHoverError,Math.Abs((t.Position-e.Position).Magnitude/config.Distance-1));
                    result.MoonBound &= InitialConditions.MoonBound(s,out _,out _);
                    if(k%yearly==0 || k==count-1)
                    {
                        var orbit=InitialConditions.EarthMoonBarycenter(s); double mu=Constants.G*(s.Bodies[s.Index("Sun")].Mass+e.Mass+s.Bodies[s.Index("Moon")].Mass);
                        double a=KeplerOrbit.SemiMajorAxis(orbit.Position,orbit.Velocity,mu); double ecc=KeplerOrbit.EccentricityVector(orbit.Position,orbit.Velocity,mu).Magnitude;
                        file?.WriteLine(string.Join(",",new[]{s.Time,dv.Value,a,ecc,result.MaxHoverError,runner.Ledger.MaxMomentumError,runner.Ledger.MaxAngularMomentumError,runner.Ledger.MaxEnergyError,result.MoonBound?1:0}.SelectInvariant()));
                        progress?.Invoke((prescribed?"Mode B":"Mode A")+" year="+(s.Time/Constants.JulianYear).ToString("F3",System.Globalization.CultureInfo.InvariantCulture));
                        result.A=a; result.E=ecc;
                    }
                }
            }
            result.DeliveredDeltaV=dv.Value; result.EnergyError=runner.Ledger.MaxEnergyError; result.MomentumError=runner.Ledger.MaxMomentumError; result.AngularError=runner.Ledger.MaxAngularMomentumError;
            result.ImpulseError=drive.MaxKickImpulseError; result.ReactionError=runner.Ledger.MaxReactionClosure; result.LedgerReport=runner.Ledger.Report(); return result;
        }
        public static double DeliveredAcceleration(SystemState s)
        {
            var t=s.Bodies[s.Index("Tractor")]; var e=s.Bodies[s.Index("Earth")]; var m=s.Bodies[s.Index("Moon")];
            var sum=new KahanVector(); foreach(var body in new[]{e,m}) { var d=t.Position-body.Position; sum.Add(d*(Constants.G*t.Mass*body.Mass/Math.Pow(d.Magnitude,3))); }
            return Vector3d.Dot(sum.Value,InitialConditions.EarthMoonBarycenter(s).Velocity.Normalized)/(e.Mass+m.Mass);
        }
        public static List<BenchmarkResult> G2(string dataPath,string folder,double dt=300,Action<string> progress=null)
        {
            Directory.CreateDirectory(folder);
            var a=Run(dataPath,false,10,dt,Path.Combine(folder,"g2_mode_a.csv"),progress);
            var b=Run(dataPath,true,10,dt,Path.Combine(folder,"g2_mode_b.csv"),progress);
            var rows=new List<BenchmarkResult>{
                new BenchmarkResult("G2.deltaV","10yr Mode A/B delivered delta-v agreement",Math.Abs(a.DeliveredDeltaV-b.DeliveredDeltaV)/Math.Abs(b.DeliveredDeltaV),0,0.01,"relative","A dv="+a.DeliveredDeltaV+" m/s; B dv="+b.DeliveredDeltaV+" m/s",dt),
                new BenchmarkResult("G2.aDifference","Final semi-major difference",Math.Abs(a.A-b.A),0,Constants.AU,"m","Diagnostic difference, not the acceptance criterion",dt),
                new BenchmarkResult("G2.eDifference","Final eccentricity difference",Math.Abs(a.E-b.E),0,1,"dimensionless","Diagnostic difference",dt)};
            Add(rows,"A",a); Add(rows,"B",b);
            BenchmarkSuite.Write(Path.Combine(folder,"g2_results.json"),rows);
            File.WriteAllText(Path.Combine(folder,"g2_ledger.txt"),"Mode A:\n"+a.LedgerReport+"\nMode B:\n"+b.LedgerReport);
            return rows;
        }
        static void Add(List<BenchmarkResult> rows,string mode,TractorRunMeasurement r)
        {
            rows.Add(new BenchmarkResult("G2."+mode+".P","Momentum ledger "+mode,r.MomentumError,0,1e-9,"relative",dt:r.Dt));
            rows.Add(new BenchmarkResult("G2."+mode+".L","Angular ledger "+mode,r.AngularError,0,1e-9,"relative",dt:r.Dt));
            rows.Add(new BenchmarkResult("G2."+mode+".E","Energy ledger "+mode,r.EnergyError,0,1e-9,"relative",dt:r.Dt));
            rows.Add(new BenchmarkResult("G2."+mode+".kick","Actual tractor impulse "+mode,r.ImpulseError,0,1e-9,"relative",dt:r.Dt));
            rows.Add(new BenchmarkResult("G2."+mode+".reaction","Reaction closure "+mode,r.ReactionError,0,1e-9,"relative",dt:r.Dt));
            rows.Add(new BenchmarkResult("G2."+mode+".Moon","Moon bound throughout "+mode,r.MoonBound?1:0,1,0,"boolean",dt:r.Dt));
        }
    }
}
