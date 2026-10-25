using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OctoEarth.Editor
{
    public static class OctoEarthSceneBuilder
    {
        const string ScenePath="Assets/Scenes/OctoEarth_Main.unity";
        [MenuItem("OctoEarth/Create Milestone 1 Scene")]
        public static void Create()
        {
            if (System.IO.File.Exists(ScenePath)) throw new InvalidOperationException("OctoEarth_Main already exists; open it instead of overwriting.");
            if (EditorSceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the existing scene before creating a new one.");
            System.IO.Directory.CreateDirectory("Assets/Materials/OctoEarth");
            System.IO.Directory.CreateDirectory("Assets/UI"); AssetDatabase.Refresh();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.skybox=null; RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=new Color(0.15f,0.19f,0.27f);
            var sunMat=Material("Sun",new Color(1,0.68f,0.15f),true);
            sunMat.shader=Shader.Find("Universal Render Pipeline/Lit"); sunMat.EnableKeyword("_EMISSION"); sunMat.SetColor("_EmissionColor",new Color(2,1.36f,0.3f));
            var earthMat=Material("Earth",new Color(0.12f,0.4f,0.85f),false);
            var lineMat=Material("Reference",new Color(0.28f,0.38f,0.48f),true);
            var targetMat=Material("Target",new Color(0.8f,0.48f,0.13f),true);
            var trailMat=Material("Trajectory",new Color(0.05f,0.9f,1),true);
            var sun=Sphere("Sun",Vector3.zero,4f,sunMat);
            var earth=Sphere("Earth",new Vector3(20,0,0),2f,earthMat);
            var controller=new GameObject("Simulation").AddComponent<OrbitController>(); controller.earth=earth.transform;
            Ring("1 AU Reference",20,lineMat); Ring("2.7 AU Target Reference",54,targetMat);
            controller.trajectory=Line("Computed Trajectory",trailMat,0.2f);
            var cameraObject=new GameObject("Main Camera"); cameraObject.tag="MainCamera";
            var camera=cameraObject.AddComponent<Camera>(); camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(0.003f,0.007f,0.018f);
            camera.nearClipPlane=0.1f; camera.farClipPlane=1000; camera.fieldOfView=50;
            cameraObject.AddComponent<AudioListener>(); cameraObject.AddComponent<UniversalAdditionalCameraData>();
            var orbit=cameraObject.AddComponent<OrbitCamera>(); orbit.focus=new Vector3(-34,0,0);
            var rotation=Quaternion.Euler(orbit.pitch,orbit.yaw,0); camera.transform.SetPositionAndRotation(orbit.focus-rotation*Vector3.forward*orbit.distance,rotation);
            var light=new GameObject("Directional Light").AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.5f; light.transform.rotation=Quaternion.Euler(45,-30,0); light.gameObject.AddComponent<UniversalAdditionalLightData>();
            var panel=ScriptableObject.CreateInstance<PanelSettings>(); panel.scaleMode=PanelScaleMode.ConstantPixelSize;
            AssetDatabase.CreateAsset(panel,"Assets/UI/OctoEarthPanelSettings.asset");
            // Runtime UI requires the default theme; persist a theme asset for builds too.
            System.IO.File.WriteAllText("Assets/UI/OctoEarthTheme.tss","@import url(\"unity-theme://default\");"); AssetDatabase.ImportAsset("Assets/UI/OctoEarthTheme.tss");
            panel.themeStyleSheet=AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/UI/OctoEarthTheme.tss"); EditorUtility.SetDirty(panel);
            var dashboard=new GameObject("Mission Dashboard"); var doc=dashboard.AddComponent<UIDocument>(); doc.panelSettings=panel;
            dashboard.AddComponent<MissionDashboard>().controller=controller;
            // Recognizable blue planet, using a small deterministic procedural albedo asset.
            var texture=new Texture2D(256,128,TextureFormat.RGB24,false);
            for(int y=0;y<128;y++) for(int x=0;x<256;x++)
            {
                float lon=x/256f*Mathf.PI*2, lat=(y/127f-0.5f)*Mathf.PI;
                float land=Mathf.Sin(lon*3+Mathf.Sin(lat*5))*Mathf.Cos(lat*3)+0.45f*Mathf.Sin(lon*7+lat*6);
                Color c=land>0.45f ? new Color(0.18f,0.43f,0.24f) : new Color(0.025f,0.19f,0.56f);
                if(Mathf.Abs(lat)>1.25f) c=new Color(0.85f,0.94f,1);
                texture.SetPixel(x,y,c);
            }
            texture.Apply(); AssetDatabase.CreateAsset(texture,"Assets/Materials/OctoEarth/EarthAlbedo.asset"); earthMat.SetColor("_BaseColor",Color.white); earthMat.SetTexture("_BaseMap",texture); EditorUtility.SetDirty(earthMat);
            Label("Sun Label","SUN",new Vector3(0,3,0),camera);
            Label("Earth Label","EARTH (size exaggerated)",new Vector3(20,2,0),camera,earth.transform);
            Label("Target Label","2.7 AU REFERENCE / NO TRANSFER",new Vector3(15,0,54),camera);
            Label("Scale Label","Planetary sizes exaggerated • 1 AU = 20 units",new Vector3(0,0,-58),camera);
            EditorSceneManager.SaveScene(scene,ScenePath); AssetDatabase.SaveAssets();
            Selection.activeGameObject=controller.gameObject;
            Debug.Log("OctoEarth Milestone 1 scene created; baseline only.");
        }
        static Material Material(string name,Color color,bool unlit)
        {
            var shader=Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
            if(shader==null) throw new InvalidOperationException("Required URP shader missing.");
            var m=new Material(shader); m.SetColor("_BaseColor",color);
            if(unlit) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor",color*2); }
            AssetDatabase.CreateAsset(m,"Assets/Materials/OctoEarth/"+name+".mat"); return m;
        }
        static GameObject Sphere(string name,Vector3 position,float diameter,Material material)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Sphere); o.name=name; o.transform.position=position; o.transform.localScale=Vector3.one*diameter;
            o.GetComponent<Renderer>().sharedMaterial=material; UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>()); return o;
        }
        static LineRenderer Line(string name,Material material,float width)
        {
            var line=new GameObject(name).AddComponent<LineRenderer>(); line.sharedMaterial=material; line.widthMultiplier=width; line.useWorldSpace=true; line.positionCount=0; return line;
        }
        static void Ring(string name,float radius,Material material)
        {
            var line=Line(name,material,0.3f); line.loop=true; line.positionCount=360;
            for(int i=0;i<360;i++) { float a=i*Mathf.PI*2/360; line.SetPosition(i,new Vector3(radius*Mathf.Cos(a),-0.08f,radius*Mathf.Sin(a))); }
        }
        static void Label(string name,string text,Vector3 position,Camera camera,Transform parent=null)
        {
            var o=new GameObject(name); o.transform.position=position; o.transform.rotation=camera.transform.rotation;
            if(parent!=null) o.transform.SetParent(parent,true);
            var mesh=o.AddComponent<TextMesh>(); mesh.text=text; mesh.characterSize=1.2f; mesh.fontSize=40; mesh.anchor=TextAnchor.MiddleCenter; mesh.color=new Color(0.7f,0.85f,1);
            o.AddComponent<CameraFacingLabel>();
        }
    }
}
