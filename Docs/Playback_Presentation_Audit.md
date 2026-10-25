# Recorded-prefix presentation audit

The selected design remains **FAIL - StructuralSafety** at the original 5 MPa scalar allowable. No physics, strengths, thresholds or tolerances were changed. The only new numerical work is the fast EditMode suite and a thermal-only replay of existing CSV forcing; no new mission integration was started. The earlier stop report describes the previous viewer and test state.

## Numerical interpretation (A2-A4)

The fast EditMode suite ran once: 35 passed, 0 failed, 1 explicit 10,000-year test skipped, 13.8747327 s. Job 572d610dba9d45979db68f4d3e87d361; full payload: `Results/presentation_editmode_tests.json`. Viewer assertions cover paused zero-time entry, no advance while paused, clamping, restart, seeking without changing the initial data frame, Moon object activation/position and toggle. These are assertions within one viewer regression, not eight separate test cases.

X6 endpoint and successive-difference rows are **info**: their 1 AU envelopes are diagnostic, not evidence of precision. Only the fitted order rows are convergence acceptance tests. G2.aDifference and G2.eDifference are also **info**, with existing 1 AU and 1 dimensionless envelopes; delivered-dv and Moon-bound checks remain the actual G2 acceptance evidence. No tolerances were tightened. Raw cached benchmark JSON and the test payload retain their historical boolean fields; this report reclassifies their evidentiary meaning.

G3's target times differ by 47.356490988 yr, 0.54365% of the fine time. These are a separate Sun/Earth/tractor toy spiral (`SpiralGate`, `GravityTractor.Toy`, `PrescribedTractorStepper`), not the selected 13-body mission. Its errors against the slow circular analytic estimate are 0.007187409 and 0.001741412, ratio 4.1272. This is consistent with a leading second-order timestep error in the constrained splitting, but two steps do not isolate it from the analytic model approximation. Yoshida's fourth-order free-gravity accuracy does not imply fourth-order accuracy for prescribed impulses. The selected mission's measured ten-year order study is approximately second order as well. There is no contradiction between those late toy target-time differences and smaller selected-prefix scalar differences: they differ in system, controller, duration and observable. The selected prefixes do not reach the target, so their target-time convergence is unknown.

The shared final ejecta difference is -2.34547569763e19 kg (coarse minus fine), about -0.03316% of the fine cumulative total. Exact shared epochs show -9.92735e15 kg at year 1, -9.93272e17 kg at year 100, and -9.93348e18 kg at year 1000. This cannot be dismissed as a thrust-sampling artifact: `MissionConstraintStepper.Ejected` sums actual ejection bookings from both half-step kicks and all three controlled bodies; `thrust_N` reports the tractor's final kick force only. A monthly integral of that scalar would not recover cumulative ejecta. The nearly constant rate difference is a real timestep-dependent bookkeeping/control result in these integrations. Its precise numerical mechanism is not isolated without further controlled runs, which were not performed. The maximum interpolation correction, 1.70828e15 kg, is far smaller than the final difference.

| Prefix h (s) | Tractor mass (kg) | Radius (km) | Density (kg/m3) | E1 mass used (kg) | E1 cap used | Jet kinetic energy (J) |
|---:|---:|---:|---:|---:|---:|---:|
| 3600 | 1.0098073172297392e19 | 92.9700111632 | 3000 | 1.6352623536368827e23 | 54.50874512% | 7.358523518883205e33 |
| 1800 | 1.0098073172297392e19 | 92.9700111632 | 3000 | 7.0729921504789316e22 | 23.57664050% | 3.1827669934659376e33 |

Mass is the recorded tractor mass (constant in each prefix); radius uses its spherical density assumption. E1 counts initial infrastructure, growth and supplied propellant; the initially placed tractor is E3. The mass cap is the retained 3e23 kg. Jet energy is cumulative total ejecta times exhaust speed squared / 2, for 300,000 m/s exhaust. Under the existing E2 bookkeeping, the array supplies 100% of the booked required jet energy at efficiency 1: every ejection is checked against available array electrical energy in `MissionSimulation.CheckDraw`. This is a model accounting identity, not an independent transmission demonstration. The interrupted CSV lacks cumulative array generation/unused-energy totals, so the fraction of all generated array energy actually used cannot be measured from it.

