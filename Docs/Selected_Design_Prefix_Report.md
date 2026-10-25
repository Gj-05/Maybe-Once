# Selected-design stopped-prefix report

**Presentation update:** See [Playback presentation audit](Playback_Presentation_Audit.md) for the newly authorised fast test results, no-mirror thermal replay, Moon data/visibility changes, playback controls and verification. Historical stop-time viewer and test statements below describe the earlier state. The design verdict and preserved mission prefixes remain unchanged.

**Verdict: FAIL - StructuralSafety.** Both selected-design simulations are stopped at the user request. No full mission integration has been restarted. The newly authorised fast tests and replay are documented in the presentation audit. No threshold, strength or existing tolerance was changed.

| h (s) | Last preserved complete CSV sample (yr) | a_EMB (AU) | e_EMB | Cumulative minimum SF |
|---:|---:|---:|---:|---:|
| 3600 | 5460.78040155 | 1.76381410035 | 0.0103894265571 | 0.13896574092 |
| 1800 | 2361.14995437 | 1.25155318802 | 0.0129081699012 | 0.138952643637 |

No shading, plume, tide or flux flags were observed in the first 5460.780402 yr (h=3600) and 2361.149954 yr (h=1800) of preserved selected-design data. The cumulative flag count remains four. The names are StructuralSafety, FocusingOpticsUnresolved, MaterialStrengthUnvalidated and ThermalStructuralModelIncomplete, all present in the initial report; no new flag was added in either recorded prefix. The flags are monotonic and never cleared.

The selected design uses a co-moving array at Earth-radius ratio 1, inclination 10 degrees, phase 180 degrees; tractor separation125,000 km and Sunward bias 5 degrees; proposed post-target departure. Earlier original-layout results must not be attributed to it. The original fixed 1 AU array, phase 0, inclination 10, tractor d50,000 km/bias 0, parking-policy run did complete10,000 yr and reach2.7 AU at 8742.175906913073 yr.

| Selected-design event | Evidence status |
|---|---|
| Arrival at 2.7 AU | NOT OBSERVED |
| Post-target thrust period or policy comparison | NOT OBSERVED |
| Late-time array/mirror geometry | NOT OBSERVED |
| Full-duration h/h2 convergence | NOT OBSERVED |

## Partial-duration convergence

**partial-duration convergence (first 2361.149954369 years)**. Coarse-minus-fine signed differences are used below. Fine recorded timestamps are the matching-time grid. Coarse scalar records are linearly aligned onto that grid; native monthly grids differ by1800 s. These are interpolation-based differences, not extra integrations. Exact shared epochs are reported separately. The isolated1800 s startup sample is excluded because no full coarse step existed then. Ledger maxima are cumulative sampled quantities; interpolation is diagnostic only.

| Quantity | Maximum absolute difference | Final signed difference | Maximum alignment correction from nearest coarse record |
|---|---:|---:|---:|
| a_EMB_AU | 0.000150723885071 | 3.84817543568E-05 | 3.28953988493E-08 |
| eccentricity | 0.000114159662753 | 1.08746306466E-05 | 2.42758731624E-08 |
| Earth_Moon_distance_m | 2686256.06632 | -138834.389107 | 45720.0839905 |
| thrust_N | 3.16316351921E+14 | -1.05733521843E+14 | 2.35888800598E+12 |
| ejected_kg | 2.34547569763E+19 | -2.34547569763E+19 | 1.70827521406E+15 |
| temperature_K | 0.000281309785294 | -1.04766763798E-06 | 5.03264294593E-05 |
| relative_P | 1.55344149996E-13 | 1.72264393282E-14 | 3.33808332724E-18 |
| relative_L | 3.28347919664E-13 | -1.31346616118E-13 | 4.60858242434E-18 |
| relative_E | 5.65206017211E-13 | -4.95602115191E-13 | 5.29887103174E-17 |

The alignment correction is reported to expose sampling sensitivity; it is not a rigorous interpolation-error bound. No additional physics integration was used. The final aligned time is2361.149954369153 yr. Scalar Moon separation is aligned, rather than claiming unrecorded lunar positions. Cumulative ledger quantities are compared diagnostically on this same grid. The untouched raw grids and exact shared checkpoints remain available.

