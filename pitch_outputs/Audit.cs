using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using OctoEarth.Core;
public static class PitchAudit {
 public static void Main(string[] args) {
  CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;
  string root=args[0], output=Path.Combine(root,"pitch_outputs");
  var c=MissionConfigurationIO.Read<MissionConfiguration>(Path.Combine(root,"Config/mission_design_candidate.json"));
  var initial=new MissionSimulation(Path.Combine(root,"Assets/Data/solar_j2000.csv"),c,new MissionThresholds()).DiagnosticState;
  var budget=new MissionBudget(Constants.AU,2.7*Constants.AU,Constants.PairMass,10000*Constants.JulianYear);
  var facts=new Dictionary<string,double>{{"budgetDeltaV_km_s",budget.DeltaV/1000},{"equilibrium1AU_K",Insolation.EquilibriumTemperature(Constants.AU)},{"equilibriumTargetWithoutMirror_K",Insolation.EquilibriumTemperature(2.7*Constants.AU)}};
  MissionConfigurationIO.Write(Path.Combine(output,"computed_code_facts.json"),facts);
  foreach(var item in new[]{new[]{"selected_h3600","Results/design_audit/full_h3600/run_mission.csv"},new[]{"selected_h1800","Results/design_audit/full_h1800/run_mission.csv"},new[]{"original_full","Results/mission_h3600_y10000/run_mission.csv"},new[]{"fresh_1yr","pitch_outputs/selected_diagnostic_1yr/run_mission.csv"}}) {
   string file=Path.Combine(root,item[1]);
   var stats=new Dictionary<string,double>(); double minMargin=double.PositiveInfinity; int count=0;
   using(var reader=File.OpenText(file)) using(var writer=new StreamWriter(Path.Combine(output,item[0]+"_charts.csv"))) {
    var h=reader.ReadLine().Split(','); string line;
    writer.WriteLine("time_yr,a_AU,earth_sun_distance_AU,temperature_K,thrust_N,ejected_kg,array_area_m2,relative_P,relative_L,relative_E,moon_distance_m,binding_margin_J_kg");
    while((line=reader.ReadLine())!=null) {
     if(string.IsNullOrWhiteSpace(line))continue; var f=line.Split(',');
     double V(string name)=>double.Parse(f[Array.IndexOf(h,name)],CultureInfo.InvariantCulture);
     Vector3d P(string name)=>new Vector3d(V(name+"_x"),V(name+"_y"),V(name+"_z"));
     Vector3d W(string name)=>new Vector3d(V(name+"_vx"),V(name+"_vy"),V(name+"_vz"));
     double distance=(P("Moon")-P("Earth")).Magnitude;
     double binding=(W("Moon")-W("Earth")).SqrMagnitude/2-Constants.G*initial.Bodies[initial.Index("Earth")].Mass/distance;
     minMargin=Math.Min(minMargin,-binding);
     foreach(string key in new[]{"temperature_K","tide_ratio","effective_flux_W_m2","shading_fraction","e","array_area_m2","ejected_kg"}) {
      double v=V(key); if(!stats.ContainsKey("min_"+key)) {stats["min_"+key]=v;stats["max_"+key]=v;}
      stats["min_"+key]=Math.Min(stats["min_"+key],v);stats["max_"+key]=Math.Max(stats["max_"+key],v);
     }
     stats["final_year"]=V("time_s")/Constants.JulianYear;stats["final_a_AU"]=V("a_m")/Constants.AU;stats["final_e"]=V("e");stats["final_moon_distance_m"]=distance;
     stats["final_array_area_m2"]=V("array_area_m2");stats["final_ejected_kg"]=V("ejected_kg");if(Array.IndexOf(h,"tractor_mass_kg")>=0)stats["initial_tractor_mass_kg"]=V("tractor_mass_kg");
     writer.WriteLine(string.Join(",",new[]{V("time_s")/Constants.JulianYear,V("a_m")/Constants.AU,(P("Earth")-P("Sun")).Magnitude/Constants.AU,V("temperature_K"),V("thrust_N"),V("ejected_kg"),V("array_area_m2"),V("relative_P"),V("relative_L"),V("relative_E"),distance,-binding}.SelectInvariant())); count++;
    }
   }
   stats["sample_count"]=count;stats["minimum_sampled_binding_margin_J_kg"]=minMargin;
   MissionConfigurationIO.Write(Path.Combine(output,item[0]+"_sampled_stats.json"),stats);
   Console.WriteLine(item[0]+": "+count+" samples exported");
  }
 }
}
