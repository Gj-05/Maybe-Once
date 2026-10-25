# Milestone 2 implementation and validation

## Status

The original M2 run exposed a rounded reference inconsistency (details retained below). The user subsequently approved independent analytic acceptance at 1e-9 relative and a separate 2% guide sanity check. That correction is implemented in MissionBudgetChecks.cs, with three input fixtures and all budget fields audited. Actual rerun: 29/29 M1/M2 Unity tests passed; adding G1 produces 30/30. The physical budget equations and constants are unchanged. M3 is advancing in gate order; no mission result is claimed. Historical failed-reference files remain preserved.

## Assemblies and file changes

- `Assets/Scripts/Core/OctoEarth.Core.asmdef`: No Engine References, namespace OctoEarth.Core. Four M1 source files and their .meta GUIDs moved from Physics to Core: Vector3d.cs, OrbitalState.cs, OrbitalMechanics.cs, OrbitSimulation.cs. M1 equations/initial GM and test assertions are preserved. The M1 GM is 1.32712440041e20; new Constants uses the requested R2 1.32712440018e20.
- New Core files: Constants.cs (Units/KahanSum/KahanVector), SystemState.cs (Body/GravityModel/IForceModel), Integrators.cs (IIntegrator/Verlet/Yoshida4/SimulationClock), KeplerOrbit.cs, InitialConditions.cs, Engineering.cs (Lorentz/Beam/Insolation/Maneuvers/MissionBudget), Ledger.cs (LedgerLine/PhotonSink), EjectaDrive.cs, Encke.cs, SimulationRunner.cs, BenchmarkSuite.cs, ValidationSuite.cs.
- Runtime asmdef renamed to `OctoEarth.Viewer.asmdef` while preserving its GUID. Existing MonoBehaviour names, namespaces, script files and GUIDs are preserved. OrbitController delegates deterministic scheduling to Core and N-body integration/ledger to SimulationRunner. OrbitalCoordinates, MissionDashboard and Editor scene builder references point to Core. Added MoonInset.cs and ValidationPanel.cs.
- Test asmdef renamed `OctoEarth.Tests.EditMode.asmdef`, references Core. OrbitalTests.cs only changes namespace import; the original eight tests remain. Added CoreTests.cs, shared through conditional NUnit/xUnit attributes.
- Editor assembly references Core and Viewer. Added Milestone2Tools.cs for benchmark menu and viewer upgrade.
- Added `Assets/Data/solar_j2000.csv` with sources, epoch, approximation flag and explicit lunar idealization.
- Scene `Assets/Scenes/OctoEarth_Main.unity` upgraded; added Moon.mat, Earth–Moon inset and ValidationPanel. SampleScene untouched (SHA256 07FCE8B04804DDCCB3026DB5B43D8DC41BA138E49D50D36A5C24877081EACDCA).
- Added Tools/OctoEarth.Core.Shared, Tools/OctoEarth.Cli, Tools/OctoEarth.Cli.Tests project files, CLI Program.cs and Build-HeadlessWithoutSdk.ps1. Tools/HeadlessBuild contains locally generated compiler outputs and response file.
- Results/ contains executed benchmark JSON/text, convergence JSON/CSV, Unity test job JSON, and 10,000-year baseline validation/CSV/metadata. README, Docs/M1_Inventory.md, this report and generated benchmark table document reproduction and limitations. `Docs/M2_Files.txt` lists exact current project files in these changed scopes, including .meta files.

## Model, equations and numerical method

All authoritative positions/velocities/time/mass are doubles. Body accelerations sum G m_j (r_j-r_i)/|r_j-r_i|^3, with compensated component sums. Default softening is zero. The gravitational force pair is internal. Kinetic energy is sum m v²/2; potential is sum over unique pairs -G m_i m_j/r. Momentum is sum m v and angular momentum sum r×m v. Initial barycentric position and velocity subtract the mass-weighted center and speed. Model masses are GM/G so force, inertia and ledger use a consistent mass convention; separately rounded R2 reference masses remain available for budgets.

Verlet applies half kick, drift, half kick. Yoshida4 composes Verlet with w1=1/(2−2^(1/3)), w0=−2^(1/3)/(2−2^(1/3)), w1. Negative substeps apply only to gravity. Nonconservative forces are positive-time half-kicks before and after gravity, so the forced method is second-order, not a claim of fourth-order forced integration.

Kepler elements use elliptic anomalies with Newton solution of E−e sin E=M and 3D rotations. State-derived elements use specific energy, h=r×v and eccentricity vector v×h/mu−r/|r|. Hohmann uses circular speeds and vis-viva for both burns; transfer time is half the transfer ellipse period. Gauss da/dt=2a²(v·a_push)/mu. Benchmark formulas and numeric curvature integration are separate implementations.

Encke evolves delta r/delta v around an analytic reference ellipse with RK4 and a cancellation-resistant acceleration difference, using q=delta r·(2r_ref+delta r)/|r_ref|². This is validated for a day-long tiny push. It does not implement long-run reference rectification and is not used to claim a long-duration forced mission.

Lorentz F=I L×B; closed-loop segment sum; B_wire=mu0 I/(2πr). Beam root moment qL²/2, tip displacement qL⁴/(8EI), stress |N/A|+|Mc/I|, Euler Pcr=π²EI/(KL)², self-support height sigma/(rho g), safety factor allowable/actual. Radiation S=L/(4πr²); equilibrium T=[S(1−albedo)/(4 sigma)]^(1/4). These are isolated benchmark models, not an implemented planetary structure or climate model.

## Ledger scope and force accounting

