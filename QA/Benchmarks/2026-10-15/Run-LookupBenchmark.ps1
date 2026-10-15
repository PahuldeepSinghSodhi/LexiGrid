$ErrorActionPreference = 'Stop'
$benchmarkWorkspace = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
Add-Type -Path @(
    (Join-Path $benchmarkWorkspace 'Assets/Scripts/DataStructures/TrieNode.cs'),
    (Join-Path $benchmarkWorkspace 'Assets/Scripts/DataStructures/Trie.cs'),
    (Join-Path $PSScriptRoot 'LookupBenchmark.cs')
) -CompilerOptions '/optimize+'
[LexiGridLookupBenchmark]::Run((Join-Path $benchmarkWorkspace 'Assets/Resources/wordlist.txt')) | ConvertTo-Json -Depth 6
