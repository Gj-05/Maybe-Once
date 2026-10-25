param([string]$MissionFolder='Results/mission_h3600_y10000')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms.DataVisualization
$root=Split-Path $PSScriptRoot -Parent
$stream=[IO.FileStream]::new((Join-Path $root "$MissionFolder/run_mission.csv"),[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
$reader=[IO.StreamReader]::new($stream)
$chart=New-Object System.Windows.Forms.DataVisualization.Charting.Chart
$chart.Width=1200; $chart.Height=760; $chart.BackColor=[Drawing.Color]::White
$area=New-Object System.Windows.Forms.DataVisualization.Charting.ChartArea
$area.AxisX.Title='Elapsed time (Julian years)'; $area.AxisY.Title='Earth-Moon barycentre semi-major axis (AU)'
$area.AxisX.Minimum=0; $area.AxisY.Minimum=0.9; $area.AxisY.Maximum=3
$area.AxisX.LabelStyle.Format='0'; $area.AxisY.LabelStyle.Format='0.0'
$area.AxisX.MajorGrid.LineColor=[Drawing.Color]::LightGray; $area.AxisY.MajorGrid.LineColor=[Drawing.Color]::LightGray
$chart.ChartAreas.Add($area); $chart.Legends.Add((New-Object System.Windows.Forms.DataVisualization.Charting.Legend))
$series=New-Object System.Windows.Forms.DataVisualization.Charting.Series('Measured full mission CSV')
$series.ChartType=[System.Windows.Forms.DataVisualization.Charting.SeriesChartType]::Line; $series.BorderWidth=3
try {
    $null=$reader.ReadLine(); $lastYear=-2.0; $lastX=0.0; $lastY=1.0
    while(-not $reader.EndOfStream) {
        $p=$reader.ReadLine().Split(','); $x=[double]::Parse($p[0],[Globalization.CultureInfo]::InvariantCulture)/31557600; $y=[double]::Parse($p[1],[Globalization.CultureInfo]::InvariantCulture)/149597870700
        if($x-$lastYear -ge 1){$series.Points.AddXY($x,$y) | Out-Null; $lastYear=$x}; $lastX=$x; $lastY=$y
    }
    if($lastYear -ne $lastX){$series.Points.AddXY($lastX,$lastY) | Out-Null}
} finally {$reader.Dispose()}
$chart.Series.Add($series)
$target=New-Object System.Windows.Forms.DataVisualization.Charting.Series('2.7 AU target reference')
$target.ChartType=[System.Windows.Forms.DataVisualization.Charting.SeriesChartType]::Line; $target.BorderDashStyle='Dash'; $target.BorderWidth=2
$target.Points.AddXY(0,2.7) | Out-Null; $target.Points.AddXY($lastX,2.7) | Out-Null; $chart.Series.Add($target)
$chart.Titles.Add('Measured mission orbit growth - no mission-success inference') | Out-Null
$folder=Join-Path $root 'Results/figures'; New-Item -Path $folder -ItemType Directory -Force | Out-Null
$chart.SaveImage((Join-Path $folder 'mission_diagnosis_a.png'),[System.Windows.Forms.DataVisualization.Charting.ChartImageFormat]::Png); $chart.Dispose()
