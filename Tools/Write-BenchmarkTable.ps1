$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$rows=(Get-Content -Raw -LiteralPath (Join-Path $root 'Results/benchmarks.json') | ConvertFrom-Json).results
$table=New-Object 'System.Collections.Generic.List[string]'
$table.Add('# Executed Core validation results'); $table.Add('')
$table.Add('Generated from Results/benchmarks.json. Budget analytic checks use 1e-9 relative acceptance; guide rounding checks are separately labelled at 2%. Numerical benchmark tolerances remain unchanged.'); $table.Add('')
$table.Add('| ID | Check | Status | Measured | Error | Tolerance | Units | dt (s) |')
$table.Add('|---|---|---|---:|---:|---:|---|---:|')
foreach($r in $rows) {
    $status=if($r.passed){'PASS'}else{'FAIL'}
    $table.Add('| '+$r.id+' | '+$r.name+' | '+$status+' | '+$r.measured.ToString('G17',[cultureinfo]::InvariantCulture)+' | '+$r.error.ToString('G17',[cultureinfo]::InvariantCulture)+' | '+$r.tolerance.ToString('G17',[cultureinfo]::InvariantCulture)+' | '+$r.unit+' | '+$r.dt.ToString('G17',[cultureinfo]::InvariantCulture)+' |')
}
[IO.File]::WriteAllLines((Join-Path $root 'Docs/M2_BenchmarkTable.md'),$table)
