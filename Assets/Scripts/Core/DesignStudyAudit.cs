using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
namespace OctoEarth.Core
{
    public sealed class ShadowEpoch
    {
        public double seconds,temperature,shadeEarth,shadeMirror,directVisible,reflectedVisible,infrared,totalFlux;
    }
    public sealed class ShadowAudit
    {
        public ShadowEpoch earthMaximum,mirrorMaximum,minimumTotalFlux;
        public double fullEarthShadowHours,fullMirrorShadowHours,minimumTemperature,maximumTemperature;
        public long records;
    }
    public sealed class StructureOption
    {
        public string mount,model,governingMode;
        public double contactFraction,radius,contactArea,force,axialStress,selfGravityPressure,compressiveStress,tensileStress,shearStress,bendingStress,safetyFactor,strengthRequiredForSF1_5,allowableStrength;
        public bool passes;
    }
    public static class DesignStudyAudit
    {
        static double V(string[] f,int k)=>double.Parse(f[k],CultureInfo.InvariantCulture);
        public static ShadowAudit ClimateTrace(string path)
        {
            var result=new ShadowAudit{minimumTemperature=double.PositiveInfinity}; double previous=0;
            using(var reader=File.OpenText(path))
            {
                var h=reader.ReadLine().Split(','); int time=Array.IndexOf(h,"time_s"),temp=Array.IndexOf(h,"temperature_K"),se=Array.IndexOf(h,"shade_earth"),sm=Array.IndexOf(h,"shade_mirror"),direct=Array.IndexOf(h,"direct_visible_W_m2"),reflected=Array.IndexOf(h,"reflected_visible_W_m2"),ir=Array.IndexOf(h,"array_IR_W_m2"),flux=Array.IndexOf(h,"effective_flux_W_m2"); string line;
                while((line=reader.ReadLine())!=null)
                {
                    var f=line.Split(','); var e=new ShadowEpoch{seconds=V(f,time),temperature=V(f,temp),shadeEarth=V(f,se),shadeMirror=V(f,sm),directVisible=V(f,direct),reflectedVisible=V(f,reflected),infrared=V(f,ir),totalFlux=V(f,flux)};
                    double dt=e.seconds-previous; previous=e.seconds; result.records++;
                    if(e.shadeEarth==1) result.fullEarthShadowHours+=dt/3600;
                    if(e.shadeMirror==1) result.fullMirrorShadowHours+=dt/3600;
                    if(result.earthMaximum==null || e.shadeEarth>result.earthMaximum.shadeEarth) result.earthMaximum=e;
                    if(result.mirrorMaximum==null || e.shadeMirror>result.mirrorMaximum.shadeMirror) result.mirrorMaximum=e;
                    if(result.minimumTotalFlux==null || e.totalFlux<result.minimumTotalFlux.totalFlux) result.minimumTotalFlux=e;
                    result.minimumTemperature=Math.Min(result.minimumTemperature,e.temperature); result.maximumTemperature=Math.Max(result.maximumTemperature,e.temperature);
                }
            }
            return result;
        }
        public static List<StructureOption> Structures(double mass,double force,double density,double allowable)
        {
            double r=Math.Pow(3*mass/(4*Math.PI*density),1.0/3),p=2*Math.PI/3*Constants.G*density*density*r*r;
            var list=new List<StructureOption>();
            double old=density*(force/mass*2*r+Constants.G*mass/r);
            list.Add(new StructureOption{model="Original bulk heuristic, unclassified axial/inertial plus self-gravity",mount="unspecified",governingMode="bulk combined scalar",radius=r,force=force,axialStress=old,allowableStrength=allowable,safetyFactor=allowable/old,strengthRequiredForSF1_5=1.5*old,passes=allowable/old>=1.5});
            foreach(bool earthFacing in new[]{false,true}) foreach(double fraction in new[]{0.01,0.1,0.5,1.0}) foreach(double offset in new[]{0.0,0.25})
            {
                double area=Math.PI*r*r*fraction,axial=force/area,shear=force*Math.Tan(Units.Radians(25))/area,bend=force*(offset*r)*r/(Math.PI*Math.Pow(r,4)/4);
                double compression=(earthFacing?axial:0)+p+bend,tension=(earthFacing?0:axial)+bend;
                double peak=Math.Max(compression,Math.Max(tension,shear)); string mode=peak==compression?"compression":peak==tension?"tension":"shear";
                list.Add(new StructureOption{model="Uniform-sphere hydrostatic pressure plus distributed contact loads; not FEA",mount=(earthFacing?"Earth-facing compression":"Far-side tension")+"; offset/R="+offset,governingMode=mode,contactFraction=fraction,radius=r,contactArea=area,force=force,axialStress=axial,selfGravityPressure=p,compressiveStress=compression,tensileStress=tension,shearStress=shear,bendingStress=bend,allowableStrength=allowable,safetyFactor=allowable/peak,strengthRequiredForSF1_5=1.5*peak,passes=allowable/peak>=1.5});
            }
            return list;
        }
    }
}
