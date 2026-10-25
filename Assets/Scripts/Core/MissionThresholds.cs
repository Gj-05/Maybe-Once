using System;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public sealed class MissionThreshold
    {
        public double value;
        public bool placeholder=true;
        public string units;
        public MissionThreshold() { }
        public MissionThreshold(double value,string units) { this.value=value; this.units=units; }
    }
    public sealed class MissionThresholds
    {
        // Rulebook absent: all defaults are explicit placeholders supplied/required by the user.
        public MissionThreshold temperatureMinimum=new MissionThreshold(278,"K"),temperatureMaximum=new MissionThreshold(298,"K");
        public MissionThreshold liquidFractionMinimum=new MissionThreshold(1,"fraction"),tideRatioMaximum=new MissionThreshold(0.01,"Moon tide ratio");
        public MissionThreshold moonDistanceMinimum=new MissionThreshold(3e8,"m"),moonDistanceMaximum=new MissionThreshold(5e8,"m");
        public MissionThreshold plumeExclusionRadii=new MissionThreshold(3,"Earth radii"),plumeIrradianceMaximum=new MissionThreshold(1,"W/m2");
        public MissionThreshold shadingMaximum=new MissionThreshold(0.01,"solar disc fraction"),mirrorFluxMaximum=new MissionThreshold(1.05*1361,"W/m2");
        public MissionThreshold eccentricityMaximum=new MissionThreshold(0.1,"dimensionless"),minimumSafetyFactor=new MissionThreshold(1.5,"dimensionless");
        public MissionThreshold ledgerMaximum=new MissionThreshold(1e-9,"relative"),convergenceMaximum=new MissionThreshold(0.01,"relative");
        public bool AnyPlaceholder
        { get { foreach(var f in GetType().GetFields()) if(f.GetValue(this) is MissionThreshold t && t.placeholder) return true; return false; } }
    }
    public sealed class MissionFlag
    {
        public string code,cause;
        public double firstTimeSeconds;
        public bool enabler;
    }
    public sealed class MissionFlags
    {
        readonly Dictionary<string,MissionFlag> first=new Dictionary<string,MissionFlag>();
        public void Add(double time,string code,string cause,bool enabler=false)
        { if(!first.ContainsKey(code)) first.Add(code,new MissionFlag{firstTimeSeconds=time,code=code,cause=cause,enabler=enabler}); }
        public MissionFlag[] All { get { var list=new List<MissionFlag>(first.Values); list.Sort((a,b)=>a.firstTimeSeconds.CompareTo(b.firstTimeSeconds)); return list.ToArray(); } }
        public bool HasUnresolved { get { foreach(var f in first.Values) if(!f.enabler) return true; return false; } }
    }
}
