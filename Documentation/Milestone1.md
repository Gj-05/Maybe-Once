# OctoEarth Milestone 1

Open `Assets/Scenes/OctoEarth_Main.unity`, enter Play mode, and click Start.
Pause freezes physics; Resume continues; Reset restores the exact circular initial state and clears trajectory and numerical monitors. Speed is simulated days per real second. Changing the step dropdown resets the run. Default step: 3600 s; selectable 1800/3600/7200 s. The inspector exposes an explicit step in seconds (set before Play). Camera: right drag orbit, middle drag pan, scroll zoom outside the dashboard. Scroll the dashboard on short Game views.

This is BASELINE ORBIT — NO PROPULSION. The 2.7 AU circle is a reference, not a transfer. The 10,000-year transfer objective, propulsion, structures, thermal limits and safety are not implemented. Earth is initialized on a circular idealization, not an ephemeris of present-day Earth. The Sun is fixed; Earth is a test particle. No mutual solar recoil, other bodies, relativity or material stresses. Monitored quantities are heliocentric **specific** energy and angular momentum, not total energy/momentum of a fully interacting solar system.

## Constants and equations

- AU = 149597870700 m (exact IAU conventional SI distance), [JPL parameters](https://ssd.jpl.nasa.gov/astro_par.html).
- Solar GM = 1.32712440041e20 m³/s², TDB-compatible DE421 fit listed by [IAU NSFA](https://iau-a3.gitlab.io/NSFA/NSFA_cbe.html). This is an observational solar parameter, not an exact measurement or a multiplication of separately rounded G and mass.
- r0 = (AU,0,0), v0 = (0,sqrt(GM/AU),0), time = 0.
- a(r) = -GM r / |r|³.
- Velocity Verlet: r1 = r0 + v0 dt + a(r0) dt²/2; v1 = v0 + [a(r0)+a(r1)] dt/2.
- Specific energy ε = |v|²/2 − GM/|r|, specific angular momentum h = r × v.
- Osculating semi-major axis a = −GM/(2ε); eccentricity e = |(v × h)/GM − r/|r||.
- Analytic circular reference: r(t) = AU (cos(nt),sin(nt),0), n = sqrt(GM/AU³), P = 2π/n.

Authoritative state is double precision in the Unity-independent `OctoEarth.Physics` assembly. No Rigidbody or Unity integration timestep is used. Wall time schedules an integer number of explicit fixed steps; it never replaces dt. Work is capped at 2000 steps/frame and queued time is retained and displayed. A slow Editor can build a backlog; actual simulated time is always the integrated state time. Distance maps to 20 Unity units/AU, physics XY to visual XZ. Visual floats never feed physics. Bodies are exaggerated. The computed trail retains at most 4096 approximately daily samples plus the current position.

## Executed numerical validation

Unity 6000.3.0f1; Test Framework 1.6.0; EditMode through Unity MCP. Job `c5084210755541738db7526ebd79fd62`: **8 passed, 0 failed, 0 skipped**, 0.0895239 s test duration.

| Check | Observed | Asserted tolerance |
|---|---|---|
| Circular initialization | speed 29784.69183427775 m/s; e = 1.1102230246251565e-16 | speed 1e-10 m/s absolute; e < 1e-14 |
| Period from unwrapped phase crossing with linear interpolation | 365.2569608762015 d; analytic 365.25689832762095 d | relative error 1.7124544626767602e-7 < 1e-6; within 0.01 d of 365.25636 d sidereal year |
| Maximum relative specific energy drift, ten orbits | 1.2456717395734752e-13 | < 1e-10 |
| Maximum relative specific angular momentum drift, ten orbits | 3.265460766563085e-14 | < 1e-11 |
| Maximum radial deviation, ten orbits | 2.5686827020976466e-7 AU | < 1e-6 AU |
| Analytic endpoint position error, 8766 × 3600 s | 1.0759771015077251e-6 AU | < 2e-6 AU |
| Same endpoint, 17532 × 1800 s | 2.6899430846955429e-7 AU | < 5e-7 AU |
| Coarse/fine error ratio | 3.9999995079059745 | 3.8–4.2 |
| Reset | all position/velocity coordinates and elapsed time match exactly | exact double equality |
| Vector operations and invalid timestep | passed | arithmetic checks; zero/NaN rejected |

Tolerances reflect the second-order method: (n dt)² ≈ 5.14e-7 at dt=3600 s. Radial and accumulated phase errors scale as dt²; the circular-orbit energy error is especially small and bounded. Angular momentum is preserved algebraically by central-force Verlet up to rounding; 1e-11 permits accumulated floating point error over roughly 87,664 steps. Energy tolerance 1e-10 leaves margin above the observed circular baseline, and is not a promise for eccentric or forced trajectories. Halving dt must independently reduce the analytic position error by about four; agreement between two numerical runs alone is not used as truth. These tests cover ten orbits, not the future 10,000-year mission.

Run again via Window > General > Test Runner > EditMode > OctoEarth.Tests. The tests print measured errors to their output. Live dashboard bounds are intentionally looser and are explicitly separate from test results. The observed period differs slightly from the actual sidereal year because this idealized model omits planetary masses and perturbations.

## Scene and assets

Scene roots: Sun, Earth (with Earth Label child), Simulation, 1 AU Reference, 2.7 AU Target Reference, Computed Trajectory, Main Camera, Directional Light, Mission Dashboard, Sun Label, Target Label, Scale Label.

Sun/Earth use MeshFilter/MeshRenderer; reference paths and trajectory use LineRenderer. Simulation has OrbitController. Main Camera has Camera, AudioListener, UniversalAdditionalCameraData and OrbitCamera. Directional Light has Light and UniversalAdditionalLightData. Mission Dashboard has UIDocument and MissionDashboard. World labels have TextMesh/MeshRenderer and CameraFacingLabel.

Earth albedo is a deterministic schematic blue/green/ice texture, not geographically accurate imagery. Sun uses an emissive URP material; no bloom is required. Dashboard uses UI Toolkit and a persisted default theme.

Created source files:

- `Assets/Scripts/Physics/Vector3d.cs`
- `Assets/Scripts/Physics/OrbitalState.cs`
- `Assets/Scripts/Physics/OrbitalMechanics.cs`
- `Assets/Scripts/Physics/OrbitSimulation.cs`
- `Assets/Scripts/Physics/OctoEarth.Physics.asmdef`
- `Assets/Scripts/Runtime/OrbitalCoordinates.cs`
- `Assets/Scripts/Runtime/OrbitController.cs`
- `Assets/Scripts/Runtime/OrbitCamera.cs`
- `Assets/Scripts/Runtime/MissionDashboard.cs`
- `Assets/Scripts/Runtime/CameraFacingLabel.cs`
- `Assets/Scripts/Runtime/OctoEarth.Runtime.asmdef`
- `Assets/Scripts/Editor/OctoEarthSceneBuilder.cs`
- `Assets/Scripts/Editor/OctoEarth.Editor.asmdef`
- `Assets/Tests/EditMode/OrbitalTests.cs`
- `Assets/Tests/EditMode/OctoEarth.Tests.asmdef`

Created content: `Assets/Scenes/OctoEarth_Main.unity`; `Assets/Materials/OctoEarth/{Sun,Earth,Reference,Target,Trajectory}.mat`; `Assets/Materials/OctoEarth/EarthAlbedo.asset`; `Assets/UI/OctoEarthPanelSettings.asset`; `Assets/UI/OctoEarthTheme.tss`; Unity-generated `.meta` files for these assets and new folders. Documentation and captures live outside Assets. No package version changes. SampleScene is preserved.

MCP tools used: read_mcp_resource, list_mcp_resources, list_mcp_resource_templates, manage_scene, manage_asset, unity_reflect, refresh_unity, execute_code (compiled builder invocation and runtime inspection), run_tests, get_test_job, manage_editor and manage_camera, read_console. The MCP CodeDom runner needs reflection to access project asmdef types and an explicit return value. Earlier failed runner snippets were corrected; their compilation failures were runner diagnostics, separate from Unity project compilation.

Next milestones should first specify model fidelity, external-force and energy budgets, transfer feasibility and independent long-duration validation. Structural and safety models require their own defensible assumptions. No internal-force propulsion is implied.

## Final runtime inspection

Programmatic Play mode smoke checks through MCP (not automated PlayMode tests): dashboard constructed; Start advanced time and moved Earth; the trajectory grew to 54 points after 4,492,800 simulated seconds; Pause held that time exactly across subsequent tool calls; Resume set Running true; changing dt to 1800 s reset time to 0 and Earth to visual (20,0,0). Reset also set Running false. Mouse gestures were implemented but not exercised with physical mouse input. No standalone player build was made. The screenshot was visually inspected; the dashboard scrolls vertically on the small Editor Game view. Runtime captures: `Captures/Milestone1-runtime.png` (initial layout), `Captures/Milestone1-runtime-final.png` (running), `Captures/Milestone1-ready.png` (final layout).

Final Console read after the last compile and Play run: 0 errors, 0 warnings. An earlier MCP WebSocket warning and a transient new-script import compile error were resolved by reconnection/full asset refresh. Final editor: OctoEarth_Main open, Play stopped. SampleScene SHA-256 before and after: `07FCE8B04804DDCCB3026DB5B43D8DC41BA138E49D50D36A5C24877081EACDCA`.

Additional layout capture: `Captures/Milestone1-ready-1.png`. This exposed inherited Earth-label scaling after enlarging the body; the label world scale was then restored in the saved scene. The final saved scene uses 0.3-unit reference lines for visibility. Captures are inspection artifacts, not scientific evidence.
