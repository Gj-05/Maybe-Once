using System;
using System.IO;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public sealed class MissionOrderPoint
    {
        public double dt,years,finalA_AU,earthPositionErrorAU,aErrorAU;
        public string folder;
    }
    public sealed class MissionOrderReport
    {
        public string scope="Full forced 13-body candidate mission, common first 10 years. This is not a full-duration order fit. All output times agree. Finest 450s result is a numerical reference, not an analytic solution.";
        public double logLogPositionOrder,logLogSemiMajorOrder;
        public MissionOrderPoint[] points;
    }
    public static class MissionConvergenceStudy
    {
        public static MissionOrderReport Run(string dataPath,MissionConfiguration configuration,MissionThresholds limits,string folder,Action<string> progress=null)
        {
            Directory.CreateDirectory(folder); var points=new List<MissionOrderPoint>(); var positions=new List<Vector3d>();
            foreach(double dt in new[]{7200.0,3600,1800,900,450})
            {
                var c=MissionConfigurationIO.Clone(configuration); c.Years=10; c.Dt=dt; c.RecordEveryStep=false;
                string output=Path.Combine(folder,"h"+dt); progress?.Invoke("Mission order study dt="+dt);
                var model=new MissionSimulation(dataPath,c,limits); var report=model.Run(output,true);
                if(!report.completed) throw new InvalidOperationException("Order run did not complete: "+dt);
                var state=model.DiagnosticState; positions.Add(state.Bodies[state.Index("Earth")].Position-state.Bodies[state.Index("Sun")].Position);
                points.Add(new MissionOrderPoint{dt=dt,years=10,finalA_AU=report.finalA_AU,folder=output});
            }
            int reference=points.Count-1;
            for(int i=0;i<points.Count;i++) {points[i].earthPositionErrorAU=(positions[i]-positions[reference]).Magnitude/Constants.AU; points[i].aErrorAU=Math.Abs(points[i].finalA_AU-points[reference].finalA_AU);}
            double Slope(bool position)
            {
                double x=0,y=0,xy=0,xx=0,n=reference;
                for(int i=0;i<reference;i++) {double a=Math.Log(points[i].dt),b=Math.Log(position?points[i].earthPositionErrorAU:points[i].aErrorAU); x+=a;y+=b;xy+=a*b;xx+=a*a;}
                return (n*xy-x*y)/(n*xx-x*x);
            }
            var result=new MissionOrderReport{points=points.ToArray(),logLogPositionOrder=Slope(true),logLogSemiMajorOrder=Slope(false)};
            MissionConfigurationIO.Write(Path.Combine(folder,"mission_order.json"),result); return result;
        }
    }
}
