$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Set-Location $root
$text=Get-Content PITCH_FACTS.md -Raw -Encoding UTF8
function Replace-Section($key,$lines) { $script:text=$script:text.Replace('{{'+$key+'}}',($lines -join "`n")) }
function F($x) {if($x -is [double]) {return $x.ToString('R',[Globalization.CultureInfo]::InvariantCulture)}; return [string]$x}
function Stats($name) {$r=@{}; Get-Content "pitch_outputs/${name}_sampled_stats.json" -Raw | ConvertFrom-Json | ForEach-Object {$r[$_.Key]=$_.Value}; return $r}
$bench=(Get-Content pitch_outputs/Results/benchmarks.json -Raw | ConvertFrom-Json).results
$conv=Get-Content Results/partial_convergence.json -Raw | ConvertFrom-Json
$old=Get-Content Results/mission_h3600_y10000/mission_report.json -Raw | ConvertFrom-Json
$fresh=Get-Content pitch_outputs/selected_diagnostic_1yr/mission_report.json -Raw | ConvertFrom-Json
$c=Stats selected_h3600; $f=Stats selected_h1800; $o=Stats original_full; $n=Stats fresh_1yr
$rows=@('| Quantity | Formula | Code-computed value | Source |','|---|---|---:|---|')
$formulas=New-Object 'System.Collections.Generic.Dictionary[string,string]'
$formulas.Add('E','mu*M/2*(1/a1-1/a2)');$formulas.Add('dv','sqrt(mu/a1)-sqrt(mu/a2)');$formulas.Add('p','M*delta-v');$formulas.Add('F','impulse/tau');$formulas.Add('P','delta-E/tau');$formulas.Add('L','M*(sqrt(mu*a2)-sqrt(mu*a1))');$formulas.Add('solarDays','delta-E/L_sun/day')
foreach($b in $bench | Where-Object {$_.id -like 'Budget.analytic.nominal.*'}) {$field=$b.id.Split('.')[-1]; $rows+='| '+$field+' | '+$formulas[$field]+' | '+(F $b.measured)+' '+$b.unit+' | `Engineering.cs: MissionBudget`; `pitch_outputs/Results/benchmarks.json` |'}
$rows+=''; $rows+='Code-called delta-v conversion: 11.65830562662991 km/s (`pitch_outputs/computed_code_facts.json`, `pitch_outputs/Audit.cs`).'
Replace-Section BUDGET $rows
$rows=@('```text') + @(Get-Content Results/mission_h3600_y10000/ledger.txt) + @('```')
Replace-Section LEDGER $rows
$rows=@('| Evidence | Final time | Final a_EMB | Final eccentricity | Target crossing / verdict | Source |','|---|---:|---:|---:|---|---|')
foreach($r in $conv.runs) {$rows+='| Selected h='+(F $r.stepSeconds)+' s stopped prefix | '+(F $r.lastPreservedYear)+' yr | '+(F $r.aAU)+' AU | '+(F $r.eccentricity)+' dimensionless | NOT OBSERVED; incomplete | `Results/partial_convergence.json` / raw `Results/design_audit/full_h'+$r.stepSeconds+'/run_mission.csv`; `MissionSimulation.WriteRow` |'}
$rows+='| Original layout full run | 10000 yr | '+(F $old.finalA_AU)+' AU | '+(F $old.finalE)+' dimensionless | Crossing '+(F ($old.targetTimeSeconds/31557600))+' yr; within limit, mission FAIL | `Results/mission_h3600_y10000/mission_report.json`; `MissionSimulation.Run` |'
Replace-Section RUNS $rows
$rows=@('| Fresh selected diagnostic | Value | Source |','|---|---:|---|')
foreach($pair in @(@('Elapsed',($fresh.seconds/31557600),'yr'),@('Final a',$fresh.finalA_AU,'AU'),@('Final e',$fresh.finalE,'dimensionless'),@('Ejected',$fresh.finalEjectedKg,'kg'),@('Minimum SF',$fresh.minimumSafetyFactor,'dimensionless'),@('Maximum flux',$fresh.maximumFlux,'W/m2'),@('Maximum tide ratio',$fresh.maximumTideRatio,'Moon tide ratio'),@('Earth plume clearance',$fresh.minimumEarthPlumeClearance,'m'),@('Moon plume clearance',$fresh.minimumMoonPlumeClearance,'m'))) {$rows+='| '+$pair[0]+' | '+(F $pair[1])+' '+$pair[2]+' | `pitch_outputs/selected_diagnostic_1yr/mission_report.json`; `MissionSimulation.Run` |'}
$rows+='';$rows+='Target not reached; departureComplete=false; status FAIL. Diagnostic completes its requested interval, not the mission duration.'
Replace-Section FRESH $rows
$rows=@('| Source run | Final ejecta | Final array area | Sampled minimum binding margin | Final Moon distance |','|---|---:|---:|---:|---:|')
foreach($item in @(@('Selected h=3600 s',$c),@('Selected h=1800 s',$f),@('Original full',$o),@('Fresh diagnostic',$n))) {$s=$item[1];$rows+='| '+$item[0]+' | '+(F $s.final_ejected_kg)+' kg | '+(F $s.final_array_area_m2)+' m2 | '+(F $s.minimum_sampled_binding_margin_J_kg)+' J/kg | '+(F $s.final_moon_distance_m)+' m |'}
$rows+='';$rows+='Source: `pitch_outputs/*_sampled_stats.json`, `pitch_outputs/Audit.cs`, extracting complete raw CSV rows; `InitialConditions.MoonBound` binding formula. Array starts at 1.09516103015928E+20 m2 in each listed run; original fixed collector does not grow. Original ejecta agrees with `ledger.txt`.'
Replace-Section RESOURCES $rows
Copy-Item Results/design_audit/siting/array_siting_sweep.csv pitch_outputs/array_siting_sweep.csv
$sweep=Import-Csv pitch_outputs/array_siting_sweep.csv
$rows=@('| Radius / multiplier | Configurations | Zero sampled shading/intersection candidates | Max array-jet power across screen |','|---|---:|---:|---:|')
foreach($group in $sweep | Group-Object radius_AU) {$maximum=($group.Group | ForEach-Object {[double]$_.max_array_jet_W} | Measure-Object -Maximum).Maximum; $zero=@($group.Group | Where-Object {$_.zero_sampled_shading_and_intersections -eq '1'}).Count;$rows+='| '+$group.Name+' AU fixed / dimensionless co-moving | '+$group.Count+' configurations | '+$zero+' candidates | '+(F $maximum)+' W |'}
$rows+='';$rows+='Source: `ArraySitingSweep.Run` and copied full CSV; maxima are kinematic sampled-screen quantities.'
Replace-Section SWEEP $rows
$rows=@('| Safety metric | Retained threshold | Selected h=3600 s prefix evidence | Original full evidence | Status / source |','|---|---|---|---|---|')
$rows+='| Temperature | 278..298 K | 288..'+(F $c.max_temperature_K)+' K sampled | 288..'+(F $old.temperatureMaximum)+' K all-step | Proxy within bounds; `MissionMonitors.Check` |'
$rows+='| Tide | 0.01 lunar-tide ratio | '+(F $c.max_tide_ratio)+' ratio sampled | '+(F $o.max_tide_ratio)+' ratio sampled | Selected no tide flag; original FAIL; `MissionMonitors.Check` |'
$rows+='| Incoming flux | 1429.05 W/m2 | '+(F $c.max_effective_flux_W_m2)+' W/m2 sampled | '+(F $o.max_effective_flux_W_m2)+' W/m2 sampled | Selected no flux flag; original FAIL; `MissionRadiation.Evaluate` |'
$rows+='| Shading | 0.01 solar-disc fraction | 0 fraction sampled | 1 fraction cumulative | Selected no shading flag; original FAIL; `MissionGeometry.Shading` |'
$rows+='| Eccentricity | 0.1 dimensionless | '+(F $c.max_e)+' dimensionless sampled | '+(F $o.max_e)+' dimensionless sampled | No eccentricity flag; `MissionMonitors.Check` |'
$rows+='| Moon distance | 300000000..500000000 m | 374491272.76501817..399757824.84215826 m sampled | Final '+(F $o.final_moon_distance_m)+' m | Cumulative Moon-bound true; `Results/partial_convergence.json`, original report |'
$rows+='| Lunar binding / Hill | Specific energy <0 J/kg and distance < computed Hill radius (m) | Cumulative true | Cumulative true in original safe-condition detail | `InitialConditions.MoonBound`; minimum all-step margin NOT FOUND |'
$rows+='| Plume exclusion | 3 Earth radii; no cone-target intersection | No plume flag; all-step extrema NOT FOUND | Plume intersections flagged | `MissionMonitors.Check`; fresh clearances in §7 |'
$rows+='| Plume heating | 1 W/m2 | No corresponding flag; maximum NOT FOUND | PlumeHeating.Array flagged; maximum NOT FOUND | `MissionMonitors.Check` |'
$rows+='| Liquid fraction | 1 fraction | Boolean proxy; no LiquidOcean/Snowball flag | No corresponding flag | `EnergyBalanceClimate`; not a spatial fraction model |'
$rows+='| Safety factor | 1.5 dimensionless | 0.13896574091969552 dimensionless cumulative | 0.09209717940210621 dimensionless cumulative | FAIL both; `MissionMonitors.Structural` |'
$rows+='| Ledger P/L/E | 1E-9 relative | All below threshold; §6 / §10 | All below threshold; §6 | Numeric PASS, not safety validation; `Ledger.Check` |'
$rows+='| Convergence | 0.01 relative | Full-duration NOT OBSERVED | Not a matched selected-design comparison | `MissionThresholds`; partial absolute differences in §10 |'
$rows+='';$rows+='Sources: `Config/thresholds.json`, `Results/partial_convergence.json`, original `mission_report.json`; sampled original and selected values independently extracted in `pitch_outputs/Audit.cs`. Sampled maxima are lower bounds on all-step maxima, not all-step certifications. Fine-prefix evidence is retained in `Results/partial_convergence.json`.'
Replace-Section SAFETY $rows
Replace-Section TEMPERATURES @('254.0386205814548 K at 1 AU; 154.60298107773218 K at 2.7 AU without restoration')
$rows=@('| Original full-run flag | First occurrence | Enabler-caused? | Recorded cause |','|---|---:|---|---|')
foreach($flag in $old.flags) {$rows+='| '+$flag.code+' | '+(F $flag.firstTimeSeconds)+' s | '+$flag.enabler+' | '+$flag.cause+' |'}
Replace-Section FLAGS $rows
$rows=@('| Component/evidence | Worst stress | Allowable / SF | Result | Source |','|---|---:|---|---|')
foreach($r in $conv.runs) {$rows+='| Rock proxy selected h='+(F $r.stepSeconds)+' s | '+(F $r.inferredGoverningRockStressPa)+' Pa | 5000000 Pa / '+(F $r.minimumSafetyFactorCumulative)+' dimensionless | FAIL | `Results/partial_convergence.json`, `MissionMonitors.Structural` |'}
$rows+='| Fresh diagnostic array local beam | '+(F $fresh.maximumArrayBendingStress)+' Pa | 200000000 Pa / '+(F (200000000/$fresh.maximumArrayBendingStress))+' dimensionless | Local bending PASS only | fresh report, `MissionMonitors.Structural`, `Beam.SafetyFactor` |'
$rows+='| Fresh diagnostic mirror local beam | '+(F $fresh.maximumMirrorBendingStress)+' Pa | 200000000 Pa / '+(F (200000000/$fresh.maximumMirrorBendingStress))+' dimensionless | Local bending PASS only | same source |'
$rows+='| Original combined minimum | NOT FOUND per-component in old terminal summary | '+(F $old.minimumSafetyFactor)+' dimensionless | FAIL | original `mission_report.json` |'
$rows+='| Global array/mirror, thruster mounts, thermal coupling | NOT VALIDATED | NOT FOUND | No adequacy claim | `ThermalStructuralModelIncomplete`, `DesignStudyAudit.Structures` |'
Replace-Section STRUCTURE $rows
$rows=@('| ID | Description | Expected | Tolerance | Measured | Result | Test / producer |','|---|---|---:|---:|---|---|')
foreach($b in $bench) {$producer=if($b.id -like 'B[0-9]*') {'BenchmarkSuite.Run'} elseif($b.id -like 'Budget.*') {'MissionBudgetChecks'} else {'ValidationSuite.Run'};$status=if($b.passed){'PASS'}else{'FAIL'};$rows+='| '+$b.id+' | '+$b.name+' | '+(F $b.expected)+' '+$b.unit+' | '+(F $b.tolerance)+' '+$b.unit+' | '+(F $b.measured)+' '+$b.unit+' | '+$status+' | `CoreTests.cs`; `'+$producer+'` |'}
Replace-Section BENCHMARKS $rows
$rows=@('| Quantity | Maximum absolute difference | Final signed coarse-minus-fine | Max nearest-record alignment correction |','|---|---:|---:|---:|')
$units=@{a_EMB_AU='AU';eccentricity='dimensionless';Earth_Moon_distance_m='m';thrust_N='N';ejected_kg='kg';temperature_K='K';relative_P='relative';relative_L='relative';relative_E='relative'}
foreach($m in $conv.metrics) {$u=$units[$m.metric];$rows+='| '+$m.metric+' | '+(F $m.maxAbsoluteDifference)+' '+$u+' | '+(F $m.finalSignedDifference)+' '+$u+' | '+(F $m.maxAlignmentCorrectionFromNearestCoarseRecord)+' '+$u+' |'}
Replace-Section CONVERGENCE $rows
$rows=@('| Energy/resource quantity | Original full run (cached) | Fresh selected diagnostic |','|---|---:|---:|')
foreach($item in @(@('Incident collected solar energy','solarInputJ','J'),@('Jet energy required','jetEnergyJ','J'),@('Reported source fraction','sourceFraction','fraction'),@('Material including growth/propellant','materialKg','kg'))) {$rows+='| '+$item[0]+' | '+(F $old.($item[1]))+' '+$item[2]+' | '+(F $fresh.($item[1]))+' '+$item[2]+' |'}
$energy=($bench | Where-Object {$_.id -eq 'Budget.analytic.nominal.E'}).measured
$rows+='| Orbital budget energy (analytic, not measured gain) | '+(F $energy)+' J | Not a diagnostic energy requirement |'
Replace-Section ENERGY $rows
if($text -match '\{\{[A-Z]+\}\}') {throw 'Unfilled report placeholder'}
[IO.File]::WriteAllText((Join-Path $root PITCH_FACTS.md),$text,(New-Object Text.UTF8Encoding($false)))
Write-Output ('Report filled: '+$bench.Count+' fresh benchmark comparison rows.')
