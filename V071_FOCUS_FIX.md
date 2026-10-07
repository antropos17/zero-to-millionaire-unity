# Deferred focus correction carried into v0.8

New pointer activation, paired Tab navigation or valid submit invalidates older
scheduled focus work. A direct focus change also prevents the obsolete callback.
Initial dialogue/modal focus, restore targets, held-submit guards and disposal
remain intact. No economy or save schema changes.

Evidence/v071-focus-intent-after3.txt: PASS 12 assertions, zero failures,
actual UI Toolkit pointer and paired keyboard events in graphics-enabled Unity
Editor Playmode, isolated private save. One initial rapid navigation probe did
not establish intent before modal layout and is explicitly inconclusive.
The batch harness now waits for elapsed wall time as well as Editor frames;
the earlier nine failures were premature scheduler observations.

No OS input or interaction with the user's live v0.7 process (PID 40896).
The bounded ComputeBuffer allocation ownership report is not a stack capture;
the pre-existing diagnostic remains unresolved and is not suppressed.
