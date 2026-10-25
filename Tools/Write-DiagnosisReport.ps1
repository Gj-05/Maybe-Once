$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$d=Get-Content -Raw -LiteralPath (Join-Path $root 'Results/tractor_diagnosis.json') | ConvertFrom-Json
$a=Get-Content -Raw -LiteralPath (Join-Path $root 'Results/diagnosis_initial_audit.json') | ConvertFrom-Json
$b=Get-Content -Raw -LiteralPath (Join-Path $root 'Results/diagnosis_before_after.json') | ConvertFrom-Json
$s=$d.missionSummary
if($null -eq $s){throw 'Full mission has no terminal report yet; do not publish a final diagnosis.'}
function N($x){if($null -eq $x){return 'not logged'}; return [string]::Format([Globalization.CultureInfo]::InvariantCulture,'{0:G12}',[double]$x)}
$lines=[Collections.Generic.List[string]]::new()
function L([string]$text=''){$lines.Add($text)}
L '# Tractor visibility and orbit-transfer diagnosis'
L
L 'Executed on 2026-10-25. No existing acceptance tolerance was changed. Primary evidence: Results/tractor_diagnosis.json, diagnosis_initial_audit.json, diagnosis_unity_regression.json and the full mission CSV/JSON.'
L
L '## Failing layer'
L
L 'The headless trajectory grows. The visibility/motion failure was in presentation: the earlier inspection loaded only a separate 100-year diagnostic and replaced the recorded direction with a constant Earth-centred +X separation. It hid the Sun and the heliocentric trajectory. The separately selectable live M2 baseline contains no tractor propulsion and was not a mission run. Playback previously advanced sample indices, not a configurable simulation-time clock.'
L
L 'The corrected default view streams Results/mission_h3600_y10000/run_mission.csv and labels its exact source. It preserves Sun-relative double coordinates before float conversion, supports Sun / transfer and Earth + tractor views at the same time, and shows a visible enlarged tractor callout with a leader to the true CSV position. Earth-tractor close views preserve the recorded direction in inertial axes. The Sun, Earth and marker enlargement factors are labelled. Schematic thruster dimensions are not claimed as validated hardware. A gravitational pull direction arrow and a rolling two-orbit CSV trajectory are visible. The playback clock supports 1e9x and a full-duration year slider. Between samples, positions use Hermite display interpolation; a/thrust readouts use linear interpolation. No interpolated result is used for physics or gate acceptance.'
L
L '## Full configured run'
L
L ('Command: dotnet Tools/HeadlessMission/OctoEarth.Cli.dll mission. Configured duration '+(N $a.configuredYears)+' Julian years = '+(N $a.durationSeconds)+' s; fixed timestep '+(N $a.dtSeconds)+' s. Terminal time '+(N ($s.seconds/31557600))+' yr; completed='+$s.completed+'; partialStep='+$s.partialStep+'.')
if($s.targetTimeSeconds -ge 0){L ('The recorded EMB semi-major axis reached 2.7 AU at '+(N ($s.targetTimeSeconds/31557600))+' yr. Terminal a_EMB='+(N $s.finalA_AU)+' AU.')}else{L ('The run did NOT reach 2.7 AU. Terminal a_EMB='+(N $s.finalA_AU)+' AU.')}
L ('Mission status: '+$s.status+'. Terminal EMB eccentricity='+(N $s.finalE)+'. Reaching a semi-major-axis reference does not establish a circular, safe mission or WIN. Powered radial parking after the target is part of the configured implementation and its mass/power remain booked.')
L
L '| Time (yr) | Earth heliocentric a (AU) | EMB a from CSV (AU) | Thrust (N) | mdot (kg/s) | All infrastructure ejecta (kg) |'
L '|---:|---:|---:|---:|---:|---:|'
foreach($row in @($d.epochs)+@($d.lastRecorded)){L ('| '+(N $row.year)+' | '+(N $row.aEarthAU)+' | '+(N $row.aEmbAU)+' | '+(N $row.thrustN)+' | '+(N $row.mdotKgPerSecond)+' | '+(N $row.totalEjectedKg)+' |')}
L
L 'The exact 0/1/10/100/1000-year rows come from a second run of the same unmodified dynamics with additional read-only logging. Across all 12,176 shared records its original columns match the full run byte-for-byte. The final row comes from the full-duration CSV. Earth a uses GM_sun+GM_earth; the mission CSV a uses the Earth-Moon barycentre and GM_sun+GM_earth+GM_moon. Lunar motion gives Earth short-period variations; Earth a need not rise between every selected epoch. The initial thrust=0 row is written before the first command, not a guidance cutoff. mdot is derived from the recorded end-half-kick thrust divided by ve*cos(cant); measured cumulative ejecta includes tractor, array and mirror. End-half-kick thrust is not a step-average thrust.'
L
L ('Tractor mass='+(N $a.tractorMassKg)+' kg. Initial d='+(N $a.distanceMetres)+' m; final recorded d='+(N $d.lastRecorded.tractorDistanceMetres)+' m. Mass is replenished under E1; the tractor body mass is held finite and constant, while supplied/ejected material is booked. Initial resolved gravitational acceleration along Earth velocity='+(N $a.earthAccelerationAlongVelocity)+' m/s^2; final='+(N $d.lastRecorded.earthTractorAccelerationAlongVelocity)+' m/s^2.')
L ('Tractor-only instantaneous EMB da/dt at t=0='+(N $d.epochs[0].tractorContributionDaDtAUPerYear)+' AU/yr. Direct measured changes in a_EMB give first-year mean '+(N ($d.epochs[1].aEmbAU-$d.epochs[0].aEmbAU))+' AU/yr and first-100-year mean '+(N (($d.epochs[3].aEmbAU-$d.epochs[0].aEmbAU)/100))+' AU/yr. The full N-body derivative includes planetary/lunar perturbations; it is not identical to the tractor-only energy derivative.')
L
L '## Ordered force, units and control audit'
L
L ('1. Actual body list has '+$a.bodyCount+' finite bodies, including tractor mass '+(N $a.tractorMassKg)+' kg. Its Earth-relative offset has cosine '+(N $a.aheadCosine)+' with heliocentric EMB velocity.')
L ('2. Reciprocal pair gravity: Earth force '+(N $a.earthTractorForceN)+' N toward tractor; tractor Earth-gravity acceleration '+(N $a.tractorEarthGravityMetresPerSecondSquared)+' m/s^2 toward Earth. Relative pair force imbalance '+(N $a.reciprocalForceRelativeError)+'. G1 also measures finite Earth velocity change with the solar contribution removed, rather than relying only on a formula evaluation.')
L ('3. Initial feedforward thrust '+(N $a.feedforwardThrustN)+' N, outward-direction cosine '+(N $a.feedforwardOutwardCosine)+'. Gravity + command - prescribed acceleration residual '+(N $a.feedforwardResidualAcceleration)+' m/s^2. Exhaust uses the opposite thrust direction in both canted streams. G1/G2 exercise the actual booked kicks; initial feedforward magnitude differs slightly from the discrete Mode B end-half-kick diagnostic.')
L ('4. ve='+(N $a.exhaustSpeedMetresPerSecond)+' m/s; d in metres; mass in kg; dt='+(N $a.dtSeconds)+' s; duration='+(N $a.durationSeconds)+' s. No km/s or year/second substitution.')
L ('5. Initial a_EMB='+(N $a.aEmbAU)+' AU; cutoffAtStart='+$a.cutoffAtStart+'. Cutoff compares SI semi-major axis to '+(N $a.targetMetres)+' m (2.7 AU).')
L ('6. Initial array area '+(N $a.initialArrayArea)+' m^2 supplies '+(N $a.initialSolarElectricalPowerW)+' W electrical versus initial tractor feedforward jet demand '+(N $a.feedforwardJetPowerW)+' W. E1 cap '+(N $a.materialCapKg)+' kg; terminal material used '+(N $s.materialKg)+' kg. Actual resource/stop flags are listed below, without changing limits.')
L ('7. Earlier viewer coverage was only 100 years; this command requested '+(N $a.configuredYears)+' years. Terminal duration and completion state are reported above.')
L ('8. Mode B recomputes its target each step. On a cloned initial full state, the one-step prescribed target moves '+(N $a.prescribedTargetMovementOneStepMetres)+' m; it is not a fixed world-space location. The actual logged hover separation and G2 impulse comparison validate the running solver.')
L
L '## Ledger and flags'
L
L ('Full-run maxima: relative P='+(N $s.momentumError)+', L='+(N $s.angularError)+', E='+(N $s.energyError)+', actual-kick mismatch='+(N $s.impulseError)+', reaction closure='+(N $s.reactionError)+'. Mechanical acceptance remains 1e-9. See mission ledger.txt for solar energy-sector closure. Minimum illustrative structural factor='+(N $s.minimumSafetyFactor)+'; temperature range='+(N $s.temperatureMinimum)+'..'+(N $s.temperatureMaximum)+' K; maximum shading='+(N $s.maximumShade)+'.')
L
foreach($flag in $s.flags){L ('- '+$flag.code+' at '+(N ($flag.firstTimeSeconds/31557600))+' yr: '+$flag.cause)}
L
L '## Fresh acceptance reruns and viewer verification'
L
foreach($gate in @(@{name='G1';path='Results/g1_results.json'},@{name='G2 (300 s)';path='Results/g2_h300/g2_results.json'},@{name='G3 (3600 s)';path='Results/g3_h3600/g3_results.json'},@{name='G3 (1800 s)';path='Results/g3_h1800/g3_results.json'})) {
    L ('### '+$gate.name)
    $g=Get-Content -LiteralPath (Join-Path $root $gate.path) -Raw | ConvertFrom-Json
    L '| Check | Measured | Expected | Unchanged tolerance | Result |'
    L '|---|---:|---:|---:|---|'
    foreach($r in $g.results){L ('| '+$r.id+' | '+(N $r.measured)+' | '+(N $r.expected)+' | '+(N $r.tolerance)+' | '+$(if($r.passed){'PASS'}else{'FAIL'})+' |')}
    L
}
L '32/32 fast Unity tests passed through MCP (job 393a7ced54164a4bb2f49d49f534546f). Two viewer regressions passed after presentation refinements (job f8eb6d82c99f4e20b3320b833f8e5a4a) and again after adding script-reload cache recovery (job f1709406f80c4b5a9bbecec1e8a1bffc: succeeded, two completed, no failures; its per-test result payload was unavailable after reload). The fixture initially failed because Unity Test Runner uses an unsaved scene; the isolated-object fixture was corrected without changing any assertion or physics tolerance. That failure is preserved in diagnosis_viewer_fixture_initial_failure.json. A live-file sharing error was fixed using a read-only FileStream with FileShare.ReadWrite. Editing a script during Play exposed lost parsed recording state; LateUpdate now reloads that cache before rendering, with an exact-count recovery assertion.'
L
L 'The viewer regression measured a quarter-year Earth displacement of 28.52745 Unity units (old fixed-origin inspection: zero). It checks exact CSV-to-display coordinates, Sun visibility, unchanged playback time between views, 1e9 simulated seconds per real second, endpoint clamping and unchanged baseline physics. Actual MCP Play-mode inspection measured the local quarter-year tractor position as (1.58, 0, -7.84) units, instead of the old fixed (8,0,0), and confirmed the M2 baseline clock is paused, the same Earth is retained, the full mission filename is loaded and the playback advances exactly 1e9 seconds from a one-second clock input. Game View screenshots verify Sun/tractor marker visibility and the close view. Headless acceptance was run by CLI, not through MCP; no live 10,000-year mission integration in Unity or xUnit SDK run is claimed. G4/G8 remain unaccepted; this task does not bypass their separate gates.'
L
L '## Source files changed'
L
foreach($path in @('Assets/Scripts/Core/MissionRecording.cs','Assets/Scripts/Core/MissionDiagnosticAudit.cs','Assets/Scripts/Core/MissionSimulation.cs','Assets/Scripts/Runtime/TractorInspection.cs','Assets/Scripts/Editor/TractorViewSetup.cs','Assets/Scenes/OctoEarth_Main.unity','Assets/Tests/EditMode/CoreTests.cs','Assets/Tests/EditMode/MissionViewerTests.cs','Assets/Tests/EditMode/OctoEarth.Tests.EditMode.asmdef','Tools/OctoEarth.Cli/Program.cs','Tools/Diagnose-Mission.ps1','Tools/Plot-MissionDiagnosis.ps1','Tools/Write-DiagnosisReport.ps1','Tools/Update-GateReport.ps1','README.md','LOG.md','Docs/M3_Design.md','Docs/Tractor_Diagnosis.md')){L ('- '+$path)}
L
L 'Unity also generated .meta files for the two new Core scripts and MissionViewerTests.cs. The full mission CSV/JSON/ledger/final-state/enabler outputs, checkpoint run, rerun gate evidence, diagnosis JSON, Unity test reports, static plot and Game View captures are generated evidence. Physics integration, forces, guidance, resource limits, initial conditions and acceptance tolerances were not altered; MissionSimulation changes expose a cloned audit state and add read-only checkpoint/diagnostic logging.'
L
L '![Measured mission orbit growth](../Results/figures/mission_diagnosis_a.png)'
[IO.File]::WriteAllLines((Join-Path $root 'Docs/Tractor_Diagnosis.md'),$lines)
