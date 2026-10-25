# Selected-design status: stopped prefixes

**Current presentation work:** The newly authorised fast EditMode suite ran once: 35 passed, 0 failed, 1 explicit long test skipped (13.8747327 s). No new mission integration was run. Moon vectors already exist in the prefixes; the viewer now starts paused at zero and displays the recorded Moon. A conditional no-mirror thermal replay exits the unchanged temperature placeholder range near year 606. See [presentation audit](../Docs/Playback_Presentation_Audit.md), [test evidence](presentation_editmode_tests.json) and [Moon/replay evidence](moon_report.json). Earlier stopped-viewer statements below are historical.

Both full-duration simulations were stopped at the user's request. No full mission integration has been restarted. Newly authorised fast tests and replay are documented above. Existing prefix CSV/JSON files are preserved unchanged.

| Selected-design run | Last preserved complete CSV sample (Julian yr) | a_EMB (AU) | Recorded flags | Cumulative minimum SF |
|---|---:|---:|---:|---:|
| h=3600 s prefix | 5460.780401551448 | 1.763814100347 | 4 | 0.138965740920 |
| h=1800 s prefix | 2361.149954369153 | 1.251553188020 | 4 | 0.138952643637 |

These are recorded-prefix endpoints, not 10,000-year completions. The last internal step after the last flushed sample is not recoverable from the interrupted processes.

No shading, plume, tide or flux flags were observed in the first **5460.780402 yr (h=3600)** and **2361.149954 yr (h=1800)** of preserved selected-design data. Four flags remain in each prefix: StructuralSafety, FocusingOpticsUnresolved, MaterialStrengthUnvalidated and ThermalStructuralModelIncomplete. Flag count is cumulative and monotonic; the initial ten-year report names these same four flags.

**Selected-design verdict: FAIL.** StructuralSafety: SF approximately **0.139**, required **1.5**. The assumed rock allowable stress remains **5 MPa**. The three assumptions remain unresolved: focusing optics acceptance, material-strength validation, and coupled thermal/global structural validation.

| Selected-design event or claim | Status |
|---|---|
| Arrival at 2.7 AU | NOT OBSERVED |
| Post-target thrust period | NOT OBSERVED |
| Late-time array/mirror geometry | NOT OBSERVED |
| Full-duration convergence | NOT OBSERVED |

The earlier **original-layout** run completed 10,000 years and reached a_EMB=2.7 AU at **8742.175906913073 yr**. That was the fixed 1 AU array at 10 degrees inclination, phase 0; d=50,000 km tractor with zero Sunward bias; original parking policy. It is not the selected design, which uses a co-moving opposite-phase array, d=125,000 km, Sunward bias 5 degrees and a proposed departure policy. The selected departure policy has not been reached or tested by these prefixes.

Prefix cumulative ledger maxima at the last recorded sample:

| h (s) | Relative P | Relative L | Relative E |
|---:|---:|---:|---:|
| 3600 | 6.772858933534e-13 | 9.944638781712e-13 | 6.731478973504e-13 |
| 1800 | 4.048599911339e-13 | 4.894805691220e-13 | 1.168750012541e-12 |

All remain below the unchanged 1e-9 ledger threshold over their recorded prefixes. The partial-duration comparison and report are complete: Docs/Selected_Design_Prefix_Report.md and Results/partial_convergence.json. G1-G3 reruns passed; G4 is incomplete because the selected full-duration runs were stopped. G8 partial playback was checked via Unity MCP: source label partial run: first 5460.780402 yr; zero Console errors, zero missing scripts, playback/physics clocks stopped. The Moon is NOT visible in the mission viewer. No new simulation or test job was started. Raw prefix and frozen-input SHA256 checks passed.
