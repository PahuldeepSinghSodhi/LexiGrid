param([switch]$Unity, [switch]$Build, [switch]$NoBrowser, [int]$Port = 8765)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$editorPath = 'C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe'
if ($Unity -or $Build) {
    if (!(Test-Path -LiteralPath $editorPath)) { throw 'Install Unity 6000.5.10f1 using Unity Hub first.' }
    $stagedAssets = Join-Path $projectRoot 'UnityProject\Assets'
    New-Item -ItemType Directory -Force -Path $stagedAssets | Out-Null
    foreach ($folder in @('Scripts', 'Tests', 'Resources', 'Editor')) {
        Copy-Item -LiteralPath (Join-Path $projectRoot "Assets\$folder") -Destination $stagedAssets -Recurse -Force
    }
    $unityArguments = @('-projectPath', ('"' + (Join-Path $projectRoot 'UnityProject') + '"'))
    if ($Build) {
        $buildLogs = Join-Path $projectRoot 'Builds\Logs'
        New-Item -ItemType Directory -Force -Path $buildLogs | Out-Null
        $unityArguments += @('-batchmode', '-quit', '-executeMethod', 'LexiGrid.Editor.LexiGridBuild.BuildWindows', '-logFile', ('"' + (Join-Path $buildLogs 'build.log') + '"'))
        $buildProcess = Start-Process -FilePath $editorPath -ArgumentList $unityArguments -WindowStyle Hidden -PassThru -Wait
        if ($buildProcess.ExitCode -ne 0) { throw "Unity build failed (exit $($buildProcess.ExitCode)). Check Builds\Logs\build.log; an active Unity licence is required." }
        Start-Process -FilePath (Join-Path $projectRoot 'Builds\Windows\LexiGrid.exe')
    } else { Start-Process -FilePath $editorPath -ArgumentList $unityArguments }
    exit
}
$nodeCommand = Get-Command node -ErrorAction SilentlyContinue
$nodePath = if ($nodeCommand) { $nodeCommand.Source } else { Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe' }
if (!(Test-Path -LiteralPath $nodePath)) { throw 'Node.js is required. Install Node.js or run this from the provided environment.' }
$url = "http://127.0.0.1:$Port"
$running = $false
try { $running = (Invoke-RestMethod "$url/health" -TimeoutSec 2).app -eq 'LexiGrid preview' } catch {}
if (!$running) {
    $logs = Join-Path $projectRoot 'Builds\Logs'
    New-Item -ItemType Directory -Force -Path $logs | Out-Null
    Start-Process -FilePath $nodePath -ArgumentList @(('"' + (Join-Path $projectRoot 'Play\server.mjs') + '"'), "$Port") -WindowStyle Hidden -RedirectStandardOutput (Join-Path $logs 'preview.log') -RedirectStandardError (Join-Path $logs 'preview-errors.log') | Out-Null
    for ($attempt = 0; $attempt -lt 15; $attempt++) {
        Start-Sleep -Milliseconds 200
        try { $running = (Invoke-RestMethod "$url/health" -TimeoutSec 1).app -eq 'LexiGrid preview'; if ($running) { break } } catch {}
    }
    if (!$running) { throw "Cannot start the game at $url. Check preview-errors.log or choose another -Port." }
}
Write-Host "LexiGrid is ready: $url"
if (!$NoBrowser) { Start-Process $url }
