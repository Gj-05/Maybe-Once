using System;
namespace OctoEarth.Core
{
    public static class PlumeSteering
    {
        // Rotate the symmetric pair about the REQUIRED force axis. Net force, cant, ve and
        // power are unchanged. If no orientation clears the protected targets, retain the
        // least-overlapping candidate and let the unchanged monitors report failure.
        public static Vector3d Select(SystemState s,int source,Vector3d axis,Vector3d hint,MissionConfiguration c)
        {
            axis=axis.Normalized; hint=(hint-axis*Vector3d.Dot(axis,hint)).Normalized;
            if(hint.Magnitude<0.1) hint=Vector3d.Cross(axis,new Vector3d(1,0,0)).Normalized;
            if(hint.Magnitude<0.1) hint=Vector3d.Cross(axis,new Vector3d(0,1,0)).Normalized;
            var other=Vector3d.Cross(axis,hint); var origin=s.Bodies[source]; var sun=s.Bodies[s.Index("Sun")];
            double co=Math.Cos(Units.Radians(c.CantDegrees)),sn=Math.Sin(Units.Radians(c.CantDegrees));
            Span<double> ax=stackalloc double[5],hx=stackalloc double[5],hy=stackalloc double[5],limit=stackalloc double[5]; int n=0;
            foreach(var body in s.Bodies)
            {
                if(body==origin || (body.Name!="Earth" && body.Name!="Moon" && body.Name!="Tractor" && body.Name!="Array" && body.Name!="Mirror")) continue;
                var offset=body.Position-origin.Position; double distance=offset.Magnitude; var q=offset/distance;
                double radius=body.Name=="Earth"?3*Constants.EarthRadius:body.Name=="Moon"?MissionGeometry.MoonRadius:body.Name=="Tractor"?Math.Pow(3*body.Mass/(4*Math.PI*c.RockDensity),1.0/3):Math.Sqrt(body.Mass/(body.Name=="Array"?c.ArrayDensity:c.MirrorDensity)/Math.PI);
                ax[n]=-co*Vector3d.Dot(axis,q); hx[n]=sn*Vector3d.Dot(hint,q); hy[n]=sn*Vector3d.Dot(other,q);
                limit[n]=Math.Cos(Math.Asin(Math.Min(1,radius/distance))+Units.Radians(c.PlumeHalfAngleDegrees)); n++;
            }
            var radial=(origin.Position-sun.Position).Normalized;
            double baseRadial=Vector3d.Dot(origin.Velocity-sun.Velocity,radial)/c.ExhaustSpeed-co*Vector3d.Dot(axis,radial);
            double rx=sn*Vector3d.Dot(hint,radial),ry=sn*Vector3d.Dot(other,radial);
            double best=double.PositiveInfinity,bestAngle=0;
            for(int k=0;k<32;k++)
            {
                double angle=k*Math.PI/32,x=Math.Cos(angle),y=Math.Sin(angle),collision=-2;
                for(int j=0;j<n;j++) collision=Math.Max(collision,ax[j]+Math.Abs(hx[j]*x+hy[j]*y)-limit[j]);
                double inward=Math.Abs(rx*x+ry*y)-baseRadial;
                if(k==0 && collision<=0 && inward<0) return hint;
                double score=collision>0?1+collision:Math.Max(collision,Math.Max(0,inward));
                if(score<best) {best=score; bestAngle=angle;}
            }
            return hint*Math.Cos(bestAngle)+other*Math.Sin(bestAngle);
        }
    }
}
