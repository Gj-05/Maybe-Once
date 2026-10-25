using System;
namespace OctoEarth.Core
{
    public static class Constants
    {
        public const double G = 6.67430e-11; // CODATA 2018, https://ssd.jpl.nasa.gov/astro_par.html
        public const double SolarMu = 1.32712440018e20; // Supplied R2 convention; differs from DE440/JPL current value.
        public const double EarthMu = 3.986004418e14; // R2, conventional geocentric GM (IERS).
        public const double MoonMu = 4.9028e12; // R2 rounded lunar GM, JPL astro_par.html.
        public const double SolarMass = 1.98841e30; // R2 rounded solar mass, kg.
        public const double EarthMass = 5.9722e24; // R2 rounded Earth mass, kg.
        public const double MoonMass = 7.342e22; // R2 rounded Moon mass, kg.
        public const double PairMass = 6.045e24; // R2 rounded mission sizing mass; not exactly EarthMass+MoonMass.
        public const double AU = 149597870700; // IAU 2012 exact SI AU, JPL astro_par.html.
        public const double EarthRadius = 6.371e6; // R2 mean Earth radius; JPL phys_par.html.
        public const double MoonDistance = 3.844e8; // R2 idealized mean lunar orbital radius (not an ephemeris).
        public const double C = 299792458; // Exact SI speed of light.
        public const double SolarLuminosity = 3.828e26; // IAU 2015 nominal luminosity, R2.
        public const double StefanBoltzmann = 5.670374419e-8; // CODATA 2018, R2.
        public const double BondAlbedo = 0.306; // R2 Earth Bond albedo model parameter.
        public const double JulianYear = 31557600; // IAU Julian year 365.25 SI days.
        public const double Mu0 = 4*Math.PI*1e-7; // R2 conventional permeability; post-2019 SI this is an approximation.
    }
    public static class Units
    {
        public const double Day = 86400; // SI convention, JPL astro_par.html.
        public static double Radians(double degrees) => degrees*Math.PI/180;
    }
    public struct KahanSum
    {
        double sum, correction;
        public double Value => sum;
        public void Add(double value) { double y=value-correction; double t=sum+y; correction=(t-sum)-y; sum=t; }
    }
    public struct KahanVector
    {
        KahanSum x,y,z;
        public Vector3d Value => new Vector3d(x.Value,y.Value,z.Value);
        public void Add(Vector3d v) { x.Add(v.X); y.Add(v.Y); z.Add(v.Z); }
    }
}