## No-mirror replay and optical sensitivity (A5)

`Results/no_mirror_replay_h3600.csv` and `Results/no_mirror_replay_h1800.csv` contain T(t) and ice fraction from the unchanged `EnergyBalanceClimate`, with reflected mirror illumination removed. Recorded direct light and array IR are held across each output interval; daily RK4 substeps use the existing model. Trajectory, array deployment, waste heat and mirror station-keeping expenditure are not recomputed. This is a conditional thermal replay, not an integrated mission with A1 rejected. Monthly forcing limits the accuracy of crossing times; daily crossing resolution is not a rigorous error bound.

| h (s) | First T outside 278..298 K (yr) | Sampled minimum T (K) | Maximum T (K) | Final T (K) | Final ice fraction |
|---:|---:|---:|---:|---:|---:|
| 3600 | 605.982318047 | 179.123864203 | 289.622180976 | 179.132125416 | 1 |
| 1800 | 605.976785307 | 212.552605262 | 289.622123143 | 212.591973283 | 1 |

All placeholder limits remain unchanged. `Results/mirror_optics_sensitivity.csv` sweeps d_m = 1e8, 5e8, 1e9, 2e9, 5e9 m and delta = 0, 0.001, 0.003, 0.01, 0.02 rad. The finite solar angular diameter is theta_sun = 0.00930096804723 rad at 1 AU. Full interception requires delta <= sqrt((2 R_E / d_m)^2 - theta_sun^2). At d_m = 1e9 m, delta_max = 0.00870922254764 rad; at 5e8 m it is 0.0237260668756 rad. At 2e9 and 5e9 m the radicand is negative: even delta = 0 cannot fit the whole solar image on Earth at this source distance. NaN means no real full-interception bound, not a safe value. This is an optical geometry sensitivity at 1 AU, not a validation of a giant focusing device or its late-time pointing.

## Strength modes: sensitivity, not adopted (A6)

The code does not identify the 5 MPa input as a measured tensile, compressive or shear allowable. It compares a positive bulk scalar proxy, rho*(2 R a + G M/R), to `RockAllowableStress`. The self-gravity term suggests a compressive scale, but the summed scalar lacks stress signs, tensors, confinement and interfaces. Calling it a validated compressive allowable would overstate the model. It is an unvalidated generic bulk-rock assumption; failure remains at 5 MPa and SF = 0.13895.

The following are mode-resolved literature sensitivities. The last column is strength / 35.9834823515 MPa solely to expose scale; it is **not** a mode-specific computed SF. No listed strength has been adopted and no mount is validated.

| Mode | Example literature parameter | MPa | Scalar comparison only | Limit on interpretation |
|---|---|---:|---:|---|
| Tensile | Yosemite granitic rocks, Brazilian test range [S1] | 3.5..9.4 | 0.0973..0.2612 | Small intact specimens; not a 93 km rock mass |
| Compression | Granite example, unconfined [S2] | 167.887 | 4.666 | Example 24,350 psi; defects/confinement/scale matter |
| Shear | Intact granite cohesion c [S3] | 39.49 | 1.0974 at zero normal compression | Mohr-Coulomb tau = c + sigma_n tan(56.95 degrees); sigma_n not resolved here |
| Bending tension | Tensile range used as an illustrative cracking envelope [S1] | 3.5..9.4 | 0.0973..0.2612 | Inference only; no flexural strength or section stress has been established |

