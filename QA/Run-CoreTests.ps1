$ErrorActionPreference = 'Stop'
$workspace = Split-Path $PSScriptRoot -Parent
$unityData = 'C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Data'
$mono = Join-Path $unityData 'MonoBleedingEdge\bin\mono.exe'
$compiler = Join-Path $unityData 'MonoBleedingEdge\lib\mono\4.5\csc.exe'
$nunit = Join-Path $unityData 'Resources\PackageManager\BuiltInPackages\com.unity.ext.nunit\net472\unity-custom\nunit.framework.dll'
$output = Join-Path $workspace 'Builds\CoreTests'
New-Item -ItemType Directory -Force -Path $output | Out-Null
Copy-Item -LiteralPath $nunit -Destination $output -Force
$sources = @(Get-ChildItem (Join-Path $workspace 'Assets\Scripts\DataStructures') -Filter '*.cs' | ForEach-Object FullName)
foreach ($file in @('BoardGenerator.cs', 'LetterFrequency.cs', 'ScoringEngine.cs', 'ComboTracker.cs')) { $sources += Join-Path $workspace "Assets\Scripts\Core\$file" }
$sources += @(Get-ChildItem (Join-Path $workspace 'Assets\Tests\EditMode') -Filter '*.cs' | ForEach-Object FullName)
$sources += Join-Path $PSScriptRoot 'NUnitRunner.cs'
& $mono $compiler -nologo -target:exe ("-out:" + (Join-Path $output 'CoreTests.exe')) ("-r:" + $nunit) $sources
if ($LASTEXITCODE -ne 0) { throw 'Core tests failed to compile.' }
& $mono (Join-Path $output 'CoreTests.exe') (Join-Path $output 'results.xml')
if ($LASTEXITCODE -ne 0) { throw 'Core tests failed.' }
