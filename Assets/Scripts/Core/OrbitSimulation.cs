using System;

namespace OctoEarth.Core
{
    // No Unity dependency. Fixed-Sun heliocentric test particle; no perturbations or thrust.
    public sealed class OrbitSimulation
    {
        public OrbitalState State { get; private set; }
        public OrbitalState InitialState { get; }
        public double StepSeconds { get; }
        public OrbitSimulation(double stepSeconds = 3600) : this(OrbitalMechanics.CircularInitial,stepSeconds) { }
        public OrbitSimulation(OrbitalState initial, double stepSeconds)
        {
            if (!(stepSeconds > 0) || double.IsInfinity(stepSeconds)) throw new ArgumentOutOfRangeException(nameof(stepSeconds));
            OrbitalMechanics.Acceleration(initial.Position);
            InitialState = initial; State = initial; StepSeconds = stepSeconds;
        }
        public void Reset() { State = InitialState; }
        public void Step()
        {
            var a0 = OrbitalMechanics.Acceleration(State.Position);
            var r1 = State.Position + State.Velocity*StepSeconds + a0*(0.5*StepSeconds*StepSeconds);
            var v1 = State.Velocity + (a0+OrbitalMechanics.Acceleration(r1))*(0.5*StepSeconds);
            State = new OrbitalState(r1,v1,State.ElapsedSeconds+StepSeconds);
        }
    }
}
