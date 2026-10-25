using System;
using System.IO;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public static class DesignClimateChecks
    {
        public static List<BenchmarkResult> Run(string folder=null)
        {
            var rows=new List<BenchmarkResult>(); var c=new MissionConfiguration{EnableMirror=false};
            foreach(double height in new[]{1e7,1e8,1e9,1e10})
            {
                double radius=1e9,a=Math.PI*radius*radius,heat=1e20,hypot=Math.Sqrt(height*height+radius*radius);
                double exact=heat/a*radius*radius/(hypot*(hypot+height));
                double measured=MissionRadiation.ExtendedDiscInfrared(new Vector3d(0,0,height),new Vector3d(),new Vector3d(0,0,1),a,heat,out _);
                double old=MissionRadiation.LegacyDiscInfrared(new Vector3d(0,0,height),new Vector3d(),new Vector3d(0,0,1),a,heat,out _);
                rows.Add(new BenchmarkResult("IR.onAxis."+height,"Finite disc analytic on-axis irradiance",measured/exact-1,0,1e-12,"relative","old midpoint relative error="+(old/exact-1)));
            }
            var s=new SystemState(new Body("Sun",Constants.SolarMu/Constants.G,new Vector3d(),new Vector3d()),new Body("Earth",Constants.EarthMu/Constants.G,new Vector3d(Constants.AU,0,0),new Vector3d()),new Body("Array",1,new Vector3d(Constants.AU/2,0,0),new Vector3d()),new Body("Mirror",1,new Vector3d(Constants.AU,1e9,0),new Vector3d()));
            double area=Math.PI*1e18; var radiation=new MissionRadiation(s,c,area,0); radiation.Evaluate(s);
            rows.Add(new BenchmarkResult("Climate.fullShade","Earth-view disc completely obscured",radiation.ArrayShadeEarth,1,0,"fraction"));
            rows.Add(new BenchmarkResult("Climate.direct","Full shading removes direct visible flux",radiation.DirectFlux,0,0,"W/m2"));
            rows.Add(new BenchmarkResult("Climate.visible","Disabled mirror cannot replace shaded sunlight",radiation.VisibleFlux,0,0,"W/m2"));
            rows.Add(new BenchmarkResult("Climate.IR","Array waste heat remains included",radiation.InfraredFlux>0?1:0,1,0,"boolean"));
            var legacy=new EnergyBalanceClimate(288,100); var coupled=new EnergyBalanceClimate(288,100); var unshaded=new EnergyBalanceClimate(288,100);
            StreamWriter output=null; if(folder!=null) { Directory.CreateDirectory(folder); output=new StreamWriter(Path.Combine(folder,"full_shading_temperature.csv")); output.WriteLine("day,T_before_IR_fix_K,T_after_IR_fix_K,T_unshaded_K,direct_visible_W_m2,reflected_visible_W_m2,array_IR_W_m2"); }
            for(int day=0;day<=3650;day++)
            {
                if(day==0 || day==1 || day==7 || day==30 || day==365 || day==1825 || day==3650) output?.WriteLine(string.Join(",",new[]{(double)day,legacy.Temperature,coupled.Temperature,unshaded.Temperature,radiation.DirectFlux,radiation.ReflectedFlux,radiation.InfraredFlux}.SelectInvariant()));
                if(day==365) rows.Add(new BenchmarkResult("Climate.cool","One year full shade cools below initial temperature",coupled.Temperature<288?1:0,1,0,"boolean","actual K="+coupled.Temperature));
                if(day==3650) break;
                legacy.Step(Units.Day,radiation.EarthFlux); coupled.StepRadiation(Units.Day,radiation.VisibleFlux,radiation.InfraredFlux); unshaded.StepRadiation(Units.Day,Insolation.Flux(Constants.AU),radiation.InfraredFlux);
            }
            output?.Dispose();
            // An independently isolated heat-input check: IR absorption has no visible Bond-albedo factor.
            var visibleOnly=new EnergyBalanceClimate(288,100); var infraredOnly=new EnergyBalanceClimate(288,100);
            visibleOnly.StepRadiation(1,100,0); infraredOnly.StepRadiation(1,0,100);
            rows.Add(new BenchmarkResult("Climate.IRabsorption","Equal IR irradiance heats more than reflected visible flux",infraredOnly.Temperature>visibleOnly.Temperature?1:0,1,0,"boolean"));
            if(folder!=null) BenchmarkSuite.Write(Path.Combine(folder,"climate_checks.json"),rows); return rows;
        }
    }
}
