# Planetary Propulsion Simulation — pitch facts

Target reached: N for selected design; Y only for earlier original layout (§7).
Earth safety: N demonstrated; selected structural failure and unvalidated safety criteria (§8).
Structure >= 1.5: N; selected recorded minimum 0.13895264363665349 dimensionless (§9).
Benchmarks passed: 10/10 benchmark groups; fresh comparisons in §10.
Unresolved flags: 4 flags in selected prefixes; all non-enabler (§8).
Ledger residual: selected h=3600 s maximum relative P 6.7728589335338011E-13 dimensionless (§6).
Convergence difference: maximum |delta a| 0.00015072388507064716 AU, partial interval only (§10).
Energy source: modelled solar collection; no declared energy-source enabler (§11).
Enablers: 3 declarations, E1/E2/E3 (§3).
Mission verdict: FAIL; selected full-duration arrival and departure NOT OBSERVED (§7).

## 1. PROJECT SUMMARY

**OctoEarth Simulator / Planetary Propulsion Simulation:** a Unity demonstration of a solar-powered remote gravity tractor intended to move the Earth–Moon system outward. The tractor pulls Earth and Moon through reciprocal Newtonian gravity. Canted reaction-mass jets keep the tractor near its commanded hover site; exhaust receives the reaction momentum. Solar collection powers the jets; finite mirror infrastructure attempts to restore insolation. Sources: `Assets/Scripts/Core/GravityTractor.cs` (`GravityTractor.Planetary`, `TractorHoverDrive.Target`), `MissionConstraintStepper.cs` (`Step`, `Kick`), `MissionRadiation.cs` (`Evaluate`). Core file names below are relative to `Assets/Scripts/Core/` unless a full path is supplied.

| Parameter | Selected design | Original completed layout | Source |
|---|---:|---:|---|
| Initial tractor mass | 1.0098073172297392E+19 kg | 1.6104890014987876E+18 kg | selected CSV `tractor_mass_kg`; `Results/diagnosis_initial_audit.json`; `GravityTractor.SizeMass` |
| Earth-centre hover separation | 125000000 m | 50000000 m | `Config/mission_design_candidate.json`, original `mission_report.json`; `TractorHoverDrive.Target` |
| Exhaust speed | 300000 m/s | 300000 m/s | same configs; `EjectaDrive.KickCanted` |
| Cant angle | 25 degrees | 25 degrees | same configs |
| Sunward bias | 5 degrees | 0 degrees | same configs; `TractorHoverDrive.Target` |
| Nominal thrust sizing margin | 1.15 dimensionless | 1.15 dimensionless | `GravityTractor.Planetary` |

No exhaust-speed or hover-distance sweep was found. The implemented siting screen varies array radius, inclination, phase and co-motion (§7); it is not a propellant trade study.

## 2. SECTION-3 ESTIMATES

Sizing uses `Constants.PairMass = 6.045E+24 kg`, an explicitly rounded Earth+Moon mission mass, **not** exactly the sum of the separate constants and not Earth-only. Actual dynamical masses use GM/G in `InitialConditions.SolarSystem`. The toy tractor uses Earth-only mass in `GravityTractor.Toy`. The budget assumes circular endpoints at 1 AU and 2.7 AU over 10000 Julian yr; it is not the simulated mission result. Source: `Engineering.cs` (`MissionBudget`, `Maneuvers.LowThrustDeltaV`), `Constants.cs`, `MissionBudgetChecks.AddAnalytic`.

| Quantity | Formula | Code-computed value | Source |
|---|---|---:|---|
| E | mu*M/2*(1/a1-1/a2) | 1.6882536167514338E+33 J | `Engineering.cs: MissionBudget`; `pitch_outputs/Results/benchmarks.json` |
| dv | sqrt(mu/a1)-sqrt(mu/a2) | 11658.30562662991 m/s | `Engineering.cs: MissionBudget`; `pitch_outputs/Results/benchmarks.json` |
| p | M*delta-v | 7.0474457512977811E+28 N s | `Engineering.cs: MissionBudget`; `pitch_outputs/Results/benchmarks.json` |
| F | impulse/tau | 2.2332007983172931E+17 N | `Engineering.cs: MissionBudget`; `pitch_outputs/Results/benchmarks.json` |
| P | delta-E/tau | 5.3497528859971415E+21 W | `Engineering.cs: MissionBudget`; `pitch_outputs/Results/benchmarks.json` |
| L | M*(sqrt(mu*a2)-sqrt(mu*a1)) | 1.7323635432564215E+40 kg m2/s | `Engineering.cs: MissionBudget`; `pitch_outputs/Results/benchmarks.json` |
| solarDays | delta-E/L_sun/day | 51.04486002117178 days | `Engineering.cs: MissionBudget`; `pitch_outputs/Results/benchmarks.json` |

Code-called delta-v conversion: 11.65830562662991 km/s (`pitch_outputs/computed_code_facts.json`, `pitch_outputs/Audit.cs`).

Here mu is `Constants.SolarMu`, M is `Constants.PairMass`, and tau is the sizing duration. The positive low-thrust delta-v is integrated prograde acceleration for an outward spiral; final circular orbital speed is lower.

| Additional sizing fact | Code formula / result | Source |
|---|---|---|
| Propellant | NOT FOUND as an independent Section-3 analytic output. Actual ejection uses dm = F dt / (v_e cos(cant)); cumulative run values in §7. | `EjectaDrive.KickCanted`, `MissionConstraintStepper.Kick` |
| Jet power | P_jet = F v_e / (2 cos(cant)); original initial feedforward 4.249643418612868E+22 W | `MissionDiagnosticAudit.Initial`; `Results/diagnosis_initial_audit.json` |
| Tractor requirement | desired prograde force / sum of Earth+Moon gravitational acceleration projections; actual masses in §1 | `GravityTractor.SizeMass` |
| Initial array area | (1+reserve) requestedPower / (solarFlux*efficiency − stationKeepingPowerPerArea); 1.09516103015928E+20 m2 | `MissionSimulation` constructor; selected CSV initial row |
| Initial array mass | 1.0951610301592801E+18 kg | same row `material_kg` includes initial mirror; area*density in constructor; initial array area times 0.01 kg/m2 is the array mass |

No independent hand estimate has been substituted for a missing code output. Reproduce computed temperature and delta-v values with `dotnet pitch_outputs/build/PitchAudit.dll E:/spidergamedev/OctoEarth-Simulator`; source `pitch_outputs/Audit.cs`, calling production core methods.

## 3. HYPOTHETICAL ENABLERS

Exactly these declarations are exported by `MissionSimulation.Run` to `enablers.json`:

| ID | Declaration / bound | What it replaces | Flags |
|---|---|---|---|
| E1 | Supplied material at consumer velocity; cap 3E+23 kg; delivery 1E+13 kg/s | Mining, transport and delivery; includes initial collector/mirror, later growth and station-keeping propellant | `MaterialCap` is enabler=true; resource exceptions add `Stopped`. Replenishment books `EnablerSource`. |
| E2 | Ideal conversion, transmission, thrusters and station keeping; thruster efficiency 1 dimensionless; exhaust 300000 m/s | Hardware/conversion losses and engineering realization; impulses still require actual solar power and material | No automatic flag merely for declaration; resource/ledger failures remain possible. |
| E3 | Tractor initially placed; selected bound 1.0098073172297392E+19 kg | Acquisition and placement of tractor | No automatic flag. Tractor initial mass differs by design. |

Sources: `MissionSimulation.Run`, `Grow`, `CheckDraw`; `Config/mission_design_candidate.json`; fresh `pitch_outputs/selected_diagnostic_1yr/enablers.json`. Original full-run E3 bound is its different mass (§1).

**No energy source is declared as an enabler.** Solar luminosity and geometric collection supply energy. Array conversion efficiency is 0.3 dimensionless, reflectivity 0.1 dimensionless, areal density 0.01 kg/m2, area cap 1E+21 m2, power reserve 0.05 dimensionless. Constructor predeployment is assumed; later growth is delivery-limited. `MissionSimulation.Advance` records incident, reflected, heat, used and unused electrical energy. `MissionRadiation.Evaluate` models array waste heat. `Config/engineering_assumptions.json` explicitly leaves giant focusing optics unresolved, outside E1–E3; this matches `Docs/M3_Design.md`. `README.md` contains stale original-layout/default-viewer details; the stopped report is more specific about selected evidence.

## 4. ARCHITECTURE

