using System;
namespace OctoEarth.Core
{
    public static class FiniteDiscRadiation
    {
        // Integrate a two-sided Lambertian disc's solid angle in observer-centred annuli.
        // Unlike disc-centred midpoint sampling, the on-axis near-field limit is exact.
        public static double Irradiance(Vector3d observer,Vector3d center,Vector3d normal,double area,double heat,out Vector3d meanDirection)
        {
            meanDirection=new Vector3d(); if(area<=0 || heat<=0) return 0;
            normal=normal.Normalized; var offset=observer-center; double signed=Vector3d.Dot(offset,normal),h=Math.Abs(signed);
            var sideways=offset-normal*signed; double rho=sideways.Magnitude,radius=Math.Sqrt(area/Math.PI),omega=0,vertical=0,lateral=0;
            var face=normal*(signed<0?-1:1);
            if(h==0) { if(rho>=radius) return 0; meanDirection=face/2; return heat/area; }
            if(rho<radius)
            {
                double r=radius-rho,hypot=Math.Sqrt(h*h+r*r);
                omega=2*Math.PI*r*r/(hypot*(hypot+h)); vertical=Math.PI*r*r/(h*h+r*r);
            }
            if(rho>radius*1e-12)
            {
                double lo=Math.Atan2(Math.Abs(radius-rho),h),hi=Math.Atan2(radius+rho,h),step=Math.PI/2/128;
                double os=0,vs=0,ls=0;
                for(int j=1;j<128;j++)
                {
                    double phi=j*step,s=Math.Sin(phi),theta=lo+(hi-lo)*s*s,r=h*Math.Tan(theta);
                    double alpha=Math.Acos(MissionGeometry.Clamp((r*r+rho*rho-radius*radius)/(2*r*rho)));
                    double st=Math.Sin(theta),ct=Math.Cos(theta),jac=(hi-lo)*Math.Sin(2*phi),w=(j%2==0?2:4)*jac;
                    os+=w*2*alpha*st; vs+=w*2*alpha*st*ct; ls+=w*2*Math.Sin(alpha)*st*st;
                }
                omega+=os*step/3; vertical+=vs*step/3; lateral+=ls*step/3;
            }
            if(omega<=0) return 0;
            meanDirection=(face*vertical+(rho>0?sideways*(lateral/rho):new Vector3d()))/omega;
            return heat/(2*Math.PI*area)*omega;
        }
    }
}