| Exact shared year | delta a_EMB (AU) | delta e | delta Moon distance (m) | delta thrust (N) | delta ejecta (kg) | delta T (K) |
|---:|---:|---:|---:|---:|---:|---:|
| 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| 1 | -2.982519276E-08 | 2.51436309273E-10 | 18.6281517744 | -8.56094905564E+13 | -9.92734839875E+15 | -2.92804075457E-05 |
| 10 | -2.98193099546E-07 | 3.22868575972E-09 | -373.432386696 | -8.47892078751E+13 | -9.93224044828E+16 | 1.29883907789E-05 |
| 100 | -3.04837951925E-06 | 3.30075633249E-08 | -194.280040324 | -8.68487305758E+13 | -9.93272162678E+17 | 0.000194226599547 |
| 1000 | -2.54656995224E-05 | -8.4356006782E-06 | 662701.257094 | -1.5749769162E+14 | -9.93348464537E+18 | -8.23674201911E-06 |

Full aligned records: Results/partial_convergence_series.csv. Metrics and exact shared-epoch arrays: Results/partial_convergence.json. The h1800 reconstruction checkpoint is Results/prefix_checkpoint_h1800.json; it retains the exact final complete CSV row and header. It does not contain controller history, compensated sums or full ledger transfer histories, so it is not a byte-exact process restart.

## Ledger, Moon, tide and flux prefix evidence

| h (s) | max relative P | max relative L | max relative E | sampled Moon min/max (km) | final Moon distance (km) |
|---:|---:|---:|---:|---|---:|
| 3600 | 6.77285893353E-13 | 9.94463878171E-13 | 6.7314789735E-13 | 374491.272765 / 399757.824842 | 379920.316112 |
| 1800 | 4.04859991134E-13 | 4.89480569122E-13 | 1.16875001254E-12 | 374502.453343 / 399761.572411 | 397166.987098 |

Ledger entries above are cumulative maxima stored by the solver, not inferred from the sampled trajectory. All are below the unchanged1e-9 threshold through the preserved endpoints. MoonBound is cumulatively true in both prefixes; no Moon-distance flag was added. The sampled lunar range is not an all-step extremum. Its unchanged placeholder interval is300,000..500,000 km.

| h (s) | sampled max tide ratio | unchanged tide threshold | sampled max incoming flux (W/m2) | unchanged flux threshold | sampled temperature min/max (K) |
|---:|---:|---:|---:|---:|---|
| 3600 | 0.0044963586023 |0.01| 1407.9297394 |1429.05| 288 / 290.887158935 |
| 1800 | 0.00449648505757 |0.01| 1407.92859389 |1429.05| 288 / 290.887107185 |

Exact all-step extrema for these interrupted prefixes were not written to terminal JSON. The earlier completed ten-year candidate probe recorded tide max0.004497568177, incoming-flux max1408.030292421 W/m2 and Earth/Moon plume clearances33,714,277.25 /205,206,830.44 m. Those clearances describe that ten-year probe only. The long prefixes support the narrower statement that no corresponding threshold flag occurred. Tide, flux, temperature, lunar-distance, plume and SF thresholds are explicitly placeholders; their values were retained.

## Structure failure

| h (s) prefix | SF | governing bulk-rock proxy stress (MPa) | fixed allowable (MPa) | strength required for SF1.5 (MPa) | multiplier required |
|---:|---:|---:|---:|---:|---:|
| 3600 | 0.13896574092 | 35.98009097 |5| 53.970136455 | 10.794027291 |
| 1800 | 0.138952643637 | 35.9834823515 |5| 53.9752235273 | 10.7950447055 |

The required strength is calculated and has NOT been adopted. The inferred stress uses the recorded cumulative SF and the fixed 5 MPa rock allowable. The completed ten-year component audit identified rock stress35.978756 MPa as governing, with array/mirror local bending stresses32.007669 MPa and3.612737 MPa against their unchanged200 MPa allowable. The existing bulk-rock scalar proxy does not resolve a full stress tensor or validate a thruster mount. The separate contact-area/mount studies report tension, compression, shear and bending and all fail at the retained rock strength. At least53.975224 MPa (about10.795x) would be required by the worst recorded selected-prefix proxy for SF1.5; this is a failed requirement, not a proposed strength change.