| Folder/module | Purpose |
|---|---|
| `Assets/Scripts/Core/` | Unity-independent numerical physics and recording parser |
| `Assets/Scripts/Runtime/` | Unity scene, camera, playback and UI |
| `Assets/Scripts/Editor/` | Editor setup/benchmark tooling; inspect actual editor menu attributes before capture |
| `Assets/Tests/EditMode/` | `CoreTests` shared regression and `OrbitalTests` baseline regression |
| `Assets/Data/` | Approximate J2000 initial elements and playback fallback |
| `Assets/Scenes/`, `Assets/Prefabs/GravityTractor/` | Demo scenes and schematic tractor asset |
| `Config/` | Mission, selected candidate, thresholds and assumptions |
| `Tools/OctoEarth.Core.Shared/` | Links the exact core C# sources into headless shared project |
| `Tools/OctoEarth.Cli/`, `Tools/OctoEarth.Cli.Tests/` | Headless command routing and xUnit linkage |
| `Tools/*.ps1` | Build, analysis, plots and report generation |
| `Results/`, `Docs/` | Recorded numerical evidence and design reports |

| Responsibility | Classes / files |
|---|---|
| Orbits/N-body | `KeplerOrbit`, `InitialConditions`, `GravityModel` in `SystemState.cs`, `Yoshida4`/`VelocityVerlet` in `Integrators.cs` |
| Forces | `GravityTractor`, `EjectaDrive`, `MissionRadiation`, `PlumeSteering` |
| Ledger | `Ledger`, `LedgerLine`, `PhotonSink` |
| Structure | `Beam` in `Engineering.cs`; `MissionMonitors.Structural`; `DesignStudyAudit.Structures` |
| Habitability | `EnergyBalanceClimate`, `FocusingMirror` in `MissionEnvironment.cs`; `MissionMonitors.Check` |
| Controller | `MissionTargets`, `MissionConstraintStepper`, `TractorHoverDrive` |
| Validation | `BenchmarkSuite`, `ValidationSuite`, `MissionBudgetChecks`, `DesignClimateChecks`, `CoreTests`, `OrbitalTests` |
| Dashboard | `Assets/Scripts/Runtime/MissionDashboard.cs`, `TractorInspection.cs`, `ValidationPanel.cs` |
| Export | `MissionSimulation.Run/WriteRow`, `BenchmarkSuite.Write`, `MissionConfigurationIO` |

Data flow: config + J2000 CSV → `InitialConditions` / `GravityTractor.SizeMass` → `MissionSimulation` → resource allocation / radiation → `MissionConstraintStepper` + gravity / ejecta / ledger → climate → monitors / flags → CSV + JSON + ledger text → `MissionRecording` → Unity `TractorInspection` / dashboard.

Core state uses C# `double` (64-bit), `Vector3d`, compensated `KahanSum` / `KahanVector`; see `Vector3d.cs`, `SystemState.cs`, `Ledger.cs`. No Unity imports were found in the Core source search. Mission playback reads recorded state and uses Unity float coordinates only for rendering (`MissionRecording.Read`, `OrbitalCoordinates.ToUnity`). The separate baseline UI **does** start/step/reset a core baseline (`OrbitController.Advance`); claiming that every visualization component is read-only would be inaccurate.

## 5. TECH STACK

| Item | Verified value / role | Source |
|---|---|---|
| Language | C#; headless language version 9.0 | `Tools/OctoEarth.Core.Shared/OctoEarth.Core.Shared.csproj`, fallback compiler arguments |
| Unity | 6000.3.0f1 | `ProjectSettings/ProjectVersion.txt` |
| Shared / CLI targets | netstandard2.1 / net8.0 | corresponding `.csproj` files |
| Rendering / input | URP 17.3.0; Input System 1.16.0; UGUI 2.0.0; UI Toolkit used by code | `Packages/manifest.json`, Runtime imports |
| Testing | Unity Test Framework 1.6.0; xUnit 2.9.2, runner 2.8.2, Microsoft.NET.Test.Sdk 17.11.1 | manifest and CLI Tests csproj |
| Other declared packages | AI Navigation 2.0.9, Collab Proxy 2.10.2, Rider 3.0.38, VS 2.0.25, Multiplayer Center 1.0.1, Timeline 1.8.9, Visual Scripting 1.9.9; Unity-MCP Git dependency | `Packages/manifest.json`; presence does not mean mission physics depends on each |
| External execution | PowerShell, Unity-bundled Roslyn, installed .NET runtime 8.0.12; .NET SDK NOT FOUND on this machine | `Tools/Build-HeadlessWithoutSdk.ps1`; fresh `dotnet --list-sdks` / `dotnet test` attempts |
| Python / REBOUND | No project Python scripts or REBOUND dependency found in Assets/Tools/package/project searches; Python command NOT FOUND | source inventory; not a claim about every installed machine tool |

Conservative N-body uses Yoshida4, with VelocityVerlet available and validated. Forced positive-time half-kicks make the mission composition second-order; the selected design's short order study supports that (§10). Encke RK4 deviation tracking exists and is tested, but is not the mission integrator (`Encke.cs`, `ValidationSuite.Run`).

Selected coarse/fine steps: 3600 s / 1800 s. Climate RK4 substeps are capped at 86400 s; finite-disc infrared evaluation is refreshed after 21600 s or area change. Output cadence is nominally 30 d plus explicit checkpoints; output cadence is not a separate coarse dynamical integrator. Playback advances on a wall clock with independently chosen playback speed (`MissionSimulation.Run`, `EnergyBalanceClimate.StepRadiation`, `MissionRadiation.Evaluate`, `TractorInspection.AdvancePlayback`).

Mission contains 13 bodies: Sun, eight planets, Moon, tractor, array, mirror (`GravityTractor.Planetary`, `MissionSimulation` constructor; CSV body columns). Ejecta are exported ledger parcels, **not propagated N-body particles**. Gravity is pairwise Newtonian, with prescribed infrastructure positions realized using finite booked impulses rather than massless animation.

## 6. MOMENTUM AND ENERGY LEDGER

| Force/transfer | Applied to | Reaction / accounting | Source |
|---|---|---|---|
| Pairwise gravity | Both participating bodies | Equal opposite partner acceleration | `GravityModel.Accelerations`, `SystemState.cs` |
| Tractor/array/mirror station-keeping jets | Emitting infrastructure | `EjectaSink`, material through `EnablerSource`, jet input through `SolarArrayJetEnergySource` | `EjectaDrive.KickCanted`, `MissionConstraintStepper.Kick` |
| Direct solar pressure / mirror reflection / array IR | Earth, array, mirror | `PhotonSink` momentum and mechanical work | `MissionRadiation.Apply/Kick`, `PhotonSink.Book` |
| Delivered construction/replenishment mass | Receiving infrastructure | Negative mass, momentum, angular momentum, kinetic + parcel potential in `EnablerSource` | `MissionSimulation.Grow`, `EjectaDrive` |
| Benchmark artificial prograde acceleration | Benchmark body | Explicit benchmark external partner; no mission energy claim | `ProgradeAcceleration` in `EjectaDrive.cs` |

**Full-run ledger below is cached original-layout evidence, not a selected-design full run.** `Results/mission_h3600_y10000/ledger.txt`, generated by `Ledger.Report`:

```text
Ledger (SI; relative errors normalized by initial gross body scales)
Name | mass kg | px,py,pz kg m/s | Lx,Ly,Lz kg m²/s | energy J
EnablerSource | -2.9653741635095254E+23 | -3.0033419693503277E+23,3.5611004284021345E+23,-8.709633493674219E+21 | -6.357200378128513E+36,-1.6520005500253058E+37,-1.7374638736471064E+39 | 8.450908617373256E+31
PhotonSink | 0 | 2.546942503943053E+21,-1.6899433815875065E+21,-2.982845633399154E+20 | 2.5302776860652255E+29,-6.2997009011242106E+29,4.7286603302154205E+30 | -7.757589436564978E+25
EjectaSink | 2.965363210899224E+23 | -1.7093440098422692E+26,-8.405533523924727E+25,-4.745591434836984E+23 | -5.016644091027974E+37,-1.3241022320180741E+38,-1.5551502787711253E+40 | 1.157480412109479E+34
SolarArrayJetEnergySource | 0 | 0,0,0 | 0,0,0 | -1.3344134449046507E+34
net momentum gained by Earth system vs momentum carried by ejecta
Applied impulse magnitude 1.905826457415287E+26; reaction impulse magnitude 1.905826457415287E+26
Max relative P=6.234982129254249E-13; L=1.1280453487679633E-12; E=7.55742060768802E-12; force closure=3.6275527103799024E-16; reaction closure=6.633850613292896E-16

Solar energy sector relative closure=1.5317453390610187E-16
```

That report's impulse line is the magnitude of the accumulated **vector**, not a scalar sum of all thrust magnitudes, and is not Earth-only impulse. Relative P uses initial gross sum(m|v|), relative L initial gross sum(|r×mv|), relative E initial |mechanical energy| (`Ledger` constructor / `Check`). Small normalized residual does not establish engineering safety.

