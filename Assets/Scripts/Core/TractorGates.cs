using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public sealed class HoverMeasurement
    {
        public double Dt,Seconds,Mass,MaxDistanceError,MaxAccelerationError,EnergyError,MomentumError,AngularError,ImpulseError,ReactionError,Propellant;
        public Vector3d FinalEarthVelocity;
        public string LedgerReport;
    }
    public static class TractorGates
    {
        public static HoverMeasurement ToyHover(double dt,double days=30,string csv=null)
        {
            var config=new TractorParameters(); var initial=GravityTractor.Toy(config);
            var runner=new SimulationRunner(initial,dt); var drive=new TractorHoverDrive(runner.State,config); runner.Forces.Add(drive);
            int e=runner.State.Index("Earth"),sol=runner.State.Index("Sun"),t=runner.State.Index("Tractor");
            var result=new HoverMeasurement{Dt=dt,Mass=initial.Bodies[t].Mass};
            StreamWriter file=csv==null?null:new StreamWriter(csv);
            file?.WriteLine("time_s,separation_m,relative_distance_error,relative_acceleration_error,thrust_N,ejected_kg,max_relative_P,max_relative_L,max_relative_E");
            try
            {
                long steps=(long)Math.Round(days*Units.Day/dt);
                for(long k=0;k<steps;k++)
                {
                    var before=runner.State.Bodies[e].Velocity;
                    var solarBefore=SolarAcceleration(runner.State,e,sol);
                    var relativeBefore=runner.State.Bodies[t].Position-runner.State.Bodies[e].Position;
                    runner.Step();
                    var measured=(runner.State.Bodies[e].Velocity-before)/dt-(solarBefore+SolarAcceleration(runner.State,e,sol))/2;
                    var relative=runner.State.Bodies[t].Position-runner.State.Bodies[e].Position;
                    var midpoint=(relativeBefore+relative)/2;
                    var analytic=midpoint*(Constants.G*runner.State.Bodies[t].Mass/Math.Pow(midpoint.Magnitude,3));
                    result.MaxDistanceError=Math.Max(result.MaxDistanceError,Math.Abs(relative.Magnitude/config.Distance-1));
                    result.MaxAccelerationError=Math.Max(result.MaxAccelerationError,(measured-analytic).Magnitude/analytic.Magnitude);
                    if(k%Math.Max(1,(int)(Units.Day/dt))==0 || k==steps-1) file?.WriteLine(string.Join(",",new[]{runner.State.Time,relative.Magnitude,result.MaxDistanceError,result.MaxAccelerationError,drive.ThrustVector.Magnitude,drive.Propellant,runner.Ledger.MaxMomentumError,runner.Ledger.MaxAngularMomentumError,runner.Ledger.MaxEnergyError}.SelectInvariant()));
                }
            }
            finally { file?.Dispose(); }
            result.Seconds=runner.State.Time; result.FinalEarthVelocity=runner.State.Bodies[e].Velocity;
            result.EnergyError=runner.Ledger.MaxEnergyError; result.MomentumError=runner.Ledger.MaxMomentumError; result.AngularError=runner.Ledger.MaxAngularMomentumError;
            result.ImpulseError=Math.Max(drive.MaxKickImpulseError,runner.Ledger.MaxForceClosure); result.ReactionError=runner.Ledger.MaxReactionClosure;
            result.Propellant=drive.Propellant; result.LedgerReport=runner.Ledger.Report(); return result;
        }
        static Vector3d SolarAcceleration(SystemState s,int e,int sun)
        { var d=s.Bodies[sun].Position-s.Bodies[e].Position; return d*(Constants.G*s.Bodies[sun].Mass/Math.Pow(d.Magnitude,3)); }
        public static List<BenchmarkResult> G1(string resultsFolder=null)
        {
            if(resultsFolder!=null) Directory.CreateDirectory(resultsFolder);
            var coarse=ToyHover(60,30,resultsFolder==null?null:Path.Combine(resultsFolder,"g1_hover_h60.csv"));
            var fine=ToyHover(30,30,resultsFolder==null?null:Path.Combine(resultsFolder,"g1_hover_h30.csv"));
            var rows=new List<BenchmarkResult>(); Add(rows,coarse); Add(rows,fine);
            rows.Add(new BenchmarkResult("G1.halfstep","h versus h/2 final Earth velocity difference",(coarse.FinalEarthVelocity-fine.FinalEarthVelocity).Magnitude,0,1e-3,"m/s","Diagnostic convergence bound, not an altered G1 threshold"));
            if(resultsFolder!=null)
            {
                BenchmarkSuite.Write(Path.Combine(resultsFolder,"g1_results.json"),rows);
                File.WriteAllText(Path.Combine(resultsFolder,"g1_ledger.txt"),coarse.LedgerReport+"\nFine-step:\n"+fine.LedgerReport);
                File.WriteAllText(Path.Combine(resultsFolder,"g1_config.json"),"{\"schema\":1,\"gate\":\"G1\",\"days\":30,\"dt\":[60,30],\"distance_m\":50000000,\"tractorMass_kg\":"+coarse.Mass.ToString("R",CultureInfo.InvariantCulture)+",\"bodies\":[\"Sun\",\"Earth\",\"Tractor\"],\"exhaust_m_s\":300000,\"cant_degrees\":25,\"energySource\":\"explicit toy jet energy input; solar power not yet modeled\",\"integrator\":\"Yoshida4 gravity, positive external half-kicks (forced order 2)\"}");
            }
            return rows;
        }
        static void Add(List<BenchmarkResult> rows,HoverMeasurement h)
        {
            string id="G1.h"+h.Dt.ToString("0",CultureInfo.InvariantCulture);
            rows.Add(new BenchmarkResult(id+".duration","Thirty-day hover duration",h.Seconds,30*Units.Day,0,"s",dt:h.Dt));
            rows.Add(new BenchmarkResult(id+".distance","Max hover separation error",h.MaxDistanceError,0,1e-3,"relative d",dt:h.Dt));
            rows.Add(new BenchmarkResult(id+".acceleration","Measured Earth acceleration vs G mt/r2",h.MaxAccelerationError,0,1e-3,"relative","Finite velocity change minus averaged solar acceleration; vector comparison",h.Dt));
            rows.Add(new BenchmarkResult(id+".P","Global body-plus-source momentum",h.MomentumError,0,1e-9,"initial gross P",dt:h.Dt));
            rows.Add(new BenchmarkResult(id+".L","Global angular momentum",h.AngularError,0,1e-9,"initial gross L",dt:h.Dt));
            rows.Add(new BenchmarkResult(id+".E","Global energy including jet input",h.EnergyError,0,1e-9,"relative",dt:h.Dt));
            rows.Add(new BenchmarkResult(id+".impulse","Measured tractor kick vs requested impulse",h.ImpulseError,0,1e-9,"relative impulse",dt:h.Dt));
            rows.Add(new BenchmarkResult(id+".reaction","Ejecta plus supplier reaction",h.ReactionError,0,1e-9,"relative impulse",dt:h.Dt));
        }
    }
}
