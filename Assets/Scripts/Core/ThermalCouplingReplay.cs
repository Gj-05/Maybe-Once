using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
namespace OctoEarth.Core
{
    public sealed class ThermalReplayReport
    {
        public string scope="Thermal-only replay of the exact hourly original-layout geometry and areas. Full dynamics are not re-integrated for this comparison.";
        public double legacyMinimum=288,legacyMaximum=288,absorptionFixMinimum=288,absorptionFixMaximum=288,correctedMinimum=288,correctedMaximum=288,maximumReproductionError;
        public ShadowEpoch correctedAtFullEarthShade;
    }
    public static class ThermalCouplingReplay
    {
        public static ThermalReplayReport Run(string dataPath,MissionConfiguration c,string csv,string folder)
        {
            Directory.CreateDirectory(folder); var initial=GravityTractor.Planetary(dataPath,c.Tractor()); var list=new List<Body>(initial.Bodies); list.Add(new Body("Array",1,new Vector3d(),new Vector3d())); list.Add(new Body("Mirror",1,new Vector3d(),new Vector3d())); var state=new SystemState(list.ToArray());
            var radiation=new MissionRadiation(state,c,1,1); var legacy=new EnergyBalanceClimate(288,100); var absorption=new EnergyBalanceClimate(288,100); var corrected=new EnergyBalanceClimate(288,100); var result=new ThermalReplayReport(); double previous=0; int row=0;
            using(var reader=File.OpenText(csv)) using(var writer=new StreamWriter(Path.Combine(folder,"thermal_coupling_replay.csv")))
            {
                var h=reader.ReadLine().Split(','); int time=Array.IndexOf(h,"time_s"),direct=Array.IndexOf(h,"direct_visible_W_m2"),reflected=Array.IndexOf(h,"reflected_visible_W_m2"),ir=Array.IndexOf(h,"array_IR_W_m2"),temperature=Array.IndexOf(h,"temperature_K"),array=Array.IndexOf(h,"array_area_m2"),mirror=Array.IndexOf(h,"mirror_area_m2");
                var offsets=new int[list.Count]; for(int k=0;k<offsets.Length;k++) offsets[k]=Array.IndexOf(h,list[k].Name+"_x");
                writer.WriteLine("time_yr,T_original_K,T_absorption_fix_K,T_absorption_and_quadrature_fix_K,direct_visible_W_m2,reflected_visible_W_m2,IR_original_W_m2,IR_corrected_W_m2,shade_earth,shade_mirror"); string line;
                while((line=reader.ReadLine())!=null)
                {
                    var f=line.Split(','); double Number(int k)=>double.Parse(f[k],CultureInfo.InvariantCulture);
                    double now=Number(time),dt=now-previous; previous=now; state.Time=now;
                    for(int k=0;k<list.Count;k++){int i=offsets[k]; list[k].Position=new Vector3d(Number(i),Number(i+1),Number(i+2)); list[k].Velocity=new Vector3d(Number(i+3),Number(i+4),Number(i+5));}
                    radiation.ArrayArea=Number(array); radiation.MirrorArea=Number(mirror); list[state.Index("Array")].Mass=radiation.ArrayArea*c.ArrayDensity; list[state.Index("Mirror")].Mass=radiation.MirrorArea*c.MirrorDensity; radiation.Evaluate(state);
                    if(dt>0) {legacy.Step(dt,Number(direct)+Number(reflected)+Number(ir)); absorption.StepRadiation(dt,Number(direct)+Number(reflected),Number(ir)); corrected.StepRadiation(dt,radiation.VisibleFlux,radiation.InfraredFlux);}
                    result.maximumReproductionError=Math.Max(result.maximumReproductionError,Math.Abs(absorption.Temperature-Number(temperature)));
                    result.legacyMinimum=Math.Min(result.legacyMinimum,legacy.Temperature); result.legacyMaximum=Math.Max(result.legacyMaximum,legacy.Temperature);
                    result.absorptionFixMinimum=Math.Min(result.absorptionFixMinimum,absorption.Temperature); result.absorptionFixMaximum=Math.Max(result.absorptionFixMaximum,absorption.Temperature);
                    result.correctedMinimum=Math.Min(result.correctedMinimum,corrected.Temperature); result.correctedMaximum=Math.Max(result.correctedMaximum,corrected.Temperature);
                    if(result.correctedAtFullEarthShade==null && radiation.ArrayShadeEarth==1) result.correctedAtFullEarthShade=new ShadowEpoch{seconds=now,temperature=corrected.Temperature,shadeEarth=radiation.ArrayShadeEarth,shadeMirror=radiation.ArrayShadeMirror,directVisible=radiation.DirectFlux,reflectedVisible=radiation.ReflectedFlux,infrared=radiation.InfraredFlux,totalFlux=radiation.EarthFlux};
                    if(row%24==0 || radiation.ArrayShadeEarth>0 || radiation.ArrayShadeMirror>0) writer.WriteLine(string.Join(",",new[]{now/Constants.JulianYear,legacy.Temperature,absorption.Temperature,corrected.Temperature,radiation.DirectFlux,radiation.ReflectedFlux,Number(ir),radiation.InfraredFlux,radiation.ArrayShadeEarth,radiation.ArrayShadeMirror}.SelectInvariant())); row++;
                }
            }
            MissionConfigurationIO.Write(Path.Combine(folder,"thermal_replay_report.json"),result); return result;
        }
    }
}
