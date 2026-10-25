using System;
using System.IO;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public static class SpiralGate
    {
        public static List<BenchmarkResult> Run(double dt,string folder,Action<string> progress=null)
        {
            Directory.CreateDirectory(folder); var config=new TractorParameters(); var s=GravityTractor.Toy(config);
            int earth=s.Index("Earth"),sun=s.Index("Sun"),tractor=s.Index("Tractor");
            var stepper=new PrescribedTractorStepper(s,config); var ledger=new Ledger(s);
            double acceleration=Constants.G*s.Bodies[tractor].Mass/(config.Distance*config.Distance);
            double v1=Math.Sqrt(Constants.SolarMu/Constants.AU);
            double expected=(v1-Math.Sqrt(Constants.SolarMu/(2.7*Constants.AU)))/acceleration;
            var rows=new List<BenchmarkResult>();
            int nextSample=1; long count=(long)Math.Ceiling(10000*Constants.JulianYear/dt); double maxE=0,timeToTarget=double.NaN;
            using(var file=new StreamWriter(Path.Combine(folder,"spiral.csv")))
            {
                file.WriteLine("time_s,a_m,e,analytic_a_m,earth_x_m,earth_y_m,earth_z_m,tractor_x_m,tractor_y_m,tractor_z_m,max_relative_P,max_relative_L,max_relative_E,ejected_kg");
                for(long k=0;k<count;k++)
                {
                    stepper.Step(s,ledger,dt);
                    var r=s.Bodies[earth].Position-s.Bodies[sun].Position; var v=s.Bodies[earth].Velocity-s.Bodies[sun].Velocity;
                    double mu=Constants.G*(s.Bodies[earth].Mass+s.Bodies[sun].Mass);
                    double a=KeplerOrbit.SemiMajorAxis(r,v,mu),e=KeplerOrbit.EccentricityVector(r,v,mu).Magnitude;
                    maxE=Math.Max(maxE,e); double analytic=Constants.SolarMu/Math.Pow(v1-acceleration*s.Time,2);
                    if(nextSample<=4 && s.Time>=expected*nextSample/4)
                    {
                        rows.Add(new BenchmarkResult("G3.a.sample"+nextSample,"Semi-major axis versus slow circular spiral",a/analytic-1,0,0.02,"relative","time yr="+(s.Time/Constants.JulianYear),dt)); nextSample++;
                    }
                    if(k%Math.Max(1,(int)(100*Constants.JulianYear/dt))==0 || a>=2.7*Constants.AU || k==count-1)
                    {
                        var ep=s.Bodies[earth].Position; var tp=s.Bodies[tractor].Position;
                        file.WriteLine(string.Join(",",new[]{s.Time,a,e,analytic,ep.X,ep.Y,ep.Z,tp.X,tp.Y,tp.Z,ledger.MaxMomentumError,ledger.MaxAngularMomentumError,ledger.MaxEnergyError,stepper.Drive.Propellant}.SelectInvariant()));
                        progress?.Invoke("G3 year="+(s.Time/Constants.JulianYear).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+" a/AU="+(a/Constants.AU).ToString("F6",System.Globalization.CultureInfo.InvariantCulture));
                    }
                    if(a>=2.7*Constants.AU) { timeToTarget=s.Time; break; }
                }
            }
            rows.Add(new BenchmarkResult("G3.targetTime","Target time versus delta-v / acceleration",double.IsNaN(timeToTarget)?double.PositiveInfinity:timeToTarget/expected-1,0,0.05,"relative","analytic yr="+expected/Constants.JulianYear+"; actual yr="+timeToTarget/Constants.JulianYear,dt));
            rows.Add(new BenchmarkResult("G3.e","Maximum osculating eccentricity",maxE,0,0.1,"dimensionless",dt:dt));
            rows.Add(new BenchmarkResult("G3.P","Momentum ledger diagnostic",ledger.MaxMomentumError,0,1e-9,"relative",dt:dt));
            rows.Add(new BenchmarkResult("G3.L","Angular momentum diagnostic",ledger.MaxAngularMomentumError,0,1e-9,"relative",dt:dt));
            rows.Add(new BenchmarkResult("G3.E","Energy ledger diagnostic",ledger.MaxEnergyError,0,1e-9,"relative",dt:dt));
            BenchmarkSuite.Write(Path.Combine(folder,"g3_results.json"),rows); File.WriteAllText(Path.Combine(folder,"ledger.txt"),ledger.Report());
            return rows;
        }
    }
}
