using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
namespace OctoEarth.Core
{
    public struct MissionFrame
    {
        public double Time,A,Thrust,Ejected,Flags,MoonDistance,RelativeP,RelativeL,RelativeE;
        public Vector3d MoonRelative,MoonVelocity;
        public bool HasMoon;
        public Vector3d Earth,EarthVelocity,TractorRelative,TractorRelativeVelocity;
    }
    // Sun-relative double coordinates, parsed before any Unity float conversion.
    public sealed class MissionRecording
    {
        public readonly List<MissionFrame> Frames=new List<MissionFrame>();
        public double EndTime => Frames[Frames.Count-1].Time;
        public static MissionRecording Read(TextReader reader)
        {
            var result=new MissionRecording(); var first=reader.ReadLine(); if(first==null) throw new FormatException("Empty mission CSV");
            var header=first.Split(',');
            int ti=Column(header,"time_s"),ai=Column(header,"a_m"),fi=Column(header,"thrust_N"),ei=Column(header,"ejected_kg"),ci=Column(header,"flag_count");
            int sun=Column(header,"Sun_x"),earth=Column(header,"Earth_x"),tractor=Column(header,"Tractor_x");
            int moon=Array.IndexOf(header,"Moon_x");
            string line;
            while((line=reader.ReadLine())!=null)
            {
                if(string.IsNullOrWhiteSpace(line)) continue;
                var f=line.Split(','); double time=Number(f[ti]);
                if(result.Frames.Count>0 && time<result.EndTime) throw new FormatException("Mission timestamps decrease");
                if(result.Frames.Count>0 && time==result.EndTime) continue;
                var sp=Vector(f,sun); var ep=Vector(f,earth); var tp=Vector(f,tractor);
                var sv=Vector(f,sun+3); var ev=Vector(f,earth+3); var tv=Vector(f,tractor+3);
                result.Frames.Add(new MissionFrame{Time=time,A=Number(f[ai]),Thrust=Number(f[fi]),Ejected=Number(f[ei]),Flags=Number(f[ci]),Earth=ep-sp,EarthVelocity=ev-sv,TractorRelative=tp-ep,TractorRelativeVelocity=tv-ev});
                var frame=result.Frames[result.Frames.Count-1];
                if(moon>=0) {frame.HasMoon=true; frame.MoonRelative=Vector(f,moon)-ep; frame.MoonVelocity=Vector(f,moon+3)-ev; frame.MoonDistance=frame.MoonRelative.Magnitude;}
                int p=Array.IndexOf(header,"relative_P"),l=Array.IndexOf(header,"relative_L"),e=Array.IndexOf(header,"relative_E");
                frame.RelativeP=p<0?0:Number(f[p]); frame.RelativeL=l<0?0:Number(f[l]); frame.RelativeE=e<0?0:Number(f[e]);
                result.Frames[result.Frames.Count-1]=frame;
            }
            if(result.Frames.Count==0) throw new FormatException("No mission samples"); return result;
        }
        static int Column(string[] header,string name) { int k=Array.IndexOf(header,name); if(k<0) throw new FormatException("Missing column "+name); return k; }
        static double Number(string value) => double.Parse(value,CultureInfo.InvariantCulture);
        static Vector3d Vector(string[] fields,int offset) => new Vector3d(Number(fields[offset]),Number(fields[offset+1]),Number(fields[offset+2]));
        public int IndexAt(double time)
        {
            int lo=0,hi=Frames.Count-1; while(lo<hi) { int mid=(lo+hi+1)/2; if(Frames[mid].Time<=time) lo=mid; else hi=mid-1; } return lo;
        }
        public MissionFrame At(double time)
        {
            time=Math.Max(Frames[0].Time,Math.Min(EndTime,time)); int k=IndexAt(time); var a=Frames[k];
            if(k==Frames.Count-1 || a.Time==time) return a;
            var b=Frames[k+1]; double dt=b.Time-a.Time,u=(time-a.Time)/dt;
            // Hermite interpolation is presentation only. Diagnostics retain exact sample values.
            return new MissionFrame{Time=time,A=a.A+(b.A-a.A)*u,Thrust=a.Thrust+(b.Thrust-a.Thrust)*u,Ejected=a.Ejected+(b.Ejected-a.Ejected)*u,Flags=a.Flags,
                HasMoon=a.HasMoon, MoonRelative=Hermite(a.MoonRelative,a.MoonVelocity,b.MoonRelative,b.MoonVelocity,dt,u),MoonVelocity=a.MoonVelocity*(1-u)+b.MoonVelocity*u,MoonDistance=a.MoonDistance+(b.MoonDistance-a.MoonDistance)*u,
                RelativeP=a.RelativeP,RelativeL=a.RelativeL,RelativeE=a.RelativeE,
                Earth=Hermite(a.Earth,a.EarthVelocity,b.Earth,b.EarthVelocity,dt,u),TractorRelative=Hermite(a.TractorRelative,a.TractorRelativeVelocity,b.TractorRelative,b.TractorRelativeVelocity,dt,u)};
        }
        static Vector3d Hermite(Vector3d a,Vector3d va,Vector3d b,Vector3d vb,double dt,double t)
        { double t2=t*t,t3=t2*t; return a*(2*t3-3*t2+1)+va*(dt*(t3-2*t2+t))+b*(-2*t3+3*t2)+vb*(dt*(t3-t2)); }
    }
}