## AssumptionRegister

| ID | Assumption | Status and consequence |
|---|---|---|
|A1|Giant focusing optical capability|Unresolved pending organiser confirmation. E1/E2 acceptance is not inferred.|
|A2|Material properties represent actual components|Unvalidated illustrative inputs: rock5 MPa, local frame200 MPa, Young modulus50 GPa. Kept unchanged; structure fails.|
|A3|Local proxies adequately represent global thermal/structural/optical behaviour|Not validated. Coupled thermal deformation, global stability and optical pointing structure remain outside the demonstrated local checks.|
|A4|E2 ideal power transmission and conversion|Assumed efficiency 1; no beam propagation/loss validation. If rejected: collector, heat rejection and station-keeping budgets must be recomputed; current solar accounting cannot demonstrate feasibility.|
|A5|Ballistic ejecta remain clear|Only launch energy/direction and instantaneous cones checked. If rejected: propagate parcels with the bodies; future collision/return clearance remains unproven.|
|A6|Simple global EBM predicts habitability|No spatial climate, clouds or carbon cycle. If rejected: temperature/ice proxies cannot establish habitability; use a validated climate model. No-mirror replay is conditional on this EBM.|
|A7|Prescribed tractor/infrastructure paths are attainable|Finite impulses and supplies are booked under E2; no autonomous control/assembly validation. If rejected: guidance, stability, deployment and thrust/power budgets need a coupled redesign.|
|A8|Placeholder thresholds define acceptable conditions|Unvalidated criteria retained. If rejected: mission acceptability is undetermined; obtain physical requirements and re-evaluate without retroactively calling existing data safe.|

E1 supplied material, E2 ideal conversion/transmission/station-keeping and E3 initially placed tractor remain declared enablers. A1-A3 are unresolved assumptions, not additional sources of force, power or unlimited strength.

## Benchmarks and gates

| Evidence | Status | Measured result / unchanged criterion |
|---|---|---|
|G1|PASS|17 checks; h60/h30 maximum separation errors0.000475132/0.000238364 against 0.001; measured acceleration errors5.79363e-6/7.58422e-6 against 0.001; ledgers below1e-9.|
|G2|PASS|15 checks; 10 yr delivered-dv relative difference0.004605148765 against 0.01; both Moon-bound checks pass.|
|G3 h3600|PASS|9 checks; target8758.151380333 yr, time error0.007187409 against 0.05; largest sampled a error0.009116630 against 0.02.|
|G3 h1800|PASS|9 checks; target8710.794889345 yr, time error0.001741412 against 0.05; largest sampled a error0.002232396 against 0.02.|
|G4 selected design|INCOMPLETE|User stopped both full runs. Arrival, post-target period and full-duration convergence NOT OBSERVED. Only the labelled partial-duration comparison exists.|
|G8 selected-design viewer|PARTIAL PLAYBACK VERIFIED|OctoEarth_Main loaded 66,491 selected-prefix samples; source label states partial run: first 5460.780402 yr. Zero Console errors and missing scripts. Moon is NOT visible. Full mission demonstration incomplete.|

The three new climate/steering/radius tests were reported succeeded by Unity MCP before the stop request (three completed, no failures; detailed payload unavailable after reload). The previous32-test suite predates this design review. No final35-test rerun was started after the user stopped jobs. Older numbered benchmark results below are cached evidence, not newly executed tests of the final viewer edits.

