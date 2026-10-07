# Initial v0.7 bug-polish findings checkpoint

This is the requested initial reproduced-findings checkpoint, before production fixes. Canonical project X:/Projects/ZERO-TO-MILLIONAIRE, branch prototype/v0.7.1-bug-polish. Starting delivery HEAD7dad4e1a7fcd8d1bf87b6c56533793fb4ab6eb3d was clean. Game/build source remains dd74c983dcb22eaa7a03b54027dfa3b3aefe8926. All production scripts/art/scenes/settings/packages remain byte-identical in Git. Old releases remain unchanged.

## Confirmed defect: replay focus overrides a later SPIN pointer action

Reproduction on unchanged v0.7 with real UI Toolkit event paths and actual rendered Editor frames:
1. Open MENU and repeat the tutorial.
2. Before its deferred initial focus runs, send a pointer down/up to the existing SPIN button.
3. SPIN correctly gains focus and commits exactly one spin.
4. Two advancing frames later, focus is tutorial-next rather than spin.

Evidence/v071-focus-intent-before3.txt:
- immediate pointer focus=spin, committed_spins=1.
- expected spin; actual tutorial-next.
- RESULT REPRODUCED; one established-intent assertion, one failure.
Actual 360x640 pixels in v071-focus-intent-before3-pointer-spin.png show the focused Next button while STOP is visible. The visual was inspected.

Consequence: a later keyboard activation can target the tutorial rather than the player's last selected SPIN/STOP action. The transaction itself committed once; this is an input/focus defect, not duplicate payment or save loss.

Cause: UiInputCoordinator.FocusAfterLayout versions competing scheduled requests, but an explicit accepted pointer action does not invalidate the pending request. Proposed narrow fix: invalidate stale deferred focus when a valid scoped pointer/navigation action establishes newer intent; preserve modal scope, paired-navigation behavior and exactly-once resource changes. Regression should retain this exact before/after flow plus menu/navigation and replay restore checks.

A separate rapid Tab probe could not establish navigation before modal layout resolved; it is INCONCLUSIVE and is explicitly not counted as another defect.

## Validation so far

Unchanged v0.7 baseline checks rerun successfully:
- 107,769 economy assertions and 1,000 completed model levels;49–60spins,median53,p95 57.
- 432,000 movement samples.
- 52 offline session assertions covering interrupted writes, rollback, concurrent taps, restart and reset backups.
- 20,998 migration/layout assertions preserving old balances, ownership and parts.

New full critical/UI/tutorial/save-recovery and compact-layout runs remain for the fix stage. Previous v0.7 checks are preserved; they are not represented as new runs.

Two supported native-input guard attempts stopped BEFORE launching a test Player because of recent external user input. No OS mouse/keyboard input, no screensaver/security change, no user game interaction. Physical Windows input remains untested.

## ComputeBuffer bounded investigation

Read-only Mono.Cecil examination of exact shipped v0.7 managed engine/game assemblies found zero direct ComputeBuffer constructor calls in Assembly-CSharp. The managed constructor sites found are engine Physics2D debug drawing methods. This does NOT prove those sites caused the actual shutdown warning; native/internal allocation and teardown remain possible. No unknown engine buffer was released, no engine/package upgrade or reflective cleanup was applied, and no warning was suppressed.

Unity's reference source identifies this warning as a non-disposed buffer reaching finalization. Exact runtime allocation-stack ownership is not established by this pass. The known warning remains unresolved; further investigation must be bounded rather than displacing gameplay regressions.
Primary source: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Shaders/ComputeBuffer.bindings.cs

## Tooling interruptions and preservation

The computer transport disconnected briefly and recovered after a harmless read retry. Original owned Editor16444 exited independently. The first Editor-only reproduction harness lost its report/tag fields across Playmode domain reload; that harness failure is separate from product defects and was corrected. Only task-created audit Editor41556 was force-stopped after its failing harness starved its command queue. Subsequent task-created Editor44536 waited at a Recovering Scene Backups dialog; no dialog input was sent. One scene backup was copied byte-for-byte to .tooling/v071-editor-scene-backup-preservation before that owned instance was stopped.

The corrected reproduction ran in documented Unity batch mode WITH graphics, using a private profile, then exited. No unrelated/user Editor or Player was closed. Supported Unity batch mode was used to avoid unattended startup dialogs; no compiler errors were ignored and no API updates or new terms were accepted.
Primary source: https://docs.unity3d.com/6000.3/Documentation/Manual/EditorCommandLineArguments.html

User save was hash-checked only, never supplied to a test. It remains785be6ebbcbaee012786e88d3acf7b67c45ddddea1faf0d87de67083a5d2cfbc (1908bytes), equal to the start baseline. Unity's55 art metadata diffs were proven whitespace-only and normalized without changing GUID/settings.

## Next slice

Fix the confirmed focus defect, add targeted regressions for interrupted tutorial/story/menu/reset/spin and save recovery, rerun critical/UI/model checks, then build a separate Windows bugfix release if runtime code changes. Verify private native/extracted launch and attempt physical input only if the supported guard permits. Preserve approved visuals and old deliveries. Final source/Windows archives must include all six references and be split to<=15,000,000bytes, hash/extraction checked and saved with confirmed Library IDs. Android remains deferred. No publication/push/ads/payments/new timer.