Absolute final residual vector and separately reported total bodies momentum change: **NOT FOUND** in the saved ledger. Exact producer is `Ledger.Momentum(finalState) - Ledger.Momentum(initialState)` with matching initial source histories; `Ledger.Check` computes the residual but only exports cumulative normalized maxima. Reproduce with an external harness around `MissionSimulation.DiagnosticState` and `Ledger`, preserving constructor `initialTransfers`; do not infer an absolute residual from net final momentum or confuse net vector impulse with scalar delta-v.

No-push cached baseline: 10000 yr, step 3600 s; maximum relative energy drift 2.197799408716484E-12 dimensionless, P 7.30874606216296E-13 dimensionless, L 1.0662853070396398E-12 dimensionless; threshold 1E-9 dimensionless. This baseline is Sun–Earth–Moon, not all planets (`Results/baseline_validation.json`, `ValidationSuite.Slow`). The separate all-planet no-push check is B5 (§10).

## 7. SIMULATION RESULTS

**Selected design: INCOMPLETE / FAILS structure. No full-duration target or departure demonstration exists.** A fresh 1 yr diagnostic was run for this request; no new full-duration mission was run. It cannot replace the stopped prefixes. Original raw results were preserved. All new build, diagnostics, derived CSVs and plots are under repository-relative `pitch_outputs/`.

| Evidence | Final time | Final a_EMB | Final eccentricity | Target crossing / verdict | Source |
|---|---:|---:|---:|---|---|
| Selected h=3600 s stopped prefix | 5460.7804015514484 yr | 1.7638141003465406 AU | 0.010389426557126257 dimensionless | NOT OBSERVED; incomplete | `Results/partial_convergence.json` / raw `Results/design_audit/full_h3600/run_mission.csv`; `MissionSimulation.WriteRow` |
| Selected h=1800 s stopped prefix | 2361.1499543691534 yr | 1.251553188019723 AU | 0.012908169901198465 dimensionless | NOT OBSERVED; incomplete | `Results/partial_convergence.json` / raw `Results/design_audit/full_h1800/run_mission.csv`; `MissionSimulation.WriteRow` |
| Original layout full run | 10000 yr | 2.699138036643842 AU | 0.02076560175195826 dimensionless | Crossing 8742.175906913073 yr; within limit, mission FAIL | `Results/mission_h3600_y10000/mission_report.json`; `MissionSimulation.Run` |

The original full run reached the target before its configured end and later parked; its final a need not remain at the crossing value. Its thermal model predates the corrected IR coupling. Do not use its successful arrival as selected-design evidence (`Docs/Selected_Design_Prefix_Report.md`, `ThermalCouplingReplay`).

| Fresh selected diagnostic | Value | Source |
|---|---:|---|
| Elapsed | 1 yr | `pitch_outputs/selected_diagnostic_1yr/mission_report.json`; `MissionSimulation.Run` |
| Final a | 1.0001119024400609 AU | `pitch_outputs/selected_diagnostic_1yr/mission_report.json`; `MissionSimulation.Run` |
| Final e | 0.016732263784286116 dimensionless | `pitch_outputs/selected_diagnostic_1yr/mission_report.json`; `MissionSimulation.Run` |
| Ejected | 2.9944250310908662E+19 kg | `pitch_outputs/selected_diagnostic_1yr/mission_report.json`; `MissionSimulation.Run` |
| Minimum SF | 0.1389710958720456 dimensionless | `pitch_outputs/selected_diagnostic_1yr/mission_report.json`; `MissionSimulation.Run` |
| Maximum flux | 1408.030292421153 W/m2 | `pitch_outputs/selected_diagnostic_1yr/mission_report.json`; `MissionSimulation.Run` |
| Maximum tide ratio | 0.0044899379564733825 Moon tide ratio | `pitch_outputs/selected_diagnostic_1yr/mission_report.json`; `MissionSimulation.Run` |
| Earth plume clearance | 33714282.35750267 m | `pitch_outputs/selected_diagnostic_1yr/mission_report.json`; `MissionSimulation.Run` |
| Moon plume clearance | 205449095.8691701 m | `pitch_outputs/selected_diagnostic_1yr/mission_report.json`; `MissionSimulation.Run` |

Target not reached; departureComplete=false; status FAIL. Diagnostic completes its requested interval, not the mission duration.

Fresh commands, from the repository root unless a `Set-Location` is shown:

```powershell
./Tools/Build-HeadlessWithoutSdk.ps1 -OutputDirectory pitch_outputs/build
Set-Location pitch_outputs
dotnet build/OctoEarth.Cli.dll bench --data E:/spidergamedev/OctoEarth-Simulator/Assets/Data/solar_j2000.csv
dotnet build/OctoEarth.Cli.dll mission --config E:/spidergamedev/OctoEarth-Simulator/Config/mission_design_candidate.json --data E:/spidergamedev/OctoEarth-Simulator/Assets/Data/solar_j2000.csv --years 1 --output selected_diagnostic_1yr
```

Fresh raw time series: `pitch_outputs/selected_diagnostic_1yr/run_mission.csv`. Derived chart-ready CSVs: `pitch_outputs/selected_h3600_charts.csv`, `selected_h1800_charts.csv`, `original_full_charts.csv`, `fresh_1yr_charts.csv`. Each carries time (yr), a (AU), actual Earth–Sun distance (AU), temperature (K), thrust (N), ejected mass (kg), array area (m2), relative P/L/E (dimensionless), Moon distance (m), sampled binding margin (J/kg). Derived data source and formulas: `pitch_outputs/Audit.cs`; raw sources named in that file. Scalar a is EMB a; actual Earth distance is separate.

| Source run | Final ejecta | Final array area | Sampled minimum binding margin | Final Moon distance |
|---|---:|---:|---:|---:|
| Selected h=3600 s |  kg |  m2 |  J/kg |  m |
| Selected h=1800 s |  kg |  m2 |  J/kg |  m |
| Original full |  kg |  m2 |  J/kg |  m |
| Fresh diagnostic |  kg |  m2 |  J/kg |  m |

Source: `pitch_outputs/*_sampled_stats.json`, `pitch_outputs/Audit.cs`, extracting complete raw CSV rows; `InitialConditions.MoonBound` binding formula. Array starts at 1.09516103015928E+20 m2 in each listed run; original fixed collector does not grow. Original ejecta agrees with `ledger.txt`.

Moon bound is cumulatively true in both selected prefixes (`Results/partial_convergence.json`, `MissionMonitors.Check`). Minimum **all-step** lunar binding margin is NOT FOUND: it is not accumulated/exported by `MissionMonitors`. Sampled margins above use the same specific-energy expression as `InitialConditions.MoonBound`, negated so positive means bound. They do not replace the separate Hill-radius criterion or establish future stability. Exact all-step minimum would require a monitor in an external harness at every step; `InitialConditions.MoonBound` is the producer. No source code was changed for this report.

Siting screen: radius 0.3 / 0.5 / 1 AU (fixed radius or co-moving multiplier); inclination 10 / 20 / 30 / 45 degrees; phase 0 / 90 / 180 / 270 degrees; both fixed and co-moving options. **96 configurations, 36 with zero sampled shading/intersections**. Source `ArraySitingSweep.Run`, `Results/design_audit/siting/array_siting_sweep.csv/json`. These reuse original monthly samples and approximate feedforward forces; they are not independent integrated missions or a proof between samples.

| Radius / multiplier | Configurations | Zero sampled shading/intersection candidates | Max array-jet power across screen |
|---|---:|---:|---:|
| 0.3 AU fixed / dimensionless co-moving | 32 configurations | 12 candidates | 1.0442392377392221E+21 W |
| 0.5 AU fixed / dimensionless co-moving | 32 configurations | 12 candidates | 9.6009276646778746E+20 W |
| 1 AU fixed / dimensionless co-moving | 32 configurations | 12 candidates | 1.7614138072481247E+24 W |

Source: `ArraySitingSweep.Run` and copied full CSV; maxima are kinematic sampled-screen quantities.

Full screen table is copied to `pitch_outputs/array_siting_sweep.csv`. Propellant-versus-exhaust/array optimization sweep: **NOT IMPLEMENTED** as a saved study; no such trade-off plot is claimed.

## 8. EARTH SAFETY AND HABITABILITY

Every configured mission threshold is marked `placeholder=true`. Passing these proxies cannot establish “without harming Earth.” Source: `Config/thresholds.json`, `MissionThresholds`, `MissionMonitors.Check`.

