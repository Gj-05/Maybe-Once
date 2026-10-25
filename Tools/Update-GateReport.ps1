$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$entries=@(
    @{gate='G1';files=@('Results/g1_results.json')},
    @{gate='G2';files=@('Results/g2_h300/g2_results.json','Results/g2_h150/g2_results.json')},
    @{gate='G3';files=@('Results/g3_h3600/g3_results.json','Results/g3_h1800/g3_results.json')},
    @{gate='G4';files=@('Results/g4_results.json')},
    @{gate='G5';files=@('Results/g5_results.json')},
    @{gate='G6';files=@('Results/g6_results.json')},
    @{gate='G7';files=@('Results/g7_results.json')},
    @{gate='G8';files=@('Results/g8_results.json')}
)
$gates=@()
foreach($entry in $entries) {
    $status='PASS'; $checks=@()
    foreach($file in $entry.files) {
        $path=Join-Path $root $file
        if(-not (Test-Path -LiteralPath $path)) { if($status -ne 'FAIL') {$status='NOT_RUN'}; continue }
        $data=Get-Content -Raw -LiteralPath $path | ConvertFrom-Json
        foreach($result in $data.results) { $checks+=$result; if(-not $result.passed) {$status='FAIL'} }
    }
    $gates+=[pscustomobject]@{id=$entry.gate;status=$status;files=$entry.files;checks=$checks}
}
$report=@{schema=1;missionStatus='NOT_EVALUATED';note='Gate status is read from executed result files. Pending later gates are not mission success.';gates=$gates}
[IO.File]::WriteAllText((Join-Path $root 'Results/m3_gate_status.json'),($report | ConvertTo-Json -Depth 12))
$gates | Select-Object id,status | Format-Table -AutoSize
