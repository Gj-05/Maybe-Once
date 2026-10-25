# Uses the installed Unity compiler and .NET 8 runtime only; no downloads/SDK installation.
param([string]$UnityData='C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Data',[string]$OutputDirectory='Tools/HeadlessBuild')
$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
$outputPath=Join-Path $projectRoot $OutputDirectory
New-Item -ItemType Directory -Force -Path $outputPath | Out-Null
$runtimePath='C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.12'
$arguments=New-Object 'System.Collections.Generic.List[string]'
$arguments.Add('-nostdlib+'); $arguments.Add('-langversion:9.0'); $arguments.Add('-optimize+'); $arguments.Add('-target:exe')
$arguments.Add('-out:"'+(Join-Path $outputPath 'OctoEarth.Cli.dll')+'"')
Get-ChildItem -LiteralPath $runtimePath -Filter '*.dll' | ForEach-Object { try { [Reflection.AssemblyName]::GetAssemblyName($_.FullName) | Out-Null; $arguments.Add('-r:"'+$_.FullName+'"') } catch [System.BadImageFormatException] { } }
Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets/Scripts/Core') -Filter '*.cs' -Recurse | ForEach-Object { $arguments.Add('"'+$_.FullName+'"') }
$arguments.Add('"'+(Join-Path $projectRoot 'Tools/OctoEarth.Cli/Program.cs')+'"')
$response=Join-Path $outputPath 'compile.rsp'
[IO.File]::WriteAllLines($response,$arguments)
& (Join-Path $UnityData 'NetCoreRuntime/dotnet.exe') (Join-Path $UnityData 'DotNetSdkRoslyn/csc.dll') ('@'+$response)
if($LASTEXITCODE -ne 0) { throw 'Headless compilation failed' }
[IO.File]::WriteAllText((Join-Path $outputPath 'OctoEarth.Cli.runtimeconfig.json'),'{"runtimeOptions":{"tfm":"net8.0","framework":{"name":"Microsoft.NETCore.App","version":"8.0.12"}}}')
Write-Output 'Run: dotnet Tools/HeadlessBuild/OctoEarth.Cli.dll bench'