| Safety metric | Retained threshold | Selected h=3600 s prefix evidence | Original full evidence | Status / source |
|---|---|---|---|---|
| Temperature | 278..298 K | 288.. K sampled | 288..291.81176481764356 K all-step | Proxy within bounds; `MissionMonitors.Check` |
| Tide | 0.01 lunar-tide ratio |  ratio sampled |  ratio sampled | Selected no tide flag; original FAIL; `MissionMonitors.Check` |
| Incoming flux | 1429.05 W/m2 |  W/m2 sampled |  W/m2 sampled | Selected no flux flag; original FAIL; `MissionRadiation.Evaluate` |
| Shading | 0.01 solar-disc fraction | 0 fraction sampled | 1 fraction cumulative | Selected no shading flag; original FAIL; `MissionGeometry.Shading` |
| Eccentricity | 0.1 dimensionless |  dimensionless sampled |  dimensionless sampled | No eccentricity flag; `MissionMonitors.Check` |
| Moon distance | 300000000..500000000 m | 374491272.76501817..399757824.84215826 m sampled | Final  m | Cumulative Moon-bound true; `Results/partial_convergence.json`, original report |
| Lunar binding / Hill | Specific energy <0 J/kg and distance < computed Hill radius (m) | Cumulative true | Cumulative true in original safe-condition detail | `InitialConditions.MoonBound`; minimum all-step margin NOT FOUND |
| Plume exclusion | 3 Earth radii; no cone-target intersection | No plume flag; all-step extrema NOT FOUND | Plume intersections flagged | `MissionMonitors.Check`; fresh clearances in §7 |
| Plume heating | 1 W/m2 | No corresponding flag; maximum NOT FOUND | PlumeHeating.Array flagged; maximum NOT FOUND | `MissionMonitors.Check` |
| Liquid fraction | 1 fraction | Boolean proxy; no LiquidOcean/Snowball flag | No corresponding flag | `EnergyBalanceClimate`; not a spatial fraction model |
| Safety factor | 1.5 dimensionless | 0.13896574091969552 dimensionless cumulative | 0.09209717940210621 dimensionless cumulative | FAIL both; `MissionMonitors.Structural` |
| Ledger P/L/E | 1E-9 relative | All below threshold; §6 / §10 | All below threshold; §6 | Numeric PASS, not safety validation; `Ledger.Check` |
| Convergence | 0.01 relative | Full-duration NOT OBSERVED | Not a matched selected-design comparison | `MissionThresholds`; partial absolute differences in §10 |

Sources: `Config/thresholds.json`, `Results/partial_convergence.json`, original `mission_report.json`; sampled original and selected values independently extracted in `pitch_outputs/Audit.cs`. Sampled maxima are lower bounds on all-step maxima, not all-step certifications. Fine-prefix evidence is retained in `Results/partial_convergence.json`.

| Additional safety question | Finding / source |
|---|---|
| Atmosphere, atmospheric escape, rotation, magnetosphere, biological radiation dose | NOT IMPLEMENTED as mission safety models or thresholds; `MissionMonitors.Check` has no such checks. Plume irradiance is not a general radiation-dose model. |
| Liquid ocean / snowball | Boolean global-temperature proxies, not spatial liquid fraction; `EnergyBalanceClimate.LiquidOcean`, `Snowball`. Liquid threshold is 1 fraction; ice branch begins at 263.15 K. |
| Equilibrium at initial/target distance | 254.0386205814548 K at 1 AU; 154.60298107773218 K at 2.7 AU without restoration (`Engineering.cs`, `Insolation.EquilibriumTemperature`; fresh direct core calls in `pitch_outputs/computed_code_facts.json`). Target value excludes mirror and greenhouse effects. |
| Mirror restoration | Finite optical intercept, angular solar size, pointing error, reflectivity and missing visible-flux target; required area capped, construction delivery-limited. `FocusingMirror.Evaluate`, `MissionRadiation.Evaluate`, `MissionSimulation.Advance`. Capability remains unresolved. |
| Waste heat | Incident remainder after reflection and electricity radiates from finite collector disc; corrected infrared absorption enters EBM without visible albedo. `FiniteDiscRadiation.Irradiance`, `EnergyBalanceClimate.StepRadiation`. Ideal thruster efficiency removes configured thruster losses, not collector heat. |
| Tractor tides | (tractor mass / Moon mass)*(Moon separation / tractor separation)^3 relative to lunar tide; `MissionMonitors.Check`. This is a gradient proxy, not ocean dynamics. |
| Shading | Angular-disc overlap from Earth and mirror; flagged cumulatively; `MissionGeometry.Shading`, `MissionMonitors.Check`. |

Selected flags are `StructuralSafety` (measured proxy failure), `FocusingOpticsUnresolved`, `MaterialStrengthUnvalidated`, `ThermalStructuralModelIncomplete` (unresolved assumptions). All have enabler=false; none is excused by E1/E2/E3. No additional flag appeared in the preserved prefixes (`Results/partial_convergence.json`, `MissionMonitors` constructor / `Structural`).

All flags from the **original completed** full run, with their recorded causes and enabler classifications:

| Original full-run flag | First occurrence | Enabler-caused? | Recorded cause |
|---|---:|---|---|
| FocusingOpticsUnresolved | 0 s | False | Giant focusing optics remain outside E1-E3 pending organiser confirmation. |
| MaterialStrengthUnvalidated | 0 s | False | Rock and frame strength inputs are illustrative engineering assumptions, not validated components. |
| ThermalStructuralModelIncomplete | 0 s | False | Local beam/column checks omit coupled thermal deformation and global optical stability. |
| StructuralSafety | 0 s | False | A modeled component safety factor is below 1.5; rock/frame assumptions do not imply unlimited strength. |
| InitiallyInwardEjecta.Tractor | 3600 s | False | An unintegrated parcel is not initially radially outbound; ballistic future escape is not assumed. |
| TractorTides | 612000 s | False | Tractor tide gradient exceeds placeholder Moon-relative threshold. |
| PlumeIntersection.Tractor.Array | 1864836000 s | False | Plume cone or exclusion zone intersects target. |
| FluxCap | 1987567200 s | False | Received visible plus array waste-heat flux exceeds placeholder limit. |
| PlumeIntersection.Mirror.Array | 2018581200 s | False | Plume cone or exclusion zone intersects target. |
| ArrayShading | 2019160800 s | False | Array overlaps Sun as seen from Earth or mirror beyond threshold. |
| PlumeIntersection.Array.Mirror | 9703861200 s | False | Plume cone or exclusion zone intersects target. |
| PlumeHeating.Array | 39322850400 s | False | Gaussian plume irradiance on Earth's near limb exceeds threshold. |
| PlumeIntersection.Array.Moon | 39322868400 s | False | Plume cone or exclusion zone intersects target. |
| PlumeIntersection.Array.Earth | 41465844000 s | False | Plume cone or exclusion zone intersects target. |

All-step plume clearances for selected long prefixes are NOT FOUND; terminal JSON was never written. The fresh diagnostic has its own all-step extrema (§7), not late-time evidence. Earlier original-layout maximum shading is 1 solar-disc fraction and caused a failure. `ThermalCouplingReplay` and `DesignClimateChecks` separately demonstrate thermal corrections and sustained-shading cooling; they do not validate selected late-time climate.

## 9. STRUCTURE

Structure is applicable and **FAILS**. No tether, ring or planetary attachment transmits force in this concept, but tractor rock, mounts and optical infrastructure still need strength.

| Component/evidence | Worst stress | Allowable / SF | Result | Source |
|---|---:|---|---|
| Rock proxy selected h=3600 s | 35980090.969970524 Pa | 5000000 Pa / 0.13896574091969552 dimensionless | FAIL | `Results/partial_convergence.json`, `MissionMonitors.Structural` |
| Rock proxy selected h=1800 s | 35983482.351544693 Pa | 5000000 Pa / 0.13895264363665349 dimensionless | FAIL | `Results/partial_convergence.json`, `MissionMonitors.Structural` |
| Fresh diagnostic array local beam | 31928293.5102108 Pa | 200000000 Pa / 6.2640366274520489109263737813 dimensionless | Local bending PASS only | fresh report, `MissionMonitors.Structural`, `Beam.SafetyFactor` |
| Fresh diagnostic mirror local beam | 3612737.0161862164 Pa | 200000000 Pa / 55.3596896491319693357999412 dimensionless | Local bending PASS only | same source |
| Original combined minimum | NOT FOUND per-component in old terminal summary | 0.09209717940210621 dimensionless | FAIL | original `mission_report.json` |
| Global array/mirror, thruster mounts, thermal coupling | NOT VALIDATED | NOT FOUND | No adequacy claim | `ThermalStructuralModelIncomplete`, `DesignStudyAudit.Structures` |

