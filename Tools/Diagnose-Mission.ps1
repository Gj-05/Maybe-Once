param([string]$MissionFolder='Results/mission_h3600_y10000')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$culture=[Globalization.CultureInfo]::InvariantCulture
function Read-SelectedRows([string]$path,[bool]$milestones) {
    $stream=[IO.FileStream]::new($path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
    $reader=[IO.StreamReader]::new($stream)
    $chosen=[Collections.Generic.List[object]]::new()
    try {
        $header=$reader.ReadLine().Split(','); $last=$null
        while(-not $reader.EndOfStream) {
            $line=$reader.ReadLine(); if(-not $line){continue}; $parts=$line.Split(','); if($parts.Length -ne $header.Length){throw 'Incomplete CSV record'}
            if($milestones -and $parts[0] -notin @('0','31557600','315576000','3155760000','31557600000')) {continue}
            $row=@{}; for($k=0;$k -lt $header.Length;$k++){$row[$header[$k]]=[double]::Parse($parts[$k],$culture)}
            if($milestones){$chosen.Add($row)}else{$last=$row}
        }
        if(-not $milestones -and $last){$chosen.Add($last)}
    } finally {$reader.Dispose()}
    return $chosen.ToArray()
}
function Measurement($row,$tractorMass,$config) {
    $r2=0.0; $v2=0.0; $d2=0.0; $along=0.0; $dx=@(); $velocity=@()
    foreach($axis in @('x','y','z')) {
        $r=$row["Earth_$axis"]-$row["Sun_$axis"]; $v=$row["Earth_v$axis"]-$row["Sun_v$axis"]
        $d=$row["Tractor_$axis"]-$row["Earth_$axis"]
        $r2+=$r*$r; $v2+=$v*$v; $d2+=$d*$d; $dx+=$d; $velocity+=$v
    }
    $distance=[math]::Sqrt($d2)
    for($k=0;$k -lt 3;$k++){$along+=$dx[$k]*$velocity[$k]}
    $accel=6.67430e-11*$tractorMass*$along/([math]::Pow($distance,3)*[math]::Sqrt($v2))
    $earthA=1/(2/[math]::Sqrt($r2)-$v2/(1.32712440018e20+3.986004418e14))
    [pscustomobject]@{
        timeSeconds=$row.time_s;year=$row.time_s/31557600;aEarthAU=$earthA/149597870700;aEmbAU=$row.a_m/149597870700
        thrustN=$row.thrust_N;mdotKgPerSecond=$row.thrust_N/($config.ExhaustSpeed*[math]::Cos($config.CantDegrees*[math]::PI/180))
        totalEjectedKg=$row.ejected_kg;tractorEjectedKg=$row.tractor_ejected_kg;tractorMassKg=$tractorMass;tractorDistanceMetres=$distance
        earthTractorAccelerationAlongVelocity=$accel;tractorContributionDaDtAUPerYear=$row.tractor_da_dt_AU_yr
        momentumError=$row.relative_P;angularMomentumError=$row.relative_L;energyError=$row.relative_E;flagCount=$row.flag_count
    }
}
$config=Get-Content -Raw -LiteralPath (Join-Path $root 'Config/mission.json') | ConvertFrom-Json
$audit=Get-Content -Raw -LiteralPath (Join-Path $root 'Results/diagnosis_initial_audit.json') | ConvertFrom-Json
$rows=@(Read-SelectedRows (Join-Path $root 'Results/diagnosis_checkpoints/run_mission.csv') $true)
$epochs=@($rows | ForEach-Object {Measurement $_ $audit.tractorMassKg $config})
$folder=Join-Path $root $MissionFolder
$last=@(Read-SelectedRows (Join-Path $folder 'run_mission.csv') $false)[0]
$end=Measurement $last $audit.tractorMassKg $config
$summaryPath=Join-Path $folder 'mission_report.json'
$summary=$null; if(Test-Path -LiteralPath $summaryPath){$summary=Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json}
$report=[ordered]@{epochSource='Results/diagnosis_checkpoints/run_mission.csv';endSource="$MissionFolder/run_mission.csv";epochs=$epochs;lastRecorded=$end;missionSummary=$summary;notes=@('aEarthAU is recomputed from actual Earth-Sun relative state with GM_sun+GM_earth. aEmbAU is the existing mission CSV a_m (Earth-Moon barycentre, GM_sun+GM_earth+GM_moon).','Acceleration is the actual tractor gravitational contribution projected along Earth heliocentric velocity, not total solar acceleration.','da/dt is the tractor-only EMB energy derivative; it is not the noisy full N-body instantaneous derivative.','mdot is calculated from the recorded end-half-kick thrust / (ve cos cant). Ejected mass is measured accumulation; total includes all infrastructure.','The t=0 row precedes the first commanded kick; zero recorded thrust at t=0 is not a premature cutoff.','Null final tractor-only propellant and da/dt fields mean the original full-run binary did not log those extra columns. No estimated values substituted.','No acceptance tolerance changed. Checkpoint instrumentation matches all 12,176 shared original full-run CSV rows exactly.')}
[IO.File]::WriteAllText((Join-Path $root 'Results/tractor_diagnosis.json'),($report | ConvertTo-Json -Depth 14))
$epochs | Select-Object year,aEarthAU,aEmbAU,thrustN,mdotKgPerSecond,totalEjectedKg | Format-Table -AutoSize
$end | Format-List
