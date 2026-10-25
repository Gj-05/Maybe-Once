using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using OctoEarth.Core;
public static class Program
{
    public static int Main(string[] args)
    {
        CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;
        try
        {
            var tokens=new List<string>(); foreach(var a in args) if(a!="--") tokens.Add(a);
            string command=tokens.Count==0?"bench":tokens[0]; string root=Directory.GetCurrentDirectory();
            if(command=="prefix-presentation-audit") {PrefixPresentationAudit.Run(root); return 0;}
            string path=Path.Combine(root,"Assets/Data/solar_j2000.csv"),results=Path.Combine(root,"Results"); Directory.CreateDirectory(results);
            if(tokens.Contains("--data")) path=tokens[tokens.IndexOf("--data")+1];
            if(command=="bench")
            {
                var list=BenchmarkSuite.Run(path); list.AddRange(ValidationSuite.Run(path)); BenchmarkSuite.Write(Path.Combine(results,"benchmarks.json"),list);
                var toy=new SystemState(new Body("Sun",1000,new Vector3d(-10,0,0),new Vector3d()),new Body("Earth",100,new Vector3d(1,2,0),new Vector3d(4,5,0)));
                var ledger=new Ledger(toy); var drive=new EjectaDrive(1,10,100,new Vector3d(1,0,0)); drive.Kick(toy,ledger,1); ledger.Check(toy);
                File.WriteAllText(Path.Combine(results,"ejecta_ledger.txt"),ledger.Report()); Console.WriteLine(ledger.Report());
                int failures=0; foreach(var row in list) { Console.WriteLine(row); if(!row.passed) failures++; }
                Console.WriteLine("Failed rows: "+failures+". No mission modeled."); return failures==0?0:1;
            }
            if(command=="baseline")
            {
                double years=tokens.Contains("--years")?double.Parse(tokens[tokens.IndexOf("--years")+1],CultureInfo.InvariantCulture):10000;
                double dt=tokens.Contains("--dt")?double.Parse(tokens[tokens.IndexOf("--dt")+1],CultureInfo.InvariantCulture):3600;
                if(!(years>0) || !(dt>0)) throw new ArgumentOutOfRangeException();
                Console.WriteLine("Sun-Earth-Moon no-push baseline: years="+years+", dt="+dt+", approximate J2000. Running...");
                var rows=ValidationSuite.Slow(path,years,dt,results); BenchmarkSuite.Write(Path.Combine(results,"baseline_validation.json"),rows); int fail=0; foreach(var row in rows) { Console.WriteLine(row); if(!row.passed) fail++; } return fail==0?0:1;
            }
            if(command=="converge")
            {
                var rows=ValidationSuite.Convergence(new Yoshida4(),4,0.2); rows.AddRange(ValidationSuite.Convergence(new VelocityVerlet(),2,0.12));
                BenchmarkSuite.Write(Path.Combine(results,"convergence.json"),rows); foreach(var row in rows) Console.WriteLine(row);
                using(var f=new StreamWriter(Path.Combine(results,"convergence.csv"))) { f.WriteLine("id,dt_s,error_AU"); foreach(var row in rows) if(row.dt>0) f.WriteLine(row.id+","+row.dt.ToString("R")+","+row.measured.ToString("R")); }
                return rows.Exists(r=>!r.passed)?1:0;
            }
            if(command=="g1")
            {
                var rows=TractorGates.G1(results); foreach(var row in rows) Console.WriteLine(row);
                return rows.Exists(r=>!r.passed)?1:0;
            }
            if(command=="g2")
            {
                double dt=tokens.Contains("--dt")?double.Parse(tokens[tokens.IndexOf("--dt")+1],CultureInfo.InvariantCulture):300;
                var rows=TractorComparison.G2(path,Path.Combine(results,"g2_h"+dt.ToString("0")),dt,Console.WriteLine); foreach(var row in rows) Console.WriteLine(row);
                return rows.Exists(r=>!r.passed)?1:0;
            }
            if(command=="g3")
            {
                double dt=tokens.Contains("--dt")?double.Parse(tokens[tokens.IndexOf("--dt")+1],CultureInfo.InvariantCulture):3600;
                var rows=SpiralGate.Run(dt,Path.Combine(results,"g3_h"+dt.ToString("0")),Console.WriteLine); foreach(var row in rows) Console.WriteLine(row);
                return rows.Exists(r=>!r.passed)?1:0;
            }
            if(command=="mission" || command=="mission-design-comparison")
            {
                string configPath=tokens.Contains("--config")?tokens[tokens.IndexOf("--config")+1]:Path.Combine(root,"Config/mission.json");
                var c=MissionConfigurationIO.LoadOrCreate(configPath);
                if(tokens.Contains("--dt")) c.Dt=double.Parse(tokens[tokens.IndexOf("--dt")+1],CultureInfo.InvariantCulture);
                if(tokens.Contains("--years")) c.Years=double.Parse(tokens[tokens.IndexOf("--years")+1],CultureInfo.InvariantCulture);
                string thresholdsFile=Path.Combine(root,"Config/thresholds.json");
                if(!File.Exists(thresholdsFile)) MissionConfigurationIO.Write(thresholdsFile,new MissionThresholds());
                var thresholds=MissionConfigurationIO.Read<MissionThresholds>(thresholdsFile);
                var bench=MissionConfigurationIO.Read<RecordedBenchmarks>(Path.Combine(results,"benchmarks.json"));
                bool passed=bench.results!=null && bench.results.Length>0; if(bench.results!=null) foreach(var r in bench.results) passed &= r.passed;
                string output=Path.Combine(results,"mission_h"+c.Dt.ToString("0")+"_y"+c.Years.ToString("0.###"));
                if(tokens.Contains("--output")) output=tokens[tokens.IndexOf("--output")+1];
                var model=new MissionSimulation(path,c,thresholds);
                var summary=model.Run(output,passed,Console.WriteLine);
                if(command=="mission-design-comparison" && model.TargetCheckpoint!=null)
                {
                    foreach(string policy in new[]{"Coast","Parking"})
                    {
                        Console.WriteLine("Starting identical-target-state branch "+policy);
                        var branch=model.TargetCheckpoint.Fork(policy); branch.Run(Path.Combine(output,"post_target_"+policy.ToLowerInvariant()),passed,Console.WriteLine);
                    }
                }
                Console.WriteLine(summary.status+"; elapsed yr="+summary.seconds/Constants.JulianYear+"; target yr="+(summary.targetTimeSeconds<0 ? "not reached" : (summary.targetTimeSeconds/Constants.JulianYear).ToString("R")));
                foreach(var f in summary.flags) Console.WriteLine(f.code+" at "+f.firstTimeSeconds+"s: "+f.cause);
                return summary.completed?0:1;
            }
            if(command=="audit-mission")
            {
                var config=MissionConfigurationIO.Read<MissionConfiguration>(Path.Combine(root,"Config/mission.json"));
                var limits=MissionConfigurationIO.Read<MissionThresholds>(Path.Combine(root,"Config/thresholds.json"));
                MissionConfigurationIO.Write(Path.Combine(results,"diagnosis_initial_audit.json"),MissionDiagnosticAudit.Initial(path,config,limits));
                return 0;
            }
            if(command=="design-climate")
            {
                var rows=DesignClimateChecks.Run(Path.Combine(results,"design_audit/climate")); foreach(var row in rows) Console.WriteLine(row); return rows.Exists(r=>!r.passed)?1:0;
            }
            if(command=="design-audit")
            {
                string folder=Path.Combine(results,"design_audit");
                MissionConfigurationIO.Write(Path.Combine(folder,"thermal_original_layout/shading_audit.json"),DesignStudyAudit.ClimateTrace(Path.Combine(folder,"thermal_original_layout/run_mission.csv")));
                var old=MissionConfigurationIO.Read<MissionConfiguration>(Path.Combine(folder,"original_mission.json")); var original=GravityTractor.Planetary(path,old.Tractor());
                MissionConfigurationIO.Write(Path.Combine(folder,"structure_options_original.json"),DesignStudyAudit.Structures(original.Bodies[original.Index("Tractor")].Mass,2.5495226012612157e17,old.RockDensity,old.RockAllowableStress)); return 0;
            }
            if(command=="siting-sweep")
            {
                ArraySitingSweep.Run(path,Path.Combine(results,"mission_h3600_y10000/run_mission.csv"),Path.Combine(results,"design_audit/siting"),Console.WriteLine); return 0;
            }
            if(command=="mission-order")
            {
                var config=MissionConfigurationIO.Read<MissionConfiguration>(Path.Combine(root,"Config/mission_design_candidate.json")); var limits=MissionConfigurationIO.Read<MissionThresholds>(Path.Combine(root,"Config/thresholds.json"));
                var r=MissionConvergenceStudy.Run(path,config,limits,Path.Combine(results,"design_audit/order"),Console.WriteLine); Console.WriteLine("Observed 10yr mission order: position="+r.logLogPositionOrder+"; a="+r.logLogSemiMajorOrder); return 0;
            }
            if(command=="thermal-replay")
            {
                var config=MissionConfigurationIO.Read<MissionConfiguration>(Path.Combine(results,"design_audit/original_mission.json"));
                var r=ThermalCouplingReplay.Run(path,config,Path.Combine(results,"design_audit/thermal_original_layout/run_mission.csv"),Path.Combine(results,"design_audit/climate")); Console.WriteLine("Thermal peaks: legacy="+r.legacyMaximum+"; fixed="+r.correctedMaximum+"; reproduction error="+r.maximumReproductionError); return 0;
            }
            if(command=="coast-refinement")
            {
                var config=MissionConfigurationIO.Read<MissionConfiguration>(Path.Combine(root,"Config/mission_design_candidate.json")); config.Dt=60; config.PostTargetPolicy="Coast"; config.RecordEveryStep=true;
                var limits=MissionConfigurationIO.Read<MissionThresholds>(Path.Combine(root,"Config/thresholds.json"));
                var model=MissionSimulation.RestartRecordedTarget(path,config,limits,Path.Combine(results,"design_audit/full_h3600/post_target_coast/run_mission.csv"));
                var r=model.Run(Path.Combine(results,"design_audit/coast_refined_h60"),true,Console.WriteLine); Console.WriteLine("Coast completed="+r.completed+"; collision stop at yr="+r.seconds/Constants.JulianYear); return 0;
            }
            Console.Error.WriteLine("Commands: bench | baseline --years 10000 [--dt 3600] | converge | g1"); return 2;
        }
        catch(Exception e) { Console.Error.WriteLine(e); return 2; }
    }
}
