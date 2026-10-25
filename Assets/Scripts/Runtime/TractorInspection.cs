using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using OctoEarth.Core;
namespace OctoEarth
{
    [DefaultExecutionOrder(75)]
    public sealed class TractorInspection : MonoBehaviour
    {
        public OrbitController controller;
        public MoonInset views;
        public OrbitCamera orbitCamera;
        public Transform tractor;
        public TextAsset recording;
        public string missionCsvPath="Results/mission_h3600_y10000/run_mission.csv";
        public double tractorMassKg=1.61048900149879e18,rockDensity=3000;
        public LineRenderer separation;
        public TextMesh tractorLabel;
        public bool openOnPlay=true;
        public bool partialRun=false,openAtRecordingEnd=false;
        public bool Active {get;private set;}
        public bool SolarView {get;private set;}=true;
        public int SampleIndex => data==null?0:data.IndexAt(playbackTime);
        public int SampleCount => data==null?0:data.Frames.Count;
        public double SeparationMetres => data==null?0:data.At(playbackTime).TractorRelative.Magnitude;
        public double PlaybackTime => playbackTime;
        public string RecordingError {get;private set;}
        public string SourceLabel {get;private set;}
        public double PlaybackSpeed=1e7;
        public bool IsPlaying => playing;
        public bool MoonVisible=true;
        public bool MoonView {get;private set;}
        public double EndTime => data==null?0:data.EndTime;
        public MissionFrame CurrentFrame => data.At(playbackTime);
        public bool DemoActive {get;private set;}
        double elapsedWall,demoStart;
        int demoStage=-1;
        Label moonMetric,ledgerMetric,resourceMetric,clockMetric,endMetric;
        DropdownField speedChoice;
        LineRenderer moonTrail,moonLeader;
        TextMesh moonCaption;
        MissionThresholds displayLimits;
        double[] moonMin,moonMax;
        double LocalScale => MoonView?8.0/Constants.MoonDistance:data==null?8.0/5e7:8.0/data.Frames[0].TractorRelative.Magnitude;
        MissionRecording data;
        VisualElement panel,baseline,badge,controls,statusBar;
        Label metrics,source,scaleLabel,axisMetric,thrustMetric,distanceMetric,flagMetric;
        Slider timeline;
        Button play;
        bool playing,closeFocus;
        double playbackTime,previousWall,drawnTrailTime=double.NaN;
        TextMesh earthLabel,pullLabel,markerLabel,targetLabel;
        string originalTargetLabel;
        Transform solarMarker;
        LineRenderer markerLeader,pull,trail;
        Renderer[] tractorRenderers;
        readonly Vector3[] trailPoints=new Vector3[513];
        public Vector3 DisplayEarthPosition(MissionFrame f) => SolarView?OrbitalCoordinates.ToUnity(f.Earth):Vector3.zero;
        public Vector3 DisplayTractorPosition(MissionFrame f) => SolarView?OrbitalCoordinates.ToUnity(f.Earth+f.TractorRelative):OrbitalCoordinates.RelativeTo(f.TractorRelative,new Vector3d(),LocalScale);
        void Start() { if(openOnPlay) { Reload(); playbackTime=0; Enter(); } else tractor.gameObject.SetActive(false); }
        public void Reload()
        {
            try
            {
                string path=Path.GetFullPath(Path.Combine(Application.dataPath,"..",missionCsvPath));
                if(File.Exists(path)) { using(var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)) using(var reader=new StreamReader(stream)) data=MissionRecording.Read(reader); SourceLabel=missionCsvPath; }
                else if(recording!=null) { using(var reader=new StringReader(recording.text)) data=MissionRecording.Read(reader); SourceLabel="FALLBACK: Assets/Data/"+recording.name+".csv (100-year diagnostic)"; }
                else throw new FileNotFoundException("Mission CSV not found",path);
                RecordingError=null; playbackTime=Math.Min(playbackTime,data.EndTime);
                displayLimits=MissionConfigurationIO.Read<MissionThresholds>(Path.Combine(Application.dataPath,"../Config/thresholds.json"));
                var config=MissionConfigurationIO.Read<MissionConfiguration>(Path.Combine(Application.dataPath,"../Config/mission_design_candidate.json")); rockDensity=config.RockDensity;
                using(var r=File.Exists(path)?File.OpenText(path):(TextReader)new StringReader(recording.text)) {var h=r.ReadLine().Split(','); var v=r.ReadLine().Split(','); int k=Array.IndexOf(h,"tractor_mass_kg"); if(k>=0) tractorMassKg=double.Parse(v[k],System.Globalization.CultureInfo.InvariantCulture);}
                moonMin=new double[data.Frames.Count]; moonMax=new double[data.Frames.Count];
                for(int k=0;k<data.Frames.Count;k++) {double d=data.Frames[k].MoonDistance; moonMin[k]=k==0?d:Math.Min(moonMin[k-1],d); moonMax[k]=k==0?d:Math.Max(moonMax[k-1],d);}
                if(partialRun) SourceLabel="partial run: first "+(data.EndTime/Constants.JulianYear).ToString("F6")+" yr\n"+missionCsvPath;
                if(timeline!=null) { timeline.highValue=(float)(data.EndTime/Constants.JulianYear); timeline.SetValueWithoutNotify((float)(playbackTime/Constants.JulianYear)); }
                drawnTrailTime=double.NaN;
            }
            catch(Exception error) { RecordingError=error.Message; Debug.LogError("Mission playback: "+error.Message); }
        }
        public void Enter()
        {
            if(data==null) Reload(); if(data==null) return;
            controller.PauseSimulation(); views.SelectSolarSystem(); views.enabled=false;
            if(earthLabel==null) earthLabel=controller.earth.GetComponentInChildren<TextMesh>(true);
            EnsureVisuals(); Active=true; playing=false; previousWall=Time.realtimeSinceStartupAsDouble;
            tractor.gameObject.SetActive(true);
            if(panel!=null) panel.style.display=DisplayStyle.Flex;
            if(controls!=null) controls.style.display=DisplayStyle.Flex; if(statusBar!=null) statusBar.style.display=DisplayStyle.Flex;
            if(baseline!=null) baseline.style.display=DisplayStyle.None;
            if(badge!=null) badge.style.display=DisplayStyle.None;
            FocusSolarSystem(); Render();
        }
        public void Exit()
        {
            Active=false; playing=false; tractor.gameObject.SetActive(false); separation.gameObject.SetActive(false);
            if(solarMarker!=null) solarMarker.gameObject.SetActive(false);
            foreach(var line in new[]{markerLeader,pull,trail}) if(line!=null) line.gameObject.SetActive(false);
            if(pullLabel!=null) pullLabel.gameObject.SetActive(false);
            views.enabled=true; views.SelectSolarSystem();
            if(targetLabel!=null) targetLabel.text=originalTargetLabel;
            if(panel!=null) panel.style.display=DisplayStyle.None;
            if(controls!=null) controls.style.display=DisplayStyle.None; if(statusBar!=null) statusBar.style.display=DisplayStyle.None;
            if(baseline!=null) baseline.style.display=DisplayStyle.Flex;
            if(badge!=null) badge.style.display=DisplayStyle.Flex;
        }
        public void SetSample(int index) { if(data!=null) SetTime(data.Frames[Mathf.Clamp(index,0,data.Frames.Count-1)].Time); }
        public void SetTime(double seconds)
        {
            if(data==null) return; playbackTime=Math.Max(data.Frames[0].Time,Math.Min(data.EndTime,seconds));
            if(timeline!=null) timeline.SetValueWithoutNotify((float)(playbackTime/Constants.JulianYear)); if(Active) Render();
        }
        public void SetPlaying(bool value) { playing=value; previousWall=Time.realtimeSinceStartupAsDouble; }
        public void AdvancePlayback(double wallSeconds)
        {
            if(!Active || !playing) return;
            SetTime(playbackTime+Math.Max(0,wallSeconds)*PlaybackSpeed); if(playbackTime>=data.EndTime) playing=false;
        }
        public void Restart() {DemoActive=false; playing=false; elapsedWall=0; FocusSolarSystem(); SetTime(0);}
        public void ToggleMoon() {MoonVisible=!MoonVisible; if(Active) Render();}
        public void FocusMoon() {MoonView=true; SolarView=false; closeFocus=false; PlaybackSpeed=Math.Min(PlaybackSpeed,1e6); orbitCamera.FrameEarthMoon(); orbitCamera.distance=28;}
        public void StartDemo() {Restart(); DemoActive=true; demoStart=Time.realtimeSinceStartupAsDouble; demoStage=-1;}
        public void FocusSolarSystem() { MoonView=false; SolarView=true; closeFocus=false; orbitCamera.FrameSolarSystem(); orbitCamera.focus=Vector3.zero; drawnTrailTime=double.NaN; }
        public void FocusPair() { MoonView=false; SolarView=false; closeFocus=false; orbitCamera.localView=true; orbitCamera.distance=22; orbitCamera.yaw=0; orbitCamera.pitch=70; }
        public void FocusTractor() { MoonView=false; SolarView=false; closeFocus=true; orbitCamera.localView=true; orbitCamera.distance=2.5f; orbitCamera.yaw=15; orbitCamera.pitch=35; }
        void LateUpdate()
        {
            double now=Time.realtimeSinceStartupAsDouble,wall=Math.Max(0,now-previousWall); previousWall=now;
            if(!Active) return;
            elapsedWall+=wall;
            var keyboard=Keyboard.current;
            if(keyboard!=null) {
                if(keyboard.spaceKey.wasPressedThisFrame) SetPlaying(!playing);
                if(keyboard.rKey.wasPressedThisFrame) Restart();
                if(keyboard.mKey.wasPressedThisFrame) FocusMoon();
                if(keyboard.tKey.wasPressedThisFrame) ToggleMoon();
                if(keyboard.dKey.wasPressedThisFrame) StartDemo();
                if(keyboard.escapeKey.wasPressedThisFrame) {DemoActive=false; SetPlaying(false);}
                var keys=new[]{keyboard.digit1Key,keyboard.digit2Key,keyboard.digit3Key,keyboard.digit4Key,keyboard.digit5Key,keyboard.digit6Key};
                var speeds=new[]{1d,1e3,1e5,1e7,1e8,1e9}; for(int k=0;k<6;k++) if(keys[k].wasPressedThisFrame) PlaybackSpeed=speeds[k];
            }
            if(DemoActive) {
                double t=now-demoStart; int stage=t<20?0:t<50?1:t<65?2:t<70?3:4;
                if(stage!=demoStage) {demoStage=stage; if(stage==0) {SetTime(0);SetPlaying(false);} if(stage==1) FocusPair(); if(stage==2) {FocusMoon();SetPlaying(true);} if(stage==3) {FocusSolarSystem(); SetPlaying(false);} if(stage==4) {PlaybackSpeed=1e9; SetPlaying(true);}}
                if(t>=180) {DemoActive=false;SetPlaying(false);}
            }
            // Unity script reload retains component references and Active, but not the parsed
            // non-Unity recording. Recover that cache before rendering another frame.
            if(data==null) { Reload(); if(data==null) { Active=false; return; } }
            AdvancePlayback(wall);
            orbitCamera.easeMotion=DemoActive;
            Render();
        }
        void EnsureVisuals()
        {
            if(tractorRenderers!=null) return;
            tractorRenderers=tractor.GetComponentsInChildren<Renderer>(true); var material=separation.sharedMaterial;
            foreach(var go in views.solarObjects) if(go!=null && go.name=="Target Label") { targetLabel=go.GetComponent<TextMesh>(); originalTargetLabel=targetLabel.text; }
            pull=Line("Tractor gravitational pull vector",material,0.06f); markerLeader=Line("Tractor marker leader (display offset)",material,0.035f); trail=Line("Mission CSV trajectory",material,0.08f);
            var marker=GameObject.CreatePrimitive(PrimitiveType.Sphere); marker.name="Tractor marker (enlarged callout)"; solarMarker=marker.transform; solarMarker.SetParent(transform,false);
            marker.GetComponent<Renderer>().sharedMaterial=material; DestroyImmediate(marker.GetComponent<Collider>());
            markerLabel=Label("Tractor marker label",0.7f); markerLabel.transform.SetParent(solarMarker,false); markerLabel.transform.localPosition=new Vector3(0,2,0); markerLabel.transform.localScale=Vector3.one/2;
            pullLabel=Label("Pull vector label",0.18f);
            moonTrail=Line("Recorded Moon trail (monthly samples)",material,0.035f); moonLeader=Line("Moon true-position leader",material,0.025f);
            moonCaption=Label("Recorded Moon caption",0.15f);
        }
        LineRenderer Line(string name,Material material,float width)
        {
            var go=new GameObject(name); go.transform.SetParent(transform,false); var line=go.AddComponent<LineRenderer>(); line.sharedMaterial=material; line.widthMultiplier=width; line.useWorldSpace=true; return line;
        }
        TextMesh Label(string name,float size)
        {
            var go=new GameObject(name); go.transform.SetParent(transform,false); var label=go.AddComponent<TextMesh>(); label.fontSize=40; label.characterSize=size; label.anchor=TextAnchor.MiddleCenter; label.color=new Color(0.5f,0.9f,1); go.AddComponent<CameraFacingLabel>(); return label;
        }
        double TractorDiameter => 2*Math.Pow(3*tractorMassKg/(4*Math.PI*rockDensity),1.0/3);
        void Render()
        {
            var f=data.At(playbackTime);
            foreach(var go in views.solarObjects) if(go!=null) go.SetActive(SolarView && go.name!="Computed Trajectory" && go.name!="Scale Label");
            if(views.orbit!=null) views.orbit.gameObject.SetActive(false);
            if(views.moon!=null) {
                views.moon.gameObject.SetActive(MoonVisible && f.HasMoon);
                foreach(var label in views.moon.GetComponentsInChildren<TextMesh>()) label.gameObject.SetActive(false);
                double ms=SolarView?20/Constants.AU:LocalScale;
                var truePosition=SolarView?OrbitalCoordinates.ToUnity(f.Earth+f.MoonRelative):OrbitalCoordinates.RelativeTo(f.MoonRelative,new Vector3d(),ms);
                float diameter=SolarView?1:MoonView?0.8f:0.5f;
                views.moon.position=truePosition; views.moon.localScale=Vector3.one*diameter;
                moonCaption.gameObject.SetActive(MoonVisible && f.HasMoon); moonCaption.transform.position=truePosition+new Vector3(SolarView?-12:0, SolarView?10:1,0);
                moonCaption.characterSize=SolarView?0.5f:MoonView?0.25f:0.16f; moonCaption.text=$"MOON marker x{diameter/(2*MissionGeometry.MoonRadius*ms):F1}";
                moonLeader.gameObject.SetActive(MoonVisible && f.HasMoon); moonLeader.positionCount=2; moonLeader.SetPosition(0,truePosition); moonLeader.SetPosition(1,moonCaption.transform.position);
                moonTrail.gameObject.SetActive(MoonVisible && f.HasMoon && !SolarView); int first=Math.Max(0,SampleIndex-12),count=SampleIndex-first+1; moonTrail.positionCount=count;
                for(int k=0;k<count;k++) moonTrail.SetPosition(k,OrbitalCoordinates.RelativeTo(data.Frames[first+k].MoonRelative,new Vector3d(),ms));
                if(moonMetric!=null) {var sample=data.Frames[SampleIndex]; double hill=sample.A*Math.Pow((Constants.EarthMass+Constants.MoonMass)/(3*Constants.SolarMass),1.0/3),energy=sample.MoonVelocity.SqrMagnitude/2-Constants.G*Constants.EarthMass/Math.Max(1,sample.MoonRelative.Magnitude);
                    moonMetric.text=$"MOON: {sample.MoonDistance/1000:N1} km | {(energy<0 && sample.MoonDistance<hill?"BOUND":"ESCAPING")}\nSample energy: {energy:E3} J/kg; r/Hill {sample.MoonDistance/hill:F3}\nSample min/max: {moonMin[SampleIndex]/1000:N0} / {moonMax[SampleIndex]/1000:N0} km\nPlaceholder: {displayLimits.moonDistanceMinimum.value/1000:N0}..{displayLimits.moonDistanceMaximum.value/1000:N0} km\n{PlaybackSpeed/(2*Math.PI*Math.Sqrt(Math.Pow(sample.MoonDistance,3)/Constants.EarthMu)):F3} nominal orbits/s\nMonthly vectors: lunar motion underresolved.\n"+(PlaybackSpeed>1e6?"Moon motion aliased at this speed":"Hermite display between monthly samples");
                    if(!SolarView) {var viewport=orbitCamera.GetComponent<Camera>().WorldToViewportPoint(truePosition); if(viewport.x<0.23f || viewport.x>0.82f || viewport.y<0.06f || viewport.y>0.95f || viewport.z<0) moonMetric.text+=$"\nMoon off-screen: {(viewport.x<0.5f?"LEFT":"RIGHT")} / {(viewport.y<0.5f?"DOWN":"UP")}, {sample.MoonDistance/1000:N0} km";}}
            }
            controller.earth.position=DisplayEarthPosition(f); controller.earth.localScale=Vector3.one*(SolarView?2:MoonView?1.1f:(float)(2*Constants.EarthRadius*LocalScale));
            tractor.position=DisplayTractorPosition(f); tractor.localScale=Vector3.one*0.8f;
            foreach(var renderer in tractorRenderers) renderer.enabled=!SolarView;
            if(tractorLabel!=null) { tractorLabel.gameObject.SetActive(!SolarView && !closeFocus); tractorLabel.text="GRAVITY TRACTOR"; tractorLabel.characterSize=0.07f; }
            if(earthLabel!=null) { earthLabel.text="EARTH"; earthLabel.characterSize=SolarView?0.6f:0.07f; earthLabel.transform.localPosition=new Vector3(0,SolarView?-2:0.8f,0); earthLabel.transform.localScale=Vector3.one; }
            if(targetLabel!=null) targetLabel.text="2.7 AU TARGET (reference)";
            solarMarker.gameObject.SetActive(SolarView); markerLeader.gameObject.SetActive(SolarView);
            if(SolarView)
            {
                solarMarker.position=tractor.position+new Vector3(10,8,0); solarMarker.localScale=Vector3.one*2;
                markerLabel.text="TRACTOR (marker)";
                markerLeader.positionCount=2; markerLeader.SetPosition(0,tractor.position); markerLeader.SetPosition(1,solarMarker.position);
            }
            else orbitCamera.focus=MoonView?Vector3.zero:closeFocus?tractor.position-new Vector3(0.3f,0,0):tractor.position/2-new Vector3(3,0,0);
            separation.gameObject.SetActive(!SolarView && !closeFocus); separation.positionCount=2;
            separation.SetPosition(0,controller.earth.position+Vector3.down*1.6f); separation.SetPosition(1,tractor.position+Vector3.down*1.6f);
            pull.gameObject.SetActive(!closeFocus); pullLabel.gameObject.SetActive(!closeFocus);
            var direction=OrbitalCoordinates.RelativeTo(f.TractorRelative,new Vector3d(),1).normalized;
            var start=controller.earth.position; var end=start+direction*(SolarView?5:(float)(f.TractorRelative.Magnitude*LocalScale));
            var side=Vector3.Cross(direction,Vector3.up).normalized*(SolarView?0.7f:0.3f);
            pull.positionCount=5; pull.SetPositions(new[]{start,end,end-direction*(SolarView?1:0.5f)+side,end,end-direction*(SolarView?1:0.5f)-side});
            pullLabel.text="PULL"; pullLabel.characterSize=SolarView?0.65f:0.12f; pullLabel.transform.position=(start+end)/2+Vector3.up*(SolarView?2:0.5f);
            trail.gameObject.SetActive(SolarView); if(SolarView && drawnTrailTime!=playbackTime) DrawTrail(f);
            if(source!=null) source.text="MISSION CSV\n"+SourceLabel+"\nLoaded through "+(data.EndTime/Constants.JulianYear).ToString("F3")+" yr. Reload to read newer output.";
            double scale=SolarView?20/Constants.AU:LocalScale;
            if(scaleLabel!=null) scaleLabel.text=SolarView
                ? $"SUN-CENTRED | 1 AU = 20 units\nEarth diameter x{2/(2*Constants.EarthRadius*scale):F0}; Sun diameter x{4/(2*MissionGeometry.SolarRadius*scale):F1}.\nTractor marker diameter x{2/(TractorDiameter*scale):F0}; callout offset schematic."
                : $"EARTH-CENTRED | {data.Frames[0].TractorRelative.Magnitude/1000:N0} km = 8 units\nEarth diameter x1; asteroid nominal diameter x{0.8/(TractorDiameter*scale):F1}.\nThruster hardware schematic. Inertial display axes.";
            if(scaleLabel!=null && MoonView) scaleLabel.text=$"MOON VIEW | Earth-centred\n{Constants.MoonDistance/1000:N0} km = 8 units\nEarth diameter x{1.1/(2*Constants.EarthRadius*scale):F1}; Moon x{0.8/(2*MissionGeometry.MoonRadius*scale):F1}\nTractor diameter x{0.8/(TractorDiameter*scale):F1}\nTrail: recorded monthly positions, not a resolved orbit.";
            if(metrics!=null) metrics.text=$"Elapsed: {f.Time/Constants.JulianYear:F6} yr";
            if(axisMetric!=null) axisMetric.text=$"a_EMB: {f.A/Constants.AU:F3} AU";
            if(thrustMetric!=null) thrustMetric.text=$"Thrust: {f.Thrust:E4} N";
            if(distanceMetric!=null) distanceMetric.text=$"Separation: {f.TractorRelative.Magnitude/1000:N1} km";
            if(flagMetric!=null) flagMetric.text=$"Flags: {f.Flags:F0} | Design status: FAIL";
            if(resourceMetric!=null) resourceMetric.text=$"Ejected: {f.Ejected:E5} kg";
            if(ledgerMetric!=null) ledgerMetric.text=$"P {f.RelativeP:E2} L {f.RelativeL:E2} E {f.RelativeE:E2}";
            if(clockMetric!=null) clockMetric.text=$"{PlaybackSpeed:E0} x | wall {elapsedWall:F1} s";
            if(endMetric!=null) endMetric.text=playbackTime>=data.EndTime?"End of recorded data (partial run)":DemoActive?"DEMO TOUR (Esc interrupts)":playing?"Playing recorded data":"Paused until Play / Space";
            if(play!=null) play.text=playing?"Pause playback":"Play mission";
            if(speedChoice!=null) speedChoice.SetValueWithoutNotify(PlaybackSpeed.ToString("0.#####E0",System.Globalization.CultureInfo.InvariantCulture)+" x");
        }
        void DrawTrail(MissionFrame frame)
        {
            double period=2*Math.PI*Math.Sqrt(Math.Pow(Math.Max(Constants.AU*0.1,frame.A),3)/Constants.SolarMu);
            double first=Math.Max(data.Frames[0].Time,playbackTime-2*period);
            for(int k=0;k<trailPoints.Length;k++) trailPoints[k]=OrbitalCoordinates.ToUnity(data.At(first+(playbackTime-first)*k/(trailPoints.Length-1)).Earth);
            trail.positionCount=trailPoints.Length; trail.SetPositions(trailPoints); drawnTrailTime=playbackTime;
        }
        public void Attach(VisualElement root,VisualElement baselinePanel,VisualElement baselineBadge)
        {
            baseline=baselinePanel; badge=baselineBadge;
            var scroll=new ScrollView(ScrollViewMode.Vertical); scroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden; scroll.contentContainer.style.width=276;
            panel=scroll; panel.style.position=Position.Absolute; panel.style.left=8; panel.style.top=8; panel.style.bottom=8; panel.style.width=330;
            panel.style.paddingLeft=panel.style.paddingRight=12; panel.style.paddingTop=panel.style.paddingBottom=12;
            panel.style.backgroundColor=new Color(0.025f,0.045f,0.08f,0.97f); panel.style.color=new Color(0.88f,0.93f,1);
            panel.Add(new Label(partialRun?"GRAVITY TRACTOR / PARTIAL RUN":"GRAVITY TRACTOR / MISSION"){style={fontSize=16,unityFontStyleAndWeight=FontStyle.Bold,whiteSpace=WhiteSpace.Normal}});
            source=new Label(); source.style.whiteSpace=WhiteSpace.Normal; source.style.fontSize=11; panel.Add(source);
            var readings=new VisualElement(); readings.style.marginTop=6; panel.Add(readings);
            Label Reading() {var label=new Label(); label.style.whiteSpace=WhiteSpace.NoWrap; label.style.height=20; label.style.minHeight=20; label.style.maxHeight=20; label.style.flexShrink=0; readings.Add(label); return label;}
            metrics=Reading(); axisMetric=Reading(); thrustMetric=Reading(); distanceMetric=Reading(); flagMetric=Reading();
            resourceMetric=Reading(); ledgerMetric=Reading(); clockMetric=Reading(); endMetric=Reading();
            moonMetric=new Label(); moonMetric.style.whiteSpace=WhiteSpace.Normal; moonMetric.style.height=156; moonMetric.style.flexShrink=0; panel.Add(moonMetric);
            panel.Add(new Button(FocusMoon){text="Moon view (M)"}); panel.Add(new Button(ToggleMoon){text="Show / hide Moon (T)"});
            panel.Add(new Button(()=>{SetPlaying(false);SetTime(EndTime);}){text="Jump to end"});
            var row=new VisualElement(); row.style.flexDirection=FlexDirection.Row; panel.Add(row);
            row.Add(new Button(()=>{SetPlaying(false);SetTime(playbackTime-Constants.JulianYear);}){text="-1 year"}); row.Add(new Button(()=>{SetPlaying(false);SetTime(playbackTime+Constants.JulianYear);}){text="+1 year"});
            row=new VisualElement(); row.style.flexDirection=FlexDirection.Row; panel.Add(row);
            row.Add(new Button(FocusSolarSystem){text="Sun / transfer"}); row.Add(new Button(FocusPair){text="Earth + tractor"});
            row=new VisualElement(); row.style.flexDirection=FlexDirection.Row; panel.Add(row);
            row.Add(new Button(FocusTractor){text="Focus tractor"}); row.Add(new Button(()=> { Reload(); if(Active) Render(); }){text="Reload CSV"});
            play=new Button(()=>SetPlaying(!playing)){text="Play mission"}; panel.Add(play);
            timeline=new Slider("Elapsed years",0,1); timeline.style.flexDirection=FlexDirection.Column; timeline.style.minHeight=46; timeline.labelElement.style.minWidth=0;
            timeline.RegisterValueChangedCallback(e=> { playing=false; SetTime(e.newValue*Constants.JulianYear); }); panel.Add(timeline);
            var speed=new DropdownField("Playback speed (keys 1-6)",new System.Collections.Generic.List<string>{"1 x","1e3 x","1e5 x","1e7 x","1e8 x","1e9 x"},3);
            speedChoice=speed;
            speed.style.flexDirection=FlexDirection.Column; speed.style.minHeight=42; speed.labelElement.style.minWidth=0;
            speed.RegisterValueChangedCallback(e=>PlaybackSpeed=double.Parse(e.newValue.Split(' ')[0],System.Globalization.CultureInfo.InvariantCulture)); panel.Add(speed);
            scaleLabel=new Label(); scaleLabel.style.whiteSpace=WhiteSpace.Normal; scaleLabel.style.fontSize=11; panel.Add(scaleLabel);
            var notes=new Label("Diagnostic only; optics unresolved. Between CSV samples: Hermite display positions; linear a/thrust readouts.\nCyan trail: last two orbits. Pull arrow: direction only. Marker leader ends at true CSV position."); notes.style.whiteSpace=WhiteSpace.Normal; notes.style.fontSize=11; panel.Add(notes);
            panel.Add(new Button(Restart){text="Restart (R)"}); panel.Add(new Button(Exit){text="Return to M2 baseline (no transfer)"});
            panel.Add(new Label("Values from headless Core run: "+missionCsvPath+"; partial run, not a full 10,000 yr result"){style={whiteSpace=WhiteSpace.Normal,fontSize=12}});
            panel.Add(new Label("Right drag: orbit | Middle drag: pan\nWheel: zoom outside dashboard"){style={whiteSpace=WhiteSpace.Normal}});
            panel.Query<Button>().ForEach(b=> { b.style.height=28; b.style.flexShrink=0; b.style.color=Color.white; b.style.backgroundColor=new Color(0.12f,0.23f,0.34f); });
            panel.style.display=Active?DisplayStyle.Flex:DisplayStyle.None; root.Add(panel);
            // Keep diagnostics visible while controls occupy a separate fixed dashboard.
            panel.style.width=420; panel.style.bottom=64; panel.style.fontSize=16; scroll.contentContainer.style.width=376;
            source.style.fontSize=14; moonMetric.style.height=210; scaleLabel.style.fontSize=14; notes.style.fontSize=14;
            controls=new VisualElement(); controls.style.position=Position.Absolute; controls.style.right=8; controls.style.top=8; controls.style.width=320;
            controls.style.paddingLeft=controls.style.paddingRight=12; controls.style.paddingTop=controls.style.paddingBottom=12;
            controls.style.backgroundColor=new Color(0.025f,0.045f,0.08f,0.97f); controls.style.color=Color.white; controls.style.fontSize=16;
            var buttons=panel.Query<Button>().ToList(); foreach(var button in buttons) {button.style.height=30; controls.Add(button);}
            controls.Add(timeline); controls.Add(speed);
            controls.Add(new Label("Space: Play/Pause | R: Restart\nM: Moon | T: Moon visibility\nKeys 1-6: speed presets\nMoon view: use <=1e6x"){style={whiteSpace=WhiteSpace.Normal,fontSize=14}});
            controls.style.display=Active?DisplayStyle.Flex:DisplayStyle.None; root.Add(controls);
            statusBar=new Label("Values from headless Core run: "+missionCsvPath+"; partial run, not a full 10,000 yr result");
            statusBar.style.position=Position.Absolute; statusBar.style.left=8; statusBar.style.right=8; statusBar.style.bottom=8; statusBar.style.height=48;
            statusBar.style.fontSize=16; statusBar.style.whiteSpace=WhiteSpace.Normal; statusBar.style.backgroundColor=new Color(0.025f,0.045f,0.08f,0.97f); statusBar.style.color=Color.white;
            statusBar.style.display=Active?DisplayStyle.Flex:DisplayStyle.None; root.Add(statusBar);
            scroll.schedule.Execute(()=>scroll.scrollOffset=Vector2.zero).ExecuteLater(100);
        }
    }
}
