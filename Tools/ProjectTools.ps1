param([ValidateSet('Open','Prepare','Build')][string]$Action='Open')
$ErrorActionPreference='Stop'
$campusRoot=[System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$campusEditor='G:\Unity\2022.3.46f1c1\Editor\Unity.exe'
if (-not (Test-Path -LiteralPath $campusEditor)) { throw 'Cannot find the configured Unity 2022.3 editor.' }
if ($Action -eq 'Open') {
    Start-Process -FilePath $campusEditor -ArgumentList @('-projectPath',('"'+$campusRoot+'"'))
} else {
    $campusLogs=Join-Path $campusRoot 'Logs'
    New-Item -ItemType Directory -Path $campusLogs -Force | Out-Null
    $campusMethod=if($Action -eq 'Build'){'CampusEngineer.Editor.CampusBuild.BuildWindows'}else{'CampusEngineer.Editor.CampusBuild.Prepare'}
    & $campusEditor -batchmode -nographics -quit -projectPath $campusRoot -executeMethod $campusMethod -logFile (Join-Path $campusLogs ($Action+'.log'))
    exit $LASTEXITCODE
}
