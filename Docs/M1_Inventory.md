# M1 inventory before M2

Inspected 2026-10-25 through Unity MCP and filesystem. Unity 6000.3.0f1; URP 17.3.0; InputSystem 1.16.0; Test Framework 1.6.0. No applicable AGENTS.md found. No .NET SDK installed (8.0.12 and 10.0.11 runtimes present).

Before any changes, Unity EditMode job `33de21175f874fc688df8bc3006d2d5d` ran all 8 M1 tests: 8 passed, 0 failed, 0 skipped. Duration 0.837567 s. Ten-orbit energy drift 1.2456717395734752e-13; specific angular momentum drift 3.265460766563085e-14; radius deviation 2.5686827020976466e-7 AU. Period 365.2569608762015 days. Analytic coarse/fine errors 1.0759771015077251e-6 / 2.6899430846955429e-7 AU. Console: no project errors; one MCP WebSocket warning.

Existing files and roles (plus Unity .meta files):

- Assets/Scripts/Physics/{Vector3d,OrbitalState,OrbitalMechanics,OrbitSimulation}.cs: engine-free double two-body baseline, diagnostics, Verlet; OctoEarth.Physics.asmdef.
- Assets/Scripts/Runtime/{OrbitalCoordinates,OrbitController,OrbitCamera,MissionDashboard,CameraFacingLabel}.cs: display conversion, scheduling, orbit controls, UI Toolkit, billboard labels; OctoEarth.Runtime.asmdef.
- Assets/Scripts/Editor/OctoEarthSceneBuilder.cs and OctoEarth.Editor.asmdef: builds baseline scene.
- Assets/Tests/EditMode/OrbitalTests.cs and OctoEarth.Tests.asmdef: eight NUnit regressions.
- Assets/Scenes/{SampleScene,OctoEarth_Main}.unity: original and M1 scene.
- Assets/Materials/OctoEarth/{Sun,Earth,Reference,Target,Trajectory}.mat and EarthAlbedo.asset.
- Assets/UI/{OctoEarthPanelSettings.asset,OctoEarthTheme.tss}: runtime panel/theme.
- Documentation/Milestone1.md: M1 numerical results and limitations.
- Captures/Milestone1-*.png: M1 visual inspection captures.
- Assets/InputSystem_Actions.inputactions, Assets/Settings/*: template input, URP renderer/pipeline and volume profiles.
- Assets/TutorialInfo/* and Assets/Readme.asset: original template documentation/editor helper.

Scene roots: Sun, Earth (Earth Label child), Simulation, 1 AU Reference, 2.7 AU Target Reference, Computed Trajectory, Main Camera, Directional Light, Mission Dashboard, Sun Label, Target Label, Scale Label. Camera components: Transform, Camera, AudioListener, UniversalAdditionalCameraData, OrbitCamera. No missing scripts observed.

M2 moves physics files and their .meta GUIDs into Core; namespaces/references change, M1 equations and assertions stay intact. Viewer script GUIDs stay intact. The M1 GM remains documented in OrbitalMechanics for regression reproducibility; new N-body code uses R2 Constants.