| Cached benchmark/check | Measured | Expected | Tolerance | Recorded result |
|---|---:|---:|---:|---|
| B1 | 0 | 0 | 1E-08 | PASS |
| B2 | 0 | 0 | 5E-08 | PASS |
| B3 | 0.0002 | 0.0002 | 2E-10 | PASS |
| B4.position | 2.50607341304E-11 | 0 | 1E-06 | PASS |
| B4.energy | 3.28685337166E-13 | 0 | 1E-09 | PASS |
| B5 | 9.56588407854E-14 | 0 | 1E-09 | PASS |
| B5.P | 8.95185280011E-15 | 0 | 1E-11 | PASS |
| B5.L | 3.2442493699E-14 | 0 | 1E-11 | PASS |
| B6 | 365.260245059 | 365.256 | 0.1 | PASS |
| B7.dv1 | 2.94605516251 | 2.94 | 0.02 | PASS |
| B7.dv2 | 2.64998208045 | 2.65 | 0.02 | PASS |
| B7.time | 258.915150234 | 259 | 2 | PASS |
| B7.simulatedRadius | 5.99520433298E-15 | 0 | 1E-08 | PASS |
| B7.simulatedBurn | 2.64998208045 | 2.65 | 0.02 | PASS |
| B7.circular | 1.58306765949E-14 | 0 | 1E-08 | PASS |
| B8 | 3170.1014834 | 3170 | 31.7 | PASS |
| B8.Gauss | 3.40860348027E-10 | 0 | 0.01 | PASS |
| B9.moment | 5000 | 5000 | 50 | PASS |
| B9.deflection | 0.00625 | 0.00625 | 6.25E-05 | PASS |
| B10 | 254.038620581 | 254 | 2 | PASS |
| X1 | 5.29033546556E-14 | 0 | 1E-09 | PASS |
| X2 | 19.683 | 19.683 | 1E-12 | PASS |
| X5.rocket | 0.100050033358 | 0.100050033358 | 1.00050033358E-10 | PASS |
| X5.momentum | 1.33582034323E-12 | 0 | 1E-09 | PASS |
| X5.energy | 1.68954309644E-13 | 0 | 1E-09 | PASS |
| X5.reaction | 1.33648881049E-13 | 0 | 1E-09 | PASS |
| X5.sourceP | 8.8774505621E-17 | 0 | 1E-09 | PASS |
| X5.sourceE | 0 | 0 | 1E-09 | PASS |
| X5.sourceL | 3.78956125739E-16 | 0 | 1E-09 | PASS |
| X5.force | 0 | 0 | 1E-09 | PASS |
| X6.Yoshida4.h100 | 0.000408637413035 | 0 | 1 | info |
| X6.Yoshida4.h200 | 2.56202659584E-05 | 0 | 1 | info |
| X6.Yoshida4.difference200 | 0.000383017148081 | 0 | 1 | info |
| X6.Yoshida4.h400 | 1.60244235436E-06 | 0 | 1 | info |
| X6.Yoshida4.difference400 | 2.40178236042E-05 | 0 | 1 | info |
| X6.Yoshida4.h800 | 1.00170596489E-07 | 0 | 1 | info |
| X6.Yoshida4.difference800 | 1.50227175788E-06 | 0 | 1 | info |
| X6.Yoshida4 | 3.99813805625 | 4 | 0.2 | PASS |
| X6.VelocityVerlet.h100 | 0.0326241175225 | 0 | 1 | info |
| X6.VelocityVerlet.h200 | 0.00811883051861 | 0 | 1 | info |
| X6.VelocityVerlet.difference200 | 0.0245064642411 | 0 | 1 | info |
| X6.VelocityVerlet.h400 | 0.00202736816928 | 0 | 1 | info |
| X6.VelocityVerlet.difference400 | 0.00609148060735 | 0 | 1 | info |
| X6.VelocityVerlet.h800 | 0.000506695608637 | 0 | 1 | info |
| X6.VelocityVerlet.difference800 | 0.0015206728454 | 0 | 1 | info |
| X6.VelocityVerlet | 2.00276910484 | 2 | 0.12 | PASS |
| X7 | 0.000391195036671 | 0 | 0.05 | PASS |
| X7.reference | 0 | 0 | 1E-20 | PASS |
| X8.float | 0 | 0 | 0 | PASS |
| X8.double | 37.3235408112 | 37.323572918 | 0.05 | PASS |
| Budget.analytic.nominal.E | 1.68825361675E+33 | 1.68825361675E+33 | 1.68825361675E+24 | PASS |
| Budget.analytic.nominal.dv | 11658.3056266 | 11658.3056266 | 1.16583056266E-05 | PASS |
| Budget.analytic.nominal.p | 7.0474457513E+28 | 7.0474457513E+28 | 7.0474457513E+19 | PASS |
| Budget.analytic.nominal.F | 2.23320079832E+17 | 2.23320079832E+17 | 223320079.832 | PASS |
| Budget.analytic.nominal.P | 5.349752886E+21 | 5.349752886E+21 | 5.349752886E+12 | PASS |
| Budget.analytic.nominal.L | 1.73236354326E+40 | 1.73236354326E+40 | 1.73236354326E+31 | PASS |
| Budget.analytic.nominal.solarDays | 51.0448600212 | 51.0448600212 | 5.10448600212E-08 | PASS |
| Budget.analytic.alternate.E | 1.35831241069E+33 | 1.35831241069E+33 | 1.35831241069E+24 | PASS |
| Budget.analytic.alternate.dv | 11692.2112715 | 11692.2112715 | 1.16922112715E-05 | PASS |
| Budget.analytic.alternate.p | 4.94755919955E+28 | 4.94755919955E+28 | 4.94755919955E+19 | PASS |
| Budget.analytic.alternate.F | 6.81646479915E+17 | 6.81646479915E+17 | 681646479.915 | PASS |
| Budget.analytic.alternate.P | 1.8714053456E+22 | 1.8714053456E+22 | 1.8714053456E+13 | PASS |
| Budget.analytic.alternate.L | 9.1251120383E+39 | 9.1251120383E+39 | 9.1251120383E+30 | PASS |
| Budget.analytic.alternate.solarDays | 41.0689876099 | 41.0689876099 | 4.10689876099E-08 | PASS |
| Budget.analytic.inward.E | -1.68825361675E+33 | -1.68825361675E+33 | 1.68825361675E+24 | PASS |
| Budget.analytic.inward.dv | -11658.3056266 | -11658.3056266 | 1.16583056266E-05 | PASS |
| Budget.analytic.inward.p | -7.0474457513E+28 | -7.0474457513E+28 | 7.0474457513E+19 | PASS |
| Budget.analytic.inward.F | -4.46640159663E+17 | -4.46640159663E+17 | 446640159.663 | PASS |
| Budget.analytic.inward.P | -1.0699505772E+22 | -1.0699505772E+22 | 1.0699505772E+13 | PASS |
| Budget.analytic.inward.L | -1.73236354326E+40 | -1.73236354326E+40 | 1.73236354326E+31 | PASS |
| Budget.analytic.inward.solarDays | -51.0448600212 | -51.0448600212 | 5.10448600212E-08 | PASS |
| X10 | 1 | 1 | 0 | PASS |
| X11 | 1 | 1 | 0 | PASS |
| X11.forceReset | 1 | 1 | 0 | PASS |
| X.kepler | 3.05996114606E-16 | 0 | 1E-12 | PASS |
| X.clock | 1 | 1 | 0 | PASS |
| X.softening | 3.39309534759E-15 | 0 | 1E-09 | PASS |
| X.photon | 0 | 0 | 1E-15 | PASS |
| Budget.guide.E | 1.68825361675E+33 | 1.69E+33 | 3.38E+31 | PASS |
| Budget.guide.v1 | 29.7846918317 | 29.78 | 0.5956 | PASS |
| Budget.guide.v2 | 18.1263862051 | 18.12 | 0.3624 | PASS |
| Budget.guide.dv | 11.6583056266 | 11.66 | 0.2332 | PASS |
| Budget.guide.p | 7.0474457513E+28 | 7.05E+28 | 1.41E+27 | PASS |
| Budget.guide.F | 2.23320079832E+17 | 2.2E+17 | 4.4E+15 | PASS |
| Budget.guide.acceleration | 3.69429412459E-08 | 3.7E-08 | 7.4E-10 | PASS |
| Budget.guide.P | 5.349752886E+21 | 5.4E+21 | 1.08E+20 | PASS |
| Budget.guide.L | 1.73236354326E+40 | 1.73E+40 | 3.46E+38 | PASS |

