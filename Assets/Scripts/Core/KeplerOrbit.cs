using System;
namespace OctoEarth.Core
{
    public struct OrbitalElements
    {
        public double A,E,I,Node,Periapsis,MeanAnomaly;
    }
    public static class KeplerOrbit
    {
        public static double Period(double a,double mu) => 2*Math.PI*Math.Sqrt(a*a*a/mu);
        public static double Energy(Vector3d r,Vector3d v,double mu) => v.SqrMagnitude/2-mu/r.Magnitude;
        public static Vector3d EccentricityVector(Vector3d r,Vector3d v,double mu) => Vector3d.Cross(v,Vector3d.Cross(r,v))/mu-r.Normalized;
        public static double SemiMajorAxis(Vector3d r,Vector3d v,double mu) => -mu/(2*Energy(r,v,mu));
        public static OrbitalState FromElements(OrbitalElements el,double mu,double time=0)
        {
            if(!(el.A>0) || el.E<0 || el.E>=1) throw new ArgumentException("Bound elliptic elements required");
            double mean=el.MeanAnomaly+Math.Sqrt(mu/(el.A*el.A*el.A))*time;
            mean=Math.IEEERemainder(mean,2*Math.PI); double eccentric=mean;
            for(int k=0;k<30;k++) { double d=(eccentric-el.E*Math.Sin(eccentric)-mean)/(1-el.E*Math.Cos(eccentric)); eccentric-=d; if(Math.Abs(d)<1e-15) break; }
            double c=Math.Cos(eccentric),s=Math.Sin(eccentric),b=Math.Sqrt(1-el.E*el.E), n=Math.Sqrt(mu/(el.A*el.A*el.A));
            return new OrbitalState(Rotate(new Vector3d(el.A*(c-el.E),el.A*b*s,0),el),Rotate(new Vector3d(-el.A*n*s/(1-el.E*c),el.A*n*b*c/(1-el.E*c),0),el),time);
        }
        static Vector3d Rotate(Vector3d p,OrbitalElements e)
        {
            double co=Math.Cos(e.Node),so=Math.Sin(e.Node),cw=Math.Cos(e.Periapsis),sw=Math.Sin(e.Periapsis),ci=Math.Cos(e.I),si=Math.Sin(e.I);
            return new Vector3d((co*cw-so*sw*ci)*p.X+(-co*sw-so*cw*ci)*p.Y,(so*cw+co*sw*ci)*p.X+(-so*sw+co*cw*ci)*p.Y,sw*si*p.X+cw*si*p.Y);
        }
        public static OrbitalElements ToElements(Vector3d r,Vector3d v,double mu)
        {
            var h=Vector3d.Cross(r,v); var n=Vector3d.Cross(new Vector3d(0,0,1),h); var ev=EccentricityVector(r,v,mu); double e=ev.Magnitude;
            double inc=Math.Acos(Clamp(h.Z/h.Magnitude)); double node=n.Magnitude>1e-15 ? Math.Atan2(n.Y,n.X) : 0;
            double peri=0,nu;
            if(e>1e-12)
            {
                peri=n.Magnitude>1e-15 ? Math.Atan2(Vector3d.Dot(Vector3d.Cross(n,ev),h)/(n.Magnitude*e*h.Magnitude),Vector3d.Dot(n,ev)/(n.Magnitude*e)) : Math.Atan2(ev.Y,ev.X);
                nu=Math.Atan2(Vector3d.Dot(Vector3d.Cross(ev,r),h)/(e*r.Magnitude*h.Magnitude),Vector3d.Dot(ev,r)/(e*r.Magnitude));
            }
            else nu=n.Magnitude>1e-15 ? Math.Atan2(Vector3d.Dot(Vector3d.Cross(n,r),h)/(n.Magnitude*r.Magnitude*h.Magnitude),Vector3d.Dot(n,r)/(n.Magnitude*r.Magnitude)) : Math.Atan2(r.Y,r.X);
            double E=2*Math.Atan2(Math.Sqrt(1-e)*Math.Sin(nu/2),Math.Sqrt(1+e)*Math.Cos(nu/2));
            return new OrbitalElements{A=SemiMajorAxis(r,v,mu),E=e,I=inc,Node=node,Periapsis=peri,MeanAnomaly=E-e*Math.Sin(E)};
        }
        static double Clamp(double x) => Math.Max(-1,Math.Min(1,x));
        public static SystemState Binary(double a,double e=0,double primaryMu=Constants.SolarMu,double secondaryMass=1)
        {
            double m=primaryMu/Constants.G; var orbit=FromElements(new OrbitalElements{A=a,E=e},primaryMu+Constants.G*secondaryMass);
            var s=new SystemState(new Body("Sun",m,new Vector3d(),new Vector3d()),new Body("Earth",secondaryMass,orbit.Position,orbit.Velocity)); s.Barycenter(); return s;
        }
    }
}
