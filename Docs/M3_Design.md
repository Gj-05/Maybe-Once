# M3 staged implementation

## Full mission diagnosis, 2026-10-25

The configured 10,000-year diagnostic completed with no partial step. Its EMB semi-major axis crossed 2.7 AU at 8742.175906913073 years and ended at 2.699138036643842 AU after continued powered radial parking; terminal eccentricity is 0.02076560175195826. It remains FAIL (provisional) with 14 unresolved safety/engineering flags. The earlier 100-year-only co-rotating inspection hid real heliocentric growth. The viewer now loads the full mission CSV with linked Sun-centred/close views, source label, explicit enlargement factors, pull direction, trajectory and time controls. This visualization is diagnostic; it does not bypass later gate acceptance. See [full diagnosis](Tractor_Diagnosis.md) and Results/tractor_diagnosis.json for measured epochs, force checks, ledgers, flags, rerun G1-G3 results and verification limits.

## Organiser decision pending

On 2026-10-25 the user instructed: "Keep unresolved pending organiser confirmation" for the giant focusing mirror. Config/engineering_assumptions.json records this decision. FocusingOpticsUnresolved remains a non-enabler flag and prevents a mission WIN. Neither E1 nor E2 is taken as approval of the optical capability. Conditional mirror calculations can be inspected without treating them as accepted engineering.

G1-G3 have executed passing results. The subsequent 100-year full-infrastructure diagnostic is not the 10,000-year G4 acceptance run. It completed with small mechanical ledger errors but failed several safety/engineering conditions, including a minimum illustrative structural safety factor of 0.0921 and complete shading during an alignment. G4 is unaccepted; G5-G8 have not passed.

## Prerequisite correction

The user explicitly approved independent analytic budget acceptance at 1e-9 relative, with rounded guide figures separated into a 2% sanity test. All seven MissionBudget fields are checked at three input fixtures; the guide figures are not an oracle for physics. Actual M1/M2 rerun: 29/29 Unity tests passed; headless bench exit 0. Maximum relative budget analytic error 5.29933578696198e-16. Historical failed-reference artifacts remain preserved. No physical constants or production budget equations were changed.

## Sign conventions and kick booking

The thrust axis is the force direction on the tractor. Each exhaust direction is opposite its individual thruster axis. Axes are e_th cos(alpha) +/- n sin(alpha), with n projected perpendicular to the required thrust axis if numerical tracking error leaves the plane. Thus the lateral impulses cancel, net thrust is T e_th, total mass flow T/(ve cos alpha), and ideal jet input 0.5 dm ve². Supplier material arrives at the body's kick-average velocity. Ejecta and supplier momenta, angular momenta and kinetic/potential energies are booked with the actual physical half-kick duration. No negative ejected mass is introduced during Yoshida's negative conservative substep.

G1 reuses the existing positive-time external half-kick composition around Yoshida4 gravity. This forced composition is second-order; no claim of fourth-order dissipative dynamics is made. Body and reaction impulse checks are distinct from global momentum error normalized by initial gross momentum. Source/sink accumulation is now compensated. Finite-mass M2 toy burns are unchanged.

## G1 toy hover

The toy contains a moving Sun, Earth and tractor in a barycentric frame, initialized with a circular Sun-Earth orbit. No Moon or other planets in this first control test. Actual tractor mass is solved from the desired prograde pull using the actual force projection, rather than adding force magnitudes in different directions. Configuration assumptions from the user: d=5e7m, margin=1.15, ve=300km/s, cant=25deg, omega_c=2e-4rad/s, damping=1. These are test/controller inputs, not proven hardware capabilities.

The dynamic tractor follows r_target=r_E+d u, v_target=v_E+d u_dot, where u is heliocentric velocity direction. The supplied circular/slow-spiral feedforward a_E-d|u_dot|²u is combined with PD tracking. Gravity acts reciprocally on every toy body. Two actual EjectaDrive instances apply the canted impulses and book the supplied mass and explicit toy jet-energy input. This gate does not claim a solar power source; collector availability is a later gate.

Measured Earth acceleration is obtained from finite velocity change, subtracting endpoint-averaged solar acceleration, then compared as a vector to G mt r_rel/|r_rel|³. Hover separation and all diagnostics are measured over every full step for 30 days. Runs at 60 and 30 seconds are saved to Results/g1_hover_h60.csv and g1_hover_h30.csv, with ledger/config/result JSON. A failed row blocks advancement. The fine timestep is chosen to resolve PD and gravitational timescales; no acceptance tolerance was changed.

G1 results are in Results/g1_results.json. The canted streams are applied simultaneously at the net kick midpoint, avoiding a spurious intermediate lateral body velocity or supplier momentum from sequential thruster impulses. G2 and subsequent gates must run in order. Mirror, array and climate have not been implemented at this stage.

## G2 prescribed-hover discretization

Mode A uses dynamic PD control. Mode B solves an initial physical canted impulse such that the following conservative N-body propagation ends on r_E+d u, then applies the physical ending impulse required for v_E+d u_dot. Both impulses use the same canted ejecta/source/energy booking as Mode A. There is no position or velocity teleport and no invented constraint-reaction sink. A fixed-point shooting solve converges to a 1cm endpoint residual; previous impulses provide an extrapolated initial guess for efficiency. The residual threshold controls the solver, not the acceptance tolerances. All eleven physical bodies (Sun, eight planets, Moon, tractor) still interact gravitationally. This is a discrete prescribed-hover method, not an exact continuous constraint solver; its timestep sensitivity is explicitly measured.

Delivered delta-v is the time integral of the actual tractor gravitational pull on Earth and Moon projected along their heliocentric barycentre velocity, divided by pair mass. This avoids confusing orbital phase separation from an unforced run with delivered scalar prograde impulse. The comparison reports final semi-major and eccentricity differences and each ledger independently. Ten-year h=300s comparison passed: A=13.341600034466166m/s; B=13.403324338459475m/s; relative difference 0.00460514887461143 <0.01. The h/2 run is separate. G3 is the ideal-powered mechanical spiral gate; no claim of sunlight supply or habitability follows from it.
