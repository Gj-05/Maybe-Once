namespace OctoEarth.Core
{
    public struct OrbitalState
    {
        public Vector3d Position { get; }
        public Vector3d Velocity { get; }
        public double ElapsedSeconds { get; }
        public OrbitalState(Vector3d position, Vector3d velocity, double elapsedSeconds = 0)
        { Position = position; Velocity = velocity; ElapsedSeconds = elapsedSeconds; }
    }
}
