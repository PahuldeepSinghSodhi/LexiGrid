# Play LexiGrid

## Play immediately

Run `./Start-Game.ps1` from PowerShell in this folder, or open http://127.0.0.1:8765 while the local server is running. Press **Play a round**. Drag across neighbouring letters and release to submit, or click tiles then choose **Submit selection**. Keyboard users can select tiles with Space and submit with Enter.

The local server runs in the background and binds only to 127.0.0.1. It uses Node.js (already present in this environment). There are no package downloads or cloud calls. Close the page when finished; the server remains available for another round. You can use `-Port 8766` if the default port is occupied.

### Correct a selection and earn extra time

Tap a selected letter to remove that letter and everything after it. Hold and retrace your path to remove later letters while keeping the letter you returned to. **Undo last letter** (or Backspace) removes one letter; **Clear selection** (or Escape) removes everything. A disconnected tap starts a fresh selection. Invalid submissions clear automatically, and cancelled/interrupted drags release their selection so input cannot remain captured.

Each non-overlapping pair of distinct, valid words submitted **less than five seconds apart** adds **two seconds** to the timer. Invalid, too-short or duplicate submissions break the pending pair. A bonus cannot revive an expired round. The existing length/speed scoring is unchanged. These rules are also implemented in the Unity source and scene builder, but native input/layout still need a licensed Play Mode check.

This is a **browser preview**, implemented in JavaScript using the project's real 358,612-word list, Trie validation, adjacency rules, scoring bands, timing multipliers, solvability checks, and top-10 MinHeap. It is not a Unity WebGL export. Browser scores/settings use localStorage and are separate from Unity's JSON scores/PlayerPrefs. Background dictionary loading uses a Web Worker. The preview includes click/keyboard selection as well as drag input.

## Run the Unity project

The local source snapshot used for this update did not include a saved game scene or tile prefabs, and placed ProjectSettings, Packages and Library inside Assets. A clean editor environment is prepared in `UnityProject/`. The GitHub repository also contains a legacy `GameScene.unity`, `AutoBuilder.cs` and other assets; those remain preserved. The new setup creates a separate `LexiGrid.unity` in the staged project and does not claim that the legacy scene has been upgraded or re-tested.

1. Activate your eligible Unity licence in Unity Hub. The installed editor is **6000.5.10f1**. The attempted build currently stops with `No valid Unity Editor license found` (exit 198).
2. Run `./Start-Game.ps1 -Unity`, or add the `UnityProject` folder in Unity Hub.
3. Allow package resolution/compilation. Required UI and testing packages are explicitly included.
4. Use the editor menu **LexiGrid > Create or rebuild playable scene**. This creates `Assets/Scenes/LexiGrid.unity`, tile and leaderboard prefabs, text resources and all scene references.
5. Press the Unity Play button. The splash screen loads the real dictionary before showing the main menu.

For a Windows executable, run `./Start-Game.ps1 -Build` after licence activation. It generates `Builds/Windows/LexiGrid.exe` and launches it if the build succeeds. The game opens in a resizable 1440 x 900 window. Keep the whole Windows build folder together when distributing it.

`Assets/Scripts`, `Assets/Editor`, `Assets/Tests` and `Assets/Resources` at this workspace root are the source of truth. The launcher copies them into `UnityProject/Assets` before opening/building Unity. Edit those original sources and rerun the launcher to synchronize them.

## What improved

- A reproducible builder for a wired Unity scene with menu, loading, game, results, instructions, settings and leaderboard; scene generation still needs licence activation.
- Scale With Screen Size canvas with desktop/tall layouts, TextMesh Pro distance-field text, large square tiles, sliced rounded panels and stronger contrast.
- Tile-pool reuse and correct deactivation when board size changes.
- HUD events wired to input, visible rejection feedback and cancelled selections after timeout.
- Correct new-personal-best detection rather than marking every leaderboard insertion a high score.
- Countdown display clamped at zero and rounded consistently.
- A guaranteed valid word placed on a neighbouring snake path if 100 random board attempts fail.
- Unity assembly definitions so tests are isolated from the runtime assembly.

Canvas scaling is configured using Unity's documented [Canvas Scaler behavior](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-CanvasScaler.html).

## Verification

- `./QA/Run-CoreTests.ps1`: **62 NUnit tests passed** (54 original + 3 fallback cases + 5 combo cases), run directly with the installed NUnit/Mono runtime. These are the pure C# tests, not a Unity Editor test run.
- `./QA/Compile-UnitySources.ps1`: both runtime and editor sources compile against installed Unity assemblies. This does not validate a generated scene or player build.
- `node --test Play/engine.test.mjs`: **17 browser-engine tests passed**, including undo/retracing, combo boundaries/reset/expiry, full dictionary loading and all 3 board sizes.
- Browser UI checked: valid click submission, drag submission, duplicate rejection, timeout/results, saved leaderboard after reload, settings navigation and phone reflow.
- Native scene generation, native Play Mode and Windows player build still require licence activation and have not been verified.

The earlier QA reports are historical artifacts and have not been rewritten as new execution evidence.

## Improvements to prioritize next

1. Verify the generated native scene and run the NUnit suite in Unity after activation; add Play Mode tests for replay, grid resizing and screen navigation.
2. Add player names, pause/resume and clear round-exit behavior.
3. Balance board difficulty by counting available words instead of merely requiring one.
4. Add optional sound/haptics, reduced-motion options, accessible feedback and an in-game dictionary explanation.
5. Make native JSON saves atomic and validate imported score data before loading it.
6. Package a release build and clean repository structure; exclude generated Library/Logs/UserSettings caches.
