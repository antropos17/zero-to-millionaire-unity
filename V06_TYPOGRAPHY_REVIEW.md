# v0.6 typography hierarchy and compact-screen evidence

Primary type sizes intentionally stay where the approved skin and fit work.
The first type-comparison-v06.png compares the safety and motion checkpoints;
it demonstrates the helper-line change, not enlargement of primary labels.

| Role | Effective logical size | Reason |
| --- | --- | --- |
| Main SPIN | 45 | Dominant action in the bottom control area |
| Travel STOP / result SKIP | 32 / 24 | Phase-accurate action; long Russian skip label fits |
| Completed control | 26 | Clear terminal state |
| Cash header | 26; 16 for >6 digits | Readable normal balance; full permitted legacy integer remains visible |
| Menu title / completion title | 23 | Strong section title |
| Quest title | 15 | Larger than state and resource counters |
| Menu body | 14 regular | Paragraph readability without all-bold text |
| Dialogue body | 13, previously 12 | More readable heroine speech within measured bubble bounds |
| Quest cash and fragment counts | 12 | Both whole values fit the compact card |
| Quest status / helper / dialogue actions | 11 | Supporting text; helper and dialogue actions were 10 |

No font outlines or game art were changed. The Pixelify derivative continues
using its already approved Cyrillic aliases. Cash and fragment labels remain
unwrapped; the full available/required cash pair is visible.

Authentic BEFORE dialogue/menu PNGs were extracted without modification from
Git snapshot f69ae158279596c981a4008b97cc05eaa4a8d987, whose rendered captures
predate the motion typography changes. AFTER frames are actual offscreen
Editor captures at 360x640, not reconstructed old styles or flattened mockups.
Menu geometry also changes because the new motion setting adds one control;
this is documented rather than treated as a font-only pixel comparison.

Stress evidence includes a ready first task with 20/20 and 60/60; maximum
permitted legacy cash 2147483647 with current final cost 355; and the largest
permitted old-curve price 110 with its preserved 16-fragment goal. Arbitrary
large task prices are not permitted by the existing save validator and were
not introduced for testing.

Native interactive verification remains blocked by the Screen-saver desktop.
The exact clean executable's existing read-only workflow captures the screen
framebuffer after WaitForEndOfFrame; it has no standalone RenderTexture capture
flag. Unity documents PanelSettings.targetTexture for offscreen UI rendering,
but the Editor-only audit currently configures it. Enabling an extra runtime
capture path would require new release instrumentation. No undocumented internal
render calls, desktop/security changes or further black timing runs were used.

Official API references checked:
https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UIElements.PanelSettings-targetTexture.html
https://docs.unity3d.com/6000.3/Documentation/ScriptReference/WaitForEndOfFrame.html
https://docs.unity3d.com/6000.3/Documentation/Manual/PlayerCommandLineArguments.html