## Completed design-study evidence

The 96-point siting screen is saved in Results/design_audit/siting/array_siting_sweep.csv/json. It reuses native monthly original-layout samples and approximate feedforward forces; it is not 96 full integrations. Its36 zero-sampled-intersection candidates do not establish late-time selected-design safety. The selected prefixes are the only integrated evidence for the selected layout.

The exact hourly original-layout thermal replay reproduced the legacy temperature history with zero reproduction error. Peak temperature changed from 291.811764818 K to 292.462340539 K after correcting IR absorption and to 292.644311224 K after also correcting finite-disc quadrature. A sustained-full-shading regression without mirror compensation cools from 288 K to 271.670612927 K after1 yr and196.732677910 K after10 yr. Array waste heat remains included. These are separate controlled tests and replays, not selected-design full-duration climate validation.

The first 10 yr full13-body candidate order study used7200/3600/1800/900/450 s. Log-log orders against the finite450 s reference: position2.13386035, a2.13387177, T2.06877756. Successive Richardson a orders2.01195979,2.00296762,2.00079178. This supports observed second-order behaviour over that interval, not fourth order or a full-duration order fit.

## Limitations

- Interrupted runs have no terminal mission report, final-state JSON or complete controller/ledger restart. Last internal unsaved steps are unknown; only complete preserved CSV rows are reported.
- No arrival, post-target operation, late-time geometry or full-duration convergence was observed for the selected design. Proposed departure has not been exercised. No propellant-saving or coast-versus-parking result is claimed.
- Native output grids differ by1800 s. Aligned comparison uses explicitly identified scalar interpolation; exact shared epochs are separate. Monthly sampling cannot recover all-step extrema, and the interpolation correction is not an error bound.
- The physical model uses approximate J2000 elements and an idealized lunar initial state, not a validated long-term ephemeris. Prescribed infrastructure is realized through booked finite impulses under E2.
- Rock and local-beam calculations are engineering proxies. Fixed strengths do not establish global, shear-interface, thermal or optical structural adequacy. The rock proxy fails.
- Giant focusing optics are unresolved, the climate model is a simplified global EBM, and exported ejecta are not propagated ballistically to prove future clearance.
- The Moon is included in numerical dynamics and monitors. Visibility in the mission viewer is a separate UI capability and will be stated explicitly.
- After the original stop, only the newly authorised fast EditMode tests and conditional thermal replay were run; no full mission was restarted.

