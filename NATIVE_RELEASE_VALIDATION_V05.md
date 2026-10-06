# Windows release validation — prototype v0.5

Frozen game source: `17f525238b1925819b857f9bfd31cb5ec4062126` (2026-10-06 13:03UTC). Feature/art/rig code unchanged afterward. Later docs/testing HEAD is recorded in Deliverables/final-v0.5-delivery.json and final handoff. Clean build used Unity6000.3.21f1, 2D Animation13.0.6, Windows64 Mono/D3D11, BuildOptions.None. Original company/product, bundleVersion0.3.0, Android configuration/signing retained. No Android rebuild, installation, publication or external push.

Final folder: `Builds/Windows-FINAL-v0.5`. Runtime files are byte-for-byte copied from immutable `Builds/Windows-rc-v0.5`; Unity BurstDebugInformation_DoNotShip omitted, Russian launch notes added. Full relative-path/size/SHA256 manifest is Evidence/release-v0.5-final-folder-hashes.json. Unity launcher EXE hash alone does not identify game revision; use the runtime manifest and managed game/core DLL hashes.

## Exact clean binary

Debug.isDebugBuild=false; GameView QA input/state methods and NativePerformanceAudit absent, private SaveStore path verified by read-only observer. Fresh and repeated launches preserve0cash/0spins/3free spins and introduction; complete profile preserves8owned/53spins/182cash. A copy of the existing schema3 user save preserves all resource fields and ownership, suppresses forced tutorial, and retains current wallpaper requirements14parts/80cash while future tasks use new values. Original user file was neither changed nor replaced.

Actual clean native frameCounts848(fresh),848(relaunch),869(complete),858(legacy). Fresh/completed screenshots are actual Player ScreenCapture images480x960, with nonzero rendered draw counters. Default vSync under Screen-saver produced roughly4ms loops; these are not display FPS. Short read-only sampling is not a thermal/endurance measurement.

Six additional exact-binary persisted private fixtures passed: resume invitation, skip suppression, resume meter explanation, pending surfaces story, seen story suppression, and several pending milestones collapsing to the latest completed story. Text, tutorial step, seen mask and all resource fields were checked. Actual frameCounts833/873/859/889/835/828, each rendered. These are restart/state-interpretation tests without input dispatch. See Evidence/release-v0.5-clean-dialogue-cases.json.

Synthetic Windows PostMessage mouse messages were accepted by the OS but did not invoke SKIP in the clean game while inputdesktop=Screen-saver. State remained unchanged. This is an unperformed clean-binary action, not a passed click. No cursor movement, SendInput, unlock, screensaver dismissal or security modification was used.

## Native Release validation variants

`Windows-release-audit-v0.5` uses None plus ZTM_RELEASE_AUDIT and is a different binary from the delivered clean Release. Seeded57 native UI callbacks completed the whole new level from a fresh state:53spins,8manual buys,61repeat guards. Completion was not injected. All reload, resize360x640/540x1080 and grounded walking checks passed.135.003s,8,087sequential completed-frame samples, frameCount77..8163,29.205m travel. Rendering counters and nonblack screenshots verified.

Frame interval median16.666ms, progression-phase p95=17.436ms, max52.840ms (three intervals>33.333ms, one>50ms). The95s progression phase includes idle walking after completion; it is not continuous spin-only timing. Timing includes limiter/scheduling/instrumentation, target60/vSync0. Zero/absent profiler counters reported unavailable. Audit cleanup printed two “Profiler is not supported” warnings; no trace was claimed. See selected CSV, manifest and summary.

`Windows-release-onboarding-check-v0.5b` is another explicit test-only Release variant (None; ZTM_RELEASE_AUDIT and ZTM_NATIVE_ONBOARDING_CHECK), using unchanged frozen game methods and an added test controller.23checks passed over488advancing frames: NEXT grants nothing, saved invitation reloads, SKIP persists, help replay keeps all resources and saved tutorial flags through all five steps, replay BUY text avoids a false readiness claim, pending story waits through spin/reward feedback, seen story does not repeat, several pending stories collapse to latest, reset cancel/confirmation work. Native reload here recreates GameView in-process; actual process restarts were tested separately on the exact clean binary. UI callbacks are synthetic NavigationSubmitEvent, not physical input.

The controller is preprocessor-excluded from the clean binary and regular Editor. Native validation GIF uses96actual captured PNGs, resized by nearest-neighbour to360x720. Identical GIF frames may merge while duration is preserved. Fixture story/reset jumps are explicitly test coverage, not ordinary pacing. An initial test recorder collision lost named screenshots; corrected v0.5b rerun produced all six named PNGs and passed the same23checks. Old unsuccessful recorder output remains private and is excluded from delivery.

## Editor and model evidence

Unity Test Framework16passed/0failed. Synthetic pointer19passed: down/up, drag-off/cancel, capture loss, blur, repeated clicks, MENU and reset cancellation. Editor tutorial UI audit passed615advancing frames at360x640/360x800/480x960; controls44high, text measured to fit; quest/reels/BUY/SPIN remain visible. Independent parent pixel review found no actionable visual issue.

107769economy assertions and1000complete seeded levels;432000navigation samples;52offline session checks (including actual locked temporary write rollback);21106layout/migration assertions. First purchase median3/p954spins. Full level range49..60, mean53.333, median53,p9557. Full policy/all-price choices: ONBOARDING_PACING_VALIDATION.md.

## Remaining limits and preservation

Physical pointer/touch, active-display presentation, Android device/emulator and battery/thermal tests remain unrun because the Windows input desktop is Screen-saver. Text tutorial/portrait has no voice. Some non-dialogue UI remains English; bubble temporarily covers the room. One fixed modest level, no millionaire luxury. Windows executable unsigned. Internal bundleVersion0.3.0 retained deliberately.

ComputeBuffer finalizer warning persists once at normal shutdown in every valid native run. Allocation owner remains unresolved; no engine edits, suppression, ForgiveLeaks or leak-freedom claim. It does not invalidate the recorded successful completion/state checks. Investigation stopped as requested.

Original user Player46276/Window61870534 remains running/responding and nonminimized; request to open restored that window, but foreground request was blocked by Screen-saver. Save SHA256 remained4B752F42C8E3354B7926DAF797E19AF60BC5FEE9ECB01BDEA2F144FB6724162F. All new QA Players were isolated and closed normally. Do not launch two ordinary versions against the same save; Russian notes explain switching and schema5 downgrade limitation.

Library initial/context/surfaces/completion checkpoints are listed in ONBOARDING_PACING_VALIDATION.md. Final source/Windows/GIF/image IDs and archive verification are in Deliverables/final-v0.5-library.json after successful delivery.

Final actual clean Player screenshots / GIF saved in Library (metadata verified):
- initial480x960: libfile_b251433e616481919930631b881aee5e
- complete480x960: libfile_21f33ccc01fc8191b1294e282c610ce9
- native validation GIF (96captured/74decoded merged frames,7.68s): libfile_05375b8f46148191ae9958efffa70fa8

Final-folder native smoke additionally passed using its exact EXE and adjacent files; private profile, original resources and tutorial preserved, QA hooks absent, normal close.
