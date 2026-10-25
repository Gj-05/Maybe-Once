using System;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public static class Lorentz
    {
        public static Vector3d Force(double current,Vector3d length,Vector3d field) => current*Vector3d.Cross(length,field);
        public static Vector3d LoopForce(double current,IReadOnlyList<Vector3d> vertices,Vector3d field)
        { var sum=new KahanVector(); for(int i=0;i<vertices.Count;i++) sum.Add(Force(current,vertices[(i+1)%vertices.Count]-vertices[i],field)); return sum.Value; }
        public static double LongWireField(double current,double radius) { if(!(radius>0)) throw new ArgumentOutOfRangeException(); return Constants.Mu0*current/(2*Math.PI*radius); }
    }
    public static class Beam
    {
        public static double RootMoment(double q,double l) => q*l*l/2;
        public static double TipDeflection(double q,double l,double young,double inertia) => q*Math.Pow(l,4)/(8*young*inertia);
        public static double Stress(double axial,double area,double moment,double c,double inertia) => Math.Abs(axial/area)+Math.Abs(moment*c/inertia);
        public static double EulerBuckling(double young,double inertia,double k,double length) => Math.PI*Math.PI*young*inertia/(k*k*length*length);
        public static double SelfSupportHeight(double allowable,double density,double gravity) => allowable/(density*gravity);
        public static double SafetyFactor(double allowable,double actual) => actual==0 ? double.PositiveInfinity : allowable/Math.Abs(actual);
    }
    public static class Insolation
    {
        public static double Flux(double radius) { if(!(radius>0)) throw new ArgumentOutOfRangeException(); return Constants.SolarLuminosity/(4*Math.PI*radius*radius); }
        public static double EquilibriumTemperature(double radius,double albedo=Constants.BondAlbedo) => Math.Pow(Flux(radius)*(1-albedo)/(4*Constants.StefanBoltzmann),0.25);
    }
    public struct Transfer { public double Dv1,Dv2,Seconds; }
    public static class Maneuvers
    {
        public static Transfer Hohmann(double r1,double r2,double mu=Constants.SolarMu)
        {
            double a=(r1+r2)/2;
            return new Transfer{Dv1=Math.Sqrt(mu/r1)*(Math.Sqrt(2*r2/(r1+r2))-1),Dv2=Math.Sqrt(mu/r2)*(1-Math.Sqrt(2*r1/(r1+r2))),Seconds=Math.PI*Math.Sqrt(a*a*a/mu)};
        }
        public static double LowThrustDeltaV(double r1,double r2,double mu=Constants.SolarMu) => Math.Sqrt(mu/r1)-Math.Sqrt(mu/r2);
        public static double GaussDaDt(double a,Vector3d velocity,Vector3d acceleration,double mu=Constants.SolarMu) => 2*a*a*Vector3d.Dot(velocity,acceleration)/mu;
    }
    public sealed class MissionBudget
    {
        public double Energy,DeltaV,Impulse,Force,Power,AngularMomentum,SolarDays;
        public MissionBudget(double a1,double a2,double mass,double duration)
        {
            Energy=Constants.SolarMu*mass/2*(1/a1-1/a2); DeltaV=Maneuvers.LowThrustDeltaV(a1,a2); Impulse=mass*DeltaV; Force=Impulse/duration; Power=Energy/duration;
            AngularMomentum=mass*(Math.Sqrt(Constants.SolarMu*a2)-Math.Sqrt(Constants.SolarMu*a1)); SolarDays=Energy/Constants.SolarLuminosity/Units.Day;
        }
    }
}
