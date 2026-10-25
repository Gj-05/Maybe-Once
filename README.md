# OctoEarth Simulator — Milestone 2

**The default Unity view now plays back the M3 mission CSV; the live M2 baseline remains separately selectable.** G1-G3 have measured passing results. The latest fast regression passed 32/32, including mission playback tests. Precise budget acceptance remains independently evaluated at 1e-9 relative; rounded guide figures remain a separate 2% sanity check. Read `Results/m3_gate_status.json` and `Docs/M3_Design.md`; playback is diagnostic and does not imply mission acceptance or a WIN.

## Run with Unity

Open this project in Unity 6000.3.0f1 and `Assets/Scenes/OctoEarth_Main.unity`, then press Play. **Play mission** advances the recorded mission; the year slider scrubs its full available duration. **Sun / transfer**, **Earth + tractor** and **Focus tractor** share one playback time and Earth instance. Speeds extend to 1e9 simulated seconds per real second. The screen names the loaded CSV, elapsed years, semi-major axis, thrust and visual enlargement factors. The tractor has a visible solar-view callout whose leader ends at its true recorded position, a pull direction arrow and a close inspection asset. The cyan line follows the last two orbits from the mission CSV. Positions between samples use Hermite interpolation for display; a/thrust readouts interpolate linearly. Right drag orbits, middle drag pans and the wheel zooms; scroll the dashboard for scale details and notes.

**Return to M2 baseline (no transfer)** exposes the original Sun + eight planets + Moon live baseline controls (Yoshida4, 3600 s); the M1 fixed-Sun option remains available there. This baseline is paused while mission playback is active. Its Solar System / Earth-Moon views and validation panel remain available. No live mission integration occurs inside the playback viewer.

`Tools > OctoEarth > Run Benchmarks` executes the shared core suite and writes that JSON. Unity Test Runner EditMode runs the M1 regression class and CoreTests. The Slow test is explicit; do not select it in an ordinary interactive run. For batch mode, close the interactive Editor first to release the project lock:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Unity.exe' -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testFilter OctoEarth.Tests.OrbitalTests -testResults Results/editmode.xml -logFile Results/unity-batch.log
```

For the complete fast suite, use the test list in `Results/unity-editmode.json` (all except Slow10000YearConservation), or select those tests in Test Runner. MCP assembly-only selection may explicitly select slow tests; use explicit fast test names.

## Run without Unity graphics or the Editor

With .NET 8 SDK:

```powershell
dotnet build Tools/OctoEarth.Cli/OctoEarth.Cli.csproj
dotnet test Tools/OctoEarth.Cli.Tests/OctoEarth.Cli.Tests.csproj
dotnet run --project Tools/OctoEarth.Cli -- bench
dotnet run --project Tools/OctoEarth.Cli -- baseline --years 10000
dotnet run --project Tools/OctoEarth.Cli -- converge
```

Run from the repository root. `--data <CSV>` selects different initial-condition data; `baseline --dt <seconds>` configures the timestep. `bench` returns exit code 1 while any comparison fails. `baseline` is Sun–Earth–Moon, not the full planetary model; B5 and the ten-year Moon check include all planets. The shared project links the exact Core source files (netstandard2.1). The CLI is net8.0. The xUnit project links the same CoreTests source used by NUnit.

This machine has .NET runtimes but **no SDK**. `dotnet build` and `dotnet test` were attempted and could not run. The following no-download fallback was compiled and actually executed using Unity's bundled C# 9 compiler, with no Unity references in the resulting program:

```powershell
./Tools/Build-HeadlessWithoutSdk.ps1
dotnet Tools/HeadlessBuild/OctoEarth.Cli.dll bench
dotnet Tools/HeadlessBuild/OctoEarth.Cli.dll baseline --years 10000
dotnet Tools/HeadlessBuild/OctoEarth.Cli.dll converge
```

The fallback script targets the installed .NET 8.0.12 runtime and accepts `-UnityData` for the compiler location. It does not validate SDK/MSBuild or xUnit package restoration. Unity NUnit tests were run separately. No separate Unity batchmode process was launched against the already-open project.

## Evidence and limits

- [M1 inventory](Docs/M1_Inventory.md), [M2 implementation and limitations](Docs/M2_Report.md).
- [All measured checks](Docs/M2_BenchmarkTable.md), generated from [benchmarks.json](Results/benchmarks.json).
- [Unity test results](Results/unity-editmode.json): 27 passed, 1 failed (R3 rounding discrepancy).
- [10,000-year baseline validation](Results/baseline_validation.json); drift CSV `Results/run_baseline_10000.csv` and metadata JSON.
- [Convergence](Results/convergence.json), with numeric samples in `Results/convergence.csv`.
- [Budget correction regression](Results/budget_approval_unity.json), [G1 regression](Results/m3_g1_regression_unity.json), [M3 gate status](Results/m3_gate_status.json). Historical failed-reference result files remain as audit evidence.

The currently implemented staged commands are `g1`, `g2 --dt 300` (also run at 150s), and `g3 --dt 3600` (also run at 1800s). Use the same SDK/fallback CLI prefixes above. They write gate-specific CSV/JSON/ledger evidence. `./Tools/Plot-Gates.ps1` regenerates static figures from completed gate CSVs with the installed .NET chart library; `./Tools/Update-GateReport.ps1` collects actual result statuses. Plot files are in Results/figures. No mirror/array/climate gate is implied by these mechanical checks.

All scientific state is double precision. Gravity is pairwise Newtonian with compensated acceleration summation and equal partner terms. Yoshida4 is fourth-order only for the conservative subsystem; external positive-time half-kicks make the forced composition second-order. Open-system ejecta accounting includes EjectaSink, supplied mass/momentum/potential/kinetic energy, and explicit jet energy input. It is a toy tested force module, not a mission energy source.

Initial conditions are **approximate J2000**, from [JPL's element table](https://ssd.jpl.nasa.gov/planets/approx_pos.html), with a deliberately circular, coplanar lunar state. GM data use [JPL](https://ssd.jpl.nasa.gov/astro_par.html) except the supplied R2 overrides. No Horizons ephemeris was downloaded. This is not a validated 10,000-year real ephemeris or proof of safety. The M3 diagnostic includes finite infrastructure, source/sink accounting, illustrative structure/climate checks and placeholder thresholds. Unresolved engineering flags, including organiser approval for giant focusing optics, block a WIN.
# Tractor asset and inspection

Open `Assets/Scenes/OctoEarth_Main.unity` and press Play. It opens the Sun-centred mission playback. Use **Focus tractor** for the asteroid and two schematic canted thrusters; **Earth + tractor** shows their recorded relative positions in inertial display axes. Switching views preserves time.

The reusable asset is `Assets/Prefabs/GravityTractor/GravityTractor.prefab`; its scene instance is **Gravity Tractor**. The primary source is `Results/mission_h3600_y10000/run_mission.csv`. **Reload CSV** reads newer saved output. If that file is missing, the viewer explicitly labels the 100-year embedded diagnostic fallback `Assets/Data/tractor_diagnostic.csv`. This playback does not constitute an accepted G4/G8 gate. Core physics receives no Unity display coordinates.

