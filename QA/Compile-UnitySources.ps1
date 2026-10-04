param([string]$UnityData = 'C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Data')
$ErrorActionPreference = 'Stop'
$workspace = Split-Path $PSScriptRoot -Parent
$destination = Join-Path $workspace 'Builds\SourceCheck'
New-Item -ItemType Directory -Force -Path $destination | Out-Null
$mono = Join-Path $UnityData 'MonoBleedingEdge\bin\mono.exe'
$compiler = Join-Path $UnityData 'MonoBleedingEdge\lib\mono\4.5\csc.exe'
$cached = Join-Path $UnityData 'Resources\PackageManager\ProjectTemplates\libcache\com.unity.template.2d-cross-platform-2d-6.1.5\ScriptAssemblies'
$references = @(Get-ChildItem (Join-Path $UnityData 'Managed\UnityEngine') -Filter 'UnityEngine*.dll' | ForEach-Object { '-r:' + $_.FullName })
$references += @('-r:' + (Join-Path $UnityData 'MonoBleedingEdge\lib\mono\4.5\Facades\netstandard.dll'))
$references += @(('-r:' + (Join-Path $cached 'UnityEngine.UI.dll')), ('-r:' + (Join-Path $cached 'Unity.TextMeshPro.dll')))
$runtimeFiles = @(Get-ChildItem (Join-Path $workspace 'Assets\Scripts') -Recurse -Filter '*.cs' | ForEach-Object FullName)
& $mono $compiler -nologo -target:library ("-out:" + (Join-Path $destination 'LexiGrid.Runtime.dll')) $references $runtimeFiles
if ($LASTEXITCODE -ne 0) { throw 'Unity runtime source compilation failed.' }
$editorReferences = $references + @(Get-ChildItem (Join-Path $UnityData 'Managed') -Filter 'UnityEditor*.dll' | ForEach-Object { '-r:' + $_.FullName })
$editorReferences += '-r:' + (Join-Path $destination 'LexiGrid.Runtime.dll')
$editorFiles = @(Get-ChildItem (Join-Path $workspace 'Assets\Editor') -Filter '*.cs' | ForEach-Object FullName)
& $mono $compiler -nologo -target:library ("-out:" + (Join-Path $destination 'LexiGrid.Editor.dll')) $editorReferences $editorFiles
if ($LASTEXITCODE -ne 0) { throw 'Unity editor source compilation failed.' }
Write-Host 'Unity runtime and editor source compile successfully against installed Unity assemblies. This is a source check, not a Unity player build or scene test.'
