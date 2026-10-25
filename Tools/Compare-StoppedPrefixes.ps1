$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$culture=[Globalization.CultureInfo]::InvariantCulture
function Read-Prefix([string]$path) {
    $reader=[IO.StreamReader]::new($path); $rows=[Collections.Generic.List[object]]::new(); $lastRecord=$null
    try {
        $header=$reader.ReadLine().Split(','); $index=@{}; for($k=0;$k -lt $header.Length;$k++){$index[$header[$k]]=$k}
        while(-not $reader.EndOfStream) {
            $line=$reader.ReadLine(); $p=$line.Split(','); if($p.Length -ne $header.Length){continue}
            $t=[double]::Parse($p[0],$culture); $moon2=0.0
            foreach($axis in @('x','y','z')){$d=[double]::Parse($p[$index["Moon_$axis"]],$culture)-[double]::Parse($p[$index["Earth_$axis"]],$culture);$moon2+=$d*$d}
            $v=[double[]]@(([double]::Parse($p[1],$culture)/149597870700),[double]::Parse($p[2],$culture),[math]::Sqrt($moon2),[double]::Parse($p[5],$culture),[double]::Parse($p[6],$culture),[double]::Parse($p[3],$culture),[double]::Parse($p[10],$culture),[double]::Parse($p[11],$culture),[double]::Parse($p[12],$culture))
            $rows.Add([pscustomobject]@{t=$t;v=$v;sf=[double]::Parse($p[13],$culture);tide=[double]::Parse($p[14],$culture);bound=[double]::Parse($p[15],$culture);shade=[double]::Parse($p[17],$culture);flags=[int]$p[18];flux=[double]::Parse($p[4],$culture)})
            $lastRecord=$line
        }
    } finally {$reader.Dispose()}
    [pscustomobject]@{header=$header;rows=$rows;lastRecord=$lastRecord}
}
$coarse=Read-Prefix (Join-Path $root 'Results/design_audit/full_h3600/run_mission.csv')
$fine=Read-Prefix (Join-Path $root 'Results/design_audit/full_h1800/run_mission.csv')
$names=@('a_EMB_AU','eccentricity','Earth_Moon_distance_m','thrust_N','ejected_kg','temperature_K','relative_P','relative_L','relative_E')
$max=[double[]]::new(9); $maxTimes=[double[]]::new(9); $correction=[double[]]::new(9); $final=[double[]]::new(9); $finalCoarse=$null; $finalFine=$null
$exact=[Collections.Generic.List[object]]::new();$count=0;$cursor=0;$first=$null
$path=Join-Path $root 'Results/partial_convergence_series.csv'; $writer=[IO.StreamWriter]::new($path)
try {
    $header='time_yr'; foreach($name in $names){$header+=",coarse_$name,fine_$name,difference_$name"}; $writer.WriteLine($header)
    foreach($f in $fine.rows) {
        # Half-step startup at1800s has no completed coarse-step counterpart. Do not
        # interpolate the pre-command zero thrust into a fictitious startup ramp.
        if($f.t -gt 0 -and $f.t -lt 3600){continue}
        while($cursor+1 -lt $coarse.rows.Count -and $coarse.rows[$cursor+1].t -le $f.t){$cursor++}
        $a=$coarse.rows[$cursor]; if($a.t -gt $f.t){continue}
        $b=$a; if($a.t -lt $f.t){if($cursor+1 -ge $coarse.rows.Count){break};$b=$coarse.rows[$cursor+1]}
        $fraction=0.0; if($b.t -ne $a.t){$fraction=($f.t-$a.t)/($b.t-$a.t)}
        $aligned=[double[]]::new(9); $differences=[double[]]::new(9); $values=[Collections.Generic.List[string]]::new();$values.Add(($f.t/31557600).ToString('R',$culture))
        for($k=0;$k -lt 9;$k++){
            $aligned[$k]=$a.v[$k]+($b.v[$k]-$a.v[$k])*$fraction; $delta=$aligned[$k]-$f.v[$k];$differences[$k]=$delta
            if([math]::Abs($delta) -gt $max[$k]){$max[$k]=[math]::Abs($delta);$maxTimes[$k]=$f.t/31557600}
            $near=if($fraction -ge 0.5){$b.v[$k]}else{$a.v[$k]};$correction[$k]=[math]::Max($correction[$k],[math]::Abs($aligned[$k]-$near))
            $values.Add($aligned[$k].ToString('R',$culture));$values.Add($f.v[$k].ToString('R',$culture));$values.Add($delta.ToString('R',$culture))
            $final[$k]=$delta
        }
        $writer.WriteLine([string]::Join(',',$values));$count++;if($null -eq $first){$first=$f.t};$finalCoarse=$aligned;$finalFine=$f.v
        if($a.t -eq $f.t){$exact.Add([pscustomobject]@{timeYear=$f.t/31557600;coarse=$aligned;fine=$f.v;difference=$differences})}
    }
} finally {$writer.Dispose()}
$metrics=@();for($k=0;$k -lt 9;$k++){$metrics+=[pscustomobject]@{metric=$names[$k];maxAbsoluteDifference=$max[$k];maxAtYear=$maxTimes[$k];finalSignedDifference=$final[$k];finalCoarseAligned=$finalCoarse[$k];finalFineRecorded=$finalFine[$k];maxAlignmentCorrectionFromNearestCoarseRecord=$correction[$k]}}
function Prefix-Summary($data,$step) {
    $last=$data.rows[$data.rows.Count-1];$minMoon=[double]::PositiveInfinity;$maxMoon=0.0;$maxTide=0.0;$maxFlux=0.0;$minTemp=[double]::PositiveInfinity;$maxTemp=0.0;$allBound=$true
    foreach($r in $data.rows){$minMoon=[math]::Min($minMoon,$r.v[2]);$maxMoon=[math]::Max($maxMoon,$r.v[2]);$maxTide=[math]::Max($maxTide,$r.tide);$maxFlux=[math]::Max($maxFlux,$r.flux);$minTemp=[math]::Min($minTemp,$r.v[5]);$maxTemp=[math]::Max($maxTemp,$r.v[5]);$allBound=$allBound -and $r.bound -eq 1}
    [pscustomobject]@{stepSeconds=$step;lastPreservedYear=$last.t/31557600;lastSeconds=$last.t;records=$data.rows.Count;aAU=$last.v[0];eccentricity=$last.v[1];moonDistanceFinalMetres=$last.v[2];moonDistanceSampledMinMetres=$minMoon;moonDistanceSampledMaxMetres=$maxMoon;moonBoundCumulative=$allBound;recordedFlagCount=$last.flags;minimumSafetyFactorCumulative=$last.sf;inferredGoverningRockStressPa=5e6/$last.sf;requiredRockStrengthForSF1_5Pa=5e6*1.5/$last.sf;requiredStrengthMultiplier=1.5/$last.sf;sampledMaxTideRatio=$maxTide;sampledMaxFluxWm2=$maxFlux;sampledMinTemperatureK=$minTemp;sampledMaxTemperatureK=$maxTemp;momentumErrorMaxCumulative=$last.v[6];angularErrorMaxCumulative=$last.v[7];energyErrorMaxCumulative=$last.v[8]}
}
$report=[ordered]@{label=('partial-duration convergence (first '+($fine.rows[$fine.rows.Count-1].t/31557600).ToString('F9',$culture)+' years)');completedFullDuration=$false;overlapYears=$fine.rows[$fine.rows.Count-1].t/31557600;alignedRecords=$count;method='Fine recorded timestamps are the matching-time grid. Coarse scalar records are linearly aligned onto that grid; native monthly grids differ by1800s. These are interpolation-based differences, not extra integrations. Exact shared epochs are reported separately. The isolated1800s startup sample is excluded because no full coarse step existed then. Ledger maxima are cumulative sampled quantities; interpolation is diagnostic only.';metrics=$metrics;exactSharedEpochs=$exact.ToArray();runs=@((Prefix-Summary $coarse 3600),(Prefix-Summary $fine 1800));flagNames=@('StructuralSafety','FocusingOpticsUnresolved','MaterialStrengthUnvalidated','ThermalStructuralModelIncomplete')}
[IO.File]::WriteAllText((Join-Path $root 'Results/partial_convergence.json'),($report | ConvertTo-Json -Depth 10))
$checkpoint=[ordered]@{label='Recorded-state reconstruction checkpoint; not a byte-exact solver/ledger restart';source='Results/design_audit/full_h1800/run_mission.csv';timeSeconds=$fine.rows[$fine.rows.Count-1].t;timeYears=$fine.rows[$fine.rows.Count-1].t/31557600;header=$fine.header;originalCompleteRecord=$fine.lastRecord;missing='Unrecorded final internal steps, controller history, Kahan accumulators and full ledger source/sink histories cannot be restored from this row.'}
[IO.File]::WriteAllText((Join-Path $root 'Results/prefix_checkpoint_h1800.json'),($checkpoint | ConvertTo-Json -Depth 5))
$metrics | Select-Object metric,maxAbsoluteDifference,finalSignedDifference | Format-Table -AutoSize
$report.runs | ConvertTo-Json -Depth 5
