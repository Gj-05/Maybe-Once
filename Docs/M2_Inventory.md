# M2 inventory and M3 prerequisite audit

**Subsequent approved resolution:** the user authorized independent 1e-9 analytic budget checks plus a separate 2% rounded-guide sanity test. Rerun 63519e37b51940019ab9d40daf426242 passed 29/29; current 30/30 regression includes G1. The original audit below is retained as historical evidence. Current M3 status is recorded in Docs/M3_Design.md and Results/m3_gate_status.json.

Inspected 2026-10-25 against the standalone M3 prompt. **M3 prerequisite not satisfied: 27/28 fast Unity tests pass.** No M3 mission code, configuration, scenes or success conditions have been implemented during this audit.

The supplied prompt section 0 requires: "run the tests, and confirm they pass before changing anything." Section 2 repeats the rounded force reference and a 1% comparison requirement. Its rule 9 says to trust computed numbers when they disagree. These requirements currently conflict at the literal average-force comparison. The result below is retained as a failure; no constants, expected values or tolerances were changed.

## Actual verification in this audit

- Unity 6000.3.0f1 connected through Unity MCP, instance OctoEarth-Simulator@2a0fa0e96f9cccbe.
- Unity EditMode job `a849e8b2855d44c0a22b2e7f43b01e66`: **28 total, 27 passed, 1 failed, 0 skipped**, 10.0276191 seconds test duration. Eight original M1 tests and ten numbered benchmark tests passed. Raw measured results are in `Results/m3_prerequisite_unity.json`.
- Only failing test: `OctoEarth.Tests.CoreTests.MissionBudgetReference`, row `X9.F`.
- No SDK listed by `dotnet --list-sdks`. The prompt's SDK assumption is not true on this machine. The existing no-download compiler fallback built the current Core/CLI successfully under C# 9, then `dotnet Tools/HeadlessBuild/OctoEarth.Cli.dll bench` actually ran and returned exit code 1 because of the same reference failure. It updated the normal `Results/benchmarks.json` and `Results/ejecta_ledger.txt` outputs. SDK/MSBuild/xUnit were not run.
- Unity Console before tests: zero errors or warnings. Active scene `Assets/Scenes/OctoEarth_Main.unity`, thirteen roots. The core contains no UnityEngine references.
- The explicit 10,000-year Slow test was not rerun in this prerequisite audit. Previous executed headless baseline remains in `Results/baseline_validation.json` and `Results/run_baseline_10000.csv`: energy 2.197799408716484e-12, momentum 7.30874606216296e-13, angular momentum 1.0662853070396398e-12, versus 1e-9. Those are prior-run results, not new measurements here.

## Reference discrepancy

Using the unmodified supplied constants:

```
mu = 1.32712440018e20 m^3/s^2
M = 6.045e24 kg
a1 = 149597870700 m
a2 = 2.7 * a1
T = 10000 * 31557600 s
delta_v = sqrt(mu/a1) - sqrt(mu/a2) = 11658.30562662991 m/s
p = M * delta_v = 7.047445751297781e28 N s
F_avg = p / T = 2.233200798317293e17 N
literal reference = 2.2e17 N
absolute difference = 3.320079831729312e15 N
allowed difference = 2.2e15 N (1%)
relative difference = 1.5091271962405964%
```

This is an algebraic/rounding inconsistency, not timestep error. Reducing dt or changing integrator cannot resolve it. A concrete specification correction would use a sufficiently precise force reference, e.g. 2.2332e17 N, retaining 1%. This has **not** been applied. Treating the rounded 2.2e17 as a displayed estimate rather than a literal acceptance bound is another specification decision; it is not silently inferred here. M3 stays gated pending resolution.

## Assemblies and source inventory

`Assets/Scripts/Core/OctoEarth.Core.asmdef`: namespace OctoEarth.Core, noEngineReferences=true. All authoritative physics uses double.

| Source file in Core | Current role and reuse constraints |
|---|---|
| Vector3d.cs | Double vectors, arithmetic, dot/cross, magnitude, normalization |
| OrbitalState.cs | Position, velocity, elapsed time value type |
| OrbitalMechanics.cs | Preserved M1 fixed-Sun circular reference and diagnostics; historical M1 GM retained |
| OrbitSimulation.cs | Preserved M1 fixed-step velocity Verlet and exact reset |
| Constants.cs | R2 SI constants, source comments, Units, KahanSum/KahanVector |
| SystemState.cs | Body, SystemState, barycentric centering, compensated pairwise GravityModel, IForceModel |
| Integrators.cs | IIntegrator, VelocityVerlet, Yoshida4 gravity composition, deterministic SimulationClock |
| KeplerOrbit.cs | Elliptic elements/state conversion, energy, eccentricity, period, binary initialization |
| InitialConditions.cs | Approximate J2000 data loading, Earth-Moon split, barycentre and Moon-bound diagnostics |
| Engineering.cs | Lorentz, Beam stress/deflection/buckling/support, Insolation, Maneuvers, MissionBudget |
| Ledger.cs | Body-plus-source/sink accounting, impulse checks, reports, PhotonSink booking |
| EjectaDrive.cs | Replenished or finite-rocket toy kicks, parcel quadrature, EjectaSink/EnablerSource/jet energy input, bound-ejecta flag; ProgradeAcceleration benchmark force |
| Encke.cs | RK4 deviation about analytic Kepler reference; day-long tiny-push validation; no long-run rectification |
| SimulationRunner.cs | State/integrator/forces/ledger ownership, reset, monitor/logger hooks, CSV/JSON output |
| BenchmarkSuite.cs | Ten numbered benchmarks, measured rows, JSON writer |
| ValidationSuite.cs | Additional conservation/rocket/convergence/precision/budget/binding/reset tests, Slow baseline |

