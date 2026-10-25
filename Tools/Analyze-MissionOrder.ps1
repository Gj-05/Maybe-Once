$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$order=Get-Content -LiteralPath (Join-Path $root 'Results/design_audit/order/mission_order.json') -Raw | ConvertFrom-Json
$rows=@()
foreach($p in $order.points) {
    $r=Get-Content -LiteralPath (Join-Path $p.folder 'mission_report.json') -Raw | ConvertFrom-Json
    $last=(Get-Content -LiteralPath (Join-Path $p.folder 'run_mission.csv') -Tail 1).Split(',')
    $rows+=[pscustomobject]@{dt=[double]$p.dt;a=[double]$p.finalA_AU;positionError=[double]$p.earthPositionErrorAU;temperature=[double]::Parse($last[3],[Globalization.CultureInfo]::InvariantCulture);material=[double]$r.materialKg;jetEnergy=[double]$r.jetEnergyJ}
}
function Fit-Slope($values) {
    $x=0.0;$y=0.0;$xx=0.0;$xy=0.0;$n=0
    foreach($v in $values){if($v.error -le 0){continue};$a=[math]::Log($v.dt);$b=[math]::Log($v.error);$x+=$a;$y+=$b;$xx+=$a*$a;$xy+=$a*$b;$n++}
    if($n -lt 2){return $null}; return ($n*$xy-$x*$y)/($n*$xx-$x*$x)
}
$reference=$rows[-1]; $fits=@()
foreach($metric in @('a','temperature','material','jetEnergy')) {
    $points=@(); foreach($row in $rows[0..3]){$points+=[pscustomobject]@{dt=$row.dt;error=[math]::Abs($row.$metric-$reference.$metric)}}
    $fits+=[pscustomobject]@{metric=$metric;logLogOrder=(Fit-Slope $points);points=$points}
}
$richardson=@()
for($k=0;$k -lt 3;$k++){$first=[math]::Abs($rows[$k].a-$rows[$k+1].a);$second=[math]::Abs($rows[$k+1].a-$rows[$k+2].a);$richardson+=[pscustomobject]@{coarseDt=$rows[$k].dt;order=[math]::Log($first/$second,2)}}
$result=[pscustomobject]@{scope=$order.scope;fits=$fits;richardsonA=$richardson;raw=$rows;note='Fit against a finite numerical reference biases the slope; Richardson differences are also reported. Thermal and resource observables are evaluated separately from orbital position.'}
[IO.File]::WriteAllText((Join-Path $root 'Results/design_audit/order/observable_orders.json'),($result | ConvertTo-Json -Depth 10))
$fits | Select-Object metric,logLogOrder | Format-Table
$richardson | Format-Table
