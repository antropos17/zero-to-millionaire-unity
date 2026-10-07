# v0.8 English and animation checkpoint

Open Evidence/v08-room-before-after.png, v08-menu-before-after.png and
v08-tutorial-before-after.png for typography. Watch v08-spin-before-after.gif
and v08-repair-before-after.gif for real motion. Both columns are actual Unity
captures at approximately20fps. The baseline has v0.7 visuals plus the isolated
focus fix; no generated/tweened game frames were added.

Authoritative validation:

| Check | Result |
|---|---|
| Obsolete deferred focus vs pointer/navigation/disposal |12 PASS|
| Core economy, sessions, migration, walking |107769 economy;1000 levels;52 sessions;20998 layout;432000 walk samples PASS|
| Unity critical EditMode |24/24 PASS|
| English resource bars, all8 quests, max legacy balances, four sizes |2624 PASS,160 combinations|
| Five tutorial steps, four stories, five replay pages, menu/reset |540 PASS,64 actual layouts|
| Panel picks/44-unit edges, paired Tab, held input, rollback/retry/reset |256 PASS,three sizes|
| STOP/SKIP, duplicate awards, disk during animation, calm setting |30 PASS|
| Fresh full level, every purchase, disk and actual scene reload, reset |52 spins;8 buys;4 stories;456909 checks PASS|
| Editor timing during actual level |median16.67ms;p95 17.90ms;7763 frames|

Timing includes screenshot pauses and runs alongside the user game; it is an
Editor observation, not a standalone or device benchmark. Input is synthetic
UI Toolkit routing; no OS input was sent to the user's game. New executable,
APK, standalone/process relaunch and extraction checks follow visual review.

Earlier results are retained honestly: the first large-balance layout run failed
21 clipping checks, fixed by widening full cash counters; final run passed.
One combined test start overlapped EditMode teardown with Playmode. The older
large usability stress runner then failed an immediate fresh-key assertion and
timed out; its reports are not counted as PASS. The bounded compact runner uses
the actual runtime clock and independently passes hold/new key, purchase,
truthful errors, retry, modal and reset scenarios. Its result is authoritative.
The pre-existing Unity ComputeBuffer finalizer diagnostic stays unsuppressed;
the bounded allocation-ownership scan is not a native allocation-stack capture.

Run in Editor: use Unity6000.3.21f1, open the project folder, open
Assets/Scenes/Level1.unity and press Play. A normal run uses the local player
save; all QA runs used separate .tooling profiles. Avoid a normal Editor session
if the user is simultaneously playing the old executable with the same save.
For isolated review, set ZTM_QA_SAVE_DIR to a fresh local directory before
launching Editor. Current shipped Windows run remains:
Builds/Windows-v0.7-candidate-1/ZeroToMillionaire.exe.

Next slice: independent review of the English/font/motion checkpoint, then a
separate clean v0.8 Windows build, private native launch/quit/relaunch, ZIP/source
identity and extraction checks. Preserve v0.7 and the active user session.
