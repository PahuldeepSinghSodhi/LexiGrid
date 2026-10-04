# Project updates — 4 October 2026

This describes the changes added during the interface, run-environment and selection/combo work. The existing Unity game architecture, dictionary, score bands and speed multipliers remain the baseline. The browser preview is a separate JavaScript implementation, not an exported Unity player.

## 1. Playable browser environment

Added `Play/index.html`, `style.css`, `game.mjs`, `engine.mjs`, `dictionary-worker.mjs` and `server.mjs`.

- A localhost-only Node server serves an explicit allowlist of game files and the existing dictionary. It provides a health endpoint, no-cache responses and basic content/security headers.
- The full 358,612-word dictionary loads in a Web Worker with visible progress. Compact typed-array Trie data is transferred to the main thread, and Play is enabled only after loading.
- Menu, loading feedback, game, settings, instructions, results and leaderboard screens.
- Grid sizes 3x3, 4x4 and 5x5; round lengths 30, 60, 90 and 120 seconds.
- Live prefix feedback, drawn selection path, accepted-word history, score, countdown and timer bar.
- Top-ten local leaderboard and remembered settings via browser localStorage. These are separate from native JSON/PlayerPrefs.
- Existing C# rule equivalents: weighted letters, DFS/Trie solvability checks, guaranteed fallback, length/speed scoring and MinHeap retention.
- Browser error/loading feedback and invalid/duplicate submission messages.

## 2. Interface and readability

- Responsive browser layout: square tiles, clearer typography, dark panels, mint selection and distinct invalid states. Game details stack below the board on narrow screens.
- Browser keyboard focus styles, accessible letter labels, selection state, live feedback, reduced-motion handling and large tile targets.
- Unity `ResponsiveInterface.cs` reflows the generated board/sidebar/path/controls for desktop and tall windows without stretching tiles.
- Native Canvas Scaler uses Scale With Screen Size; text uses TextMesh Pro; the new builder creates rounded panel artwork and readable letter prefabs.
- Updated native tile palette and HUD validity/timer colours.

Browser layout was exercised at phone widths. Native readability/layout and physical touchscreen behavior are not yet verified.

## 3. Selection correction and stuck-input recovery

Changed browser `game.mjs`/`engine.mjs` and native `InputManager.cs`.

- Tap a selected letter: remove it and every later letter.
- Drag back onto an earlier selected letter: remove later letters but retain that anchor, allowing another continuation without tile reuse.
- Tap an unconnected new letter: begin a fresh path. Unconnected movement while dragging remains ignored.
- Explicit Submit, Undo last letter and Clear controls. Browser Enter submits, Backspace undoes and Escape clears; Space activates a focused tile.
- Invalid and too-short submissions release/clear selection before displaying rejection. Duplicate words do not score.
- Browser pointer release is handled at window level. Pointer cancellation/lost capture, focus loss and page hiding clean up an interrupted gesture.
- Native disable/focus/pause cancellation resets held input; public Submit/Undo/Clear methods are wired by the new scene builder.

The reported invalid TADG screenshot established an unusable selection, not proof of a frozen countdown. Source inspection found that selected tiles were previously ignored, making retracing impossible. The subsequent browser checks confirmed correction and continued play after rejection.

## 4. Two-word timer combos

Added native `ComboTracker.cs` and its browser equivalent.

- A pair of distinct, accepted words submitted less than five seconds apart earns exactly two additional seconds.
- Pairs do not overlap: words 1+2 can earn one bonus, then words 3+4 can earn another.
- Invalid, too-short or duplicate submissions reset the pending pair. Every new round resets it too.
- Expired rounds cannot earn a bonus or be revived.
- The browser immediately updates countdown/feedback, shows pending/earned combo status and clamps the timer bar to 100% when extra time exceeds the original duration.
- Existing scoring bands and speed multipliers are unchanged; timer bonuses are additional gameplay behavior.

This is the concrete interpretation of the requested back-to-back-word pattern; no geometric pattern detector was added.

## 5. Native reliability fixes

