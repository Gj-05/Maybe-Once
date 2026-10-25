using System;
namespace OctoEarth.Core
{
    public sealed class MissionTargets
    {
        readonly MissionConfiguration c;
        readonly TractorHoverDrive tractor;
        readonly int earth,sun,lunar,tractorIndex,array,mirror;
        readonly GravityModel gravity=new GravityModel();
        readonly Vector3d[] accelerations;
        double phase,phaseStart,timeStart,startN;
        public double CutoffTime=double.NaN;
        public bool TractorUnconstrained => !double.IsNaN(CutoffTime) && (c.PostTargetPolicy=="Coast" || c.PostTargetPolicy=="Depart");
        public MissionTargets(SystemState s,MissionConfiguration config)
        {
            c=config; earth=s.Index("Earth"); sun=s.Index("Sun"); lunar=s.Index("Moon"); tractorIndex=s.Index("Tractor"); array=s.Index("Array"); mirror=s.Index("Mirror");
            tractor=new TractorHoverDrive(s,c.Tractor()); accelerations=new Vector3d[s.Bodies.Length]; phase=Units.Radians(c.ArrayPhaseDegrees);
        }
        public int[] Controlled => new[]{tractorIndex,array,mirror};
        public void Begin(SystemState s) { phaseStart=phase; timeStart=s.Time; double r=(s.Bodies[earth].Position-s.Bodies[sun].Position).Magnitude; startN=Math.Sqrt(Constants.SolarMu/(r*r*r)); }
        public void Finish(SystemState s)
        {
            if(c.ArrayCoMoving) { double r=(s.Bodies[earth].Position-s.Bodies[sun].Position).Magnitude; double n=Math.Sqrt(Constants.SolarMu/(r*r*r)); phase=phaseStart+(s.Time-timeStart)*(startN+n)/2; }
        }
        public void Get(SystemState s,int index,out Vector3d position,out Vector3d velocity)
        {
            if(index==tractorIndex && double.IsNaN(CutoffTime)) { tractor.Target(s,out position,out velocity,out _,out _); return; }
            var e=s.Bodies[earth]; var sol=s.Bodies[sun]; var relative=e.Position-sol.Position; var rv=e.Velocity-sol.Velocity;
            if(index==tractorIndex)
            {
                gravity.Accelerations(s,accelerations); var parkMoon=s.Bodies[lunar]; double parkMass=e.Mass+parkMoon.Mass;
                var parkV=(e.Velocity*e.Mass+parkMoon.Velocity*parkMoon.Mass)/parkMass-sol.Velocity;
                var parkA=(accelerations[earth]*e.Mass+accelerations[lunar]*parkMoon.Mass)/parkMass-accelerations[sun];
                var parkU=parkV.Normalized; var parkUDot=(parkA-parkU*Vector3d.Dot(parkA,parkU))/parkV.Magnitude;
                var radial=relative.Normalized; var parkRadialDot=(rv-radial*Vector3d.Dot(rv,radial))/relative.Magnitude;
                if(c.TractorSunwardBiasDegrees!=0)
                {
                    double beta=Units.Radians(c.TractorSunwardBiasDegrees); var initial=parkU*Math.Cos(beta)-radial*Math.Sin(beta);
                    var derivative=parkUDot*Math.Cos(beta)-parkRadialDot*Math.Sin(beta); parkU=initial.Normalized;
                    parkUDot=(derivative-parkU*Vector3d.Dot(derivative,parkU))/initial.Magnitude;
                }
                // A real tractor cannot switch gravity off at cutoff. Rotate the hover site over five
                // days to a radial parking direction and continue powered hover; book every impulse.
                double duration=5*Units.Day,x=Math.Max(0,Math.Min(1,(s.Time-CutoffTime)/duration));
                double angle=Math.PI/2*x*x*(3-2*x),omega=Math.PI/2*6*x*(1-x)/duration;
                var q=parkU*Math.Cos(angle)+radial*Math.Sin(angle);
                var qdot=parkUDot*Math.Cos(angle)+parkRadialDot*Math.Sin(angle)+(radial*Math.Cos(angle)-parkU*Math.Sin(angle))*omega;
                var direction=q.Normalized; qdot=(qdot-direction*Vector3d.Dot(qdot,direction))/q.Magnitude;
                position=e.Position+direction*c.Distance; velocity=e.Velocity+qdot*c.Distance; return;
            }
            if(index==array)
            {
                if(!c.ArrayCoMoving)
                {
                    var orbit=KeplerOrbit.FromElements(new OrbitalElements{A=c.ArrayRadiusAU*Constants.AU,I=Units.Radians(c.ArrayInclinationDegrees),MeanAnomaly=Units.Radians(c.ArrayPhaseDegrees)},Constants.SolarMu,s.Time);
                    position=sol.Position+orbit.Position; velocity=sol.Velocity+orbit.Velocity; return;
                }
                if(c.ArrayTracksEarthLongitude)
                {
                    double rr=relative.Magnitude,rd=Vector3d.Dot(rv,relative.Normalized);
                    double longitude=Math.Atan2(relative.Y,relative.X)+Units.Radians(c.ArrayPhaseDegrees);
                    double omega=(relative.X*rv.Y-relative.Y*rv.X)/(relative.X*relative.X+relative.Y*relative.Y);
                    double inclination=Units.Radians(c.ArrayInclinationDegrees),radius=rr*c.ArrayRadiusAU;
                    var a=new Vector3d(Math.Cos(longitude),Math.Sin(longitude)*Math.Cos(inclination),Math.Sin(longitude)*Math.Sin(inclination));
                    var b=new Vector3d(-Math.Sin(longitude),Math.Cos(longitude)*Math.Cos(inclination),Math.Cos(longitude)*Math.Sin(inclination));
                    position=sol.Position+a*radius; velocity=sol.Velocity+a*(rd*c.ArrayRadiusAU)+b*(radius*omega); return;
                }
                double r=relative.Magnitude,radial=Vector3d.Dot(rv,relative.Normalized),n=Math.Sqrt(Constants.SolarMu/(r*r*r));
                double theta=phaseStart+(s.Time-timeStart)*(startN+n)/2,inc=Units.Radians(c.ArrayInclinationDegrees);
                var axis=new Vector3d(Math.Cos(theta),Math.Sin(theta)*Math.Cos(inc),Math.Sin(theta)*Math.Sin(inc));
                var tangent=new Vector3d(-Math.Sin(theta),Math.Cos(theta)*Math.Cos(inc),Math.Cos(theta)*Math.Sin(inc));
                position=sol.Position+axis*r; velocity=sol.Velocity+axis*radial+tangent*(r*n); return;
            }
            gravity.Accelerations(s,accelerations);
            var moon=s.Bodies[lunar]; double mass=e.Mass+moon.Mass;
            var bv=(e.Velocity*e.Mass+moon.Velocity*moon.Mass)/mass-sol.Velocity;
            var ba=(accelerations[earth]*e.Mass+accelerations[lunar]*moon.Mass)/mass-accelerations[sun];
            var u=bv.Normalized; var udot=(ba-u*Vector3d.Dot(ba,u))/bv.Magnitude;
            var radialUnit=relative.Normalized; var radialDot=(rv-radialUnit*Vector3d.Dot(rv,radialUnit))/relative.Magnitude;
            double psi=Units.Radians(c.MirrorDeflectionDegrees);
            position=e.Position-(radialUnit*Math.Cos(psi)+u*Math.Sin(psi))*c.MirrorDistance;
            velocity=e.Velocity-(radialDot*Math.Cos(psi)+udot*Math.Sin(psi))*c.MirrorDistance;
        }
    }
}