[S1: USGS Data Series 1126](https://pubs.usgs.gov/ds/1126/ds1126.pdf) reports these Brazilian strengths. [S2: USACE EM 1110-2-3800, Table 2-1](https://www.publications.usace.army.mil/portals/76/publications/engineermanuals/em_1110-2-3800.pdf) gives example rock strengths and cautions that they need not represent the rock type generally. [S3: Liu et al., intact granite experiments](https://www.frontiersin.org/journals/earth-science/articles/10.3389/feart.2022.1057187/full) reports cohesion and friction angle. These sources do not establish properties of the proposed tractor.

## Moon evidence and manual location (B1-B5)

The existing object is `Orbital Views/Moon`, with child `Moon Label`. Prior to these changes its active flag was false, its renderer was enabled, layer was Default (0), and local scale was (0.8, 0.8, 0.8). `MoonInset.SetVisible` / `RenderFrame` hides it in solar view; `TractorInspection.Render` previously forced it inactive each frame. The principal failure was script-driven deactivation, not missing Moon data. The baseline Moon script uses a separate live N-body state; the mission viewer now disables that script and places the existing object from the recording instead. Camera culling and on-screen placement are checked during the new playback verification.

To find it manually: open `Assets/Scenes/OctoEarth_Main.unity`; expand **Orbital Views** in Hierarchy; select **Moon**; enable the checkbox beside its name in Inspector; expand **Mesh Renderer** and ensure its checkbox is enabled; confirm **Layer: Default**. Before the viewer fix, scripts would disable it again on entering Play, so the checkbox alone was insufficient. Now press Play, then **M** or **Moon view**. **T** toggles Moon and recorded trail, on by default.

The complete loaded CSV column list is the `columns` array in `Results/moon_report.json`. It includes Moon_x/y/z and Moon_vx/vy/vz, as well as Earth and Sun vectors. It does not contain a scalar Earth-Moon-distance column: distance is computed from those double vectors. No short Moon recording was needed. Double subtraction precedes display scaling and float conversion. Between samples Hermite interpolation is for display only; the HUD binding energy, Hill ratio and cumulative sampled min/max use the last actual sample, avoiding false binding claims from interpolated velocities. The current marker may lie on an underresolved interpolated path; it is not a recovered monthly lunar orbit. The trail connects recorded relative positions and is explicitly labelled monthly.

| h (s) | Sampled minimum / mean / maximum distance (km) | Final (km) | Worst sampled r/Hill | Maximum sampled specific energy (J/kg) |
|---:|---|---:|---:|---:|
| 3600 | 374491.272765 / 387985.439771 / 399757.824842 | 379920.316112 | 0.265833364013 | -502445.103017 |
| 1800 | 374502.453343 / 388654.933095 / 399761.572411 | 397166.987098 | 0.265832054189 | -502448.705172 |

Both prefixes retain cumulative MoonBound=true and no Moon flag was added. The mean is an arithmetic mean of samples, not a time integral; monthly output is not an all-step extremum. The Hill calculation uses recorded EMB semi-major axis and constant model masses, matching the monitor convention. The unchanged distance placeholders are 300,000..500,000 km. Full columns and statistics: `Results/moon_report.json`.

## Presenting the recording

1. Open `OctoEarth_Main`, press Unity Play. It opens at year 0, paused, in Sun/transfer view. Read the partial-run/source label aloud; this is not a full 10,000 yr result.
2. **Space** plays/pauses; **R** restarts at zero; keys **1..6** select 1, 1e3, 1e5, 1e7, 1e8, 1e9 times speed. Slider and +/-1-year buttons seek; **Jump to end** is explicit. The endpoint clamps without looping.
3. Choose **Earth + tractor** for the pull arrow, separation and schematic thrusters. Choose **M** for the Earth-centred Moon view. Keep Moon speed <=1e6x; monthly recording still underresolves the orbit. **T** hides/shows Moon and trail. Size exaggerations stay labelled.
4. **D** starts the scripted tour; **Esc** interrupts and pauses. The Moon segment lasts 15 s. The last transfer segment uses 1e9x.

There is a timing conflict in C3: 5460.780402 recorded years require 172.33 real seconds at 1e9x. A 70-second introduction plus that playback requires about 4 min 2 s. The implemented three-minute tour honours the requested 1e9x speed and stops after 180 s, so it cannot reach the endpoint in its 110-second final segment (about 3486 yr). Use Jump to end for the endpoint or continue normal playback. No silent speed increase or fabricated completion is used.

## Verification and item status

Final 1920x1080 Play-mode screenshots and measured checks are recorded in `Results/presentation_viewer_verification.json` and `Captures/PresentationVerified/`: t0, one_sim_second, one_wall_second, middle, earth_tractor, moon and endpoint. One real second at 1e9x gives exactly 1e9 recorded seconds (31.688087814 yr); one simulated second was checked separately. Restart restores exact zero time, Earth display position, zero thrust/ejecta/ledgers, four flags, solar view and paused clocks. Endpoint is exactly 172329123600 s and stops playing. The Moon's display position matches its double-vector source with zero display-unit difference; its toggle hides/restores it. Culling mask is -1 (all layers), Moon layer 0, zero missing scripts. Final clean verification had zero Console errors. Editor was returned to Stop.

Earlier direct Camera.Render calls during capture caused six render-pass/backbuffer dimension errors. They are preserved in the verification JSON; they are not counted as a clean pass. The Play session was restarted and final captures repeated using the Unity MCP camera screenshot tool, followed by a zero-error Console check. No viewer workaround or physics change was needed.

After HUD edits, the focused viewer EditMode regression also ran: 1 passed, 0 failed, 1.2129639 s (`Results/presentation_viewer_regression.json`). The last Moon framing adjustment was checked live through MCP, not by another complete suite rerun. Demo stages were checked at adjusted clock boundaries 0, 21, 51, 66, 71 and 181 s; a continuous three-minute rehearsal and physical keyboard presses were not tested. The full mission, coupled no-mirror dynamics, real material properties, ballistic ejecta clearance and long-duration convergence remain untested.

| Item | Final status | Evidence / qualification |
|---|---|---|
| A1 | done | UTF-8 report/status, unit spacing and diagnostic row classifications corrected |
| A2 | done | Fast suite once: 35 passed, 0 failed, 1 explicit skipped; X6/G2 diagnostic rows labelled info in reporting |
| A3 | partly done | Distinct G3 and selected-prefix models explained; ejecta is not a sampling artifact; exact numerical rate-offset cause not isolated |
| A4 | done | Recorded mass, radius, density, E1 fraction and conditional energy accounting above |
| A5 | done | Two conditional no-mirror replay series, ice fraction, crossing estimates and optical sweep |
| A6 | done | Generic bulk scalar clarified; sourced mode sensitivities, none adopted; FAIL retained |
| A7 | done | A4-A8 added to main report's AssumptionRegister with rejection consequences |
| B1 | done | Orbital Views/Moon and both hiding scripts identified; manual clicks above |
| B2 | done | Full column list saved; existing Moon position/velocity vectors used; no new recording needed |
| B3 | done | True-position marker/leader, pair-view off-screen direction, Moon view, recorded trail, speed warning, default-on toggle; monthly aliasing disclosed |
| B4 | done | Sample distance, Earth-relative energy, monitor-convention Hill ratio, binding status, sampled extrema and placeholder limits |
| B5 | done | Both prefixes in moon_report.json; monthly sampling limitations explicit |
| C1 | done | Clean Play entry at zero, paused, transfer view and recorded HUD |
| C2 | done | Buttons/keyboard handlers, presets, slider, year steps, elapsed clock, endpoint clamp and no loop |
| C3 | partly done | Tour/eased camera stages implemented and boundary checked; 3 min plus specified speed cannot reach full prefix endpoint; continuous rehearsal not run |
| C4 | done | Visually reviewed at 1080p: fixed metric rows, separate controls, source status bar and scale legends |
| D1 | done | All requested captures, zero-error final clean session, zero missing scripts, stopped clocks and restart checks; earlier capture errors disclosed |
| D2 | done | Added assertions in existing EditMode viewer regression; suite and focused rerun results retained |

Changed source/report files: `Assets/Scripts/Core/MissionRecording.cs`, new `PrefixPresentationAudit.cs` plus `.meta`, `Assets/Scripts/Runtime/TractorInspection.cs`, `OrbitCamera.cs`, `Assets/Scenes/OctoEarth_Main.unity`, `Assets/Tests/EditMode/MissionViewerTests.cs`, `Tools/OctoEarth.Cli/Program.cs`, new `Tools/Polish-PrefixReports.ps1`, `Docs/Selected_Design_Prefix_Report.md`, `Results/STATUS.md` and this audit. Added evidence: `Results/moon_report.json`, two `no_mirror_replay_h*.csv` files, `mirror_optics_sensitivity.csv`, test/verification/hash JSON files named above, and screenshots in `Captures/PresentationVerified/`. The local analysis executable is under `Tools/HeadlessPresentation/`. Both raw prefix SHA256 values and all three frozen-input SHA256 values still match their manifests.
