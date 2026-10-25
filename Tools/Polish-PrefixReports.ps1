$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$utf8=New-Object System.Text.UTF8Encoding($false)
foreach($name in @('Results/STATUS.md','Docs/Selected_Design_Prefix_Report.md')) {
    $path=Join-Path $root $name
    $s=[IO.File]::ReadAllText($path)
    $badDash=[string][char]0xE2+[char]0x20AC+[char]0x201D
    $s=$s.Replace($badDash,'-')
    $s=[regex]::Replace($s,'\*\*Verdict: FAIL .*?StructuralSafety\.\*\*','**Verdict: FAIL - StructuralSafety.**')
    $s=[regex]::Replace($s,'(?<=\d)(?=(?:yr|years|km|MPa|GPa|AU|W/m2|W\/m2|degrees|K|m|s)\b)',' ')
    $s=[regex]::Replace($s,'\b(first|at|through|ratio|inclination|phase|bias|fixed|completed|from|to|with|The|its|not|loaded|remaining|against|within|uses)(?=\d)','$1 ')
    $s=[regex]::Replace($s,'(?<=\d)(?=(?:full|checks|selected|zero|years|degrees|points|bodies|samples|against|after|and|of|recorded|maximum)\b)',' ')
    $s=[regex]::Replace($s,'(?m)^(\| X6\.(?:Yoshida4|VelocityVerlet)\.(?:h|difference)\d+ .*?)\| PASS \|\r?$','$1| info |')
    [IO.File]::WriteAllText($path,$s,$utf8)
}
