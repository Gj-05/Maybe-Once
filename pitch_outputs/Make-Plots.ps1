$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms.DataVisualization
$out=$PSScriptRoot
function Plot($title,$xlabel,$ylabel,$curves,$name) {
 $chart=New-Object System.Windows.Forms.DataVisualization.Charting.Chart
 $chart.Width=1400; $chart.Height=850; $chart.BackColor=[Drawing.Color]::White
 $area=New-Object System.Windows.Forms.DataVisualization.Charting.ChartArea
 $area.AxisX.Title=$xlabel; $area.AxisY.Title=$ylabel
 $area.AxisX.IsStartedFromZero=$false; $area.AxisY.IsStartedFromZero=$false
 $area.AxisX.LabelStyle.Format='0.##'; $area.AxisY.LabelStyle.Format='0.####E+0'
 $area.AxisX.MajorGrid.LineColor=[Drawing.Color]::LightGray; $area.AxisY.MajorGrid.LineColor=[Drawing.Color]::LightGray
 $chart.ChartAreas.Add($area); $chart.Titles.Add($title) | Out-Null
 $chart.Legends.Add((New-Object System.Windows.Forms.DataVisualization.Charting.Legend))
 foreach($curve in $curves) {
  $series=New-Object System.Windows.Forms.DataVisualization.Charting.Series($curve.label)
  $series.ChartType='Line'; $series.BorderWidth=3
  $stride=[Math]::Max(1,[int]($curve.rows.Count/3000))
  for($i=0;$i -lt $curve.rows.Count;$i+=$stride) {$r=$curve.rows[$i]; $series.Points.AddXY([double]$r.($curve.x),[double]$r.($curve.y)) | Out-Null}
  $r=$curve.rows[-1]; $series.Points.AddXY([double]$r.($curve.x),[double]$r.($curve.y)) | Out-Null
  $chart.Series.Add($series)
 }
 $chart.SaveImage((Join-Path $out $name),'Png'); $chart.Dispose()
}
$coarse=@(Import-Csv (Join-Path $out selected_h3600_charts.csv))
$fine=@(Import-Csv (Join-Path $out selected_h1800_charts.csv))
$old=@(Import-Csv (Join-Path $out original_full_charts.csv))
Plot 'Selected design: stopped prefixes; target arrival NOT OBSERVED' 'Time (Julian yr)' 'Semi-major axis (AU)' @(@{label='Selected h=3600 s';rows=$coarse;x='time_yr';y='a_AU'},@{label='Selected h=1800 s';rows=$fine;x='time_yr';y='a_AU'}) 'selected_orbit.png'
Plot 'Original layout ONLY: completed run; safety and structure FAIL' 'Time (Julian yr)' 'Semi-major axis (AU)' @(@{label='Original fixed array / parking';rows=$old;x='time_yr';y='a_AU'}) 'original_full_orbit.png'
Plot 'Selected stopped prefix: cumulative ledger errors' 'Time (Julian yr)' 'Relative error (dimensionless)' @(@{label='P';rows=$coarse;x='time_yr';y='relative_P'},@{label='L';rows=$coarse;x='time_yr';y='relative_L'},@{label='E';rows=$coarse;x='time_yr';y='relative_E'}) 'selected_ledger.png'
Plot 'Selected stopped prefix: ejected reaction mass' 'Time (Julian yr)' 'Ejected mass (kg)' @(@{label='Selected h=3600 s';rows=$coarse;x='time_yr';y='ejected_kg'}) 'selected_propellant.png'
Plot 'Selected stopped prefix: collector build-up' 'Time (Julian yr)' 'Collector area (m2)' @(@{label='Selected h=3600 s';rows=$coarse;x='time_yr';y='array_area_m2'}) 'selected_array.png'
Plot 'Selected stopped prefix: simplified global EBM' 'Time (Julian yr)' 'Global temperature (K)' @(@{label='Selected h=3600 s';rows=$coarse;x='time_yr';y='temperature_K'}) 'selected_temperature.png'
$aligned=@(Import-Csv (Join-Path (Split-Path $out -Parent) Results/partial_convergence_series.csv))
$aligned[0].PSObject.Properties.Name | Set-Content (Join-Path $out convergence_columns.txt)
$deltaColumn=($aligned[0].PSObject.Properties.Name | Where-Object {$_ -match 'a.*diff|delta.*a|difference.*a'} | Select-Object -First 1)
$timeColumn=($aligned[0].PSObject.Properties.Name | Where-Object {$_ -match 'year|time_yr'} | Select-Object -First 1)
if($deltaColumn -and $timeColumn) {Plot 'Partial-duration interpolation-based convergence ONLY' 'Time (Julian yr)' 'Coarse minus fine a (AU)' @(@{label='Aligned h minus h/2';rows=$aligned;x=$timeColumn;y=$deltaColumn}) 'partial_convergence.png'}
