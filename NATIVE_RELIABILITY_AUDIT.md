# Native reliability checkpoint v0.4e

The direct owner request `открой прототип` was handled by restoring the existing
Windows v0.3 Player, PID46276, HWND61870534. No second user-profile Player was
started. Its window was visible, not minimized, and responding. Its original
save SHA256 stayed 4B752F42C8E3354B7926DAF797E19AF60BC5FEE9ECB01BDEA2F144FB6724162F.

## Proven panel ownership fix

GameView creates a runtime PanelSettings. Ten native create/destroy cycles
left ten orphan PanelSettings before the fix. GameView now stores its owned
panel, detaches UIDocument and destroys that panel in OnDestroy. The same ten
cycles in the final Development build leave zero panels; each recreated view
accepts one SPIN and one manual BUY, and repeated callback attempts commit
once. Resources loaded from the project and externally supplied QA textures
are not destroyed. No art/rig changes were made.

Native evidence: Evidence/panel-v0.4e-{before,fixed}-lifecycle.csv and JSON;
actual same360x720 native screenshots are pixel-identical before/after. These
are NavigationSubmitEvent UI callback tests, not OS pointer or human touch.
Final code compiled into Builds/Windows-dev-v0.4d-reliability/ZeroToMillionaireDev.exe.
Its isolated profile was .tooling/panel-lifecycle-v0.4e-fixed/save.

## ComputeBuffer shutdown warning: allocation owner unresolved

A controlled private run without any performance/input/soak harness reproduced
one warning after Input System shutdown, before PlayerConnection cleanup.
The project constructs no ComputeBuffer or GraphicsBuffer. A second run enabled
UnsafeUtility NativeLeakDetectionMode.EnabledWithStackTrace before scene load
and full warning stacks process-locally. It produced only the finalizer stack
ComputeBuffer.Dispose(Boolean) -> Finalize, without an allocation stack.
After the proved panel ownership fix, the same single shutdown warning remains.
This does not establish that the panel defect caused the buffer warning.

The installed SDK exposes CheckForLeaks, Get/SetLeakDetectionMode and buffer
leak-detector methods. Engine field discovery listed LowLevelPhysics2D drawer
buffers, which is a candidate list, not evidence of the actual owner. Bound the
investigation here: unknown engine-resource owner; upstream is plausible but
unproven. No vendor edits, warning suppression, ForgiveLeaks or global config
changes. Native applications exited normally and saved privately. The warning
and long session do not establish leak freedom.

Primary reference checked: Unity's [ComputeBuffer reference source](https://raw.githubusercontent.com/Unity-Technologies/UnityCsReference/master/Runtime/Export/Shaders/ComputeBuffer.bindings.cs)
explains the finalizer's inability to destroy main-thread graphics resources.
[PanelSettings reference source](https://raw.githubusercontent.com/Unity-Technologies/UnityCsReference/master/Modules/UIElements/Core/GameObjects/PanelSettings.cs)
shows panel disposal through OnDisable; installed6000.3 SDK signatures were
also reflected. Master reference code differs from installed6000.3 and is
context, not a claim that allocation stacks behaved the same in this run.
Relevant finalizer stack/diagnosis are small Evidence/shutdown-v0.4e-* files.

## Actual Windows pointer: blocked, not tested

At11:19:45UTC, the process desktop was Default but OpenInputDesktop returned
Screen-saver. GetForegroundWindow returned0; SetForegroundWindow on the private
Player returnedfalse. WScript AppActivate returnedtrue but did not establish a
foreground game HWND on the input desktop. Consequently no SendInput was
attempted and no pointer events were observed. The separate observer initializes
only its private fixture and writes button bounds/state; it dispatches no input.

Pending actual OS pointer coverage: press/release visual travel, drag-off/cancel,
rapid SPIN/BUY, menu/reset cancel, resize and focus loss/restore, relaunch. No
unlock, SwitchDesktop, security change or click on another window was attempted.
Evidence/pointer-v0.4e-blocker.json contains the concrete read-only evidence.
Tools/run_reliability_audit.ps1 Observe fails before launch if input and process
desktops differ. This guard was exercised and rejected the Screen-saver state.

## Longer native session

The15-minute run began11:18UTC in immutable reliability-before, private PID39752,
profile .tooling/soak-v0.4e-before/save. Automatic UI callbacks repeatedly spin,
buy all8 upgrades, verify completion/save reload, cancel reset, confirm reset,
and keep the heroine inside walkable geometry. Duplicate SPIN/BUY calls check
single commit and exact cash deduction. This is callback automation, separately
labelled from the blocked OS input work.

Its per10-second CSV records actual frame-loop intervals, allocations, Unity and
system memory, managed heap and collection counts. Native window was visible on
Default; Screen-saver was the active input desktop. Full-session GPU/present
counters were not recorded. Treat timings as native frame-loop measurements,
not validated active-display delivery or Android/device performance. The same
SDK's short lifecycle runs captured real nonblack native360x720 renders.

Completed measurements and phase comparison are in
Evidence/native-v0.4e-soak-analysis.json, with original CSV/summary alongside.
Reported quantiles are means of per10-second bucket quantiles; they are not
whole-session frame quantiles. The panel lifetime change was verified separately
in the final reliability build; the900-second session predates that fix.

## Reproduce and next slice

From project root, use the existing Unity6000.3.21f1 Editor and official CLI.
Schedule NativeAuditBuild.BuildBaseline with an unused permitted tag; existing
build directories cannot be overwritten.

- `powershell -File Tools/run_reliability_audit.ps1 -Mode Lifecycle -RunName NEW`
- `powershell -File Tools/run_reliability_audit.ps1 -Mode Soak -RunName NEW -Seconds 900`
- `powershell -File Tools/run_reliability_audit.ps1 -Mode Observe -RunName NEW`
- `python Tools/summarize_reliability_soak.py reliability-soak-NEW`

All audit hooks are Development-only. Profile paths are reflected/verified
against each private output directory and rejected if they point to the user
persistent directory. Leave the original v0.3 Player and its schema3 save alone;
do not run a schema4 release concurrently against that same user profile.

Next: actual pointer checks when the ordinary input desktop is available,
otherwise carry the explicit blocker into final validation. Art/rig stays frozen.
Final release/build/source delivery remains reserved13:15..14:05UTC; Android is
deferred by owner instruction. One room/eight finite quests/dual-resource BUY/
local saves/confirmed reset/free recovery criteria remain unchanged.
Completed900.015s run:53579frames,57complete cycles/resets/reloads,967spins,
460manual buys,1427repeat guards,158.061m walk;0scenario errors. Nine frame-loop
intervals exceeded33.33ms and two exceeded50ms, maximum700.235ms in early setup.
No >50ms intervals occurred in the120..300 or600..900 steady comparisons.
Concurrent Editor builds/short private Players/window positioning occurred;
this is a reliability soak, not an isolated performance benchmark.

| Mean of10-second bucket quantiles / allocation rate |120..300s|600..900s|
|---|---:|---:|
|median interval ms|16.6663|16.6682|
|p95 interval ms|17.7647|17.9284|
|p99 interval ms|19.7307|21.4179|
|allocated bytes/second|323205|324472|
|>33.33ms intervals|1|3|
|>50ms intervals|0|0|

Late system-used memory308.598 ->309.238MiB. Unity reserved297.901 ->304.288MiB;
managed reserved19.152 ->25.539MiB (one late6.387MiB heap expansion). Managed used
oscillated11.984..18.520MiB; late GC0count increased47. Counters all available.
The application completed without OOM/crash, but this window does not prove
steady memory for hours or exclude the unresolved shutdown buffer issue.