Local frame allowable is 200000000 Pa; rock allowable is 5000000 Pa, density 3000 kg/m3, illustrative Young modulus 50000000000 Pa. Local panel length is 100 m; global deployment is not resolved. Sources: `MissionMonitors.Structural`, `Beam` in `Engineering.cs`, config. Bulk stress combines acceleration/self-gravity into a scalar proxy; buckling is also checked. These are not full stress tensors or validated mount designs. Separate contact-area/mount studies in `DesignStudyAudit.Structures` report tension, compression, shear and bending failures; see `Results/design_audit/structure_options_original.json` and `Docs/Selected_Design_Prefix_Report.md`.

Worst selected-prefix proxy requires 53975223.52731704 Pa allowable to achieve SF 1.5 dimensionless, a 10.795044705463408 dimensionless multiplier. **This is a failed requirement, not an adopted strength change.** Source: `Results/partial_convergence.json`, inferred from retained allowable / recorded SF.

## 10. VALIDATION

Fresh headless benchmark comparisons, including every B1–B10 subcheck and additional shared core validation. Expected/tolerance values below are test fixtures; measured values were calculated. Source: `pitch_outputs/Results/benchmarks.json`, produced by `BenchmarkSuite.Run`, `ValidationSuite.Run`, `MissionBudgetChecks`; test wrappers in `Assets/Tests/EditMode/CoreTests.cs`. No benchmark group was marked inapplicable. B1–B3 are electromagnetic validation utilities, not the propulsion mechanism.

| ID | Description | Expected | Tolerance | Measured | Result | Test / producer |
|---|---|---:|---:|---|---|
| B1 | Wire vector error | 0 N | 1E-08 N | 0 N | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B2 | Closed loop net force | 0 N | 5E-08 N | 0 N | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B3 | Long wire field | 0.0002 T | 2E-10 T | 0.0002 T | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B4.position | Eccentric Kepler return, 10 periods | 0 a | 1E-06 a | 2.5060734130426048E-11 a | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B4.energy | Eccentric Kepler energy | 0 relative | 1E-09 relative | 3.28685337165563E-13 relative | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B5 | Full solar system 5yr max energy drift | 0 relative | 1E-09 relative | 9.56588407853746E-14 relative | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B5.P | Full system max momentum drift | 0 initial gross momentum | 1E-11 initial gross momentum | 8.95185280010703E-15 initial gross momentum | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B5.L | Full system max angular momentum drift | 0 relative | 1E-11 relative | 3.2442493698991597E-14 relative | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B6 | N-body EMB sidereal period | 365.256 days | 0.1 days | 365.2602450589337 days | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B7.dv1 | Hohmann departure | 2.94 km/s | 0.02 km/s | 2.9460551625085643 km/s | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B7.dv2 | Hohmann arrival | 2.65 km/s | 0.02 km/s | 2.6499820804508647 km/s | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B7.time | Hohmann flight time | 259 days | 2 days | 258.9151502335679 days | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B7.simulatedRadius | Simulated two-burn arrival radius | 0 relative | 1E-08 relative | 5.9952043329758453E-15 relative | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B7.simulatedBurn | Simulated arrival burn | 2.65 km/s | 0.02 km/s | 2.6499820804508962 km/s | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B7.circular | Post-burn eccentricity | 0 dimensionless | 1E-08 dimensionless | 1.583067659490474E-14 dimensionless | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B8 | Prograde push semi-major growth | 3170 km | 31.7 km | 3170.1014833984377 km | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B8.Gauss | Integrated Gauss versus state delta-a | 0 relative | 0.01 relative | 3.4086034802729838E-10 relative | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B9.moment | Cantilever root moment | 5000 N m | 50 N m | 5000 N m | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B9.deflection | Cantilever tip deflection | 0.00625 m | 6.25E-05 m | 0.0062499999999999995 m | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| B10 | Earth radiative equilibrium | 254 K | 2 K | 254.0386205814548 K | PASS | `CoreTests.cs`; `BenchmarkSuite.Run` |
| X1 | Circular full-period return | 0 AU | 1E-09 AU | 5.2903354655572811E-14 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X2 | Kepler period squared ratio | 19.683000000000003 dimensionless | 1E-12 dimensionless | 19.683000000000003 dimensionless | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X5.rocket | Finite burn rocket equation | 0.10005003335834001 m/s | 1.0005003335834001E-10 m/s | 0.10005003335834001 m/s | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X5.momentum | Rocket booked momentum closure | 0 relative | 1E-09 relative | 1.3358203432289883E-12 relative | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X5.energy | Rocket energy closure | 0 relative | 1E-09 relative | 1.6895430964416829E-13 relative | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X5.reaction | Rocket impulse reaction closure | 0 relative | 1E-09 relative | 1.3364888104939896E-13 relative | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X5.sourceP | Replenished source/ejecta P closure | 0 relative | 1E-09 relative | 8.8774505621021307E-17 relative | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X5.sourceE | Replenished energy input closure | 0 relative | 1E-09 relative | 0 relative | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X5.sourceL | Replenished angular closure | 0 relative | 1E-09 relative | 3.7895612573872012E-16 relative | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X5.force | Applied force/body impulse closure | 0 relative | 1E-09 relative | 0 relative | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.Yoshida4.h100 | Analytic endpoint error | 0 AU | 1 AU | 0.0004086374130350796 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.Yoshida4.h200 | Analytic endpoint error | 0 AU | 1 AU | 2.562026595836706E-05 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.Yoshida4.difference200 | h versus h/2 endpoint difference | 0 AU | 1 AU | 0.00038301714808081777 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.Yoshida4.h400 | Analytic endpoint error | 0 AU | 1 AU | 1.6024423543647646E-06 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.Yoshida4.difference400 | h versus h/2 endpoint difference | 0 AU | 1 AU | 2.4017823604249305E-05 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.Yoshida4.h800 | Analytic endpoint error | 0 AU | 1 AU | 1.0017059648855532E-07 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.Yoshida4.difference800 | h versus h/2 endpoint difference | 0 AU | 1 AU | 1.5022717578762818E-06 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.Yoshida4 | Observed log-log order | 4 order | 0.2 order | 3.9981380562450193 order | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.VelocityVerlet.h100 | Analytic endpoint error | 0 AU | 1 AU | 0.03262411752254607 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.VelocityVerlet.h200 | Analytic endpoint error | 0 AU | 1 AU | 0.008118830518607354 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.VelocityVerlet.difference200 | h versus h/2 endpoint difference | 0 AU | 1 AU | 0.024506464241130677 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.VelocityVerlet.h400 | Analytic endpoint error | 0 AU | 1 AU | 0.0020273681692827335 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.VelocityVerlet.difference400 | h versus h/2 endpoint difference | 0 AU | 1 AU | 0.006091480607351499 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.VelocityVerlet.h800 | Analytic endpoint error | 0 AU | 1 AU | 0.0005066956086365386 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.VelocityVerlet.difference800 | h versus h/2 endpoint difference | 0 AU | 1 AU | 0.0015206728453950504 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X6.VelocityVerlet | Observed log-log order | 2 order | 0.12 order | 2.0027691048360627 order | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X7 | Encke versus double push displacement | 0 m | 0.05 m | 0.0003911950366710937 m | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X7.reference | Unforced Encke deviation | 0 m | 1E-20 m | 0 m | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X8.float | Float loses day-long tiny push | 0 m | 0 m | 0 m | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X8.double | Double retains tiny push | 37.323572917982084 m | 0.05 m | 37.32354081118029 m | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| Budget.analytic.nominal.E | Budget versus independent analytic E | 1.6882536167514338E+33 J | 1.6882536167514339E+24 J | 1.6882536167514338E+33 J | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.nominal.dv | Budget versus independent analytic dv | 11658.30562662991 m/s | 1.1658305626629911E-05 m/s | 11658.30562662991 m/s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.nominal.p | Budget versus independent analytic p | 7.0474457512977811E+28 N s | 7.0474457512977818E+19 N s | 7.0474457512977811E+28 N s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.nominal.F | Budget versus independent analytic F | 2.2332007983172931E+17 N | 223320079.83172932 N | 2.2332007983172931E+17 N | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.nominal.P | Budget versus independent analytic P | 5.3497528859971415E+21 W | 5349752885997.142 W | 5.3497528859971415E+21 W | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.nominal.L | Budget versus independent analytic L | 1.7323635432564218E+40 kg m2/s | 1.732363543256422E+31 kg m2/s | 1.7323635432564215E+40 kg m2/s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.nominal.solarDays | Budget versus independent analytic solarDays | 51.04486002117178 days | 5.104486002117178E-08 days | 51.04486002117178 days | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.alternate.E | Budget versus independent analytic E | 1.3583124106912658E+33 J | 1.3583124106912659E+24 J | 1.3583124106912661E+33 J | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.alternate.dv | Budget versus independent analytic dv | 11692.211271536698 m/s | 1.1692211271536699E-05 m/s | 11692.211271536697 m/s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.alternate.p | Budget versus independent analytic p | 4.9475591995507543E+28 N s | 4.9475591995507548E+19 N s | 4.9475591995507534E+28 N s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.alternate.F | Budget versus independent analytic F | 6.8164647991509171E+17 N | 681646479.9150918 N | 6.8164647991509158E+17 N | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.alternate.P | Budget versus independent analytic P | 1.8714053456030515E+22 W | 18714053456030.516 W | 1.871405345603052E+22 W | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.alternate.L | Budget versus independent analytic L | 9.1251120382970624E+39 kg m2/s | 9.1251120382970629E+30 kg m2/s | 9.1251120382970576E+39 kg m2/s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.alternate.solarDays | Budget versus independent analytic solarDays | 41.068987609913364 days | 4.1068987609913367E-08 days | 41.06898760991337 days | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.inward.E | Budget versus independent analytic E | -1.6882536167514338E+33 J | 1.6882536167514339E+24 J | -1.6882536167514338E+33 J | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.inward.dv | Budget versus independent analytic dv | -11658.30562662991 m/s | 1.1658305626629911E-05 m/s | -11658.30562662991 m/s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.inward.p | Budget versus independent analytic p | -7.0474457512977811E+28 N s | 7.0474457512977818E+19 N s | -7.0474457512977811E+28 N s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.inward.F | Budget versus independent analytic F | -4.4664015966345862E+17 N | 446640159.66345865 N | -4.4664015966345862E+17 N | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.inward.P | Budget versus independent analytic P | -1.0699505771994283E+22 W | 10699505771994.283 W | -1.0699505771994283E+22 W | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.inward.L | Budget versus independent analytic L | -1.7323635432564218E+40 kg m2/s | 1.732363543256422E+31 kg m2/s | -1.7323635432564215E+40 kg m2/s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.analytic.inward.solarDays | Budget versus independent analytic solarDays | -51.04486002117178 days | 5.104486002117178E-08 days | -51.04486002117178 days | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| X10 | Moon bound over 10yr including planets | 1 boolean | 0 boolean | 1 boolean | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X11 | Reset and bitwise determinism | 1 boolean | 0 boolean | 1 boolean | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X11.forceReset | Stateful force reset and determinism | 1 boolean | 0 boolean | 1 boolean | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X.kepler | Elements-state roundtrip | 0 AU | 1E-12 AU | 3.0599611460579433E-16 AU | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X.clock | Scheduler cap pause resume reset | 1 boolean | 0 boolean | 1 boolean | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X.softening | Softened-force consistent energy ledger | 0 relative | 1E-09 relative | 3.3930953475924023E-15 relative | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| X.photon | Photon sink angular booking | 0 kg m2/s | 1E-15 kg m2/s | 0 kg m2/s | PASS | `CoreTests.cs`; `ValidationSuite.Run` |
| Budget.guide.E | Rounded guide sanity E | 1.69E+33 J | 3.38E+31 J | 1.6882536167514338E+33 J | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.guide.v1 | Rounded guide sanity v1 | 29.78 km/s | 0.5956 km/s | 29.784691831696804 km/s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.guide.v2 | Rounded guide sanity v2 | 18.12 km/s | 0.3624 km/s | 18.126386205066893 km/s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.guide.dv | Rounded guide sanity dv | 11.66 km/s | 0.23320000000000002 km/s | 11.65830562662991 km/s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.guide.p | Rounded guide sanity p | 7.05E+28 N s | 1.41E+27 N s | 7.0474457512977811E+28 N s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.guide.F | Rounded guide sanity F | 2.2E+17 N | 4400000000000000 N | 2.2332007983172931E+17 N | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.guide.acceleration | Rounded guide sanity acceleration | 3.7E-08 m/s2 | 7.4E-10 m/s2 | 3.6942941245943639E-08 m/s2 | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.guide.P | Rounded guide sanity P | 5.4E+21 W | 1.08E+20 W | 5.3497528859971415E+21 W | PASS | `CoreTests.cs`; `MissionBudgetChecks` |
| Budget.guide.L | Rounded guide sanity L | 1.73E+40 kg m2/s | 3.46E+38 kg m2/s | 1.7323635432564215E+40 kg m2/s | PASS | `CoreTests.cs`; `MissionBudgetChecks` |

