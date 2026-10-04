# Selection and combo update — 2026-10-04

The reported screenshot shows an invalid TADG path. Source inspection confirmed that already-selected tiles were ignored, so the player could not undo/retrace them. The screenshot alone does not establish that the countdown itself froze. Interrupted pointer capture is now cleaned up on cancellation, lost capture, window blur and hidden-page transitions.

## Executed checks

- Browser engine: 17 tests passed, including tap suffix removal, multi-letter retracing, adjacency/restart, non-overlapping two-word combos, strict five-second boundary, reset and expiry safeguards.
- Pure C# NUnit: 62 cases passed, including five new combo cases. This was an NUnit/Mono run, not Unity Editor testing.
- Unity runtime and editor sources compile against the installed engine assemblies. Inspector-field warnings remain; no native scene/player validation is claimed.
- Browser mouse: selected C-D-P, tapped D; only C remained selected.
- Browser rejection: submitted C-D-P; rejection feedback appeared, all tiles cleared and the countdown continued. DIME was then accepted.
- Browser combo: accepted DIME then DIM; the second submission displayed +2s and the combo confirmation. Repeated with FILE then LID and captured the updated UI.
- Browser held-mouse retracing: moved C → D → P → D → C without releasing; DOM inspection showed only C selected. Releasing cleared the too-short path and displayed the correct rejection.
- Browser round expiry: the test round transitioned to results with two accepted words and 550 points.

Proof image: `Play/screenshots/selection-combo-update.jpg`.

## Remaining manual checks

- Touchscreen tap/drag behavior on physical hardware.
- Focus loss and pointer-cancel recovery on physical devices (handlers implemented, not separately exercised here).
- Native Play Mode input, generated button wiring and desktop/tall layout after Unity licence activation.

## Combo rule

Each non-overlapping pair of distinct accepted words less than five seconds apart earns two seconds. Invalid, too-short and duplicate submissions reset the pending pair. New rounds reset it too. Expired rounds cannot be revived. Existing score bands and speed multipliers are unchanged.
