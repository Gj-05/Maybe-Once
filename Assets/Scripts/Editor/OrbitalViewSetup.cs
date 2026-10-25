using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
namespace OctoEarth.Editor
{
    public static class OrbitalViewSetup
    {
        [MenuItem("Tools/OctoEarth/Configure Orbital Views")]
        public static void Configure()
        {
            var controller=Object.FindFirstObjectByType<OrbitController>();
            if(controller==null) throw new System.InvalidOperationException("Open OctoEarth_Main first");
            var view=Object.FindFirstObjectByType<MoonInset>();
            if(view==null) view=new GameObject("Orbital Views").AddComponent<MoonInset>();
            view.gameObject.name="Orbital Views"; view.controller=controller; controller.view=view;
            view.orbitCamera=Object.FindFirstObjectByType<OrbitCamera>();
            // Remove obsolete diagram visuals through Unity undo; retain the single main Earth.
            foreach(var child in view.GetComponentsInChildren<Transform>(true))
                if(child!=view.transform && (child.name.StartsWith("Inset Earth") || child.name=="Inset scale label")) Undo.DestroyObjectImmediate(child.gameObject);
            if(view.moon==null)
            {
                var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere); sphere.transform.SetParent(view.transform); view.moon=sphere.transform;
                Object.DestroyImmediate(sphere.GetComponent<Collider>());
                var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OctoEarth/Moon.mat");
                if(material==null) { material=new Material(Shader.Find("Universal Render Pipeline/Lit")); material.SetColor("_BaseColor",new Color(0.6f,0.62f,0.65f)); AssetDatabase.CreateAsset(material,"Assets/Materials/OctoEarth/Moon.mat"); }
                sphere.GetComponent<Renderer>().sharedMaterial=material;
            }
            view.moon.name="Moon";
            if(view.orbit==null) { var line=new GameObject("Moon Orbit"); line.transform.SetParent(view.transform); view.orbit=line.AddComponent<LineRenderer>(); }
            view.orbit.name="Moon Orbit"; view.orbit.widthMultiplier=0.055f; view.orbit.useWorldSpace=true;
            view.orbit.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OctoEarth/Reference.mat");
            var label=view.moon.Find("Moon Label");
            if(label==null)
            {
                var o=new GameObject("Moon Label"); o.transform.SetParent(view.moon,false); label=o.transform;
                var mesh=o.AddComponent<TextMesh>(); mesh.text="MOON"; mesh.characterSize=0.25f; mesh.fontSize=40; mesh.anchor=TextAnchor.MiddleCenter; o.AddComponent<CameraFacingLabel>();
            }
            label.localPosition=new Vector3(0,1.8f,0); label.localScale=Vector3.one/0.38f;
            var solar=new System.Collections.Generic.List<GameObject>();
            foreach(string name in new[]{"Sun","1 AU Reference","2.7 AU Target Reference","Computed Trajectory","Sun Label","Target Label","Scale Label"}) { var go=GameObject.Find(name); if(go!=null) solar.Add(go); }
            view.solarObjects=solar.ToArray(); view.SetVisible(false);
            EditorUtility.SetDirty(view); EditorUtility.SetDirty(controller); EditorUtility.SetDirty(view.orbitCamera);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene); EditorSceneManager.SaveScene(controller.gameObject.scene); AssetDatabase.SaveAssets();
        }
    }
}
