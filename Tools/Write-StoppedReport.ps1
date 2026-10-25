$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$p=Get-Content -LiteralPath (Join-Path $root 'Results/partial_convergence.json') -Raw | ConvertFrom-Json
$lines=[Collections.Generic.List[string]]::new()
function L([string]$text=''){$lines.Add($text)}
function N($v){return [string]::Format([Globalization.CultureInfo]::InvariantCulture,'{0:G12}',[double]$v)}
L '# Selected-design stopped-prefix report'
L
L '**Verdict: FAIL — StructuralSafety.** Both selected-design simulations are stopped at the user request. No new simulation or test job is being started. No threshold, strength or existing tolerance was changed.'
L
L '| h (s) | Last preserved complete CSV sample (yr) | a_EMB (AU) | e_EMB | Cumulative minimum SF |'
L '|---:|---:|---:|---:|---:|'
foreach($r in $p.runs){L ('| '+$r.stepSeconds+' | '+(N $r.lastPreservedYear)+' | '+(N $r.aAU)+' | '+(N $r.eccentricity)+' | '+(N $r.minimumSafetyFactorCumulative)+' |')}
L
L 'No shading, plume, tide or flux flags were observed in the first 5460.780402 yr (h=3600) and 2361.149954 yr (h=1800) of preserved selected-design data. The cumulative flag count remains four. The names are StructuralSafety, FocusingOpticsUnresolved, MaterialStrengthUnvalidated and ThermalStructuralModelIncomplete, all present in the initial report; no new flag was added in either recorded prefix. The flags are monotonic and never cleared.'
L
L 'The selected design uses a co-moving array at Earth-radius ratio1, inclination10 degrees, phase180 degrees; tractor separation125,000km and Sunward bias5 degrees; proposed post-target departure. Earlier original-layout results must not be attributed to it. The original fixed1AU array, phase0, inclination10, tractor d50,000km/bias0, parking-policy run did complete10,000yr and reach2.7AU at8742.175906913073yr.'
L
L '| Selected-design event | Evidence status |'
L '|---|---|'
foreach($item in @('Arrival at2.7AU','Post-target thrust period or policy comparison','Late-time array/mirror geometry','Full-duration h/h2 convergence')){L ('| '+$item+' | NOT OBSERVED |')}
L
L '## Partial-duration convergence'
L
L ('**'+$p.label+'**. Coarse-minus-fine signed differences are used below. '+$p.method)
L
L '| Quantity | Maximum absolute difference | Final signed difference | Maximum alignment correction from nearest coarse record |'
L '|---|---:|---:|---:|'
foreach($m in $p.metrics){L ('| '+$m.metric+' | '+(N $m.maxAbsoluteDifference)+' | '+(N $m.finalSignedDifference)+' | '+(N $m.maxAlignmentCorrectionFromNearestCoarseRecord)+' |')}
L
L 'The alignment correction is reported to expose sampling sensitivity; it is not a rigorous interpolation-error bound. No additional physics integration was used. The final aligned time is2361.149954369153yr. Scalar Moon separation is aligned, rather than claiming unrecorded lunar positions. Cumulative ledger quantities are compared diagnostically on this same grid. The untouched raw grids and exact shared checkpoints remain available.'
L
L '| Exact shared year | delta a_EMB (AU) | delta e | delta Moon distance (m) | delta thrust (N) | delta ejecta (kg) | delta T (K) |'
L '|---:|---:|---:|---:|---:|---:|---:|'
foreach($e in $p.exactSharedEpochs){L ('| '+(N $e.timeYear)+' | '+(N $e.difference[0])+' | '+(N $e.difference[1])+' | '+(N $e.difference[2])+' | '+(N $e.difference[3])+' | '+(N $e.difference[4])+' | '+(N $e.difference[5])+' |')}
L
L 'Full aligned records: Results/partial_convergence_series.csv. Metrics and exact shared-epoch arrays: Results/partial_convergence.json. The h1800 reconstruction checkpoint is Results/prefix_checkpoint_h1800.json; it retains the exact final complete CSV row and header. It does not contain controller history, compensated sums or full ledger transfer histories, so it is not a byte-exact process restart.'
L
L '## Ledger, Moon, tide and flux prefix evidence'
L
L '| h (s) | max relative P | max relative L | max relative E | sampled Moon min/max (km) | final Moon distance (km) |'
L '|---:|---:|---:|---:|---|---:|'
foreach($r in $p.runs){L ('| '+$r.stepSeconds+' | '+(N $r.momentumErrorMaxCumulative)+' | '+(N $r.angularErrorMaxCumulative)+' | '+(N $r.energyErrorMaxCumulative)+' | '+(N ($r.moonDistanceSampledMinMetres/1000))+' / '+(N ($r.moonDistanceSampledMaxMetres/1000))+' | '+(N ($r.moonDistanceFinalMetres/1000))+' |')}
L
L 'Ledger entries above are cumulative maxima stored by the solver, not inferred from the sampled trajectory. All are below the unchanged1e-9 threshold through the preserved endpoints. MoonBound is cumulatively true in both prefixes; no Moon-distance flag was added. The sampled lunar range is not an all-step extremum. Its unchanged placeholder interval is300,000..500,000km.'
L
L '| h (s) | sampled max tide ratio | unchanged tide threshold | sampled max incoming flux (W/m2) | unchanged flux threshold | sampled temperature min/max (K) |'
L '|---:|---:|---:|---:|---:|---|'
foreach($r in $p.runs){L ('| '+$r.stepSeconds+' | '+(N $r.sampledMaxTideRatio)+' |0.01| '+(N $r.sampledMaxFluxWm2)+' |1429.05| '+(N $r.sampledMinTemperatureK)+' / '+(N $r.sampledMaxTemperatureK)+' |')}
L
L 'Exact all-step extrema for these interrupted prefixes were not written to terminal JSON. The earlier completed ten-year candidate probe recorded tide max0.004497568177, incoming-flux max1408.030292421W/m2 and Earth/Moon plume clearances33,714,277.25 /205,206,830.44m. Those clearances describe that ten-year probe only. The long prefixes support the narrower statement that no corresponding threshold flag occurred. Tide, flux, temperature, lunar-distance, plume and SF thresholds are explicitly placeholders; their values were retained.'
L
L '## Structure failure'
L
L '| h (s) prefix | SF | governing bulk-rock proxy stress (MPa) | fixed allowable (MPa) | strength required for SF1.5 (MPa) | multiplier required |'
L '|---:|---:|---:|---:|---:|---:|'
foreach($r in $p.runs){L ('| '+$r.stepSeconds+' | '+(N $r.minimumSafetyFactorCumulative)+' | '+(N ($r.inferredGoverningRockStressPa/1e6))+' |5| '+(N ($r.requiredRockStrengthForSF1_5Pa/1e6))+' | '+(N $r.requiredStrengthMultiplier)+' |')}
L
L 'The required strength is calculated and has NOT been adopted. The inferred stress uses the recorded cumulative SF and the fixed5MPa rock allowable. The completed ten-year component audit identified rock stress35.978756MPa as governing, with array/mirror local bending stresses32.007669MPa and3.612737MPa against their unchanged200MPa allowable. The existing bulk-rock scalar proxy does not resolve a full stress tensor or validate a thruster mount. The separate contact-area/mount studies report tension, compression, shear and bending and all fail at the retained rock strength. At least53.975224MPa (about10.795x) would be required by the worst recorded selected-prefix proxy for SF1.5; this is a failed requirement, not a proposed strength change.'
L
L '## AssumptionRegister'
L
L '| ID | Assumption | Status and consequence |'
L '|---|---|---|'
L '|A1|Giant focusing optical capability|Unresolved pending organiser confirmation. E1/E2 acceptance is not inferred.|'
L '|A2|Material properties represent actual components|Unvalidated illustrative inputs: rock5MPa, local frame200MPa, Young modulus50GPa. Kept unchanged; structure fails.|'
L '|A3|Local proxies adequately represent global thermal/structural/optical behaviour|Not validated. Coupled thermal deformation, global stability and optical pointing structure remain outside the demonstrated local checks.|'
L
L 'E1 supplied material, E2 ideal conversion/transmission/station-keeping and E3 initially placed tractor remain declared enablers. A1-A3 are unresolved assumptions, not additional sources of force, power or unlimited strength.'
L
L '## Benchmarks and gates'
L
L '| Evidence | Status | Measured result / unchanged criterion |'
L '|---|---|---|'
L '|G1|PASS|17 checks; h60/h30 maximum separation errors0.000475132/0.000238364 against0.001; measured acceleration errors5.79363e-6/7.58422e-6 against0.001; ledgers below1e-9.|'
L '|G2|PASS|15 checks; 10yr delivered-dv relative difference0.004605148765 against0.01; both Moon-bound checks pass.|'
L '|G3 h3600|PASS|9 checks; target8758.151380333yr, time error0.007187409 against0.05; largest sampled a error0.009116630 against0.02.|'
L '|G3 h1800|PASS|9 checks; target8710.794889345yr, time error0.001741412 against0.05; largest sampled a error0.002232396 against0.02.|'
L '|G4 selected design|INCOMPLETE|User stopped both full runs. Arrival, post-target period and full-duration convergence NOT OBSERVED. Only the labelled partial-duration comparison exists.|'
L '|G8 selected-design viewer|Pending final read-only scene check|Partial-result playback only; full mission demonstration is incomplete. Moon visibility is reported explicitly below.|'
L
L 'The three new climate/steering/radius tests were reported succeeded by Unity MCP before the stop request (three completed, no failures; detailed payload unavailable after reload). The previous32-test suite predates this design review. No final35-test rerun was started after the user stopped jobs. Older numbered benchmark results below are cached evidence, not newly executed tests of the final viewer edits.'
L
L '| Cached benchmark/check | Measured | Expected | Tolerance | Recorded result |'
L '|---|---:|---:|---:|---|'
$bench=Get-Content -LiteralPath (Join-Path $root 'Results/benchmarks.json') -Raw | ConvertFrom-Json
foreach($b in $bench.results){L ('| '+$b.id+' | '+(N $b.measured)+' | '+(N $b.expected)+' | '+(N $b.tolerance)+' | '+$(if($b.passed){'PASS'}else{'FAIL'})+' |')}
L
L '## Completed design-study evidence'
L
L 'The96-point siting screen is saved in Results/design_audit/siting/array_siting_sweep.csv/json. It reuses native monthly original-layout samples and approximate feedforward forces; it is not96 full integrations. Its36 zero-sampled-intersection candidates do not establish late-time selected-design safety. The selected prefixes are the only integrated evidence for the selected layout.'
L
L 'The exact hourly original-layout thermal replay reproduced the legacy temperature history with zero reproduction error. Peak temperature changed from291.811764818K to292.462340539K after correcting IR absorption and to292.644311224K after also correcting finite-disc quadrature. A sustained-full-shading regression without mirror compensation cools from288K to271.670612927K after1yr and196.732677910K after10yr. Array waste heat remains included. These are separate controlled tests and replays, not selected-design full-duration climate validation.'
L
L 'The first10yr full13-body candidate order study used7200/3600/1800/900/450s. Log-log orders against the finite450s reference: position2.13386035, a2.13387177, T2.06877756. Successive Richardson a orders2.01195979,2.00296762,2.00079178. This supports observed second-order behaviour over that interval, not fourth order or a full-duration order fit.'
L
L '## Limitations'
L
foreach($t in @('Interrupted runs have no terminal mission report, final-state JSON or complete controller/ledger restart. Last internal unsaved steps are unknown; only complete preserved CSV rows are reported.','No arrival, post-target operation, late-time geometry or full-duration convergence was observed for the selected design. Proposed departure has not been exercised. No propellant-saving or coast-versus-parking result is claimed.','Native output grids differ by1800s. Aligned comparison uses explicitly identified scalar interpolation; exact shared epochs are separate. Monthly sampling cannot recover all-step extrema, and the interpolation correction is not an error bound.','The physical model uses approximate J2000 elements and an idealized lunar initial state, not a validated long-term ephemeris. Prescribed infrastructure is realized through booked finite impulses under E2.','Rock and local-beam calculations are engineering proxies. Fixed strengths do not establish global, shear-interface, thermal or optical structural adequacy. The rock proxy fails.','Giant focusing optics are unresolved, the climate model is a simplified global EBM, and exported ejecta are not propagated ballistically to prove future clearance.','The Moon is included in numerical dynamics and monitors. Visibility in the mission viewer is a separate UI capability and will be stated explicitly.','No new simulation or test job was run after the final stop instruction.')){L ('- '+$t)}
L
L '## Viewer and build checks'
L
L 'Final scene source label, Console state, missing scripts and Moon visibility will be appended from the read-only Unity check. The running headless executable used the direct Roslyn -optimize+ flag, without a debug compiler flag; this is an optimised build rather than a dotnet build -c Release invocation. Unity MCP reported editor idle, not playing, not compiling, and no test job running. WMI command-line inspection was unavailable (access denied); native process inspection confirmed the simulation processes exited.'
L
L '## Changed files and evidence'
L
L 'Design physics/analysis changes: Assets/Scripts/Core/MissionEnvironment.cs, MissionRadiation.cs, MissionSimulation.cs, MissionTargets.cs, MissionConstraintStepper.cs, GravityTractor.cs, MissionMonitors.cs, MissionConfigurationIO.cs; new Core modules FiniteDiscRadiation.cs, DesignClimateChecks.cs, DesignStudyAudit.cs, ArraySitingSweep.cs, PlumeSteering.cs, MissionFailureClassification.cs, MissionConvergenceStudy.cs, ThermalCouplingReplay.cs. Also CoreTests.cs, Tools/OctoEarth.Cli/Program.cs, Config/mission_design_candidate.json, Tools/Analyze-MissionOrder.ps1 and Docs/Design_Review_Progress.md.'
L
L 'Stop/report changes: Tools/Compare-StoppedPrefixes.ps1, Tools/Write-StoppedReport.ps1, Results/STATUS.md, Results/stopped_prefix_manifest.json, Results/partial_convergence.json, Results/partial_convergence_series.csv, Results/prefix_checkpoint_h1800.json and this report. Viewer-only changes and verification will be listed after the scene check. New Unity scripts have corresponding .meta files. The raw selected-run CSV files and existing JSON files were not edited.'
[IO.File]::WriteAllLines((Join-Path $root 'Docs/Selected_Design_Prefix_Report.md'),$lines)
