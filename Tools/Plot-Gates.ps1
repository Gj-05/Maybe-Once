# Reproducible static scientific plots using the installed .NET chart library; no graphics engine.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms.DataVisualization
$root=Split-Path $PSScriptRoot -Parent
$plotFolder=Join-Path $root 'Results/figures'
New-Item -ItemType Directory -Force -Path $plotFolder | Out-Null
function Export-GatePlot($title,$xLabel,$yLabel,$curves,$fileName) {
    $chart=New-Object System.Windows.Forms.DataVisualization.Charting.Chart
    $chart.Width=1200; $chart.Height=760; $chart.BackColor=[Drawing.Color]::White
    $area=New-Object System.Windows.Forms.DataVisualization.Charting.ChartArea
    $area.AxisX.Title=$xLabel; $area.AxisY.Title=$yLabel
    $area.AxisX.MajorGrid.LineColor=[Drawing.Color]::LightGray
    $area.AxisY.MajorGrid.LineColor=[Drawing.Color]::LightGray
    $area.AxisX.IsStartedFromZero=$false; $area.AxisY.IsStartedFromZero=$false
    $area.AxisX.Minimum=0; $area.AxisX.LabelStyle.Format='0.##'; $area.AxisY.LabelStyle.Format='0.####'
    $area.AxisX.LabelStyle.Font=New-Object Drawing.Font('Segoe UI',10)
    $area.AxisY.LabelStyle.Font=New-Object Drawing.Font('Segoe UI',10)
    $area.AxisX.TitleFont=New-Object Drawing.Font('Segoe UI',12)
    $area.AxisY.TitleFont=New-Object Drawing.Font('Segoe UI',12)
    $chart.ChartAreas.Add($area); $chart.Titles.Add($title) | Out-Null
    $legend=New-Object System.Windows.Forms.DataVisualization.Charting.Legend; $chart.Legends.Add($legend)
    foreach($curve in $curves) {
        $series=New-Object System.Windows.Forms.DataVisualization.Charting.Series($curve.name)
        $series.ChartType=[System.Windows.Forms.DataVisualization.Charting.SeriesChartType]::Line
        $series.BorderWidth=3
        foreach($point in $curve.points) { $series.Points.AddXY([double]$point.x,[double]$point.y) | Out-Null }
        $chart.Series.Add($series)
    }
    $chart.SaveImage((Join-Path $plotFolder $fileName),[System.Windows.Forms.DataVisualization.Charting.ChartImageFormat]::Png)
    $chart.Dispose()
}
$hoverCurves=@()
foreach($step in @(60,30)) {
    $path=Join-Path $root "Results/g1_hover_h$step.csv"
    if(Test-Path -LiteralPath $path) {
        $points=@(Import-Csv -LiteralPath $path | ForEach-Object { @{x=[double]$_.time_s/86400; y=[double]$_.relative_distance_error} })
        $hoverCurves+=@{name="dt=$step s";points=$points}
    }
}
$hoverCurves+=@{name='G1 limit';points=@(@{x=0;y=0.001},@{x=30;y=0.001})}
Export-GatePlot 'G1: measured dynamic hover tracking' 'Time (days)' 'Maximum relative separation error' $hoverCurves 'g1_hover.png'
$comparisonCurves=@()
foreach($entry in @(@{folder='Results';step=300},@{folder='Results/g2_h150';step=150})) {
    foreach($mode in @('a','b')) {
        $path=Join-Path $root ($entry.folder+"/g2_mode_$mode.csv")
        if(Test-Path -LiteralPath $path) {
            $points=@(Import-Csv -LiteralPath $path | ForEach-Object { @{x=[double]$_.time_s/31557600; y=[double]$_.delivered_prograde_dv_m_s} })
            $comparisonCurves+=@{name="Mode $mode, dt=$($entry.step) s";points=$points}
        }
    }
}
if($comparisonCurves.Count -gt 0) { Export-GatePlot 'G2: integrated prograde gravitational acceleration' 'Time (Julian years)' 'Delivered prograde delta-v (m/s)' $comparisonCurves 'g2_delta_v.png' }
$spiralCurves=@()
foreach($step in @(3600,1800,900)) {
    $path=Join-Path $root "Results/g3_h$step/spiral.csv"
    if((Test-Path -LiteralPath $path) -and (Test-Path -LiteralPath (Join-Path $root "Results/g3_h$step/g3_results.json"))) {
        $rows=@(Import-Csv -LiteralPath $path)
        if($rows.Count -gt 1) {
            $spiralCurves+=@{name="Computed, dt=$step s";points=@($rows | ForEach-Object { @{x=[double]$_.time_s/31557600;y=[double]$_.a_m/149597870700} })}
            if($spiralCurves.Count -eq 1) { $spiralCurves+=@{name='Analytic slow spiral';points=@($rows | ForEach-Object { @{x=[double]$_.time_s/31557600;y=[double]$_.analytic_a_m/149597870700} })} }
        }
    }
}
if($spiralCurves.Count -gt 0) { Export-GatePlot 'G3: mechanical spiral (explicit toy power input)' 'Time (Julian years)' 'Semi-major axis (AU)' $spiralCurves 'g3_spiral.png' }
