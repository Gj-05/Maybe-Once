using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    [Serializable] public sealed class BenchmarkResult
    {
        public string id,name,unit,notes;
        public double measured,expected,error,tolerance,dt;
        public bool passed;
        public BenchmarkResult(string id,string name,double measured,double expected,double tolerance,string unit,string notes="",double dt=0)
        { this.id=id; this.name=name; this.measured=measured; this.expected=expected; error=Math.Abs(measured-expected); this.tolerance=tolerance; this.unit=unit; this.notes=notes; this.dt=dt; passed=!double.IsNaN(error) && error<=tolerance; }
        public override string ToString() => string.Format(CultureInfo.InvariantCulture,"{0} {1}: {2}; measured={3:R} {4}, expected={5:R}, error={6:R}, tolerance={7:R}, dt={8:R}; {9}",id,name,passed?"PASS":"FAIL",measured,unit,expected,error,tolerance,dt,notes);
    }
    public static class BenchmarkSuite
    {
        public static List<BenchmarkResult> Run(string dataPath)
        {
            var all=new List<BenchmarkResult>(); for(int i=1;i<=10;i++) all.AddRange(Run(i,dataPath)); return all;
        }
        public static List<BenchmarkResult> Run(int number,string dataPath)
        {
            var b=new List<BenchmarkResult>(); double au=Constants.AU,mu=Constants.SolarMu;
            if(number==1) { var f=Lorentz.Force(10,new Vector3d(2,0,0),new Vector3d(0,0.5,0)); b.Add(new BenchmarkResult("B1","Wire vector error",(f-new Vector3d(0,0,10)).Magnitude,0,1e-8,"N")); }
            if(number==2) { var loop=new[]{new Vector3d(),new Vector3d(2,0,0),new Vector3d(2,3,0),new Vector3d(0,3,0)}; b.Add(new BenchmarkResult("B2","Closed loop net force",Lorentz.LoopForce(10,loop,new Vector3d(0,0.5,0)).Magnitude,0,1e-9*10*10*0.5,"N","Tolerance = 1e-9 of I*perimeter*B")); }
            if(number==3) b.Add(new BenchmarkResult("B3","Long wire field",Lorentz.LongWireField(1000,1),2e-4,2e-10,"T","1e-6 relative (stricter than absolute interpretation)"));
            if(number==4)
            {
                var s=KeplerOrbit.Binary(au,0.3); double p=KeplerOrbit.Period(au,mu+Constants.G); double dt=p/20000; var r=new SimulationRunner(s,dt); var initial=s.Bodies[1].Position-s.Bodies[0].Position;
                r.RunSteps(200000); double position=((r.State.Bodies[1].Position-r.State.Bodies[0].Position)-initial).Magnitude/au;
                b.Add(new BenchmarkResult("B4.position","Eccentric Kepler return, 10 periods",position,0,1e-6,"a",dt:dt)); b.Add(new BenchmarkResult("B4.energy","Eccentric Kepler energy",r.Ledger.MaxEnergyError,0,1e-9,"relative",dt:dt));
            }
            if(number==5)
            {
                double dt=3600; SimulationRunner r;
                do { r=new SimulationRunner(InitialConditions.SolarSystem(dataPath),dt); r.RunSteps((long)(5*Constants.JulianYear/dt)); if(r.Ledger.MaxEnergyError<=1e-9) break; dt/=2; } while(dt>=225);
                b.Add(new BenchmarkResult("B5","Full solar system 5yr max energy drift",r.Ledger.MaxEnergyError,0,1e-9,"relative","Sun+8 planets+Moon; approximate J2000; reduced dt only if necessary",r.Dt));
                b.Add(new BenchmarkResult("B5.P","Full system max momentum drift",r.Ledger.MaxMomentumError,0,1e-11,"initial gross momentum",dt:r.Dt));
                b.Add(new BenchmarkResult("B5.L","Full system max angular momentum drift",r.Ledger.MaxAngularMomentumError,0,1e-11,"relative",dt:r.Dt));
            }
            if(number==6)
            {
                var r=new SimulationRunner(InitialConditions.SolarSystem(dataPath)); double angle=0,previousAngle=0,previousTime=0; var last=InitialConditions.EarthMoonBarycenter(r.State).Position;
                while(angle<2*Math.PI && r.State.Time<400*Units.Day)
                {
                    previousAngle=angle; previousTime=r.State.Time; r.Step(); var next=InitialConditions.EarthMoonBarycenter(r.State).Position;
                    double d=Math.Atan2(next.Y,next.X)-Math.Atan2(last.Y,last.X); if(d< -Math.PI) d+=2*Math.PI; angle+=d; last=next;
                }
                double period=previousTime+(2*Math.PI-previousAngle)/(angle-previousAngle)*r.Dt;
                b.Add(new BenchmarkResult("B6","N-body EMB sidereal period",period/Units.Day,365.256,0.1,"days",dt:r.Dt));
            }
            if(number==7)
            {
                var h=Maneuvers.Hohmann(au,1.524*au);
                b.Add(new BenchmarkResult("B7.dv1","Hohmann departure",h.Dv1/1000,2.94,0.02,"km/s")); b.Add(new BenchmarkResult("B7.dv2","Hohmann arrival",h.Dv2/1000,2.65,0.02,"km/s")); b.Add(new BenchmarkResult("B7.time","Hohmann flight time",h.Seconds/Units.Day,259,2,"days"));
                var s=KeplerOrbit.Binary(au); s.Bodies[1].Velocity+=new Vector3d(0,h.Dv1,0); var r=new SimulationRunner(s,h.Seconds/10000); r.RunSteps(10000);
                var body=r.State.Bodies[1]; var sun=r.State.Bodies[0]; var relative=body.Position-sun.Position; var velocity=body.Velocity-sun.Velocity;
                double burn=Math.Sqrt(mu/(1.524*au))-velocity.Magnitude; body.Velocity+=velocity.Normalized*burn;
                b.Add(new BenchmarkResult("B7.simulatedRadius","Simulated two-burn arrival radius",relative.Magnitude/(1.524*au)-1,0,1e-8,"relative",dt:r.Dt));
                b.Add(new BenchmarkResult("B7.simulatedBurn","Simulated arrival burn",burn/1000,2.65,0.02,"km/s",dt:r.Dt));
                b.Add(new BenchmarkResult("B7.circular","Post-burn eccentricity",KeplerOrbit.EccentricityVector(relative,body.Velocity-sun.Velocity,mu).Magnitude,0,1e-8,"dimensionless",dt:r.Dt));
            }
            if(number==8)
            {
                var r=new SimulationRunner(KeplerOrbit.Binary(au),3600); r.Forces.Add(new ProgradeAcceleration(1,0,1e-8)); double start=au;
                double first=0,last=0,sum=0; int n=(int)(Constants.JulianYear/r.Dt);
                for(int i=0;i<n;i++)
                {
                    var body=r.State.Bodies[1]; var sun=r.State.Bodies[0]; var p=body.Position-sun.Position; var v=body.Velocity-sun.Velocity; double a=KeplerOrbit.SemiMajorAxis(p,v,mu);
                    if(i==0) first=a; sum+=Maneuvers.GaussDaDt(a,v,v.Normalized*1e-8)*r.Dt; r.Step();
                }
                var end=r.State.Bodies[1]; var center=r.State.Bodies[0]; last=KeplerOrbit.SemiMajorAxis(end.Position-center.Position,end.Velocity-center.Velocity,mu);
                b.Add(new BenchmarkResult("B8","Prograde push semi-major growth",(last-start)/1000,3170,31.7,"km","Reference 1% unchanged",r.Dt));
                b.Add(new BenchmarkResult("B8.Gauss","Integrated Gauss versus state delta-a",Math.Abs(sum-(last-first))/Math.Abs(last-first),0,0.01,"relative",dt:r.Dt));
            }
            if(number==9)
            {
                // Independent numerical curvature integration, q=100 N/m, L=10m, E=200GPa, I=1e-4m4 (toy fixture).
                double q=100,l=10,young=200e9,inertia=1e-4,dx=l/10000; var moment=new KahanSum(); var deflection=new KahanSum();
                for(int k=0;k<=10000;k++) { double x=k*dx,w=(k==0 || k==10000)?1:(k%2==0?2:4); moment.Add(w*q*(l-x)); deflection.Add(w*(l-x)*q*(l-x)*(l-x)/(2*young*inertia)); }
                b.Add(new BenchmarkResult("B9.moment","Cantilever root moment",moment.Value*dx/3,Beam.RootMoment(q,l),0.01*Beam.RootMoment(q,l),"N m"));
                b.Add(new BenchmarkResult("B9.deflection","Cantilever tip deflection",deflection.Value*dx/3,Beam.TipDeflection(q,l,young,inertia),0.01*Beam.TipDeflection(q,l,young,inertia),"m"));
            }
            if(number==10) b.Add(new BenchmarkResult("B10","Earth radiative equilibrium",Insolation.EquilibriumTemperature(au),254,2,"K"));
            return b;
        }
        public static void Write(string path,IEnumerable<BenchmarkResult> results)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))); var text=new StringBuilder("{\"schema\":1,\"approximate\":true,\"epoch\":\"J2000\",\"results\":["); bool first=true;
            foreach(var r in results)
            {
                if(!first) text.Append(','); first=false;
                text.Append("{\"id\":\"").Append(Json.Escape(r.id)).Append("\",\"name\":\"").Append(Json.Escape(r.name)).Append("\",\"passed\":").Append(r.passed?"true":"false");
                text.AppendFormat(CultureInfo.InvariantCulture,",\"measured\":{0:R},\"expected\":{1:R},\"error\":{2:R},\"tolerance\":{3:R},\"dt\":{4:R}",r.measured,r.expected,r.error,r.tolerance,r.dt);
                text.Append(",\"unit\":\"").Append(Json.Escape(r.unit)).Append("\",\"notes\":\"").Append(Json.Escape(r.notes)).Append("\"}");
            }
            File.WriteAllText(path,text.Append("]}").ToString());
        }
    }
}
