using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
namespace OctoEarth.Core
{
    public sealed class SitingScreen
    {
        public double radiusAU,inclinationDegrees,phaseDegrees,maximumEarthShade,maximumMirrorShade,minimumEarthDistance=1e300,minimumMirrorDistance=1e300,maximumArrayJetPower,minimumEarthPlumeClearance=1e300,minimumMoonPlumeClearance=1e300;
        public bool coMoving,zeroSampledShadingAndIntersections;
        public int records,plumeIntersectionFlags,requiredForceTowardEarthRecords;
        public MissionFlag[] flags;
        internal MissionFlags found=new MissionFlags();
    }
    public sealed class SitingSweepReport
    {
        public string scope="Kinematic screening on the original full-run native monthly CSV samples; not 96 independent mission integrations. Zero sampled flags does not certify inter-sample safety. Selected design must pass full hourly monitors.";
        public SitingScreen[] points;
    }
    public static class ArraySitingSweep
    {
        static Vector3d ReadVector(string[] f,int k)=>new Vector3d(double.Parse(f[k],CultureInfo.InvariantCulture),double.Parse(f[k+1],CultureInfo.InvariantCulture),double.Parse(f[k+2],CultureInfo.InvariantCulture));
        static Vector3d GravityAt(SystemState s,int index)
        {
            var sum=new KahanVector(); var p=s.Bodies[index].Position;
            for(int j=0;j<s.Bodies.Length;j++) if(j!=index) {var d=s.Bodies[j].Position-p; sum.Add(d*(Constants.G*s.Bodies[j].Mass/Math.Pow(d.Magnitude,3)));} return sum.Value;
        }
        public static SitingSweepReport Run(string dataPath,string csv,string folder,Action<string> progress=null)
        {
            Directory.CreateDirectory(folder); var points=new List<SitingScreen>(); var configs=new List<MissionConfiguration>();
            foreach(double radius in new[]{0.3,0.5,1.0}) foreach(double inc in new[]{10.0,20,30,45}) foreach(double phase in new[]{0.0,90,180,270}) foreach(bool moving in new[]{false,true})
            {points.Add(new SitingScreen{radiusAU=radius,inclinationDegrees=inc,phaseDegrees=phase,coMoving=moving}); configs.Add(new MissionConfiguration{ArrayRadiusAU=radius,ArrayInclinationDegrees=inc,ArrayPhaseDegrees=phase,ArrayCoMoving=moving,ArrayTracksEarthLongitude=moving,SteerPlumes=true});}
            var initial=GravityTractor.Planetary(dataPath,new TractorParameters()); var bodies=new List<Body>(initial.Bodies); bodies.Add(new Body("Array",1,new Vector3d(),new Vector3d())); bodies.Add(new Body("Mirror",1,new Vector3d(),new Vector3d())); var s=new SystemState(bodies.ToArray());
            int earth=s.Index("Earth"),moon=s.Index("Moon"),sun=s.Index("Sun"),tractor=s.Index("Tractor"),array=s.Index("Array"),mirror=s.Index("Mirror"); int[] emitters={tractor,array,mirror},targets={earth,moon,tractor,array,mirror};
            var limits=new MissionThresholds(); var optical=new FocusingMirror(); var force=new Vector3d[3];
            double requested=new MissionBudget(Constants.AU,2.7*Constants.AU,Constants.PairMass,10000*Constants.JulianYear).Force*1.15*300000/(2*Math.Cos(Units.Radians(25)));
            using(var reader=File.OpenText(csv)) using(var distanceFile=new StreamWriter(Path.Combine(folder,"original_distances.csv")))
            {
                var header=reader.ReadLine().Split(','); int time=Array.IndexOf(header,"time_s"),aa=Array.IndexOf(header,"array_area_m2"),ma=Array.IndexOf(header,"mirror_area_m2"); var offsets=new int[bodies.Count]; for(int k=0;k<offsets.Length;k++) offsets[k]=Array.IndexOf(header,bodies[k].Name+"_x");
                distanceFile.WriteLine("time_yr,earth_array_m,mirror_array_m"); string line; int rows=0;
                while((line=reader.ReadLine())!=null)
                {
                    var f=line.Split(','); s.Time=double.Parse(f[time],CultureInfo.InvariantCulture);
                    for(int k=0;k<bodies.Count;k++) {bodies[k].Position=ReadVector(f,offsets[k]); bodies[k].Velocity=ReadVector(f,offsets[k]+3);}
                    double oldArea=double.Parse(f[aa],CultureInfo.InvariantCulture),mirrorArea=double.Parse(f[ma],CultureInfo.InvariantCulture); bodies[array].Mass=oldArea*0.01; bodies[mirror].Mass=mirrorArea*0.001;
                    var e=bodies[earth]; var sol=bodies[sun]; var ep=e.Position; var sp=sol.Position; var mp=bodies[mirror].Position;
                    if(rows%12==0) distanceFile.WriteLine(string.Join(",",new[]{s.Time/Constants.JulianYear,(ep-bodies[array].Position).Magnitude,(mp-bodies[array].Position).Magnitude}.SelectInvariant()));
                    var eAcc=GravityAt(s,earth); var sunAcc=GravityAt(s,sun); var rel=ep-sp; var rv=e.Velocity-sol.Velocity; var ra=eAcc-sunAcc;
                    double rr=rel.Magnitude,rd=Vector3d.Dot(rv,rel.Normalized),rdd=(rv.SqrMagnitude+Vector3d.Dot(rel,ra)-rd*rd)/rr;
                    double planar=rel.X*rel.X+rel.Y*rel.Y,omega=(rel.X*rv.Y-rel.Y*rv.X)/planar;
                    double omegaDot=(rel.X*ra.Y-rel.Y*ra.X)/planar-2*(rel.X*rv.X+rel.Y*rv.Y)/planar*omega;
                    var normal=Vector3d.Cross(rel,rv).Normalized;
                    for(int k=0;k<points.Count;k++)
                    {
                        var point=points[k]; var c=configs[k]; double radius=c.ArrayRadiusAU*(c.ArrayCoMoving?rr:Constants.AU);
                        double theta=(c.ArrayCoMoving?Math.Atan2(rel.Y,rel.X):Math.Sqrt(Constants.SolarMu/Math.Pow(radius,3))*s.Time)+Units.Radians(c.ArrayPhaseDegrees),incl=Units.Radians(c.ArrayInclinationDegrees);
                        var axis=new Vector3d(Math.Cos(theta),Math.Sin(theta)*Math.Cos(incl),Math.Sin(theta)*Math.Sin(incl)); var tangent=new Vector3d(-Math.Sin(theta),Math.Cos(theta)*Math.Cos(incl),Math.Cos(theta)*Math.Sin(incl));
                        var ap=sp+axis*radius; var desired=c.ArrayCoMoving?sunAcc+axis*(rdd*c.ArrayRadiusAU-radius*omega*omega)+tangent*(2*rd*c.ArrayRadiusAU*omega+radius*omegaDot):sunAcc-axis*(Constants.SolarMu/(radius*radius));
                        double n=c.ArrayCoMoving?omega:Math.Sqrt(Constants.SolarMu/Math.Pow(radius,3)); bodies[array].Position=ap; bodies[array].Velocity=sol.Velocity+axis*(c.ArrayCoMoving?rd*c.ArrayRadiusAU:0)+tangent*(radius*n);
                        double flux=Insolation.Flux(radius),pressure=flux*(1+c.ArrayReflectivity)/Constants.C;
                        bodies[array].Mass=1; var acceleration=desired-GravityAt(s,array);
                        var forcePerArea=acceleration*c.ArrayDensity-axis*pressure;
                        double denominator=flux*c.ArrayEfficiency-forcePerArea.Magnitude*c.ExhaustSpeed/(2*Math.Cos(Units.Radians(c.CantDegrees)));
                        if(denominator<=0) {point.found.Add(s.Time,"ArraySelfPower","Array cannot power its prescribed trajectory"); continue;}
                        double area=requested*(1+c.PowerReserve)/denominator; bodies[array].Mass=area*c.ArrayDensity;
                        double shadeEarth=MissionGeometry.Shading(ep,sp,ap,area),shadeMirror=MissionGeometry.Shading(mp,sp,ap,area);
                        point.maximumEarthShade=Math.Max(point.maximumEarthShade,shadeEarth); point.maximumMirrorShade=Math.Max(point.maximumMirrorShade,shadeMirror);
                        if(shadeEarth>limits.shadingMaximum.value || shadeMirror>limits.shadingMaximum.value) point.found.Add(s.Time,"ArrayShading","Sampled disc obscuration exceeds unchanged threshold");
                        point.minimumEarthDistance=Math.Min(point.minimumEarthDistance,(ep-ap).Magnitude); point.minimumMirrorDistance=Math.Min(point.minimumMirrorDistance,(mp-ap).Magnitude);
                        force[1]=forcePerArea*area;
                        optical.Evaluate(sp,ep,mp,c.MirrorReflectivity,c.OpticalErrorRadians,shadeMirror,shadeEarth,mirrorArea);
                        var q=(ep-mp).Normalized;
                        force[2]=(eAcc+q*((ep-mp).Magnitude*omega*omega)-GravityAt(s,mirror))*bodies[mirror].Mass-optical.RadiationForce(Math.Min(mirrorArea,optical.RequiredArea),c.MirrorReflectivity);
                        force[0]=(eAcc-(bodies[tractor].Position-ep).Normalized*(c.Distance*omega*omega)-GravityAt(s,tractor))*bodies[tractor].Mass;
                        point.maximumArrayJetPower=Math.Max(point.maximumArrayJetPower,force[1].Magnitude*c.ExhaustSpeed/(2*Math.Cos(Units.Radians(c.CantDegrees))));
                        if(Vector3d.Dot(force[1],ep-ap)>0) point.requiredForceTowardEarthRecords++;
                        for(int emitter=0;emitter<3;emitter++)
                        {
                            var source=bodies[emitters[emitter]]; var thrustAxis=force[emitter].Normalized; var cant=PlumeSteering.Select(s,emitters[emitter],thrustAxis,normal,c);
                            foreach(int sign in new[]{-1,1})
                            {
                                var exhaust=-Math.Cos(Units.Radians(c.CantDegrees))*thrustAxis+cant*(sign*Math.Sin(Units.Radians(c.CantDegrees)));
                                if(Vector3d.Dot(source.Velocity-sol.Velocity+exhaust*c.ExhaustSpeed,source.Position-sp)<=0) point.found.Add(s.Time,"InitiallyInwardEjecta."+source.Name,"Kinematic screen: required reaction geometry cannot keep every stream initially radially outbound");
                                foreach(int target in targets)
                                {
                                    if(target==emitters[emitter]) continue; double along; double miss=MissionGeometry.RayDistance(source.Position,exhaust,bodies[target].Position,out along); if(along<=0) continue;
                                    double r=target==earth?Constants.EarthRadius:target==moon?MissionGeometry.MoonRadius:target==tractor?Math.Pow(3*bodies[tractor].Mass/(4*Math.PI*c.RockDensity),1.0/3):Math.Sqrt((target==array?area:mirrorArea)/Math.PI);
                                    double required=r+along*Math.Tan(Units.Radians(c.PlumeHalfAngleDegrees)); if(target==earth) required=Math.Max(required,limits.plumeExclusionRadii.value*Constants.EarthRadius);
                                    double clearance=miss-required;
                                    if(target==earth) point.minimumEarthPlumeClearance=Math.Min(point.minimumEarthPlumeClearance,clearance); if(target==moon) point.minimumMoonPlumeClearance=Math.Min(point.minimumMoonPlumeClearance,clearance);
                                    if(clearance<0) point.found.Add(s.Time,"PlumeIntersection."+source.Name+"."+bodies[target].Name,"Kinematic sampled plume cone/exclusion intersection");
                                }
                            }
                        }
                        point.records++;
                    }
                    rows++; if(rows%20000==0) progress?.Invoke("Siting screening rows="+rows+" / 121752");
                }
            }
            using(var file=new StreamWriter(Path.Combine(folder,"array_siting_sweep.csv")))
            {
                file.WriteLine("radius_AU,inclination_deg,phase_deg,co_moving,max_earth_shade,max_mirror_shade,min_earth_array_m,min_mirror_array_m,max_array_jet_W,min_earth_plume_clearance_m,min_moon_plume_clearance_m,zero_sampled_shading_and_intersections,flags");
                foreach(var p in points) {p.flags=p.found.All; foreach(var f in p.flags) if(f.code.StartsWith("PlumeIntersection")) p.plumeIntersectionFlags++; p.zeroSampledShadingAndIntersections=p.maximumEarthShade==0 && p.maximumMirrorShade==0 && p.plumeIntersectionFlags==0;
                    var codes=new List<string>(); foreach(var f in p.flags) codes.Add(f.code);
                    file.WriteLine(string.Join(",",new[]{p.radiusAU,p.inclinationDegrees,p.phaseDegrees,p.coMoving?1:0,p.maximumEarthShade,p.maximumMirrorShade,p.minimumEarthDistance,p.minimumMirrorDistance,p.maximumArrayJetPower,p.minimumEarthPlumeClearance,p.minimumMoonPlumeClearance,p.zeroSampledShadingAndIntersections?1:0}.SelectInvariant())+","+string.Join(";",codes)); }
            }
            var report=new SitingSweepReport{points=points.ToArray()}; MissionConfigurationIO.Write(Path.Combine(folder,"array_siting_sweep.json"),report); return report;
        }
    }
}
