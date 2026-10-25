using System;
using System.Collections.Generic;
using UnityEngine;
using OctoEarth.Core;

namespace OctoEarth
{
    public sealed class OrbitController : MonoBehaviour
    {
        public Transform earth;
        public LineRenderer trajectory;
        public MoonInset view;
        public bool useNBody = true;
        public SimulationRunner NBody { get; private set; }
        readonly SimulationClock clock = new SimulationClock();
        public OrbitalState DisplayState
        {
            get
            {
                if(!useNBody || NBody==null) return Simulation.State;
                var s=NBody.State; var e=s.Bodies[s.Index("Earth")]; var sun=s.Bodies[s.Index("Sun")];
                return new OrbitalState(e.Position-sun.Position,e.Velocity-sun.Velocity,s.Time);
            }
        }
        [Range(60,86400)] public double stepSeconds = 3600;
        public double simulatedSecondsPerRealSecond = 86400;
        public int maxStepsPerFrame = 2000;
        public OrbitSimulation Simulation { get; private set; }
        public bool Running => clock.Running;
        public bool Started => clock.Started;
        public double PendingSeconds => clock.PendingSeconds;
        public double MaxEnergyDrift { get; private set; }
        public double MaxMomentumDrift { get; private set; }
        public double MaxRadiusDrift { get; private set; }
        double previousWallTime;
        double initialEnergy, initialMomentum;
        readonly Queue<Vector3> trail = new Queue<Vector3>();
        Vector3[] points = new Vector3[4096];
        const int TrailCapacity = 4096;
        int samples;
        public event Action StateReset;
        void Awake() { Application.runInBackground = true; ResetSimulation(); }
        public void StartSimulation() { clock.Start(); previousWallTime = Time.realtimeSinceStartupAsDouble; }
        public void PauseSimulation() { clock.Pause(); }
        public void ResumeSimulation() { clock.Resume(); previousWallTime = Time.realtimeSinceStartupAsDouble; }
        public void ResetSimulation()
        {
            clock.Reset(); samples = 0;
            Simulation = new OrbitSimulation(stepSeconds);
            NBody = useNBody ? new SimulationRunner(InitialConditions.SolarSystem(System.IO.Path.Combine(Application.dataPath,"Data/solar_j2000.csv")),stepSeconds) : null;
            initialEnergy = OrbitalMechanics.SpecificEnergy(Simulation.State);
            initialMomentum = OrbitalMechanics.AngularMomentum(Simulation.State).Magnitude;
            MaxEnergyDrift = MaxMomentumDrift = MaxRadiusDrift = 0;
            trail.Clear(); trail.Enqueue(OrbitalCoordinates.ToUnity(DisplayState.Position));
            previousWallTime = Time.realtimeSinceStartupAsDouble;
            Draw(); StateReset?.Invoke();
        }
        public void ConfigureStep(double seconds) { stepSeconds = seconds; ResetSimulation(); }
        void Update()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            double wallSeconds = Math.Max(0,now-previousWallTime); previousWallTime = now;
            if (!Running) return;
            clock.Advance(wallSeconds,simulatedSecondsPerRealSecond,stepSeconds,maxStepsPerFrame,()=>
            {
                if(useNBody) NBody.Step(); else Simulation.Step();
                var s = DisplayState;
                MaxEnergyDrift = useNBody ? NBody.Ledger.MaxEnergyError : Math.Max(MaxEnergyDrift,Math.Abs((OrbitalMechanics.SpecificEnergy(s)-initialEnergy)/initialEnergy));
                MaxMomentumDrift = useNBody ? NBody.Ledger.MaxAngularMomentumError : Math.Max(MaxMomentumDrift,Math.Abs((OrbitalMechanics.AngularMomentum(s).Magnitude-initialMomentum)/initialMomentum));
                MaxRadiusDrift = Math.Max(MaxRadiusDrift,Math.Abs(s.Position.Magnitude/OrbitalMechanics.AU-1));
                // Sample about every simulated day; retain the most recent 4096 points.
                samples++;
                if (samples >= Math.Max(1,(int)(OrbitalMechanics.Day/Simulation.StepSeconds)))
                {
                    samples = 0; trail.Enqueue(OrbitalCoordinates.ToUnity(s.Position));
                    if (trail.Count > TrailCapacity) trail.Dequeue();
                }
            });
            Draw();
        }
        void Draw()
        {
            if (earth != null && (view==null || !view.IsEarthMoonView)) earth.position = OrbitalCoordinates.ToUnity(DisplayState.Position);
            if (trajectory == null) return;
            int i = 0; foreach (var p in trail) points[i++] = p;
            trajectory.positionCount = i+1;
            // Include current state without modifying the historical sample queue.
            for (int j=0;j<i;j++) trajectory.SetPosition(j,points[j]);
            trajectory.SetPosition(i,OrbitalCoordinates.ToUnity(DisplayState.Position));
        }
    }
}