## Viewer and build checks (historical stop-time state)

Unity MCP verified Assets/Scenes/OctoEarth_Main.unity loading Results/design_audit/full_h3600/run_mission.csv with the visible label "partial run: first 5460.780402 yr". Playback and baseline physics clocks were both stopped. There were zero Console errors, zero missing scripts and no recording error. The Moon is NOT visible in the mission viewer. No simulation or test job was started for this read-only playback check. The running headless executable used the direct Roslyn -optimize+ flag, without a debug compiler flag; this is an optimised build rather than a dotnet build -c Release invocation. Unity MCP reported editor idle, not playing, not compiling, and no test job running. WMI command-line inspection was unavailable (access denied); native process inspection confirmed the simulation processes exited.

## Changed files and evidence

Design physics/analysis changes: Assets/Scripts/Core/MissionEnvironment.cs, MissionRadiation.cs, MissionSimulation.cs, MissionTargets.cs, MissionConstraintStepper.cs, GravityTractor.cs, MissionMonitors.cs, MissionConfigurationIO.cs; new Core modules FiniteDiscRadiation.cs, DesignClimateChecks.cs, DesignStudyAudit.cs, ArraySitingSweep.cs, PlumeSteering.cs, MissionFailureClassification.cs, MissionConvergenceStudy.cs, ThermalCouplingReplay.cs. Also CoreTests.cs, Tools/OctoEarth.Cli/Program.cs, Config/mission_design_candidate.json, Tools/Analyze-MissionOrder.ps1 and Docs/Design_Review_Progress.md.

Stop/report changes: Tools/Compare-StoppedPrefixes.ps1, Tools/Write-StoppedReport.ps1, Results/STATUS.md, Results/stopped_prefix_manifest.json, Results/partial_convergence.json, Results/partial_convergence_series.csv, Results/prefix_checkpoint_h1800.json and this report. Viewer-only changes: Assets/Scripts/Runtime/TractorInspection.cs and Assets/Scenes/OctoEarth_Main.unity. The source is explicitly marked partial, opens paused at the recorded endpoint, uses the correct tractor mass/distance scale, and has separate fixed-height metric rows to prevent thrust text reflow. Verification: Results/stopped_viewer_verification.json. SHA256 verification confirms the stopped raw prefix files and frozen inputs remain unchanged. New Unity scripts have corresponding .meta files. The raw selected-run CSV files and existing JSON files were not edited.