The ledger compares bodies plus booked sink/source totals to initial totals. Momentum error normalizes by initial sum m|v| (not near-zero barycentric net P); angular momentum uses the initial gross magnitude sum; energy uses |E0|. Each reported metric is the maximum absolute relative deviation over all checked steps. Tiny impulse closure separately normalizes by the applied impulse.

The conservative ledger uses the matching Plummer potential if optional softening is enabled (zero by default). A dedicated softened-energy check passes with 3.3930953475924023e-15 relative error against 1e-9. Reset also clears ejecta bookkeeping and is checked for deterministic repeat runs with a force model. Detailed toy source/sink values are written to `Results/ejecta_ledger.txt`. For variable mass, the impulse comparison includes advected momentum; d(mv) is not m dv. Constant-mass replenished runs check the stated thrust impulse directly.

EjectaDrive supports (a) replenished constant mass, with supplied matter at the body's instantaneous average velocity, and (b) finite-mass rocket toy burns. mdot=T/ve; each kick books ejecta mass, momentum, angular momentum and kinetic plus gravitational potential energy. EnablerSource books negative supplied mass/momentum/energy; DriveEnergySource explicitly supplies 0.5 dm ve². That energy is derived from exhaust speed and mass, not fitted to an energy residual. The variable-mass toy integrates parcel momentum and energy independently with eight-point quadrature and checks the exact rocket equation ve ln(m0/m1). The toy has a small fractional burn, where this quadrature is accurate; arbitrarily large single-kick mass loss is not validated. Subdivide large burns.

Frozen sinks represent exported parcels at emission; parcels are not retained as gravitationally interacting N-body bodies. Their subsequent gravitational backreaction, self-gravity and plume interactions are not modeled. Their solar binding flag tests heliocentric parcel energy. This module is suitable for the stated M2 toy tests and bookkeeping infrastructure, not proof of full long-term ejecta dynamics or mission conservation. Prograde benchmark forcing has an explicit BenchmarkExternalPartner line. PhotonSink provides momentum/angular momentum/energy booking but no optical model.

## Validation and tolerances

The initial M1 regression job passed 8/8 before edits. The retrieved final M2 Unity job ran 28 explicit fast tests: 27 passed, one failed (MissionBudgetReference). All ten numbered benchmarks and all eight M1 tests passed. The full raw job output is in Results/unity-editmode.json. A first broad assembly selection included a long/explicit run and MCP disconnected; its result is not counted. The final run uses explicit fast names. No fabricated success is inferred from a lost job.

The CLI compiled under C# 9 with Unity's bundled compiler and linked only installed .NET runtime assemblies, then ran without Unity Engine assemblies or a graphics context. `bench` reports all rows, `converge` writes the order-fit samples, and `baseline --years 10000` executed 87,660,000 fixed hourly steps for Sun–Earth–Moon. Energy drift 2.197799408716484e-12, momentum drift 7.30874606216296e-13, angular momentum drift 1.0662853070396398e-12, each against 1e-9. This is a conservation record for an approximate model, not a transfer or safety result.

The benchmark tolerances are unchanged from R4. Long-wire tolerance is interpreted as 1e-6 relative (more restrictive than an absolute 1e-6 T). B4 uses T/20000 exactly for 200000 steps. B5 passes at 3600s without reduction. B7 uses 10000 equal steps to the analytic transfer endpoint, then the second burn. B8 uses 8766 hourly steps, exactly one Julian year. B9 independently integrates load moment and elastic curvature using Simpson quadrature. Extra conservation limits allow floating point accumulation. The Encke/double displacement tolerance 0.05m is below 0.14% of the ~37m signal and above cancellation-level double roundoff at AU distances. Order fits use four successively halved steps on an eccentric orbit; expected orders 4±0.2 and 2±0.12 allow pre-asymptotic error. The rows marked order-fit samples are data, not individual high-accuracy bounds.

## Initial conditions and availability

JPL Table 1 J2000 fitted elements are used only as approximate initial values, not extrapolated as a 10,000-year ephemeris. Lunar phase=0, inclination=0 and eccentricity=0 are explicitly declared idealizations at 384400km. No Horizons data was obtained or silently invented. Secular rates and a full ephemeris model are not included. JPL giant planet GM values represent their systems; their satellites other than Earth's Moon are not separate bodies.

.NET SDK is absent. Both `dotnet build` and `dotnet test` were attempted and failed due to SDK absence; xUnit/MSBuild are supplied but not executed. The fallback CLI compilation and execution are real. No separate Unity batchmode launch was attempted against the locked open project. No standalone Unity player build or manual mouse gesture test is claimed. Computer-use app inspection was unavailable because app approval timed out; MCP recovered and was used to complete inspection.

Final Unity fast test job `48e7343296fa4bef90cf5b6a19abf99b`: 28 total, 27 passed, 1 failed, 0 skipped; 11.1018397 seconds test duration. This includes the stateful reset and softened-energy checks. A subsequent viewer-only change hides the Moon inset when M1 mode is selected; project compilation is checked separately. MCP Play inspection verified ten bodies, parsed recorded `X9.F: FAIL`, live ledger labels, 87,512,400 seconds elapsed, 1,014 trajectory points and Moon inset position (34.72,0,-26.87). Missing-script count was zero. Console errors/warnings were zero at runtime. Play was stopped afterward.

## M3 gate

This historical gate was explicitly resolved by the user's approval: analytic checks are now tighter (1e-9 relative), while rounded guide illustrations have a separate 2% sanity test. No production constants were changed. G1/G2 headless tractor mechanics have since been implemented and measured; subsequent M3 gates remain separately tracked in Results/m3_gate_status.json.
