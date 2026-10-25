using System;
using UnityEngine;
using UnityEngine.UIElements;
using OctoEarth.Core;

namespace OctoEarth
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class MissionDashboard : MonoBehaviour
    {
        public OrbitController controller;
        Label metrics, validation, status;
        Button start, pause, resume;
        Button solarView,earthMoonView;
        Label viewStatus;
        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.Clear(); root.pickingMode = PickingMode.Ignore;
            var scroll = new ScrollView(ScrollViewMode.Vertical); root.Add(scroll);
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.contentContainer.style.width = 300;
            scroll.style.position = Position.Absolute; scroll.style.left = 8; scroll.style.top = 8; scroll.style.bottom = 8;
            scroll.style.width = 330;
            var panel = new VisualElement(); scroll.Add(panel);
            panel.style.width = 300;
            panel.style.paddingLeft = panel.style.paddingRight = 16;
            panel.style.paddingTop = panel.style.paddingBottom = 14;
            panel.style.backgroundColor = new Color(0.025f,0.045f,0.08f,0.96f);
            panel.style.color = new Color(0.88f,0.93f,1);
            panel.Add(new Label("OCTO-EARTH / MILESTONE 2") { style = { fontSize = 17, unityFontStyleAndWeight = FontStyle.Bold } });
            panel.Add(new Label("BASELINE ORBIT — NO PROPULSION") { style = { fontSize = 13, color = new Color(0.4f,0.85f,1) } });
            status = new Label(); panel.Add(status);
            var row = new VisualElement(); row.style.flexDirection = FlexDirection.Row; row.style.minHeight = 30; panel.Add(row);
            start = new Button(()=>controller.StartSimulation()) { text = "Start" }; row.Add(start);
            pause = new Button(()=>controller.PauseSimulation()) { text = "Pause" }; row.Add(pause);
            resume = new Button(()=>controller.ResumeSimulation()) { text = "Resume" }; row.Add(resume);
            row.Add(new Button(()=>controller.ResetSimulation()) { text = "Reset" });
            var mode = new Toggle("N-body baseline (resets)") { value = controller.useNBody };
            mode.RegisterValueChangedCallback(e=> { controller.useNBody=e.newValue; controller.ResetSimulation(); }); panel.Add(mode);
            var views=new VisualElement(); views.style.flexDirection=FlexDirection.Row; panel.Add(views);
            solarView=new Button(()=> { if(controller.view!=null) controller.view.SelectSolarSystem(); }) { text="Solar System" }; views.Add(solarView);
            earthMoonView=new Button(()=> { if(controller.view!=null) controller.view.SelectEarthMoon(); }) { text="Earth-Moon" }; views.Add(earthMoonView);
            var tractorInspection=controller.GetComponent<TractorInspection>();
            if(tractorInspection!=null) panel.Add(new Button(tractorInspection.Enter){text="Inspect gravity tractor (recorded)"});
            var caption=new Label("Choose a view. Right drag: orbit | Wheel: zoom"); caption.style.whiteSpace=WhiteSpace.Normal; panel.Add(caption);
            var viewBadge=new VisualElement(); viewBadge.pickingMode=PickingMode.Ignore;
            viewBadge.style.position=Position.Absolute; viewBadge.style.top=12; viewBadge.style.right=12; viewBadge.style.maxWidth=360;
            viewBadge.style.paddingLeft=viewBadge.style.paddingRight=12; viewBadge.style.paddingTop=viewBadge.style.paddingBottom=8;
            viewBadge.style.backgroundColor=new Color(0.025f,0.045f,0.08f,0.9f); viewBadge.style.color=new Color(0.7f,0.9f,1);
            viewStatus=new Label(); viewStatus.style.whiteSpace=WhiteSpace.Normal; viewStatus.pickingMode=PickingMode.Ignore; viewBadge.Add(viewStatus); root.Add(viewBadge);
            var speed = new Slider("Speed: 1.0 days / real second",0.1f,365.25f) { value = (float)(controller.simulatedSecondsPerRealSecond/OrbitalMechanics.Day), showInputField = false };
            speed.style.minHeight = 48; speed.style.flexDirection = FlexDirection.Column;
            speed.labelElement.style.minWidth = 0;
            speed.RegisterValueChangedCallback(e=> { controller.simulatedSecondsPerRealSecond = Math.Max(0.1,e.newValue)*OrbitalMechanics.Day; speed.label = $"Speed: {e.newValue:F1} days / real second"; }); panel.Add(speed);
            var step = new DropdownField("Step (s); resets orbit",new System.Collections.Generic.List<string>{"1800","3600","7200"},1);
            step.style.minHeight = 48; step.style.flexDirection = FlexDirection.Column; step.labelElement.style.minWidth = 0;
            step.RegisterValueChangedCallback(e=>controller.ConfigureStep(double.Parse(e.newValue,System.Globalization.CultureInfo.InvariantCulture))); panel.Add(step);
            metrics = new Label(); metrics.style.fontSize = 13; metrics.style.marginTop = 10; panel.Add(metrics);
            validation = new Label(); validation.style.whiteSpace = WhiteSpace.Normal; validation.style.marginTop = 10; panel.Add(validation);
            var resultPanel=GetComponent<ValidationPanel>();
            if(resultPanel!=null) { resultPanel.controller=controller; resultPanel.Attach(panel); }
            var notes = new Label("REFERENCE PATHS\nGrey: 1 AU   •   Amber: 2.7 AU target\nCyan: computed trajectory (last 4096 daily samples)\n\nPlanetary sizes are exaggerated.\nM2: approximate J2000 N-body gravity.\nM1 fixed-Sun mode remains selectable.\nNo relativity or planetary safety model.\n\nRight drag: orbit • Middle drag: pan\nWheel: zoom (outside dashboard)\n\n10,000-year transfer is a future mission objective.\nNo transfer or mission success is modeled here.");
            notes.style.whiteSpace = WhiteSpace.Normal; notes.style.marginTop = 12; panel.Add(notes);
            panel.Query<Button>().ForEach(b => {
                b.style.height = 28; b.style.minWidth = 0; b.style.flexGrow = 1;
                b.style.fontSize = 12; b.style.color = Color.white;
                b.style.backgroundColor = new Color(0.12f,0.23f,0.34f);
            });
            var speedInput = speed.Q<TextField>();
            if (speedInput != null) { speedInput.style.width = 60; speedInput.style.height = 24; speedInput.style.minHeight = 24; speedInput.style.flexGrow = 0; }
            panel.Query<VisualElement>(className:"unity-base-text-field__input").ForEach(input => {
                input.style.height = 24; input.style.minHeight = 24;
                input.style.backgroundColor = new Color(0.10f,0.16f,0.24f); input.style.color = Color.white;
            });
            metrics.style.whiteSpace = WhiteSpace.Normal;
            if(tractorInspection!=null) tractorInspection.Attach(root,scroll,viewBadge);
        }
        void Update()
        {
            if (controller == null || controller.Simulation == null || metrics == null) return;
            var s = controller.DisplayState;
            status.text = !controller.Started ? "READY" : controller.Running ? "RUNNING" : "PAUSED";
            start.SetEnabled(!controller.Started); pause.SetEnabled(controller.Running); resume.SetEnabled(controller.Started && !controller.Running);
            bool local=controller.view!=null && controller.view.IsEarthMoonView;
            solarView.SetEnabled(local); earthMoonView.SetEnabled(!local && controller.NBody!=null);
            viewStatus.text=controller.view!=null ? controller.view.ViewDescription : "SOLAR SYSTEM";
            metrics.text = $"Elapsed: {s.ElapsedSeconds/OrbitalMechanics.Day:F3} days\nYears (Julian): {s.ElapsedSeconds/OrbitalMechanics.JulianYear:F6}\nDistance: {s.Position.Magnitude/OrbitalMechanics.AU:F9} AU\nSpeed: {s.Velocity.Magnitude/1000:F6} km/s\nSpecific energy: {OrbitalMechanics.SpecificEnergy(s):E8} J/kg\nSpecific |h|: {OrbitalMechanics.AngularMomentum(s).Magnitude:E8} m²/s\nOsculating a: {OrbitalMechanics.SemiMajorAxis(s)/OrbitalMechanics.AU:F9} AU\nEccentricity: {OrbitalMechanics.Eccentricity(s):E5}\nFixed step: {controller.Simulation.StepSeconds:F0} s\nQueued simulation time: {controller.PendingSeconds/OrbitalMechanics.Day:F3} days";
            bool good = controller.MaxEnergyDrift < 1e-7 && controller.MaxMomentumDrift < 1e-10 && controller.MaxRadiusDrift < 2e-6;
            validation.text = $"LIVE NUMERICAL MONITOR: {(good ? "WITHIN BOUNDS" : "CHECK DRIFT")}\nMax relative Δenergy: {controller.MaxEnergyDrift:E3} (<1e-7)\nMax relative Δ|h|: {controller.MaxMomentumDrift:E3} (<1e-10)\nMax |r/AU − 1|: {controller.MaxRadiusDrift:E3} (<2e-6)\nThese are live monitors, not test-suite results.";
            if(controller.useNBody) validation.text="N-BODY / YOSHIDA4 / APPROXIMATE J2000\nSun + 8 planets + Moon. Natural orbital variations are physical; global conservation is monitored in the ledger.\nEarth heliocentric osculating elements use solar GM.\nNo propulsion, tractor or mirror.";
        }
    }
}