`Assets/Scripts/Runtime/OctoEarth.Viewer.asmdef`: references Core and Input System. Existing MonoBehaviour script GUIDs preserved.

| Viewer source | Role |
|---|---|
| OrbitController.cs | Thin scheduling/visual wrapper over Core, legacy or N-body baseline selection, trails |
| OrbitalCoordinates.cs | Sun-centred conversion and double-subtraction local floating origin |
| OrbitCamera.cs | Orbit, pan, zoom; explicit Solar System and Earth-Moon framing |
| MoonInset.cs | Legacy serialized class name; now switches main views with the single Earth object, not an inset |
| MissionDashboard.cs | UI Toolkit controls, numerical readouts, explicit view selection and scale caption |
| ValidationPanel.cs | Recorded benchmark JSON display, live N-body ledger and lunar binding |
| CameraFacingLabel.cs | Visual billboard labels |

`Assets/Scripts/Editor/OctoEarth.Editor.asmdef`: references Core, Viewer and URP.

- OctoEarthSceneBuilder.cs: M1 scene creation, guarded against overwriting.
- Milestone2Tools.cs: shared benchmark menu and viewer upgrade.
- OrbitalViewSetup.cs: explicit focus-view scene setup, removal of obsolete duplicate visual, scene save.

`Assets/Tests/EditMode/OctoEarth.Tests.EditMode.asmdef`: references Core and Test Assemblies.

- OrbitalTests.cs: eight original M1 regressions.
- CoreTests.cs: ten benchmark methods, ten additional fast test methods, one explicit Slow test. Conditional attributes allow the same source to be linked into xUnit.

## Headless tools

- `Tools/OctoEarth.Core.Shared/OctoEarth.Core.Shared.csproj`: netstandard2.1; links the same Core source files.
- `Tools/OctoEarth.Cli/OctoEarth.Cli.csproj`: net8.0 console.
- `Tools/OctoEarth.Cli/Program.cs`: commands `bench`, `baseline --years --dt`, `converge`. No mission or all-experiments command exists yet.
- `Tools/OctoEarth.Cli.Tests/OctoEarth.Cli.Tests.csproj`: net8.0 xUnit, shared CoreTests.cs; not executed because SDK is absent.
- `Tools/Build-HeadlessWithoutSdk.ps1`: installed Unity compiler plus .NET 8.0.12 runtime; no download; executable outputs in Tools/HeadlessBuild.

## Scene, data and existing results

`Assets/Data/solar_j2000.csv`: approximate=true, J2000 epoch, JPL Table 1 elements and sourced GM values. Moon state is explicitly idealized circular/coplanar, not a Horizons ephemeris.

`OctoEarth_Main` roots: Sun, Earth, Simulation, 1 AU Reference, 2.7 AU Target Reference, Computed Trajectory, Main Camera, Directional Light, Mission Dashboard, Sun Label, Target Label, Scale Label, Orbital Views. Orbital Views owns Moon and Moon Orbit. There is one Earth visual. The two explicit focus views replace the old floating diagram; keep this accepted interaction when adding mission playback.

Scene components: OrbitController; Camera/AudioListener/URP data/OrbitCamera; UIDocument/MissionDashboard/ValidationPanel; MoonInset presentation manager; mesh renderers, line renderers, labels. Materials are in Assets/Materials/OctoEarth, UI Toolkit assets in Assets/UI. SampleScene remains separate and preserved.

Existing evidence: `Results/benchmarks.json`, `Results/unity-editmode.json`, `Results/baseline_validation.json`, `Results/run_baseline_10000.csv` and JSON, `Results/convergence.json`/CSV, `Results/ejecta_ledger.txt`; explanatory files README.md, LOG.md, Docs/M1_Inventory.md, Docs/M2_Report.md, Docs/M2_BenchmarkTable.md and Docs/M2_Files.txt. This audit adds this inventory and `Results/m3_prerequisite_unity.json`.

## Scope that is not yet validated for M3

The current ejecta module is tested on toy burns with frozen exported parcels. It does not yet model canted pairs, plume geometry, array/mirror station-keeping or mission energy collection. Ejecta bound checks do not yet require an outgoing radial velocity. Ledger source/sink accumulation is not yet compensated over long thrust runs (body gravity sums are compensated). Yoshida4 applies negative substeps only to conservative gravity; the current runner splits external positive half-kicks, making the forced composition second-order. M3's requested kick callbacks must be designed without implying physically negative ejected mass. The present Encke model has no long-run rectification. The current Unity viewer integrates the baseline live; it is not yet a saved mission CSV player.

No tractor, focusing optics, solar array, EBM climate, placeholder thresholds, enabler configuration, M3 experiment sweeps, winning checklist or G1-G8 gate has been run or claimed. These remain behind the prerequisite gate.
