using System;
using System.IO;
using OctoEarth.Core;
#if XUNIT
using Test = Xunit.FactAttribute;
#else
using NUnit.Framework;
#endif
namespace OctoEarth.Tests
{
    public class CoreTests
    {
        static string DataPath
        {
            get { var dir=new DirectoryInfo(Directory.GetCurrentDirectory()); while(dir!=null) { var p=Path.Combine(dir.FullName,"Assets/Data/solar_j2000.csv"); if(File.Exists(p)) return p; dir=dir.Parent; } throw new FileNotFoundException("solar_j2000.csv"); }
        }
        static void Check(System.Collections.Generic.List<BenchmarkResult> rows)
        {
            foreach(var r in rows)
            {
#if XUNIT
                Xunit.Assert.True(r.passed,r.ToString());
#else
                TestContext.WriteLine(r); Assert.That(r.passed,Is.True,r.ToString());
#endif
            }
        }
        [Test] public void B1Wire() => Check(BenchmarkSuite.Run(1,DataPath));
        [Test] public void B2ClosedLoop() => Check(BenchmarkSuite.Run(2,DataPath));
        [Test] public void B3LongWire() => Check(BenchmarkSuite.Run(3,DataPath));
        [Test] public void B4EccentricKepler() => Check(BenchmarkSuite.Run(4,DataPath));
        [Test] public void B5SolarSystem() => Check(BenchmarkSuite.Run(5,DataPath));
        [Test] public void B6BarycenterPeriod() => Check(BenchmarkSuite.Run(6,DataPath));
        [Test] public void B7Hohmann() => Check(BenchmarkSuite.Run(7,DataPath));
        [Test] public void B8SmallPush() => Check(BenchmarkSuite.Run(8,DataPath));
        [Test] public void B9Cantilever() => Check(BenchmarkSuite.Run(9,DataPath));
        [Test] public void B10Temperature() => Check(BenchmarkSuite.Run(10,DataPath));
        [Test] public void CircularAndKeplerScaling() => Check(ValidationSuite.Run(1,DataPath));
        [Test] public void EjectaRocketAndLedger() => Check(ValidationSuite.Run(2,DataPath));
        [Test] public void IntegratorConvergence() => Check(ValidationSuite.Run(3,DataPath));
        [Test] public void EnckeAndFloatPrecision() => Check(ValidationSuite.Run(4,DataPath));
        [Test] public void MissionBudgetReference() => Check(ValidationSuite.Run(5,DataPath));
        [Test] public void MissionBudgetRoundedGuideSanity() => Check(MissionBudgetChecks.GuideSanity());
        [Test] public void G1ToyHover() => Check(TractorGates.G1());
        [Test] public void FullShadingCouplesToClimate() => Check(DesignClimateChecks.Run());
        [Test] public void PlumeRotationPreservesRequiredForceAndLedger()
        {
            var c=new MissionConfiguration(); var s=GravityTractor.Planetary(DataPath,c.Tractor()); int index=s.Index("Tractor"),earth=s.Index("Earth"),sun=s.Index("Sun");
            var axis=(s.Bodies[index].Position-s.Bodies[earth].Position).Normalized;
            var normal=Vector3d.Cross(s.Bodies[earth].Position-s.Bodies[sun].Position,s.Bodies[earth].Velocity-s.Bodies[sun].Velocity).Normalized;
            var selected=PlumeSteering.Select(s,index,axis,normal,c); var before=s.Bodies[index].Velocity; double mass=s.Bodies[index].Mass,thrust=mass*100;
            var ledger=new Ledger(s); var drive=new EjectaDrive(index,thrust,c.ExhaustSpeed,axis); drive.KickCanted(s,ledger,1,axis,selected,Units.Radians(c.CantDegrees)); ledger.Check(s);
            Check(new System.Collections.Generic.List<BenchmarkResult>{new BenchmarkResult("Plume.netForce","Measured kick retains required vector",((s.Bodies[index].Velocity-before)*mass-axis*thrust).Magnitude/thrust,0,1e-9,"relative"),new BenchmarkResult("Plume.energy","Rotated canted pair energy ledger",ledger.MaxEnergyError,0,1e-9,"relative"),new BenchmarkResult("Plume.momentum","Rotated canted pair momentum ledger",ledger.MaxMomentumError,0,1e-9,"relative")});
        }
        [Test] public void CoMovingArrayRadiusIsARealDesignVariable()
        {
            var c=new MissionConfiguration{ArrayRadiusAU=0.5,ArrayCoMoving=true,ArrayTracksEarthLongitude=true,ArrayPhaseDegrees=180};
            var model=new MissionSimulation(DataPath,c,new MissionThresholds()); var s=model.DiagnosticState;
            double ratio=(s.Bodies[s.Index("Array")].Position-s.Bodies[s.Index("Sun")].Position).Magnitude/(s.Bodies[s.Index("Earth")].Position-s.Bodies[s.Index("Sun")].Position).Magnitude;
            Check(new System.Collections.Generic.List<BenchmarkResult>{new BenchmarkResult("Array.radius","Co-moving radius ratio respected",ratio,0.5,1e-12,"ratio")});
        }
        [Test] public void MissionPlaybackRetainsMotionAndTransfer()
        {
            // Real recorded data must retain Sun-relative position AND the tractor's rotating
            // offset. The old inspection discarded both directions and held the pair still.
            string path=Path.Combine(Path.GetDirectoryName(DataPath),"tractor_diagnostic.csv");
            MissionRecording data; using(var reader=File.OpenText(path)) data=MissionRecording.Read(reader);
            var first=data.Frames[0]; var last=data.Frames[data.Frames.Count-1]; var quarter=data.At(Constants.JulianYear/4);
            Check(new System.Collections.Generic.List<BenchmarkResult>{
                new BenchmarkResult("Viewer.a","Recorded 100-year orbit grows",last.A>first.A?1:0,1,0,"boolean"),
                new BenchmarkResult("Viewer.motion","Quarter-year Sun-centred Earth displacement exceeds 1 AU",(quarter.Earth-first.Earth).Magnitude>Constants.AU?1:0,1,0,"boolean"),
                new BenchmarkResult("Viewer.heading","Recorded tractor heading rotates",Vector3d.Dot(first.TractorRelative.Normalized,quarter.TractorRelative.Normalized)<0.2?1:0,1,0,"boolean"),
                new BenchmarkResult("Viewer.endpoint","Exact sample endpoint retained",(data.At(first.Time).Earth-first.Earth).Magnitude,0,0,"m"),
                new BenchmarkResult("Viewer.clock","Time lookup reaches final timestamp",data.At(data.EndTime).Time,data.EndTime,0,"s")});
        }
        [Test] public void MoonBoundTenYears() => Check(ValidationSuite.Run(6,DataPath));
        [Test] public void ResetAndDeterminism() => Check(ValidationSuite.Run(7,DataPath));
        [Test] public void KeplerRoundTrip() => Check(ValidationSuite.Run(8,DataPath));
        [Test] public void ClockControls() => Check(ValidationSuite.Run(9,DataPath));
        [Test] public void PhotonBooking() => Check(ValidationSuite.Run(10,DataPath));
#if !XUNIT
        [Test,Category("Slow"),Explicit("Run CLI baseline --years 10000; not an interactive editor test")]
        public void Slow10000YearConservation() => Check(ValidationSuite.Slow(DataPath));
#endif
    }
}