B1 tests vector error against +z force 10 N for current 10 A, wire length 2 m, field 0.5 T. B2 verifies a closed loop has zero net force. B3 uses current 1000 A at radius 1 m. B4 uses eccentricity 0.3 dimensionless over 10 periods. Other benchmark descriptions and all expected values appear above (`BenchmarkSuite.Run`).

Unity job summary observed: **status=succeeded; completed=35 tests; total=35 tests; failures_so_far=[]**. This is the exact available job status, not an invented NUnit passed/skipped summary. The bridge returned `result=null`, so distinct passed/skipped counts and a native runner summary line are **NOT FOUND** in that payload. Evidence `pitch_outputs/unity_test_job.json`, job `dce184900656456da4c51b4c4ea28456`, requested `OctoEarth.Tests.CoreTests` and `OctoEarth.Tests.OrbitalTests`. The explicit Slow method name appears in progress; that alone does not prove execution rather than skip. Native summary producer: Unity Test Runner result export, or the README batch command with `-testResults pitch_outputs/editmode.xml` after closing the interactive editor. No claim of a new slow-baseline integration is made.

`dotnet test Tools/OctoEarth.Cli.Tests/OctoEarth.Cli.Tests.csproj` was attempted and could not execute: **No .NET SDKs were found.** SDK/xUnit execution is not validated. Fallback Roslyn compilation succeeded; headless output ends `Failed rows: 0. No mission modeled.` (`pitch_outputs/benchmarks_run.txt`). Additional Unity tests cover full shading, force-preserving plume rotation/ledger, co-moving radius, playback, Moon binding, reset, clock and photon accounting (`CoreTests`).

Partial selected convergence only: interval 2361.1499543691534 yr; h=3600 s and h/2=1800 s; coarse scalars linearly aligned to fine timestamps, with exact shared epochs separately retained. Startup half-step excluded. Interpolation correction is not an error bound; full-duration convergence **NOT OBSERVED**. Source `Results/partial_convergence.json`, `Tools/Compare-StoppedPrefixes.ps1`.

| Quantity | Maximum absolute difference | Final signed coarse-minus-fine | Max nearest-record alignment correction |
|---|---:|---:|---:|
| a_EMB_AU | 0.00015072388507064716 AU | 3.8481754356833875E-05 AU | 3.289539884931969E-08 AU |
| eccentricity | 0.00011415966275280312 dimensionless | 1.0874630646645275E-05 dimensionless | 2.4275873162366013E-08 dimensionless |
| Earth_Moon_distance_m | 2686256.0663151741 m | -138834.3891069293 m | 45720.083990514278 m |
| thrust_N | 316316351920608 N | -105733521843168 N | 2358888005984 N |
| ejected_kg | 2.3454756976325558E+19 kg | -2.3454756976325558E+19 kg | 1708275214057472 kg |
| temperature_K | 0.00028130978529361528 K | -1.0476676379767014E-06 K | 5.03264294593464E-05 K |
| relative_P | 1.5534414999550995E-13 relative | 1.7226439328155064E-14 relative | 3.3380833272371356E-18 relative |
| relative_L | 3.283479196644128E-13 relative | -1.3134661611813208E-13 relative | 4.6085824243401029E-18 relative |
| relative_E | 5.6520601721126937E-13 relative | -4.9560211519052039E-13 relative | 5.2988710317359072E-17 relative |

Short candidate study: steps 7200 / 3600 / 1800 / 900 / 450 s over 10 yr. Log-log observed orders relative to the finite 450 s reference: position 2.13386035 dimensionless, a 2.13387177 dimensionless, temperature 2.06877756 dimensionless; successive a orders 2.01195979 / 2.00296762 / 2.00079178 dimensionless (`Docs/Selected_Design_Prefix_Report.md`, `MissionConvergenceStudy.Run`, `Results/design_audit/order/`). This supports second-order forced behavior over that interval; it is not a mission-duration error guarantee.

## 11. ENERGY BONUS

Solar luminosity is fixed at 3.828E+26 W (`Constants.SolarLuminosity`). No declared energy-source enabler is used, but ideal transmission/conversion hardware remains E2. Collector predeployment and raw material delivery remain assumed engineering capabilities.