- `GridView.cs`: reuse existing pooled tile objects, deactivate leftovers when changing size, and calculate square cell sizes from the available layout space instead of accumulating another grid each round.
- `GameManager.cs`: wire live path/reset/rejection events to the HUD, guard against an unloaded/empty dictionary, avoid repeated submission subscriptions, clean up subscriptions and selections, show duplicate feedback and reject scoring once time is exhausted.
- Personal-best detection compares against the previous highest score instead of labelling every leaderboard insertion a new record. Zero-score rounds are not inserted.
- `HUDView.cs`: clamp and round the countdown consistently, show a default selection instruction and expose feedback messages.
- `ScreenManager.cs`: make fade-in visible from alpha zero and hide stray screens after interrupted transitions.
- `Trie.cs`/`BoardGenerator.cs`: find a fitting dictionary word and place it along a neighbouring snake path if random attempts fail. Empty or incompatible dictionaries produce explicit errors instead of an unsolvable fallback board.

## 6. Unity setup and launch tools

- `Start-Game.ps1`: start the local preview, optionally skip opening a browser, choose another port, or launch/build Unity.
- `UnityProject/Packages/manifest.json` and `ProjectSettings/ProjectVersion.txt`: minimal staging configuration for installed Unity 6000.5.10f1, uGUI and testing dependencies.
- `Assets/Editor/LexiGridBuild.cs`: reproducibly create a separately named scene, wire seven screens, configure HUD/input/leaderboard references, import TMP essentials, create rounded artwork and prefabs, and build a resizable Windows player after licence activation.
- Runtime, editor and Edit Mode assembly definitions isolate the new runtime/test/editor code.
- Staged `UnityProject/Assets` are copied from canonical root sources by the launcher, not maintained as a second tracked source tree.
- Existing GitHub scenes, `AutoBuilder.cs`, unrelated assets and existing `.meta` identifiers are preserved. The new builder does not replace the legacy scene in this commit.

## 7. Tests, evidence and documentation

- `Play/engine.test.mjs`: 17 automated tests for selection suffix removal, retracing/correction, adjacency/restart, combo pairing/boundary/reset/expiry, Trie transfer/case, scoring, heap retention, solver and dictionary loading across all board sizes.
- Added three NUnit board-fallback cases and five combo cases. Combined with the original 54, the pure C# suite now executes 62 cases.
- `QA/Run-CoreTests.ps1` and `QA/NUnitRunner.cs`: compile/run pure C# tests using NUnit/Mono and write local results XML.
- `QA/Compile-UnitySources.ps1`: compile runtime/editor code against installed Unity assemblies without claiming a player build or scene execution.
- `QA/Verification-2026-10-04.md`: initial interface/run checks with explicit scope and licence blocker.
- `QA/SelectionComboRegression.md`: latest selection/combo checks, proof image and outstanding physical-device/native checks.
- Historical test plan, five-bug log and regression report are preserved as historical artifacts, not rewritten as current test evidence.
- `RUNNING.md`, refreshed `README.md`, this description and two browser screenshots explain setup, controls, changes and limitations.
- Ignore rules exclude generated caches, staged duplicate assets, logs, build outputs and local score files; no credentials or local environment logs are published.

## 8. Current verification and remaining work

- **62 NUnit cases passed** using Mono, not Unity's editor test runner.
- **17 browser-engine tests passed**, including all 358,612 dictionary words.
- Runtime and editor C# source compilation passed against installed Unity assemblies. Inspector-field warnings remain.
- Browser checks confirmed tap suffix removal, invalid-word recovery, multi-letter held-mouse retracing, accepted-word scoring, live +2s combo feedback and round expiry. Earlier checks covered persistence, settings, drag submission and narrow-screen layout.
- The attempted native build stopped with no active Unity Editor licence (exit 198). No successful new native scene generation, Play Mode run or Windows executable is claimed.
- Still needed: licence activation, native scene/player validation, Unity Edit/Play Mode testing, physical touchscreen testing and explicit physical-device interruption recovery checks.

Excluded from GitHub: generated Unity caches, local `Builds/` test XML/DLLs/logs, temporary publication checkouts, staged duplicate source copies and browser/Unity personal scores.
