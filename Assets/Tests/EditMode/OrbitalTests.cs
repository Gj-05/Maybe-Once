using System;
using NUnit.Framework;
using OctoEarth.Core;

namespace OctoEarth.Tests
{
    public class OrbitalTests
    {
        static OrbitSimulation Run(double step, int count)
        { var sim = new OrbitSimulation(step); for(int i=0;i<count;i++) sim.Step(); return sim; }
        [Test] public void CircularInitialization()
        {
            var s = OrbitalMechanics.CircularInitial;
            Assert.That(s.Position.X,Is.EqualTo(OrbitalMechanics.AU));
            Assert.That(s.Position.Y,Is.Zero); Assert.That(s.Position.Z,Is.Zero);
            Assert.That(s.Velocity.Magnitude,Is.EqualTo(Math.Sqrt(OrbitalMechanics.SolarMu/OrbitalMechanics.AU)).Within(1e-10));
            Assert.That(Vector3d.Dot(s.Position,s.Velocity),Is.Zero);
            Assert.That(OrbitalMechanics.Eccentricity(s),Is.LessThan(1e-14));
            Assert.That(OrbitalMechanics.SemiMajorAxis(s)/OrbitalMechanics.AU,Is.EqualTo(1).Within(1e-14));
            TestContext.WriteLine($"v0={s.Velocity.Magnitude:R} m/s; e={OrbitalMechanics.Eccentricity(s):R}");
        }
        [Test] public void SiderealYearPeriod()
        {
            var sim = new OrbitSimulation(); double previousAngle=0, angle=0;
            double previousTime=0;
            for(int i=0;i<10000 && angle<2*Math.PI;i++)
            {
                var before = sim.State; sim.Step();
                previousAngle = angle; previousTime = before.ElapsedSeconds;
                double delta = Math.Atan2(sim.State.Position.Y,sim.State.Position.X)-Math.Atan2(before.Position.Y,before.Position.X);
                if(delta < -Math.PI) delta += 2*Math.PI;
                angle += delta;
            }
            double measured = previousTime+(2*Math.PI-previousAngle)/(angle-previousAngle)*sim.StepSeconds;
            double relative = Math.Abs(measured/OrbitalMechanics.CircularPeriod-1);
            TestContext.WriteLine($"measured period={measured/OrbitalMechanics.Day:R} d; analytic={OrbitalMechanics.CircularPeriod/OrbitalMechanics.Day:R} d; relative error={relative:R}");
            // Verlet O(dt²): (n dt)^2=5.14e-7; conservative 1e-6 period bound.
            Assert.That(relative,Is.LessThan(1e-6));
            // Model excludes Earth's mass, perturbations and actual eccentricity.
            Assert.That(Math.Abs(measured/OrbitalMechanics.Day-365.25636),Is.LessThan(0.01));
        }
        static void Drift(out double energy,out double momentum,out double radius)
        {
            var sim = new OrbitSimulation();
            double e0 = OrbitalMechanics.SpecificEnergy(sim.State), h0 = OrbitalMechanics.AngularMomentum(sim.State).Magnitude;
            energy=momentum=radius=0;
            int count=(int)Math.Ceiling(10*OrbitalMechanics.CircularPeriod/sim.StepSeconds);
            for(int i=0;i<count;i++)
            {
                sim.Step(); var s=sim.State;
                energy=Math.Max(energy,Math.Abs((OrbitalMechanics.SpecificEnergy(s)-e0)/e0));
                momentum=Math.Max(momentum,Math.Abs((OrbitalMechanics.AngularMomentum(s).Magnitude-h0)/h0));
                radius=Math.Max(radius,Math.Abs(s.Position.Magnitude/OrbitalMechanics.AU-1));
            }
            TestContext.WriteLine($"10 orbits: max relative energy={energy:R}; max relative h={momentum:R}; max radial error AU={radius:R}");
        }
        [Test] public void EnergyConservation() { Drift(out var e,out _,out _); Assert.That(e,Is.LessThan(1e-10)); }
        [Test] public void AngularMomentumConservation() { Drift(out _,out var h,out _); Assert.That(h,Is.LessThan(1e-11)); }
        [Test] public void ZeroThrustStaysNearOneAU() { Drift(out _,out _,out var r); Assert.That(r,Is.LessThan(1e-6)); }
        [Test] public void TimestepConvergesAgainstAnalyticOrbit()
        {
            // Identical endpoint, all steps fixed, no final partial step.
            const int coarseSteps=8766;
            var coarse=Run(3600,coarseSteps); var fine=Run(1800,coarseSteps*2);
            var exact=OrbitalMechanics.AnalyticCircularPosition(coarse.State.ElapsedSeconds);
            double c=(coarse.State.Position-exact).Magnitude/OrbitalMechanics.AU;
            double f=(fine.State.Position-exact).Magnitude/OrbitalMechanics.AU;
            TestContext.WriteLine($"analytic endpoint error: dt3600={c:R} AU; dt1800={f:R} AU; ratio={c/f:R}");
            Assert.That(c,Is.LessThan(2e-6)); Assert.That(f,Is.LessThan(5e-7));
            Assert.That(c/f,Is.InRange(3.8,4.2));
        }
        [Test] public void ResetRestoresExactState()
        {
            var sim=Run(3600,1000); var original=sim.InitialState; sim.Reset();
            Assert.That(sim.State.Position.X,Is.EqualTo(original.Position.X));
            Assert.That(sim.State.Position.Y,Is.EqualTo(original.Position.Y));
            Assert.That(sim.State.Position.Z,Is.EqualTo(original.Position.Z));
            Assert.That(sim.State.Velocity.X,Is.EqualTo(original.Velocity.X));
            Assert.That(sim.State.Velocity.Y,Is.EqualTo(original.Velocity.Y));
            Assert.That(sim.State.Velocity.Z,Is.EqualTo(original.Velocity.Z));
            Assert.That(sim.State.ElapsedSeconds,Is.EqualTo(original.ElapsedSeconds));
        }
        [Test] public void VectorOperationsAndInvalidSteps()
        {
            var x=new Vector3d(1,0,0); var y=new Vector3d(0,1,0);
            Assert.That(Vector3d.Cross(x,y).Z,Is.EqualTo(1));
            Assert.That((x+y).Magnitude,Is.EqualTo(Math.Sqrt(2)).Within(1e-15));
            Assert.That((3*x-x).X,Is.EqualTo(2));
            Assert.That(new Vector3d(3,4,0).Normalized.Magnitude,Is.EqualTo(1).Within(1e-15));
            Assert.Throws<ArgumentOutOfRangeException>(()=>new OrbitSimulation(0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>new OrbitSimulation(double.NaN));
        }
    }
}