| Energy/resource quantity | Original full run (cached) | Fresh selected diagnostic |
|---|---:|---:|
| Incident collected solar energy | 4.7042802873550931E+34 J | 4.8176652063127623E+30 J |
| Jet energy required | 1.3344134449046507E+34 J | 1.3474912639908898E+30 J |
| Reported source fraction | 1 fraction | 1 fraction |
| Material including growth/propellant | 2.9653741635095254E+23 kg | 3.1079660638186914E+19 kg |
| Orbital budget energy (analytic, not measured gain) | 1.6882536167514338E+33 J | Not a diagnostic energy requirement |

Source: cached original `mission_report.json` / `ledger.txt`; fresh diagnostic equivalents under `pitch_outputs/selected_diagnostic_1yr/`; formulas in `MissionSimulation.Advance/CheckDraw/Run`. `sourceFraction=1` is a clamped electrical-used versus required-jet ratio, **not** a demonstrated source-to-orbit conversion efficiency; `Run` uses `Math.Min` and does not export every sector separately. Cached original solar-sector closure is 1.5317453390610187E-16 dimensionless. Selected long-prefix integrated solar/jet totals are **NOT FOUND** because no terminal report exists; producing them requires a complete run with `MissionSimulation.Run`, not integration of sparse thrust alone. Collector growth/area and consumed material are recorded in the CSV (§7); array waste heat and optical caps are in §3/§8. Do not equate orbital energy gain with much larger jet energy or with all incident sunlight.

## 12. VISUALISATION AND USABILITY

Scenes: `Assets/Scenes/OctoEarth_Main.unity` and `Assets/Scenes/SampleScene.unity`. The main scene is the documented demo; no mission demonstration is claimed for SampleScene (`README.md`, scene inventory).

Open the project with the verified Unity version (§5), open `OctoEarth_Main`, press Editor Play. The selected recording is labelled partial and opens paused at its endpoint (`Assets/Scenes/OctoEarth_Main.unity`, `TractorInspection.Reload`). Use **Sun / transfer**, **Earth + tractor**, **Focus tractor**, **Play mission**, **Reload CSV**, timeline, **Jump to end**, **Restart (R)**. Mouse: right drag orbit, middle drag pan, wheel zoom (`TractorInspection.Attach`, `OrbitCamera`). No build was produced in this audit.

Current source additionally implements **Moon view (M)**, **Show / hide Moon (T)**, **3-minute demo (D); Esc stops**; Space toggles playback; digit keys select playback rates (`TractorInspection.LateUpdate`, `FocusMoon`, `ToggleMoon`). Moon position/velocity are read from CSV (`MissionRecording.Read`). Earlier `Results/stopped_viewer_verification.json` recorded MoonVisible=false, so its “Moon NOT visible” finding is historical. Current source capability is verified by inspection; a fresh rendered Moon/scale screenshot was **NOT captured** for this report.

Visible readouts include EMB a, thrust, separation, ejected mass, cumulative flag count, P/L/E, playback clock and Moon metrics. Scale labels state geometric scale and object enlargements; tractor hardware and marker offsets are schematic (`TractorInspection.Render/Attach`). Solar display scale is 1 AU = 20 Unity units; local initial tractor separation = 8 Unity units. These are display scales, not physical transformations of core state.

Mission mass/exhaust/geometry cannot be changed live and reintegrated through this playback UI. Judge edits require config/headless rerun/reload. Separate baseline controls expose N-body toggle, reset, speed and step choices 1800 / 3600 / 7200 s (`MissionDashboard.OnEnable`, `OrbitController.ConfigureStep`). No verified live stress-coloring or magnetic-field overlay was found. A complete all-threshold safety dashboard is **NOT IMPLEMENTED** as a demonstrated panel; flag count is not that dashboard. CSV/JSON/ledger export is headless; plots use PowerShell tools.

## 13. KEY INSIGHTS AND LIMITATIONS

| Physical honesty | Evidence |
|---|---|
| Reciprocal gravity rather than a direct unexplained Earth push | `GravityModel.Accelerations`, `GravityTractor.SizeMass` |
| Exhaust/source/photon momentum and mechanical energy are explicitly booked | `EjectaDrive`, `Ledger`, `MissionRadiation.Apply` |
| Jets draw finite solar energy and delivery-limited material | `MissionSimulation.CheckDraw/Grow/Advance` |
| Numerical benchmark expectations and tolerance are separate from rounded guide sanity checks | `MissionBudgetChecks`, `BenchmarkSuite` |
| Failed structure, unresolved assumptions and incomplete runs remain visible | `MissionMonitors`, `MissionFlags`, stopped-prefix report |

| Major limitation / failure | Best supported next test |
|---|---|
| Bulk-rock structure FAILS at retained strength | Validate a feasible tractor/mount architecture before another long mission; `DesignStudyAudit.Structures` |
| Giant focusing capability unresolved | Obtain organizer interpretation, then test physically specified optics/global pointing; `Config/engineering_assumptions.json` |
| Selected arrival, departure and long-duration convergence missing | Complete explicitly matched h/h2 runs only after resolving feasibility and preserving outputs; `MissionConvergenceStudy`, `MissionSimulation.Run` |
| Simplified EBM / placeholder safety; no full atmosphere, spin or magnetosphere physics | Replace proxies with specified, justified acceptance models; `MissionEnvironment`, `MissionThresholds` |
| Approximate ephemeris, exported ejecta, preplaced infrastructure | Validate initial conditions and propagate ejecta trajectories; `InitialConditions`, `EjectaDrive`, `MissionTargets` |

Likely challenged assumptions: E1 delivers material already at consumer velocity (booked, not an implemented transport system); E2 grants ideal hardware, not energy; E3 places the tractor initially; a giant focusing mirror is still unresolved; local beam/rock proxies do not establish global structural safety. These statements are exactly the limits in `MissionSimulation.Run` and `Config/engineering_assumptions.json`.

## 14. LIKELY JUDGE QUESTIONS

| Question | Code-supported answer |
|---|---|
| Where does the reaction go? | Jets to `EjectaSink`; photons to `PhotonSink`; delivered material enters `EnablerSource`. `EjectaDrive`, `Ledger`, `MissionRadiation.Apply`. |
| Are you pushing Earth directly with a magic force? | Mission Earth pull is tractor gravity plus modelled radiation. Infrastructure hover is prescribed but enforced by booked finite jets. `MissionConstraintStepper.Step`, `GravityTractor`. |
| Did the chosen layout reach the target? | No observed selected arrival. Original layout did, while failing safety/structure. `Results/partial_convergence.json`, original `mission_report.json`. |
| Why believe the Moon survives? | Cumulative binding/Hill checks pass in prefixes; sparse sampled margins are available, future stability is unproved. `InitialConditions.MoonBound`, `MissionMonitors.Check`. |
| What happens if exhaust speed changes? | Fixed-force mass flow scales inversely and jet power directly with exhaust speed; resource requirements and collector station keeping change. No saved exhaust sweep. `EjectaDrive.KickCanted`, `MissionSimulation` constructor. |
| Is energy supplied magically? | Solar incident energy is recorded; E2 idealizes hardware. No energy-source enabler. `MissionSimulation.Advance`, `CheckDraw`, exported enablers. |
| Does a small ledger residual prove correct physics? | No; closure is a numerical/accounting test with gross-scale normalization, while optics/strength/safety remain conditional. `Ledger.Check`, `MissionMonitors`. |
| Is it fourth-order? | Conservative Yoshida is; forced mission composition is observed second-order. `SimulationRunner.Step`, `MissionConstraintStepper`, short order study. |
| Is the asteroid structurally feasible? | The retained rock proxy fails. Increasing strength in a report would hide a failed requirement. `MissionMonitors.Structural`, prefix SF. |
| Can this demo react to a judge changing mission parameters? | Playback can scrub/speed/view; physics parameter changes require headless rerun. `TractorInspection`, `Program.Main`. |

Hard-coded / tuned items found (constants and fixtures are not measured outcomes):

