# Executed Core validation results

Generated from Results/benchmarks.json. Budget analytic checks use 1e-9 relative acceptance; guide rounding checks are separately labelled at 2%. Numerical benchmark tolerances remain unchanged.

| ID | Check | Status | Measured | Error | Tolerance | Units | dt (s) |
|---|---|---|---:|---:|---:|---|---:|
| B1 | Wire vector error | PASS | 0 | 0 | 1E-08 | N | 0 |
| B2 | Closed loop net force | PASS | 0 | 0 | 4.9999999999999998E-08 | N | 0 |
| B3 | Long wire field | PASS | 0.0002 | 0 | 2.0000000000000001E-10 | T | 0 |
| B4.position | Eccentric Kepler return, 10 periods | PASS | 2.5060734130426048E-11 | 2.5060734130426048E-11 | 9.9999999999999995E-07 | a | 1577.909800912054 |
| B4.energy | Eccentric Kepler energy | PASS | 3.2868533716556298E-13 | 3.2868533716556298E-13 | 1.0000000000000001E-09 | relative | 1577.909800912054 |
| B5 | Full solar system 5yr max energy drift | PASS | 9.5658840785374595E-14 | 9.5658840785374595E-14 | 1.0000000000000001E-09 | relative | 3600 |
| B5.P | Full system max momentum drift | PASS | 8.9518528001070295E-15 | 8.9518528001070295E-15 | 9.9999999999999994E-12 | initial gross momentum | 3600 |
| B5.L | Full system max angular momentum drift | PASS | 3.2442493698991597E-14 | 3.2442493698991597E-14 | 9.9999999999999994E-12 | relative | 3600 |
| B6 | N-body EMB sidereal period | PASS | 365.2602450589337 | 0.004245058933747714 | 0.1 | days | 3600 |
| B7.dv1 | Hohmann departure | PASS | 2.9460551625085643 | 0.006055162508564305 | 0.02 | km/s | 0 |
| B7.dv2 | Hohmann arrival | PASS | 2.6499820804508647 | 1.7919549135214652E-05 | 0.02 | km/s | 0 |
| B7.time | Hohmann flight time | PASS | 258.9151502335679 | 0.08484976643211439 | 2 | days | 0 |
| B7.simulatedRadius | Simulated two-burn arrival radius | PASS | 5.9952043329758453E-15 | 5.9952043329758453E-15 | 1E-08 | relative | 2237.0268980180263 |
| B7.simulatedBurn | Simulated arrival burn | PASS | 2.6499820804508962 | 1.7919549103684318E-05 | 0.02 | km/s | 2237.0268980180263 |
| B7.circular | Post-burn eccentricity | PASS | 1.583067659490474E-14 | 1.583067659490474E-14 | 1E-08 | dimensionless | 2237.0268980180263 |
| B8 | Prograde push semi-major growth | PASS | 3170.1014833984377 | 0.10148339843772192 | 31.7 | km | 3600 |
| B8.Gauss | Integrated Gauss versus state delta-a | PASS | 3.4086034802729838E-10 | 3.4086034802729838E-10 | 0.01 | relative | 3600 |
| B9.moment | Cantilever root moment | PASS | 5000 | 0 | 50 | N m | 0 |
| B9.deflection | Cantilever tip deflection | PASS | 0.0062499999999999995 | 8.6736173798840355E-19 | 6.2500000000000001E-05 | m | 0 |
| B10 | Earth radiative equilibrium | PASS | 254.0386205814548 | 0.038620581454807734 | 2 | K | 0 |
| X1 | Circular full-period return | PASS | 5.2903354655572811E-14 | 5.2903354655572811E-14 | 1.0000000000000001E-09 | AU | 1577.909800912054 |
| X2 | Kepler period squared ratio | PASS | 19.683000000000003 | 0 | 9.9999999999999998E-13 | dimensionless | 0 |
| X5.rocket | Finite burn rocket equation | PASS | 0.10005003335834001 | 0 | 1.0005003335834001E-10 | m/s | 0 |
| X5.momentum | Rocket booked momentum closure | PASS | 1.3358203432289883E-12 | 1.3358203432289883E-12 | 1.0000000000000001E-09 | relative | 0 |
| X5.energy | Rocket energy closure | PASS | 1.6895430964416829E-13 | 1.6895430964416829E-13 | 1.0000000000000001E-09 | relative | 0 |
| X5.reaction | Rocket impulse reaction closure | PASS | 1.3364888104939896E-13 | 1.3364888104939896E-13 | 1.0000000000000001E-09 | relative | 0 |
| X5.sourceP | Replenished source/ejecta P closure | PASS | 8.8774505621021307E-17 | 8.8774505621021307E-17 | 1.0000000000000001E-09 | relative | 0 |
| X5.sourceE | Replenished energy input closure | PASS | 0 | 0 | 1.0000000000000001E-09 | relative | 0 |
| X5.sourceL | Replenished angular closure | PASS | 3.7895612573872012E-16 | 3.7895612573872012E-16 | 1.0000000000000001E-09 | relative | 0 |
| X5.force | Applied force/body impulse closure | PASS | 0 | 0 | 1.0000000000000001E-09 | relative | 0 |
| X6.Yoshida4.h100 | Analytic endpoint error | PASS | 0.0004086374130350796 | 0.0004086374130350796 | 1 | AU | 315581.9601824108 |
| X6.Yoshida4.h200 | Analytic endpoint error | PASS | 2.562026595836706E-05 | 2.562026595836706E-05 | 1 | AU | 157790.9800912054 |
| X6.Yoshida4.difference200 | h versus h/2 endpoint difference | PASS | 0.00038301714808081777 | 0.00038301714808081777 | 1 | AU | 157790.9800912054 |
| X6.Yoshida4.h400 | Analytic endpoint error | PASS | 1.6024423543647646E-06 | 1.6024423543647646E-06 | 1 | AU | 78895.4900456027 |
| X6.Yoshida4.difference400 | h versus h/2 endpoint difference | PASS | 2.4017823604249305E-05 | 2.4017823604249305E-05 | 1 | AU | 78895.4900456027 |
| X6.Yoshida4.h800 | Analytic endpoint error | PASS | 1.0017059648855532E-07 | 1.0017059648855532E-07 | 1 | AU | 39447.74502280135 |
| X6.Yoshida4.difference800 | h versus h/2 endpoint difference | PASS | 1.5022717578762818E-06 | 1.5022717578762818E-06 | 1 | AU | 39447.74502280135 |
| X6.Yoshida4 | Observed log-log order | PASS | 3.9981380562450193 | 0.0018619437549807216 | 0.2 | order | 0 |
| X6.VelocityVerlet.h100 | Analytic endpoint error | PASS | 0.03262411752254607 | 0.03262411752254607 | 1 | AU | 315581.9601824108 |
| X6.VelocityVerlet.h200 | Analytic endpoint error | PASS | 0.008118830518607354 | 0.008118830518607354 | 1 | AU | 157790.9800912054 |
| X6.VelocityVerlet.difference200 | h versus h/2 endpoint difference | PASS | 0.024506464241130677 | 0.024506464241130677 | 1 | AU | 157790.9800912054 |
| X6.VelocityVerlet.h400 | Analytic endpoint error | PASS | 0.0020273681692827335 | 0.0020273681692827335 | 1 | AU | 78895.4900456027 |
| X6.VelocityVerlet.difference400 | h versus h/2 endpoint difference | PASS | 0.006091480607351499 | 0.006091480607351499 | 1 | AU | 78895.4900456027 |
| X6.VelocityVerlet.h800 | Analytic endpoint error | PASS | 0.0005066956086365386 | 0.0005066956086365386 | 1 | AU | 39447.74502280135 |
| X6.VelocityVerlet.difference800 | h versus h/2 endpoint difference | PASS | 0.0015206728453950504 | 0.0015206728453950504 | 1 | AU | 39447.74502280135 |
| X6.VelocityVerlet | Observed log-log order | PASS | 2.0027691048360627 | 0.0027691048360627413 | 0.12 | order | 0 |
| X7 | Encke versus double push displacement | PASS | 0.0003911950366710937 | 0.0003911950366710937 | 0.05 | m | 60 |
| X7.reference | Unforced Encke deviation | PASS | 0 | 0 | 9.9999999999999995E-21 | m | 60 |
| X8.float | Float loses day-long tiny push | PASS | 0 | 0 | 0 | m | 60 |
| X8.double | Double retains tiny push | PASS | 37.32354081118029 | 3.2106801796771833E-05 | 0.05 | m | 60 |
| Budget.analytic.nominal.E | Budget versus independent analytic E | PASS | 1.6882536167514338E+33 | 0 | 1.6882536167514339E+24 | J | 0 |
| Budget.analytic.nominal.dv | Budget versus independent analytic dv | PASS | 11658.30562662991 | 0 | 1.1658305626629911E-05 | m/s | 0 |
| Budget.analytic.nominal.p | Budget versus independent analytic p | PASS | 7.0474457512977811E+28 | 0 | 7.0474457512977818E+19 | N s | 0 |
| Budget.analytic.nominal.F | Budget versus independent analytic F | PASS | 2.2332007983172931E+17 | 0 | 223320079.83172932 | N | 0 |
| Budget.analytic.nominal.P | Budget versus independent analytic P | PASS | 5.3497528859971415E+21 | 0 | 5349752885997.142 | W | 0 |
| Budget.analytic.nominal.L | Budget versus independent analytic L | PASS | 1.7323635432564215E+40 | 2.4178516392292583E+24 | 1.732363543256422E+31 | kg m2/s | 0 |
| Budget.analytic.nominal.solarDays | Budget versus independent analytic solarDays | PASS | 51.04486002117178 | 0 | 5.104486002117178E-08 | days | 0 |
| Budget.analytic.alternate.E | Budget versus independent analytic E | PASS | 1.3583124106912661E+33 | 2.8823037615171174E+17 | 1.3583124106912659E+24 | J | 0 |
| Budget.analytic.alternate.dv | Budget versus independent analytic dv | PASS | 11692.211271536697 | 1.8189894035458565E-12 | 1.1692211271536699E-05 | m/s | 0 |
| Budget.analytic.alternate.p | Budget versus independent analytic p | PASS | 4.9475591995507534E+28 | 8796093022208 | 4.9475591995507548E+19 | N s | 0 |
| Budget.analytic.alternate.F | Budget versus independent analytic F | PASS | 6.8164647991509158E+17 | 128 | 681646479.9150918 | N | 0 |
| Budget.analytic.alternate.P | Budget versus independent analytic P | PASS | 1.871405345603052E+22 | 4194304 | 18714053456030.516 | W | 0 |
| Budget.analytic.alternate.L | Budget versus independent analytic L | PASS | 9.1251120382970576E+39 | 4.8357032784585167E+24 | 9.1251120382970629E+30 | kg m2/s | 0 |
| Budget.analytic.alternate.solarDays | Budget versus independent analytic solarDays | PASS | 41.06898760991337 | 7.1054273576010019E-15 | 4.1068987609913367E-08 | days | 0 |
| Budget.analytic.inward.E | Budget versus independent analytic E | PASS | -1.6882536167514338E+33 | 0 | 1.6882536167514339E+24 | J | 0 |
| Budget.analytic.inward.dv | Budget versus independent analytic dv | PASS | -11658.30562662991 | 0 | 1.1658305626629911E-05 | m/s | 0 |
| Budget.analytic.inward.p | Budget versus independent analytic p | PASS | -7.0474457512977811E+28 | 0 | 7.0474457512977818E+19 | N s | 0 |
| Budget.analytic.inward.F | Budget versus independent analytic F | PASS | -4.4664015966345862E+17 | 0 | 446640159.66345865 | N | 0 |
| Budget.analytic.inward.P | Budget versus independent analytic P | PASS | -1.0699505771994283E+22 | 0 | 10699505771994.283 | W | 0 |
| Budget.analytic.inward.L | Budget versus independent analytic L | PASS | -1.7323635432564215E+40 | 2.4178516392292583E+24 | 1.732363543256422E+31 | kg m2/s | 0 |
| Budget.analytic.inward.solarDays | Budget versus independent analytic solarDays | PASS | -51.04486002117178 | 0 | 5.104486002117178E-08 | days | 0 |
| X10 | Moon bound over 10yr including planets | PASS | 1 | 0 | 0 | boolean | 3600 |
| X11 | Reset and bitwise determinism | PASS | 1 | 0 | 0 | boolean | 0 |
| X11.forceReset | Stateful force reset and determinism | PASS | 1 | 0 | 0 | boolean | 0 |
| X.kepler | Elements-state roundtrip | PASS | 3.0599611460579433E-16 | 3.0599611460579433E-16 | 9.9999999999999998E-13 | AU | 0 |
| X.clock | Scheduler cap pause resume reset | PASS | 1 | 0 | 0 | boolean | 0 |
| X.softening | Softened-force consistent energy ledger | PASS | 3.3930953475924023E-15 | 3.3930953475924023E-15 | 1.0000000000000001E-09 | relative | 3600 |
| X.photon | Photon sink angular booking | PASS | 0 | 0 | 1.0000000000000001E-15 | kg m2/s | 0 |
| Budget.guide.E | Rounded guide sanity E | PASS | 1.6882536167514338E+33 | 1.7463832485662422E+30 | 3.3799999999999999E+31 | J | 0 |
| Budget.guide.v1 | Rounded guide sanity v1 | PASS | 29.784691831696804 | 0.004691831696803206 | 0.5956 | km/s | 0 |
| Budget.guide.v2 | Rounded guide sanity v2 | PASS | 18.126386205066893 | 0.006386205066892359 | 0.3624 | km/s | 0 |
| Budget.guide.dv | Rounded guide sanity dv | PASS | 11.65830562662991 | 0.0016943733700909291 | 0.23320000000000002 | km/s | 0 |
| Budget.guide.p | Rounded guide sanity p | PASS | 7.0474457512977811E+28 | 2.5542487022188258E+25 | 1.4099999999999999E+27 | N s | 0 |
| Budget.guide.F | Rounded guide sanity F | PASS | 2.2332007983172931E+17 | 3320079831729312 | 4400000000000000 | N | 0 |
| Budget.guide.acceleration | Rounded guide sanity acceleration | PASS | 3.6942941245943639E-08 | 5.7058754056361191E-11 | 7.4000000000000003E-10 | m/s2 | 0 |
| Budget.guide.P | Rounded guide sanity P | PASS | 5.3497528859971415E+21 | 5.0247114002858508E+19 | 1.08E+20 | W | 0 |
| Budget.guide.L | Rounded guide sanity L | PASS | 1.7323635432564215E+40 | 2.3635432564216038E+37 | 3.4599999999999998E+38 | kg m2/s | 0 |
