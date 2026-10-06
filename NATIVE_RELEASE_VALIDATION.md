# Windows Release candidate baseline v0.4f

Built from source ca4e19ec8aa70bf1670bb0b5a1c80cc178ad1f03, Unity6000.3.21f1,
imported 2D Animation13.0.6. BuildOptions.None, no Development/ScriptDebugging/
AutoConnectProfiler. Company/product/version/signing and old builds unchanged;
bundleVersion stays inherited0.3.0, candidate directory is the disambiguator.
No Android/publish/purchase/store action. Windows executable is unsigned.

Clean candidate: Builds/Windows-rc-v0.4f/ZeroToMillionaire.exe.
Executable SHA25648EFAB523AA684C653BD1254A6962D3410127B5C02DC1310F6F16F4810666556.
Full package manifest hash6f56a2969ca42c992f90b003c760ce1f43573d9ceb416bb2e81fdfc20adde42c.
104642815actual package bytes; all file hashes in Evidence/release-v0.4f-candidate-hashes.json.
Ordinary launch retains ordinary save path and adds no observer/action.
Explicit --ztm-private-profile ABSOLUTE_PATH requires a child of this project's
.tooling and rejects invalid/outside paths before creating a store. It attaches
read-only frame/state/screenshot instrumentation, with no action dispatch.

The exact clean binary ran fresh, completed and same-profile reload states.
Debug.isDebugBuild=false, QA fixture methods absent, automated harness absent,
actual reflected store path private. Fresh firstFrame1->lastFrame7872,33.689s,
7871samples,7869nonzero drawcounterframes,6.706m walk. Actual480x960 nonblack
render shows initial damaged empty room, adult bob heroine, correct current
quest/meters and3free spins. Completed state shows8/8 and no active quest/spin.
Same-profile actual process relaunch preserves cash/free/spins/recovery/parts/
owned/grid/quests; walking position can advance and is saved normally.
Read-only probe caps stored interval samples at12000; later elapsed/framecount
values can exceed that cap. It does not record whole long-run percentiles.
Default vSync settings and locked input desktop caused ~4ms loop intervals;
this is not active-display FPS or a clean A/B performance comparison.

Separate release audit twin: Builds/Windows-release-audit-v0.4f/ZeroToMillionaire.exe.
Also BuildOptions.None/Debug=false, but explicit extra define ZTM_RELEASE_AUDIT
includes the existing seeded60s private callback workload. It is a DIFFERENT
binary; never deliver/describe it as the clean candidate. Full manifest hash
758dfb1794b5a3280d384a67e0e0791dde82e3fe531cde78868cb006138d5328.
Source identical except the build define; 104656292actual package bytes.

Twin passed16SPIN/8manualBUY/25repeatguards/full completion/reload/walk/resize.
FrameCount86->3674,3589sequential samples,60.013s,13.703m; native drawcalls >90%
each phase and actual screenshots asserted nonblack. target60/vSync0 audit-only;
prior target60/vSync1 recorded. Median intervals16.67ms each phase; progression
p9517.82ms/p9921.96ms/max53.61ms (one >50ms); other measured phases zero >33.33ms.
No Dev/Release speedup comparison. Native current rig/UI/two-resource purchases
were exercised; no real Windows pointer/touch or Android claim.

Unity Test Framework1.6.0 EditMode:9passed/0failed/0skipped. Small typed suite
covers both balances, concurrent repeat purchase, repair->wallpaper pool, schema3
migration/resources/ownership/feet, corrupt quest IDs, save rollback/lease and
full finite completion. Pure Core assembly is separate; scene GameView identity
and JSON field names remain unchanged. Framework tests excluded from Player.

Synthetic UI Toolkit PointerEvent audit proved down-face feedback missing before
(two failures among17); Clickable stopped down before the bubble-phase Skin
handler. Skin now handles down in trickle phase and clears on cancel/capture-out/
blur/detach. Final19checks pass with correct pointerId and frame-separated BUY:
face5SPIN/face2BUY, no action on down/cancel/leave, release commits once, repeated
attempts guarded, menu/reset confirmation/cancel preserved. Actual eventdispatch,
not QaPressedSpin. Initial malformed capture-out test and same-tick gesture were
corrected; final evidence is ui-pointer-v0.4f-fixed-checks.txt/PNG. These are
synthetic Editor events, NOT Windows hardware/SendInput evidence.

ComputeBuffer finalizer warning persists once on normal clean Release shutdown
and once on audit twin. No application ComputeBuffer owner/allocation origin
isolated; retain slice5 known issue. No vendor edits/suppression/ForgiveLeaks.
All processes closed normally, no exceptions in checked completed logs. Original
v0.3 Player46276 and save SHA2564B752F42C8E3354B7926DAF797E19AF60BC5FEE9ECB01BDEA2F144FB6724162F unchanged.
Input desktop Screen-saver; no unlock/SwitchDesktop/settings change/SendInput.

Private exact-binary run from project root:
`Builds\Windows-rc-v0.4f\ZeroToMillionaire.exe -screen-fullscreen 0 -screen-width 480 -screen-height 960 -logFile X:\Projects\ZERO-TO-MILLIONAIRE\.tooling\NEW_RUN\Player.log --ztm-private-profile X:\Projects\ZERO-TO-MILLIONAIRE\.tooling\NEW_RUN\save`
Use a NEW directory and explicit Normal game-window startup from a shell.
Do not run clean schema4 candidate against the live original schema3 user save.
Next owner-authorized story/tutorial/pacing proposal is separate; baseline RC
contains none of those pending features. Art/rig remains frozen.