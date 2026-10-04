# Current verification: 4 October 2026

This records the initial interface/run check earlier that day. The later selection/combo update expanded the suites to **62 C# cases and 17 browser-engine tests**; see [SelectionComboRegression.md](SelectionComboRegression.md). The initial 57/11 results below are retained as the earlier snapshot, not the latest totals.

| Check | Result | Scope |
|---|---|---|
| Original C# tests and three new board-fallback tests | 57 passed, 0 failed, 0 skipped | NUnit directly through Mono; no Unity Engine scene required |
| Runtime and editor compilation | Passed | Installed Unity DLLs; compiler warnings for inspector-assigned fields remain |
| Browser engine suite | 11 passed, 0 failed | Trie, score thresholds, MinHeap, adjacency, generation, all 358,612 dictionary words |
| Valid word click selection | DATE accepted for 400 points | Slow-word base score |
| Drag-to-submit | EAT accepted for 100 points | Pointer drag through adjacent tiles |
| Duplicate word | EAT rejected on second submission; total stayed 500 | Per-round duplicate prevention |
| Timer expiry | Round ended and results displayed 500 / 2 words | Preview runtime |
| Score persistence | 500 remained in leaderboard after page reload | Browser localStorage |
| Phone layout | 390 x 844 viewport; 71.65 x 71.65 tiles; no horizontal overflow | Browser preview |
| Small phone, 5 x 5 board | 320 x 800 viewport; 25 square tiles, 45.04 pixels each; no horizontal overflow | Final browser styles |
| Settings persistence | 120-second preference survived reload; restored defaults to 4 x 4 / 60 seconds for handoff | Browser localStorage |
| Browser error/warning log | Empty during gameplay checks | Browser preview |
| Unity batch build | Blocked: no active Editor licence, exit 198 | No native scene or Windows executable produced |

Results XML is at `Builds/CoreTests/results.xml`; Unity activation diagnostics are at `Builds/Logs/build.log`.

The browser preview is a separate implementation of the C# rules, not a Unity export. Native UI rendering and scene wiring remain unverified until the Unity licence is active.
