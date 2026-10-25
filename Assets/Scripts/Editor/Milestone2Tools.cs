using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using OctoEarth.Core;
namespace OctoEarth.Editor
{
    public static class Milestone2Tools
    {
        [MenuItem("Tools/OctoEarth/Run Benchmarks")]
        public static void RunBenchmarks()
        {
            var rows=BenchmarkSuite.Run(Path.Combine(Application.dataPath,"Data/solar_j2000.csv")); rows.AddRange(ValidationSuite.Run(Path.Combine(Application.dataPath,"Data/solar_j2000.csv")));
            string file=Path.GetFullPath(Path.Combine(Application.dataPath,"../Results/benchmarks.json")); BenchmarkSuite.Write(file,rows);
            foreach(var row in rows) Debug.Log(row.ToString());
            Debug.Log("M2 checks written to "+file+". Read failures; this is not a mission result.");
        }
        [MenuItem("Tools/OctoEarth/Upgrade Viewer to M2")]
        public static void UpgradeViewer()
        {
            var controller=UnityEngine.Object.FindFirstObjectByType<OrbitController>(); if(controller==null) throw new InvalidOperationException("Open OctoEarth_Main first");
            controller.useNBody=true;
            var dash=UnityEngine.Object.FindFirstObjectByType<MissionDashboard>(); if(dash.GetComponent<ValidationPanel>()==null) dash.gameObject.AddComponent<ValidationPanel>().controller=controller;
            OrbitalViewSetup.Configure();
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene); EditorSceneManager.SaveScene(controller.gameObject.scene); AssetDatabase.SaveAssets();
        }
    }
}
