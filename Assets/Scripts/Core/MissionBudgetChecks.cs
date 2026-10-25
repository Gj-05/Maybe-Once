using System;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public static class MissionBudgetChecks
    {
        public const double AnalyticRelativeTolerance=1e-9;
        // User-approved separation: illustrative guide rounding has a 2% sanity envelope.
        // This is never used to accept a code-versus-formula discrepancy.
        public const double GuideRoundingRelativeTolerance=0.02;
        public static List<BenchmarkResult> Analytic()
        {
            var rows=new List<BenchmarkResult>();
            // Multiple fixtures prevent agreement at one particular mission input from masking errors.
            AddAnalytic(rows,"nominal",Constants.AU,2.7*Constants.AU,Constants.PairMass,10000*Constants.JulianYear);
            AddAnalytic(rows,"alternate",0.8*Constants.AU,1.9*Constants.AU,0.7*Constants.PairMass,2300*Constants.JulianYear);
            AddAnalytic(rows,"inward",2.7*Constants.AU,Constants.AU,Constants.PairMass,5000*Constants.JulianYear);
            return rows;
        }
        static void AddAnalytic(List<BenchmarkResult> rows,string fixture,double r1,double r2,double mass,double duration)
        {
            var actual=new MissionBudget(r1,r2,mass,duration);
            // Independent evaluation through state energies and cross products; no production budget
            // or Maneuvers helper is used to obtain an expected value.
            double mu=Constants.SolarMu, v1=Math.Sqrt(mu/r1),v2=Math.Sqrt(mu/r2);
            double eps1=v1*v1/2-mu/r1,eps2=v2*v2/2-mu/r2;
            double energy=mass*(eps2-eps1);
            double dv=Math.Sqrt(mu)*(Math.Sqrt(r2)-Math.Sqrt(r1))/Math.Sqrt(r1*r2);
            double impulse=mass*dv;
            var h1=Vector3d.Cross(new Vector3d(r1,0,0),new Vector3d(0,v1,0));
            var h2=Vector3d.Cross(new Vector3d(r2,0,0),new Vector3d(0,v2,0));
            Add(rows,fixture,"E",actual.Energy,energy,"J");
            Add(rows,fixture,"dv",actual.DeltaV,dv,"m/s");
            Add(rows,fixture,"p",actual.Impulse,impulse,"N s");
            Add(rows,fixture,"F",actual.Force,impulse/duration,"N");
            Add(rows,fixture,"P",actual.Power,energy/duration,"W");
            Add(rows,fixture,"L",actual.AngularMomentum,mass*(h2.Z-h1.Z),"kg m2/s");
            Add(rows,fixture,"solarDays",actual.SolarDays,energy/Constants.SolarLuminosity/Units.Day,"days");
        }
        static void Add(List<BenchmarkResult> rows,string fixture,string field,double actual,double expected,string unit)
        {
            rows.Add(new BenchmarkResult("Budget.analytic."+fixture+"."+field,"Budget versus independent analytic "+field,actual,expected,Math.Abs(expected)*AnalyticRelativeTolerance,unit,"1e-9 relative; expected computed from SI constants, not guide rounding"));
        }
        public static List<BenchmarkResult> GuideSanity()
        {
            var b=new MissionBudget(Constants.AU,2.7*Constants.AU,Constants.PairMass,10000*Constants.JulianYear);
            var rows=new List<BenchmarkResult>();
            Guide(rows,"E",b.Energy,1.69e33,"J");
            Guide(rows,"v1",Math.Sqrt(Constants.SolarMu/Constants.AU)/1000,29.78,"km/s");
            Guide(rows,"v2",Math.Sqrt(Constants.SolarMu/(2.7*Constants.AU))/1000,18.12,"km/s");
            Guide(rows,"dv",b.DeltaV/1000,11.66,"km/s");
            Guide(rows,"p",b.Impulse,7.05e28,"N s");
            Guide(rows,"F",b.Force,2.2e17,"N");
            Guide(rows,"acceleration",b.DeltaV/(10000*Constants.JulianYear),3.7e-8,"m/s2");
            Guide(rows,"P",b.Power,5.4e21,"W");
            Guide(rows,"L",b.AngularMomentum,1.73e40,"kg m2/s");
            return rows;
        }
        static void Guide(List<BenchmarkResult> rows,string field,double actual,double rounded,string unit)
        {
            rows.Add(new BenchmarkResult("Budget.guide."+field,"Rounded guide sanity "+field,actual,rounded,Math.Abs(rounded)*GuideRoundingRelativeTolerance,unit,"2% illustration-only rounding envelope; separate from 1e-9 analytic acceptance"));
        }
    }
}
