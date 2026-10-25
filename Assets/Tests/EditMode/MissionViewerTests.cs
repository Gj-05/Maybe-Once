#if !XUNIT
using NUnit.Framework;
using UnityEngine;
using OctoEarth.Core;
namespace OctoEarth.Tests
{
    public sealed class MissionViewerTests
    {
        static GameObject Child(GameObject root,string name) { var go=new GameObject(name); go.transform.SetParent(root.transform); return go; }
        [Test] public void SunCentredPlaybackMovesEarthAndRetainsTimeAcrossViews()
        {
            var fixture=new GameObject("Mission viewer regression fixture");
            try
            {
                var c=Child(fixture,"Test Simulation").AddComponent<OrbitController>();
                var earth=Child(fixture,"Test Earth"); c.earth=earth.transform; earth.transform.localScale=Vector3.one*2;
                var camera=Child(fixture,"Test Camera"); camera.AddComponent<Camera>(); var orbitCamera=camera.AddComponent<OrbitCamera>();
                var sun=Child(fixture,"Sun");
                var view=Child(fixture,"Test Views").AddComponent<MoonInset>(); view.controller=c; view.orbitCamera=orbitCamera; view.solarObjects=new[]{sun}; c.view=view;
                view.moon=Child(fixture,"Moon").transform;
                c.ResetSimulation(); var baseline=c.DisplayState;
                var p=c.gameObject.AddComponent<TractorInspection>(); p.controller=c; p.views=view; p.orbitCamera=orbitCamera;
                p.tractor=Child(fixture,"Test Tractor").transform; p.separation=Child(fixture,"Test Separation").AddComponent<LineRenderer>();
                p.missionCsvPath="Assets/Data/tractor_diagnostic.csv"; p.Enter();
                Assert.That(p.PlaybackTime,Is.Zero,"Opens at zero"); Assert.That(p.IsPlaying,Is.False,"Opens paused");
                p.AdvancePlayback(10); Assert.That(p.PlaybackTime,Is.Zero,"Paused clock stays still");
                Assert.That(p.SolarView,Is.True); Assert.That(p.SourceLabel,Is.EqualTo(p.missionCsvPath));
                var initial=c.earth.position; p.SetTime(Constants.JulianYear/4);
                Assert.That((c.earth.position-initial).magnitude,Is.GreaterThan(20),"Old Earth-centred inspection would remain at zero.");
                Assert.That(sun.activeSelf,Is.True,"Transfer view must show the Sun.");
                MissionRecording data; using(var reader=System.IO.File.OpenText(System.IO.Path.Combine(Application.dataPath,"Data/tractor_diagnostic.csv"))) data=MissionRecording.Read(reader);
                Assert.That(p.tractor.position,Is.EqualTo(OrbitalCoordinates.ToUnity(data.At(p.PlaybackTime).Earth+data.At(p.PlaybackTime).TractorRelative)));
                double time=p.PlaybackTime; p.FocusPair(); p.SetTime(time); Assert.That(p.PlaybackTime,Is.EqualTo(time));
                Assert.That(c.earth.position,Is.EqualTo(Vector3.zero)); Assert.That(p.tractor.position,Is.EqualTo(OrbitalCoordinates.RelativeTo(data.At(time).TractorRelative,new Vector3d(),8.0/5e7)));
                p.FocusSolarSystem(); p.SetTime(0); p.PlaybackSpeed=1e9; p.SetPlaying(true); p.AdvancePlayback(1);
                Assert.That(p.PlaybackTime,Is.EqualTo(1e9));
                p.SetTime(double.MaxValue); Assert.That(p.PlaybackTime,Is.EqualTo(data.EndTime));
                p.AdvancePlayback(10); Assert.That(p.PlaybackTime,Is.EqualTo(data.EndTime)); Assert.That(p.IsPlaying,Is.False);
                p.Restart(); Assert.That(p.PlaybackTime,Is.Zero); Assert.That(p.IsPlaying,Is.False); Assert.That(p.SolarView,Is.True);
                var initialFrame=data.Frames[0]; p.SetTime(12345); p.SetTime(0); Assert.That(p.CurrentFrame.Earth,Is.EqualTo(initialFrame.Earth),"Seeking does not mutate data");
                if(initialFrame.HasMoon) {
                    Assert.That(view.moon.gameObject.activeSelf,Is.True);
                    Assert.That(view.moon.position,Is.EqualTo(OrbitalCoordinates.ToUnity(initialFrame.Earth+initialFrame.MoonRelative)));
                    p.ToggleMoon(); Assert.That(view.moon.gameObject.activeSelf,Is.False); p.ToggleMoon(); Assert.That(view.moon.gameObject.activeSelf,Is.True);
                }
                var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
                typeof(TractorInspection).GetField("data",flags).SetValue(p,null);
                typeof(TractorInspection).GetMethod("LateUpdate",flags).Invoke(p,null);
                Assert.That(p.SampleCount,Is.EqualTo(data.Frames.Count),"Script-reload cache loss must recover without a null-reference exception.");
                p.Exit(); Assert.That(c.DisplayState.Equals(baseline),Is.True); Assert.That(c.earth,Is.SameAs(earth.transform));
                TestContext.WriteLine("CSV samples="+data.Frames.Count+"; quarter-year displacement="+(p.DisplayEarthPosition(data.At(Constants.JulianYear/4))-p.DisplayEarthPosition(data.Frames[0])).magnitude+" Unity units; 1 second at 1e9x=1e9 recorded seconds; baseline unchanged.");
            }
            finally { Object.DestroyImmediate(fixture); }
        }
    }
}
#endif
