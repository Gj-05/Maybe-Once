using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using OctoEarth.Core;
namespace OctoEarth
{
    public sealed class ValidationPanel : MonoBehaviour
    {
        [Serializable] public sealed class Report { public int schema; public bool approximate; public BenchmarkResult[] results; }
        Label rows,ledger;
        Foldout details;
        double nextRead;
        DateTime lastWrite;
        public OrbitController controller;
        public void Attach(VisualElement parent)
        {
            ledger=new Label(); ledger.style.whiteSpace=WhiteSpace.Normal; parent.Add(ledger);
            details=new Foldout {text="Validation: results not read",value=false}; parent.Add(details);
            rows=new Label("NOT RUN / results file unavailable"); rows.style.whiteSpace=WhiteSpace.Normal; details.Add(rows); nextRead=0;
        }
        void Update()
        {
            if(rows==null) return;
            if(Time.realtimeSinceStartupAsDouble>=nextRead)
            {
                nextRead=Time.realtimeSinceStartupAsDouble+2;
                string file=Path.GetFullPath(Path.Combine(Application.dataPath,"../Results/benchmarks.json"));
                if(!File.Exists(file)) rows.text="NOT RUN / results file unavailable";
                else if(File.GetLastWriteTimeUtc(file)!=lastWrite)
                {
                    lastWrite=File.GetLastWriteTimeUtc(file);
                    try
                    {
                        var report=JsonUtility.FromJson<Report>(File.ReadAllText(file));
                        if(report.schema!=1 || report.results==null || report.results.Length==0) throw new InvalidDataException("Empty/unsupported result schema");
                        var text=new StringBuilder("Recorded results, not a live test run\n");
                        int failed=0; foreach(var r in report.results) if(!r.passed) failed++;
                        details.text=$"Validation: {report.results.Length-failed} passed / {failed} failed (file)";
                        foreach(var r in report.results) text.Append($"{r.id}: {(r.passed?"PASS":"FAIL")}\nmeasured {r.measured:G6} {r.unit}\nerror {r.error:E2} / tolerance {r.tolerance:E2}\n");
                        rows.text=text.ToString();
                    }
                    catch(Exception e) { rows.text="INVALID RESULTS: "+e.Message; }
                }
            }
            if(controller!=null && controller.NBody!=null)
            {
                var l=controller.NBody.Ledger; bool bound=InitialConditions.MoonBound(controller.NBody.State,out double binding,out double hill);
                ledger.text=$"LIVE N-BODY LEDGER\nMax ΔP / initial gross P: {l.MaxMomentumError:E4}\nMax ΔL / initial gross L: {l.MaxAngularMomentumError:E4}\nMax ΔE / |initial E|: {l.MaxEnergyError:E4}\nMoon bound: {bound}\nLunar binding: {binding:E4} J/kg\nHill radius: {hill:E4} m\nNo push / no ejecta in viewer baseline.";
            }
            else ledger.text="M1 fixed-Sun mode: no N-body ledger.";
        }
    }
}
