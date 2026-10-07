# v0.8 review corrections and precise test limits

Independent visual review accepted the actual English/font/motion PNG/GIF
checkpoint. No art redesign or economy/save/schema change was requested.

The source reviewer identified an interrupted dialogue entrance. Before the
fix, opening MENU during a replay entrance hid the bubble while its scheduled
animation stopped after160ms. Returning to the same replay mode displayed
opacity0.7122219 and translationY0.1429963. The definitive actual reproduction
is Evidence/v08-entrance-before2.txt. Hide now calls FinishEntrance: pauses its
schedule, restores opacity1 and translation0, and releases the schedule handle.
The focus cancellation and normal entrance duration remain unchanged.
Evidence/v08-entrance-after3.txt passes24 checks across full/calm MENU/SPIN
interruptions, each resuming at opacity1, translation0 and exact spin count.

The older broad input timeout was a test lifecycle assumption. LoadScene
completes on a later runtime frame; the harness immediately reacquired the old
GameView while phase19 was active, advanced to phase20, then saw that instance
destroyed and restarted at phase0. Its repeated 480x960 captures demonstrate
the loop. The global180s deadline eventually expired in phase15 of a later
iteration, after many successful second-spin completions; that final waiting
phase was not the cause. The corrected harness stores the outgoing instance ID
and waits for a different initialized GameView before resuming. The final
v08-usability-checks-4.txt passes326 assertions in16.39s, including real panel
picks, held/new-key input, rollback/retry, reset, detached buttons and scene
recreation. Earlier failed reports remain evidence and are not called PASS.

Final focus checks pass12 scenarios after the dialogue fix. Motion checks cover
STOP/SKIP repeated30 times, disk before cosmetic frames, calm and queued menu.
All UI input checks route actual UI Toolkit events in the graphics Editor;
they are synthetic and send no OS input. A temporary Editor queue failed to
wait for actual Playmode teardown; final runners were driven separately.
The existing ComputeBuffer finalizer diagnostic remains unsuppressed and its
native allocation owner is unresolved. No reflection-based disposal was added.

Ready criteria remain: one runnable fixed room; correct cash/parts and manual
both-resource BUY; repeated taps cannot double-buy/overdraw; visible persistent
repairs/items; complete level and confirmed reset; save survives actual relaunch;
compact/tall portrait sizing and performance; exact Editor/native/device limits;
final source/build identity and verified split delivery; APK when agreed in scope.
Windows is current scope and Android packaging remains deferred.
