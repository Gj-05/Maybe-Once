using System;

namespace OctoEarth.Core
{
    public static class OrbitalMechanics
    {
        // IAU 2012 AU (exact SI length); IAU NSFA DE421 TDB-compatible solar GM.
        // https://ssd.jpl.nasa.gov/astro_par.html
        // https://iau-a3.gitlab.io/NSFA/NSFA_cbe.html
        public const double AU = 149597870700.0;
        public const double SolarMu = 1.32712440041e20;
        public const double Day = 86400;
        public const double JulianYear = 365.25 * Day;
        public static double CircularPeriod => 2*Math.PI*Math.Sqrt(AU*AU*AU/SolarMu);
        public static OrbitalState CircularInitial => new OrbitalState(new Vector3d(AU,0,0),new Vector3d(0,Math.Sqrt(SolarMu/AU),0));
        public static Vector3d Acceleration(Vector3d r)
        {
            double length = r.Magnitude;
            if (!(length > 0) || double.IsInfinity(length)) throw new ArgumentOutOfRangeException(nameof(r));
            return r * (-SolarMu/(length*length*length));
        }
        public static double SpecificEnergy(OrbitalState s) => 0.5*s.Velocity.SqrMagnitude-SolarMu/s.Position.Magnitude;
        public static Vector3d AngularMomentum(OrbitalState s) => Vector3d.Cross(s.Position,s.Velocity);
        public static double SemiMajorAxis(OrbitalState s) => -SolarMu/(2*SpecificEnergy(s));
        public static double Eccentricity(OrbitalState s) => (Vector3d.Cross(s.Velocity,AngularMomentum(s))/SolarMu-s.Position.Normalized).Magnitude;
        public static Vector3d AnalyticCircularPosition(double seconds)
        {
            double phase = Math.Sqrt(SolarMu/(AU*AU*AU))*seconds;
            return new Vector3d(AU*Math.Cos(phase),AU*Math.Sin(phase),0);
        }
    }
}
