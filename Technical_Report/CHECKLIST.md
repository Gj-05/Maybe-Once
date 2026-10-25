# Technical report checklist

Prepared 25 October 2026. This checklist distinguishes content coverage from demonstrated mission success. No numerical mission was rerun for this revision.

## Report requirements from the participant guide
- [x] Explain the permitted alternative concept and physical reaction mechanism.
- [x] State the 1 AU to 2.7 AU objective and 10,000-year limit.
- [x] Assess all six win conditions separately, retaining the FAIL verdict.
- [x] Include bounded E1/E2/E3 declarations and identify what they replace.
- [ ] Team must verify declarations exactly match the actual mid-point filing.
- [x] Explain real solar energy supply, conversion assumptions, waste heat and resource caps.
- [x] Identify models as simplified, enabler-dependent or unvalidated in the narrative and assumption register.
- [x] Include governing gravity, orbital energy, thrust, mass-flow, power, climate, optical and structural equations.
- [x] Give computed sizing values and distinguish them from observed mission outputs.
- [x] Cover all ten benchmark groups, measurements, implemented tolerances and provenance.
- [x] State baseline drift and identify the separate complete propulsion ledger.
- [x] Present measured results and limitations with exact evidence paths.
- [x] Organize the source into eight intended pages.
- [ ] Compile in Overleaf, check final page count <=8 and inspect every page for overflow, missing figures or unreadable tables. Page separators do not guarantee eight rendered pages.
- [ ] Check against the organizer's full rulebook/appendix; only the participant guide was supplied.

## Scientific accuracy and evidence
- [x] Separate original completed run, selected coarse/fine prefixes, toy G3 and short diagnostics.
- [x] State selected arrival, departure and full-duration convergence are unobserved.
- [x] Give selected endpoints and distinguish semi-major axis from instantaneous distance.
- [x] Report second-order forced behaviour; reserve fourth-order claim for conservative Yoshida.
- [x] Describe approximate J2000 conditions, ideal lunar initialization and 13-body scope.
- [x] Explain finite booked station-keeping impulses and unvalidated autonomous control.
- [x] Treat G2 orbital envelopes/X6 endpoint envelopes as diagnostics, not precision evidence.
- [x] State 35 passed, zero failed, one explicit long test skipped; distinguish focused viewer rerun.
- [x] Avoid claiming SDK/xUnit execution or tests on every change.
- [x] Report interpolation method, sampling limits and shared convergence interval.
- [x] Report real timestep-dependent ejecta discrepancy without claiming an isolated cause.
- [x] Include selected mass use, cap fractions, derived jet energies and absent generation totals.
- [x] Describe mirror removal as conditional thermal replay, not coupled reintegration.
- [x] Include replay crossing near year 606 and explain unequal endpoint temperatures.
- [x] Explain finite solar-disc optical sensitivity and unresolved focusing hardware.
- [x] Preserve 5 MPa rock allowable and SF approximately 0.139; do not adopt stronger literature values.
- [x] Explain generic scalar stress vs tension/compression/shear and local vs global checks.
- [x] Separate all-step cumulative flags from sampled extrema.
- [x] Preserve placeholder safety status; identify missing atmosphere/spin/magnetic/dose models.
- [x] Explain that launch plume checks do not propagate future ejecta trajectories.
- [x] Explain ledger normalization and vector impulse vs scalar thrust integral.
- [x] Distinguish solar input, jet expenditure, analytic orbital energy and sourceFraction.
- [x] Identify complete original ledger vs missing selected terminal ledger.
- [x] Treat negative source entries as reservoir accounting, not negative physical mass.

## Reproducibility and report assets
- [x] Include source/config/output index and build/run guidance.
- [x] State fresh run vs restart and use a new output directory.
- [x] Include original complete conservation record paths and no-push baseline paths.
- [x] Package selected orbit, propellant and ledger figures alongside LaTeX.
- [x] Correct viewer description: paused at year zero, recorded Moon, display-only interpolation.
- [x] State playback is not a live mission parameter solver.
- [x] State tour timing limitation and visual scaling.
- [ ] Team name, member names and individual contributions.
- [ ] Actual public/private submission repository URL, source snapshot and license decision.
- [ ] Confirm all figures render clearly and retain appropriate axis units/partial-run labels.
- [ ] Replace any remaining bracketed team placeholders.

## Separate submission deliverables (not all part of the eight-page report)
- [ ] Source repository with README setup/run/test instructions and reproducible configuration.
- [ ] Runnable simulation/executable or demonstration laptop.
- [ ] Final compiled technical report, maximum eight pages.
- [ ] Baseline drift record and original full propulsion ledger, labelled as original layout.
- [ ] Selected partial trajectory/conservation evidence, labelled incomplete.
- [ ] Demo video no longer than three minutes; account for playback tour duration limitation.
- [ ] Seven-minute pitch plus preparation for five-minute questions.
- [ ] Individual contribution log and organizer-accepted enabler declarations.

## Checks performed for this revision
- Read supplied main.tex, pitch_deck_info.txt and participant guide PDF.
- Cross-checked newer presentation audit, selected-prefix report, original mission report/ledger, baseline validation and climate source.
- LaTeX static checks cover environment matching, braces, eight intended pages, figure availability and unresolved cross-references.
- No LaTeX engine is available locally; PDF rendering, visual QA and actual page-count acceptance remain outstanding.
- Source simulation, configurations and existing raw outputs were not edited.

## Overleaf upload
Upload main.tex and the figures directory from this folder. Set main.tex as the main document and compile with pdfLaTeX. Figures also resolve from ../pitch_outputs when compiling within this workspace. This package uses only existing saved figures; no mission trajectory or chart data was regenerated.
