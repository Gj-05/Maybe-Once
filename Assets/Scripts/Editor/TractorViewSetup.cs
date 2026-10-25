using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OctoEarth.Editor
{
    public static class TractorViewSetup
    {
        const string Folder="Assets/Prefabs/GravityTractor";
        [MenuItem("Tools/OctoEarth/Configure Tractor Inspection")]
        public static void Configure()
        {
            if(Application.isPlaying) throw new InvalidOperationException("Exit Play mode before configuring the tractor asset.");
            var controller=UnityEngine.Object.FindFirstObjectByType<OrbitController>();
            var views=UnityEngine.Object.FindFirstObjectByType<MoonInset>();
            if(controller==null || views==null) throw new InvalidOperationException("Open OctoEarth_Main with orbital views first.");
            System.IO.Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            var lit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OctoEarth/Earth.mat").shader;
            var unlit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OctoEarth/Reference.mat").shader;
            var rock=Material("Asteroid",lit,new Color(0.38f,0.34f,0.29f));
            var steel=Material("Structure",lit,new Color(0.3f,0.43f,0.5f));
            var gold=Material("Thrusters",lit,new Color(0.9f,0.55f,0.16f));
            var glow=Material("Identification",unlit,new Color(0.1f,0.8f,1));
            string path=Folder+"/GravityTractor.prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab==null)
            {
                var root=new GameObject("Gravity Tractor");
                var asteroid=Primitive("Asteroid mass",PrimitiveType.Sphere,root.transform,Vector3.zero,new Vector3(1.05f,0.82f,0.88f),rock);
                var mesh=UnityEngine.Object.Instantiate(asteroid.GetComponent<MeshFilter>().sharedMesh); mesh.name="Irregular asteroid";
                var vertices=mesh.vertices;
                for(int k=0;k<vertices.Length;k++) { var p=vertices[k]; float f=1+0.16f*Mathf.Sin(p.x*17+p.y*7)*Mathf.Cos(p.z*13-p.x*4)+0.08f*Mathf.Cos(p.y*23+p.z*8); vertices[k]=p*f; }
                mesh.vertices=vertices; mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh,Folder+"/AsteroidMesh.asset"); asteroid.GetComponent<MeshFilter>().sharedMesh=mesh;
                Primitive("Station keeping crossbeam",PrimitiveType.Cube,root.transform,new Vector3(0,-0.17f,0),new Vector3(0.14f,0.14f,1.3f),steel);
                foreach(int side in new[]{-1,1})
                {
                    var mount=new Vector3(0,-0.17f,side*0.63f);
                    var nozzle=Primitive("Canted thruster "+side,PrimitiveType.Cylinder,root.transform,mount,new Vector3(0.19f,0.2f,0.19f),gold);
                    var axis=new Vector3(-Mathf.Cos(25*Mathf.Deg2Rad),0,side*Mathf.Sin(25*Mathf.Deg2Rad));
                    nozzle.transform.rotation=Quaternion.FromToRotation(Vector3.up,axis);
                    var outlet=Primitive("Nozzle outlet "+side,PrimitiveType.Sphere,root.transform,mount+axis*0.205f,Vector3.one*0.15f,glow);
                }
                prefab=PrefabUtility.SaveAsPrefabAsset(root,path); UnityEngine.Object.DestroyImmediate(root);
            }
            var inspection=controller.GetComponent<TractorInspection>(); if(inspection==null) inspection=controller.gameObject.AddComponent<TractorInspection>();
            inspection.controller=controller; inspection.views=views; inspection.orbitCamera=views.orbitCamera;
            inspection.missionCsvPath="Results/mission_h3600_y10000/run_mission.csv";
            inspection.recording=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/tractor_diagnostic.csv");
            if(inspection.recording==null) throw new InvalidOperationException("Saved tractor diagnostic recording missing");
            if(inspection.tractor==null) inspection.tractor=((GameObject)PrefabUtility.InstantiatePrefab(prefab)).transform;
            inspection.tractor.name="Gravity Tractor";
            if(inspection.tractorLabel==null)
            {
                var go=new GameObject("Tractor Label"); go.transform.SetParent(inspection.tractor,false); go.transform.localPosition=new Vector3(0,1.6f,0);
                var label=go.AddComponent<TextMesh>(); label.text="GRAVITY TRACTOR"; label.characterSize=0.42f; label.fontSize=48; label.anchor=TextAnchor.MiddleCenter; label.color=new Color(0.5f,0.9f,1);
                go.AddComponent<CameraFacingLabel>(); inspection.tractorLabel=label;
            }
            if(inspection.separation==null)
            {
                var go=new GameObject("Earth-Tractor separation guide"); inspection.separation=go.AddComponent<LineRenderer>();
                inspection.separation.sharedMaterial=glow; inspection.separation.widthMultiplier=0.025f; inspection.separation.useWorldSpace=true;
            }
            inspection.tractor.gameObject.SetActive(false); inspection.separation.gameObject.SetActive(false); inspection.openOnPlay=true;
            EditorUtility.SetDirty(inspection); EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            EditorSceneManager.SaveScene(controller.gameObject.scene); AssetDatabase.SaveAssets();
            Selection.activeGameObject=inspection.tractor.gameObject;
            Debug.Log("Tractor prefab and mission CSV playback configured. Play opens Sun-centred transfer view; Return to baseline restores M2 orbit controls.");
        }
        static Material Material(string name,Shader shader,Color color)
        {
            string path=Folder+"/"+name+".mat"; var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null) { material=new Material(shader); material.SetColor("_BaseColor",color); AssetDatabase.CreateAsset(material,path); }
            return material;
        }
        static GameObject Primitive(string name,PrimitiveType type,Transform parent,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type); go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=material; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
    }
}