| Category | Explicit source / qualification |
|---|---|
| Mission target / budget | 1 AU → 2.7 AU, duration 10000 yr repeated in `GravityTractor.Planetary`, `MissionSimulation.Advance`, `DepartureKick`; not fully configurable through the JSON. |
| Rounded masses and physics constants | `Constants.cs`, notably rounded `PairMass`; approximate J2000 input CSV and ideal circular/coplanar Moon in `InitialConditions`. |
| Design/controller choices | Exhaust, cant, margins, bias, PD frequency/damping, area caps, power reserve; `MissionConfiguration`, `TractorParameters`, `Config/*.json`. Selected siting is a screened choice, not independently validated optimum. |
| Constraint solver | Position tolerance 0.01 m, up to 16 iterations, kick-history extrapolation; `MissionConstraintStepper.Step`. Numerical settings, not measured performance guarantees. |
| Departure policy | Energy threshold 500000 J/kg; distance 2 times configured separation; thrust factor 1.02 dimensionless; departure tilt 5 degrees; parking transition 5 d. `DepartureKick`, `MissionTargets.Get`; selected departure unexercised. |
| Climate / optics / structure | Illustrative EBM coefficients, ice transitions, fixed nominal luminosity, optical error, panel geometry, assumed material strengths; `MissionEnvironment`, `MissionMonitors.Structural`, `FiniteDiscRadiation`. |
| Acceptance / calibration | All mission thresholds are placeholders; analytic tolerance and rounded-guide tolerance deliberately distinct. `MissionThresholds`, `MissionBudgetChecks`. |
| Benchmark expected values | Hard-coded reference values and comparison tolerances in `BenchmarkSuite` / `ValidationSuite`; B5 can halve dt when energy fails. These are fixtures, not injected trajectory results. |
| Analysis replay constants | `Program.Main` design-audit uses fixed load 2.5495226012612157E+17 N for original structure study; `ArraySitingSweep.Run` uses original sizing constants. These do not compute selected full mission outcomes. |
| Viewer / presentation | Default tractor mass is original mass but `Reload` can read recorded selected mass; enlarged geometry, schematic plume/marker, default CSV/fallback. `TractorInspection`. Viewer flag text explicitly displays `Design status: FAIL`; it is presentation text, not a live acceptance recomputation. |
| Cached findings | `Docs/*`, `Results/*.json`, scripts generating reports contain recorded results. They are historical artifacts; fresh headless benchmark and diagnostic evidence are separate. |

No fabricated target-reaching time or prerecorded success was found injected into the actual integrator in this inspection. This is a bounded source review, not a proof that every repository byte is free of tuning. Exact evidence-producing searches: `rg -n 'const|target|tolerance|margin|1\.02|2\.7|10000' Assets/Scripts/Core Config Tools/OctoEarth.Cli`; inspect the classes listed above. Source/config files were not edited.

## 15. TEAM AND CONTRIBUTION

**NOT FOUND.** This workspace has no accessible Git repository metadata: `git status --short`, `git remote -v`, `git log -1 --format='%H %aI %an'` failed with “not a git repository.” Author names, commit counts, module ownership and last commit date cannot be reconstructed from file timestamps. In a Git checkout, exact producers are:

```powershell
git shortlog -sne HEAD
git log --format='%an <%ae>' --name-only
git log -1 --format='%H %aI %an'
git remote -v
```

## 16. SCREENSHOT AND ASSET SHOPPING LIST

Capture scene `Assets/Scenes/OctoEarth_Main.unity` using its active Game camera, with source label and scale text visible. UI steps are grounded in `TractorInspection.Attach/FocusPair/FocusTractor/FocusSolarSystem/FocusMoon`; camera interaction in `OrbitCamera`. Captures are recommendations, not claims that these screenshots were captured here.

| Asset | Exact steps / existing artifact | Honesty constraint |
|---|---|---|
| Hero Earth + Moon + tractor | Editor Play; **Earth + tractor**; **Show / hide Moon (T)** until Moon shown; pause with Space; right-drag orbit / wheel zoom; retain scale label. **Moon view (M)** is a separate closer frame. | Current Moon rendering needs visual capture verification; enlarged objects must stay labelled. |
| Tractor / schematic jets close-up | **Focus tractor**; scrub to a recorded nonzero-thrust sample; pause; camera defaults from `FocusTractor`. | Asset is schematic, not an engineered thruster design; collector may require separate solar view. |
| Collector / system layout | **Sun / transfer** at an intermediate selected timestamp, keep source label; capture collector only if actually rendered and legible. | Detailed full collector structure is not established by a UI glyph. |
| Deployment/dashboard | Restart, pause, advance timeline; capture thrust, ejected mass and flags. | Assembly/deployment is not simulated; title it “recorded early operation,” not a deployment proof. |
| Orbit expansion | `pitch_outputs/selected_orbit.png`; optionally `original_full_orbit.png` on a separately titled slide. | Selected stops before target; original full run fails safety. |
| Ledger close-up | Pause selected recording, capture P/L/E rows; use `pitch_outputs/selected_ledger.png` for readable history. | State normalization and cumulative-error interpretation. |
| Safety / habitability | `pitch_outputs/selected_temperature.png` plus §8 threshold table and explicit structural FAIL. | No fabricated all-green safety dashboard. |
| Test runner | Window → General → Test Runner → EditMode; inspect `CoreTests` and `OrbitalTests`; capture native result after run/export. | Available bridge status saved in `pitch_outputs/unity_test_job.json`; passed/skipped breakdown missing. |
| Convergence | `pitch_outputs/partial_convergence.png`, from aligned recorded scalar differences. | Title “partial-duration, interpolation-based”; no full-duration claim. |
| Resources | `pitch_outputs/selected_propellant.png`, `selected_array.png`. | These are histories, not an exhaust/array optimization sweep. |
| Siting screen | `pitch_outputs/array_siting_sweep.csv`, table in §7. | Kinematic screen, not independent mission runs. |
| Architecture | Draw §4 arrow chain or capture `Get-ChildItem Assets/Scripts/Core,Assets/Scripts/Runtime,Tools,Config` output. | Keep physics and playback separate. |
| Structural failure evidence | Capture §9 table and `Results/partial_convergence.json` relevant SF fields. | Honest failure is a substantive pitch insight. |

Regenerate chart data with `dotnet pitch_outputs/build/PitchAudit.dll E:/spidergamedev/OctoEarth-Simulator`; regenerate PNGs with `./pitch_outputs/Make-Plots.ps1`. `pitch_outputs/Audit.cs` uses core constants and actual recorded coordinates; curves may be visually decimated by the plot script, while CSVs retain all complete records. No source simulation was rerun to fabricate chart points.

## 17. REPO FACTS

| Fact | Finding / source |
|---|---|
| Workspace path | `E:\spidergamedev\OctoEarth-Simulator` (current workspace) |
| Repository URL / last commit | NOT FOUND; Git metadata unavailable (§15). |
| README setup/run/test | Present: Unity version/scene, SDK build/test, headless fallback, benchmark/baseline/convergence commands. Some status/default-viewer claims are stale; use selected-prefix evidence. `README.md`. |
| Build verified now | Optimized direct Roslyn fallback succeeds using `-optimize+`; no SDK/MSBuild Release build claim. `Tools/Build-HeadlessWithoutSdk.ps1`, `pitch_outputs/build/compile.rsp`. |
| SDK build/test | Intended `dotnet build Tools/OctoEarth.Cli/OctoEarth.Cli.csproj`; `dotnet test Tools/OctoEarth.Cli.Tests/OctoEarth.Cli.Tests.csproj`. NOT EXECUTABLE here without SDK. |
| Full selected-run command | From root: `dotnet pitch_outputs/build/OctoEarth.Cli.dll mission --config Config/mission_design_candidate.json --dt 3600 --years 10000 --output pitch_outputs/selected_full_h3600`. Not executed for this report; it cannot be substituted by the fresh diagnostic. Fine comparison needs matching command with `--dt 1800` and distinct output. |
| License | Project-root license NOT FOUND. Third-party package licenses do not license the project. Exact inventory: `rg --files -g '*LICENSE*' -g '*LICENCE*' -g '*COPYING*' -g '!Library/**' -g '!Temp/**'`; obtain project owner's intended license. |
| Output provenance | New artifacts in `pitch_outputs/`; source/config and frozen raw mission CSVs were not intentionally modified. This Markdown is the requested deliverable. |

## MISSING OR WEAK

- Selected-design full-duration arrival, post-target departure and h/h2 convergence are missing.
- Retained tractor rock/mount strength fails the required safety factor; global thermal/optical structure is unvalidated.
- Giant focusing optics remain unresolved; predeployment, material transport and ideal conversion depend on declared enablers.
- All mission safety thresholds are placeholders; full atmosphere, rotation, magnetosphere and biological-radiation checks are absent.
- Ejecta future escape/clearance is not ballistically demonstrated; long-term ephemeris is approximate.
- Complete selected-prefix all-step extrema, restart histories and integrated energy totals were not saved.
- No exhaust/hover-distance propellant trade sweep or corresponding optimization plot exists.
- Native Unity passed/skipped summary and SDK/xUnit execution remain unavailable, despite successful bridge job and passing fresh headless comparisons.
- Current Moon/scale visuals need a fresh rendered capture; all-threshold safety dashboard and verified stress coloring are missing.
- Git author contributions, remote URL, last commit and a project license are unavailable in this workspace.
- Hackathon guide text itself was not provided; compliance claims are limited to the requested checklist, not a verified external rulebook.
