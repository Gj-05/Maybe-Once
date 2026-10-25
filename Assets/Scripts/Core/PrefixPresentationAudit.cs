#if !UNITY_5_3_OR_NEWER
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
namespace OctoEarth.Core
{
    // Read-only diagnostics. No trajectory integration and no changes to mission inputs.
    public static class PrefixPresentationAudit
    {
        public static void Run(string root)
        {
            var reports=new List<object>();
            foreach(int h in new[]{3600,1800})
            {
                string source=$"Results/design_audit/full_h{h}/run_mission.csv";
                var climate=new EnergyBalanceClimate(288,100); double previous=0,firstExit=-1,minT=288,maxT=288;
                double min=double.PositiveInfinity,max=0,sum=0,final=0,worstHill=0,maxEnergy=-1e300,mass=0,material=0,ejected=0; int count=0; bool bound=true; string[] header=null;
                using(var reader=File.OpenText(Path.Combine(root,source)))
                using(var writer=new StreamWriter(Path.Combine(root,$"Results/no_mirror_replay_h{h}.csv")))
                {
                    header=reader.ReadLine().Split(',');
                    int Index(string name)=>Array.IndexOf(header,name);
                    writer.WriteLine("time_yr,temperature_K,ice_fraction,direct_visible_W_m2,array_IR_W_m2");
                    string line; while((line=reader.ReadLine())!=null)
                    {
                        var f=line.Split(','); if(f.Length!=header.Length) continue;
                        double N(string name)=>double.Parse(f[Index(name)],CultureInfo.InvariantCulture);
                        Vector3d V(string name)=>new Vector3d(N(name+"_x"),N(name+"_y"),N(name+"_z"));
                        Vector3d Vel(string name)=>new Vector3d(N(name+"_vx"),N(name+"_vy"),N(name+"_vz"));
                        double now=N("time_s"),direct=N("direct_visible_W_m2"),ir=N("array_IR_W_m2");
                        // Monthly held forcing, daily RK4 as in the existing climate model.
                        double left=now-previous;
                        while(left>0) {double dt=Math.Min(86400,left); climate.StepRadiation(dt,direct,ir); left-=dt;
                            if(firstExit<0 && (climate.Temperature<278 || climate.Temperature>298)) firstExit=now-left;}
                        previous=now; minT=Math.Min(minT,climate.Temperature); maxT=Math.Max(maxT,climate.Temperature);
                        writer.WriteLine(string.Join(",",new[]{now/Constants.JulianYear,climate.Temperature,climate.IceFraction,direct,ir}.SelectInvariant()));
                        var r=V("Moon")-V("Earth"); var v=Vel("Moon")-Vel("Earth"); double d=r.Magnitude;
                        double hill=N("a_m")*Math.Pow((Constants.EarthMass+Constants.MoonMass)/(3*Constants.SolarMass),1.0/3);
                        maxEnergy=Math.Max(maxEnergy,v.SqrMagnitude/2-Constants.G*Constants.EarthMass/d); worstHill=Math.Max(worstHill,d/hill);
                        min=Math.Min(min,d); max=Math.Max(max,d); sum+=d; final=d; count++; bound &=N("moon_bound")==1;
                        mass=N("tractor_mass_kg"); material=N("material_kg"); ejected=N("ejected_kg");
                    }
                }
                reports.Add(new {step_s=h,source,columns=header,samples=count,minimum_distance_m=min,mean_sample_distance_m=sum/count,maximum_distance_m=max,final_distance_m=final,cumulative_bound=bound,moon_flag_observed=!bound,worst_sampled_hill_ratio=worstHill,maximum_sampled_specific_energy_J_kg=maxEnergy,tractor_mass_kg=mass,tractor_radius_m=Math.Pow(3*mass/(4*Math.PI*3000),1.0/3),rock_density_kg_m3=3000,material_kg=material,material_cap_fraction=material/3e23,ejected_kg=ejected,jet_energy_J=ejected*300000*300000/2,
                    solar_jet_energy_fraction="1 by solver bookkeeping: every ejection is debited against actual array electrical supply; exact cumulative solar totals unavailable in interrupted CSV",
                    replay=new {minimum_K=minT,maximum_K=maxT,final_K=climate.Temperature,final_ice_fraction=climate.IceFraction,first_placeholder_exit_yr=firstExit/Constants.JulianYear},
                    note="Monthly sampled distances, not all-step extrema; mean is arithmetic sample mean. No-mirror replay holds recorded direct and array IR forcing across each output interval; removes reflected light only. Conditional thermal replay, not a coupled no-mirror mission; daily crossing resolution; no mirror removal changes to dynamics or waste heat."});
            }
            File.WriteAllText(Path.Combine(root,"Results/moon_report.json"),System.Text.Json.JsonSerializer.Serialize(reports,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
            using(var w=new StreamWriter(Path.Combine(root,"Results/mirror_optics_sensitivity.csv")))
            {
                w.WriteLine("d_m,theta_sun_rad,delta_max_rad,delta_rad,intercept_fraction,scope");
                foreach(double d in new[]{1e8,5e8,1e9,2e9,5e9}) foreach(double delta in new[]{0,0.001,0.003,0.01,0.02})
                {double theta=2*Math.Asin(MissionGeometry.SolarRadius/Constants.AU),q=Math.Pow(2*Constants.EarthRadius/d,2)-theta*theta;
                    w.WriteLine(string.Join(",",new[]{d,theta,q>=0?Math.Sqrt(q):double.NaN,delta,Math.Min(1,Math.Pow(2*Constants.EarthRadius/(d*Math.Sqrt(theta*theta+delta*delta)),2))}.SelectInvariant())+",1 AU source-distance optical sensitivity only");}
            }
        }
    }
}
#endif
