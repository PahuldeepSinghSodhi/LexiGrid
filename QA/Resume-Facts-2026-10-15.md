# LexiGrid: verified facts for resume generation
Date: 15 October 2026

Use this document to write concise, defensible resume bullets. Do not invent additional percentages, timings, coverage, users, deployment reach or impact. This publication adds benchmark evidence and documentation; it does not modify gameplay or publish the separate Unity website migration.

## Project and implemented engineering
- Unity/C# timed single-player word-grid puzzle with a bundled dictionary of 358,612 words.
- Custom Trie supports case-insensitive exact membership and prefix checks. Expected lookup complexity is O(L) in query length, assuming expected constant-time child-dictionary access.
- Weighted letter generation and DFS with Trie-prefix pruning validate boards.
- Three supported board sizes: 3x3, 4x4, 5x5; four durations: 30, 60, 90, 120 seconds.
- After up to 100 random generation attempts, a fitting dictionary word is placed along a neighbouring path. This guarantees at least one word for a compatible dictionary, not balanced difficulty or a measured 100% usability score.
- Generic MinHeap retains a top-10 leaderboard. Insert/eviction operations are O(log K), where K is the retained heap size. Minimum lookup is O(1); descending display is O(K log K). No measured heap-versus-sort speedup is claimed.
- Per-round duplicate prevention, selection undo/retracing/clear, speed-based scoring and a separate +2-second bonus for non-overlapping accepted-word pairs under five seconds.
- JSON leaderboard persistence and PlayerPrefs settings. In the current local Unity Web build, leaderboard JSON uses browser-backed PlayerPrefs. This benchmark commit is not evidence of a fresh browser deployment.

## Newly measured lookup benchmark
Benchmark source, runner, methodology and raw samples: QA/Benchmarks/2026-10-15/.

Environment: Intel Core i7-13700HX, PowerShell 7.6.5, .NET 10.0.11; optimized C# compiled alongside the unchanged project Trie/TrieNode. Two fresh processes, five measured rounds per process after warm-up.

Each exact/prefix workload has 1,024 seeded uppercase queries with 512 hits and 512 misses. Exact hits sample dictionary words; misses are one-character mutations verified absent. Prefix queries use lengths 3 through min(8, source-word length). Query order is shuffled; queries may repeat and are synthetic, not player telemetry.

Timings are median batch times for 1,024 lookups, NOT individual-query percentiles:
| Lookup | Process 1 (ms) | Process 2 (ms) |
|---|---:|---:|
| Exact linear scan | 684.555800 | 809.230100 |
| Exact Trie | 0.240695 | 0.261033 |
| Exact HashSet | 0.047086 | 0.052006 |
| Prefix linear scan | 610.084000 | 782.545500 |
| Prefix Trie | 0.087881 | 0.143944 |

Observed Trie-versus-linear ratios:
- Exact: 2,844-3,100x speedup; 99.9648-99.9677% lower elapsed lookup time.
- Prefix: 5,436-6,942x speedup; 99.9816-99.9856% lower elapsed lookup time.
- Conservative resume shorthand: ">99.9% lower lookup time than linear scans in controlled C#/.NET microbenchmarks."

Limitations that must remain attached to the performance claim:
- This is a warmed synthetic microbenchmark, not Unity Mono, IL2CPP/WebAssembly, mobile, rendering, startup or end-to-end input latency.
- Fast methods repeat each fixed query batch 100 times and divide elapsed time by 100; linear baselines execute one batch per sample. Repetition and cache effects apply.
- The baseline is an early-exit linear scan with ordinal comparisons and normalization once per query. It is not a measured earlier brute-force version of the game.
- Do not claim the whole game is 99.9% faster, or that LexiGrid outperforms unrelated projects.
- HashSet was approximately 5x faster than Trie for exact membership. Trie is justified by direct prefix support and DFS pruning, not universal superiority over dictionary structures.
- Building the index is excluded from lookup timings; this is not a memory-efficiency comparison.
- All compared results agreed on the benchmark workload. Do not relabel this as universal 100% validation accuracy.

## QA evidence and boundaries
- Fresh pure C# NUnit/Mono run on 15 October 2026: 62 passed, 0 failed, 0 skipped.
- The 62 cases comprise 54 original cases, 3 board-fallback cases and 5 combo cases. They cover Trie, MinHeap, board generation, scoring and combo boundaries.
- This is NOT execution by Unity's Editor test runner and NOT a full automated UI/Play Mode suite.
- A 25-case manual test plan exists, but its recorded statuses are all "Not Run." Say "designed a 25-case test plan," not "executed all 25."
- A five-bug root-cause log and an 11-case regression report exist as historical documents. Do not present them as newly executed evidence.
- Three additional HTTP/routing cases passed against the current local Unity website. That web-server migration/suite is outside this findings-only GitHub publication; do not combine these into a claimed 65 NUnit tests.
- Test counts/pass rates are not code coverage percentages or proof of zero defects.

## Suggested resume bullets
1. Implemented Trie-based word/prefix validation over a 358K-word dictionary; measured >99.9% lower lookup time than linear scans in controlled C#/.NET microbenchmarks.
2. Built DFS board validation with Trie-prefix pruning and guaranteed-word fallback, plus a top-10 MinHeap leaderboard with O(log K) insertion/eviction.
3. Verified core algorithms and scoring/combo boundaries with 62 passing NUnit tests; designed a 25-case manual test plan and documented root-cause/regression artifacts.

Keep only the strongest two or three bullets. Do not claim percentages without the baseline and benchmark qualification. Do not claim public/global adoption, online shared scores, comprehensive platform coverage or performance gains for features that were not measured.
