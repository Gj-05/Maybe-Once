using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public static class InitialConditions
    {
        public static SystemState SolarSystem(string path,bool planets=true)
        {
            var bodies=new List<Body>{new Body("Sun",Constants.SolarMu/Constants.G,new Vector3d(),new Vector3d())};
            foreach(string row in File.ReadAllLines(path))
            {
                if(string.IsNullOrWhiteSpace(row) || row.StartsWith("#")) continue;
                var f=row.Split(','); if(!planets && f[0]!="EMB") continue;
                double[] x=new double[7]; for(int j=0;j<7;j++) x[j]=double.Parse(f[j+1],CultureInfo.InvariantCulture);
                var el=new OrbitalElements{A=x[1]*Constants.AU,E=x[2],I=Units.Radians(x[3]),Node=Units.Radians(x[6]),Periapsis=Units.Radians(x[5]-x[6]),MeanAnomaly=Units.Radians(x[4]-x[5])};
                double gm=f[0]=="EMB" ? Constants.EarthMu+Constants.MoonMu : x[0];
                var orbit=KeplerOrbit.FromElements(el,Constants.SolarMu+gm);
                if(f[0]=="EMB")
                {
                    var relative=KeplerOrbit.FromElements(new OrbitalElements{A=Constants.MoonDistance,E=0},gm);
                    double fraction=Constants.MoonMu/gm;
                    bodies.Add(new Body("Earth",Constants.EarthMu/Constants.G,orbit.Position-relative.Position*fraction,orbit.Velocity-relative.Velocity*fraction));
                    bodies.Add(new Body("Moon",Constants.MoonMu/Constants.G,orbit.Position+relative.Position*(1-fraction),orbit.Velocity+relative.Velocity*(1-fraction)));
                }
                else bodies.Add(new Body(f[0],gm/Constants.G,orbit.Position,orbit.Velocity));
            }
            var state=new SystemState(bodies.ToArray()); state.Barycenter(); return state;
        }
        public static OrbitalState EarthMoonBarycenter(SystemState s)
        {
            var e=s.Bodies[s.Index("Earth")]; var m=s.Bodies[s.Index("Moon")]; var sun=s.Bodies[s.Index("Sun")]; double total=e.Mass+m.Mass;
            return new OrbitalState((e.Position*e.Mass+m.Position*m.Mass)/total-sun.Position,(e.Velocity*e.Mass+m.Velocity*m.Mass)/total-sun.Velocity,s.Time);
        }
        public static bool MoonBound(SystemState s,out double binding,out double hill)
        {
            var e=s.Bodies[s.Index("Earth")]; var m=s.Bodies[s.Index("Moon")]; var sun=s.Bodies[s.Index("Sun")];
            var r=m.Position-e.Position; var v=m.Velocity-e.Velocity; binding=v.SqrMagnitude/2-Constants.G*e.Mass/r.Magnitude;
            var bary=EarthMoonBarycenter(s); double a=KeplerOrbit.SemiMajorAxis(bary.Position,bary.Velocity,Constants.G*(sun.Mass+e.Mass+m.Mass));
            hill=a*Math.Pow((e.Mass+m.Mass)/(3*sun.Mass),1.0/3); return binding<0 && r.Magnitude<hill;
        }
    }
}
