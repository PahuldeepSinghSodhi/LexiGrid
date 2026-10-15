# LexiGrid lookup benchmark — 15 October 2026

## Scope

Controlled synthetic microbenchmark of the unchanged canonical C# Trie and TrieNode sources. Executed on an Intel Core i7-13700HX using PowerShell 7.6.5 / .NET 10.0.11, optimized compilation. This is NOT a Unity Mono, IL2CPP/WebAssembly, mobile-device, startup, frame-rate, or end-to-end input-latency benchmark. No gameplay code was modified.

## Workload and baseline

- Bundled dictionary: 358,612 words. SHA-256 hashes for the dictionary and source files are in results.json.
- Seed: 20261015. Each of the exact and prefix workloads contains 1,024 uppercase queries: 512 successful and 512 unsuccessful. Prefix hits have lengths from 3 through min(8, source-word length). Exact query mean length: 9.1465; prefix mean length: 5.2871.
- Successful exact queries sample dictionary words uniformly. Negative queries mutate one character of the corresponding successful query, retrying until the lookup is unsuccessful. Queries are shuffled; they are not guaranteed unique and are not captured player traffic.
- Linear baselines scan the same array in original dictionary order with early exit. Each query is lowercased once, then uses ordinal Equals or StartsWith. Trie methods use their existing normalization. HashSet uses ordinal membership after the same normalization.
- Correctness: compared all baseline/Trie/HashSet exact results and all baseline/Trie prefix results before timing. Every batch returned exactly 512 hits. Agreement on this workload is not a universal accuracy claim.
- One full warm-up per method, then five measured rounds, in each of two fresh processes. Measurement order reverses on alternating rounds.
- Each fast-method sample repeats the fixed query batch 100 times and divides elapsed time by 100. Each linear sample executes one batch. Repeated-query/cache and runtime effects apply. These are warm steady-state comparisons, not cold-query latency claims.
- Summaries use the median of five batch timings within each process. Per-query averages derived from these batch medians are not individual-query latency percentiles.

## Observed results

Milliseconds per 1,024-query batch, median of five rounds:

| Lookup | Process 1 | Process 2 |
|---|---:|---:|
| Exact, linear scan | 684.555800 | 809.230100 |
| Exact, Trie | 0.240695 | 0.261033 |
| Exact, HashSet | 0.047086 | 0.052006 |
| Prefix, linear scan | 610.084000 | 782.545500 |
| Prefix, Trie | 0.087881 | 0.143944 |

Exact Trie speedup over the linear baseline: 2,844–3,100x; elapsed-time reduction: 99.9648–99.9677%.

Prefix Trie speedup over the linear baseline: 5,436–6,942x; elapsed-time reduction: 99.9816–99.9856%.

HashSet was approximately 5x faster than Trie for exact membership. This comparison does not establish Trie superiority over all dictionary structures. Trie directly supports prefix checks; a plain word HashSet does not provide the same prefix index.

Trie construction took 610.5–641.9 ms; HashSet construction took 12.6–13.8 ms. These are only two single build observations, not startup benchmarks or robust construction-time estimates. Lookup timings exclude construction.

## Interpretation and resume wording

Safe: "Benchmarked Trie-based word/prefix validation over a 358K-word dictionary, measuring over 99.9% lower lookup time than linear scans in a controlled C#/.NET microbenchmark."

This is a comparison with an explicitly implemented baseline, NOT a measured before/after change to a formerly brute-force game. It does not mean the game, website, startup, or all other projects became 99.9% faster. Hardware/runtime, workload mix, string lengths, dictionary ordering and cache behavior affect the numbers. A HashSet is a strong exact-match alternative; sorted arrays, prefix sets and compact tries would be useful additional baselines.

Fresh checks on 15 October 2026: pure C# NUnit/Mono suite 62 passed, 0 failed/skipped; web HTTP/routing suite 3 passed. These are not coverage percentages or exhaustive Unity UI tests. The 25-case manual plan still records Not Run; its existence does not prove execution. The 11-case regression report is a historical document, not a newly executed 11-case UI suite.

## Reproduce

From a fresh PowerShell 7 process at the workspace root:

```powershell
pwsh -NoProfile -File ./QA/Benchmarks/2026-10-15/Run-LookupBenchmark.ps1
```

Raw samples and calculations are preserved in results.json. The tracked publication is under QA/Benchmarks/2026-10-15; generated builds and Unity caches remain excluded. The benchmark uses PowerShell 7 (observed version 7.6.5); Windows PowerShell 5.1 is not the measured runtime.

The recorded source/dictionary hashes identify raw local files; Git line-ending conversion can change a file hash without changing the C# logic. CPU/runtime details and all raw timing samples are retained for interpretation.

The 3 HTTP/routing cases above were executed against the current local Unity website. This findings-only publication does not add that website migration or its HTTP suite to the repository. The 62-case pure C# suite and benchmark sources are reproducible from the repository.
