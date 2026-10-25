using UnityEngine;
using OctoEarth.Core;
namespace OctoEarth
{
    // Legacy class/file name retained to preserve the scene component GUID.
    // This now switches the main view; it never renders an inset or a duplicate Earth.
    [DefaultExecutionOrder(50)]
    public sealed class MoonInset : MonoBehaviour
    {
        public OrbitController controller;
        public OrbitCamera orbitCamera;
        public Transform moon;
        public LineRenderer orbit;
        public GameObject[] solarObjects;
        public bool visible=false;
        public bool IsEarthMoonView => visible;
        public const double LocalUnitsPerMetre=8.0/Constants.MoonDistance;
        Vector3 solarEarthScale;
        TextMesh earthLabel;
        Vector3 labelPosition,labelScale;
        float labelSize;
        bool initialized;
        public string ViewDescription => visible
            ? "EARTH-MOON VIEW | Earth-centred\nDistance scale: 384,400 km = 8 units\nBody diameters enlarged: Earth 1.1 units; Moon 0.38 units"
            : "SOLAR SYSTEM | Sun-centred\nDistance scale: 1 AU = 20 units\nBody sizes exaggerated. Moon available in Earth-Moon view.";
        void Initialize()
        {
            if(initialized || controller==null || controller.earth==null) return;
            initialized=true; solarEarthScale=controller.earth.localScale;
            earthLabel=controller.earth.GetComponentInChildren<TextMesh>(true);
            if(earthLabel!=null) { labelPosition=earthLabel.transform.localPosition; labelScale=earthLabel.transform.localScale; labelSize=earthLabel.characterSize; }
        }
        void Awake() { Initialize(); SelectSolarSystem(); }
        public void SelectSolarSystem() { SetVisible(false); }
        public void SelectEarthMoon() { if(controller!=null && controller.useNBody && controller.NBody!=null) SetVisible(true); }
        public void SetVisible(bool value)
        {
            Initialize(); visible=value && controller!=null && controller.NBody!=null;
            if(orbitCamera!=null) { if(visible) orbitCamera.FrameEarthMoon(); else orbitCamera.FrameSolarSystem(); }
            RenderFrame();
        }
        void LateUpdate()
        {
            if(visible && (controller==null || controller.NBody==null)) SelectSolarSystem();
            RenderFrame();
        }
        void RenderFrame()
        {
            Initialize(); if(!initialized) return;
            if(solarObjects!=null) foreach(var go in solarObjects) if(go!=null) go.SetActive(!visible);
            if(moon!=null) moon.gameObject.SetActive(visible);
            if(orbit!=null) orbit.gameObject.SetActive(visible);
            if(!visible)
            {
                controller.earth.localScale=solarEarthScale;
                if(controller.Simulation!=null) controller.earth.position=OrbitalCoordinates.ToUnity(controller.DisplayState.Position);
                if(earthLabel!=null) { earthLabel.text="EARTH (size exaggerated)"; earthLabel.characterSize=labelSize; earthLabel.transform.localPosition=labelPosition; earthLabel.transform.localScale=labelScale; }
                return;
            }
            var s=controller.NBody.State; var earth=s.Bodies[s.Index("Earth")]; var lunar=s.Bodies[s.Index("Moon")];
            controller.earth.position=Vector3.zero; controller.earth.localScale=Vector3.one*1.1f;
            if(earthLabel!=null) { earthLabel.text="EARTH"; earthLabel.characterSize=0.25f; earthLabel.transform.localScale=Vector3.one/1.1f; earthLabel.transform.localPosition=new Vector3(0,0.95f,0); }
            if(moon!=null) { moon.position=OrbitalCoordinates.RelativeTo(lunar.Position,earth.Position,LocalUnitsPerMetre); moon.localScale=Vector3.one*0.38f; }
            if(orbit!=null)
            {
                var relative=lunar.Position-earth.Position;
                var el=KeplerOrbit.ToElements(relative,lunar.Velocity-earth.Velocity,Constants.G*(earth.Mass+lunar.Mass));
                double mu=Constants.G*(earth.Mass+lunar.Mass); orbit.positionCount=128; orbit.loop=true;
                for(int k=0;k<128;k++) { var p=KeplerOrbit.FromElements(el,mu,k*KeplerOrbit.Period(el.A,mu)/128).Position;
                    orbit.SetPosition(k,OrbitalCoordinates.RelativeTo(p,new Vector3d(),LocalUnitsPerMetre)); }
            }
        }
    }
}
