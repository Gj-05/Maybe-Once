using System;

namespace OctoEarth.Core
{
    [Serializable]
    public struct Vector3d
    {
        public double X, Y, Z;
        public Vector3d(double x, double y, double z) { X = x; Y = y; Z = z; }
        public double SqrMagnitude => Dot(this, this);
        public double Magnitude => Math.Sqrt(SqrMagnitude);
        public Vector3d Normalized => Magnitude > 0 ? this / Magnitude : new Vector3d();
        public static Vector3d operator +(Vector3d a, Vector3d b) => new Vector3d(a.X+b.X,a.Y+b.Y,a.Z+b.Z);
        public static Vector3d operator -(Vector3d a, Vector3d b) => new Vector3d(a.X-b.X,a.Y-b.Y,a.Z-b.Z);
        public static Vector3d operator *(Vector3d a, double s) => new Vector3d(a.X*s,a.Y*s,a.Z*s);
        public static Vector3d operator *(double s, Vector3d a) => a*s;
        public static Vector3d operator /(Vector3d a, double s) => a*(1/s);
        public static double Dot(Vector3d a, Vector3d b) => a.X*b.X+a.Y*b.Y+a.Z*b.Z;
        public static Vector3d Cross(Vector3d a, Vector3d b) => new Vector3d(a.Y*b.Z-a.Z*b.Y,a.Z*b.X-a.X*b.Z,a.X*b.Y-a.Y*b.X);
    }
}
