# ZERO TO MILLIONAIRE — project handoff

Canonical project: X:\Projects\ZERO-TO-MILLIONAIRE.
C copy retained at C:\Users\murtu\Documents\Codex\2026-10-06\task\zero-to-millionaire-first-playable.
All 8,244 original files / 3,521,510,846 bytes matched by SHA256 before switching.
Receipt: Evidence/migration-to-x-verification.json; full manifest is local and ignored by Git.

Unity 6000.3.21f1 / C# uses installed SDK, NDK and OpenJDK on X.
Godot was absent; the installed Unity Android toolchain determined the choice.
Unity Editor: X:\Programs\Unity\6000.3.21f1\Editor\Unity.exe.
Local CLI: .tooling/unity-cli/bin/unity.exe; official project-pinned MCP is local.
No hosted backend, purchase, advertisement, login, publishing, remote or push.

## Approved scope and rules

One fixed 9m² room, initially wrecked and empty; modest repairs, wallpaper,
existing door/window, mattress, lamp and stool. Same adult black-bob heroine,
cream/forest/terracotta palette. Separate room layers, character parts, objects,
symbols and live UI; no flattened screenshot background.

Eight finite tasks: ONE active, ONE locked NEXT, room N/8 checkmarks.
Each task requires its own matching parts AND cash; only manual BUY & INSTALL
changes the room. Current-only symbol pool; one visible matching icon = one part.
Extra current-item rolls turn into visible cash. Full parts earn cash if needed;
both ready pauses collection until installation; 8/8 ends collection.
Schema3 stable IDs preserve schema1/2 purchases (including out-of-order owned),
cash, allowances and every unspent/excess part. Display only is capped.
Spin/grid and purchase/next-task state commit before cosmetics. Free starter3
and explicit recovery below5cash prevent deadlock. Reset needs confirmation.
Continuous reachable walking, grounded feet and furniture/door clearance remain.

All SIX approved refs are in references/, inspected and copied by exact hash:
- ruined: libfile_7a7cbf4a0f7481919d164be3932e4ed7
- patches: libfile_1dc46769dbfc8191bdbe2f4a248235f4
- surfaces: libfile_5e5c1e54e4908191b92941a836868c40
- wallpaper: libfile_7741c619bb5c81919e7cc726670e7564
- plain mattress: libfile_9db10562c0008191849dceaaa25fb4db
- modest finish: libfile_c42c06e8a880819188661f4787c30cbf
Identity maps, art provenance and older deliverables are retained.

## Final desktop checkpoint

Fresh v0.3 Windows build succeeded and was opened/responding at Builds/Windows-v0.3/ZeroToMillionaire.exe (PID24084, title ZERO TO MILLIONAIRE). EXE SHA25648efab523aa684c653bd1254a6962d3410127b5c02dc1310f6f16f4810666556. Initial launch preserved87cash/9spins/2owned. At closeout the user had actively reached120cash/18spins/7owned; preserve their current state, never restore an older observation. A forced close was avoided. After the user had already closed the player, the unchanged build was reopened (PID13084). Live gameplay changed fields before the after-load observation, so exact player-relaunch resource equality is unverified; preserve current state and never restore an old snapshot.

Final checks:34,586 economy assertions/1000 completed seeds14-21spins median17;432,000 navigation samples;52 targeted offline session checks. Fresh rendered UI callback run completed16spins/8buys, with resource/repeat gates, save-before-cosmetics, menu, reset-cancel/confirm and free recovery. Three portrait sizes and all six matched reference stages were rendered. Actual Editor timing is recorded in Evidence/runtime-v0.3-qa.txt; it is not an Android benchmark.

Offline GameSession now injects IStateStore and randomness, commits candidates before publishing and holds a cosmetic spin input lease. Raw schema3 identity/shape validation precedes normalization; invalid primary falls back to backup. A reset primary commit remains fresh when its backup update fails. Net cash overflow uses a wider intermediate. See ARCHITECTURE_REVIEW.md for reproduced checks and honest visual differences.

Code is unchanged since the fresh QA/build; do not rebuild or repeat the broad loop solely for delivery. Android v0.1/v0.2 files and tooling are preserved; current Android packaging is deferred.

Run/edit paths and gameplay: README.md. Durable evidence: Evidence/windows-v0.3-startup.json, windows-v0.3-closeout.json, unity-model-checks.txt, runtime-v0.3-qa.txt, windows-v0.3-final-window.png and v0.3-six-reference-review.png. Library IDs and source archive hashes are stored locally in Deliverables/library-delivery.json.

Keep only intended source, Unity .meta, docs, references and bounded QA evidence in local Git. Keep existing identity/signing configuration. Never push, publish, add monetization, reset user progress, remove the old C copy or delete prior builds/graphics. This file summarizes project state; it does not claim chat compaction.

Final observation: the live player subsequently closed during user play. No repeated relaunch was attempted. Current user save is authoritative; never restore the older87/9/2 or120/18/7 observations.

## Active six-hour visual iteration (2026-10-06)

Owner window ends14:05UTC; stop long-risk work13:15UTC and reserve final50minutes. Visual/animation first, then response/performance, then full regression and fresh desktop build/delivery. User sleeping; checkpoints go to parent, no direct outreach unless critical permission/data blocker.

Official2D Animation13.0.6 now pinned/installed forUnity6000.3.21f1, registry source and successful compile; existing dependency versions unchanged. New HeroineVisual uses two articulated segments per leg, alternating stance/swing targets, contact shadows, gradual start/stop, idle torso breathing and facing hysteresis. Canvas212 and feet178/192 anchor unchanged. This is still a procedural UI Toolkit cutout, not SpriteSkin animation. Arms remain rigid; only front/back source views plus mirroring.

Assets/Editor/VisualAudit.cs explicitly opens Level1, uses .tooling/visual-qa-save and records actual advancing Time.frameCount. Evidence/visual-v0.4-*.json contains before/after10second480x960 renders (119captures each;525/476game frames advanced;1.697/1.703m). GIFs are12Hz diagnostic capture, not native Player performance data. Native stills/GIF are inEvidence; parent should judge gait quality from the clip before claiming maximum polish.

Necessary model/session/walk checks passed unchanged (34586/52/432000). Current oldWindows-v0.3 executable is preserved; no freshWindows build for this iteration yet. Editor43948 remains ready; own test Playmode stopped. User save was not reset/restored; all animation fixtures use the private test profile.

Next visible priorities: reserve unobstructed payout/progress feedback lane; improve compact reel/icon optical fill with viewport fit; readable task metadata; lamp/stool silhouette spacing and depth; current walk+SPIN/reel-stop+installation clip. New arm parts may improve natural swing without a framework migration. Full profiler/player workload follows visible polish.


## Bounded visual/UI checkpoint v0.4b (2026-10-06)

Reward feedback uses a reserved22-unit lane between task and reels; the room
and quest illustration stay unobstructed while the parts progress bar pulses.
All9 symbol images use measured optical alpha bounds and a44-unit maximum,
centered without stretching or texture edits. Short reels144; tall endpoint204.
Task title15, status11, counters12, NEXT12; raisedSPIN base10/press5 retained.
The9m area uses a crisp drawn superscript2. Lamp anchor(.36,.56) shifts it8.16
sourcepixels left, within its existing reserve; collision geometry unchanged.

The ghostshoe cause was a vertical leg split retaining the neighbouring heel.
MaskedShin draws original whole neutral pixels with diagonal separation, so the
left toe is complete and the other heel no longer rides with it. Front/back
source alpha partition has no duplicated pixels or missing source coverage;
inspected runtime sourceframes13/15/17/66. Canvas212 and pivot178 unchanged.
Arms remain static; front/back/mirror turning still needs a restrained planted
pivot/head response or new transparent parts. Avoid claiming fully natural gait.

Actual runtime screenshots before/after360x640 and480x960, reward lane both
sizes, and an11-second480x960 clip include walking, pressedSPIN, real callback
reel stop and explicit BUY callback/visible completion. Source captures and
encoded GIF frames/duration are separately recorded in ui-v0.4b-checkpoint.json;
this is Editor capture at12Hz, not nativePlayer performance or physical touch.
848 UI assertions passed across64 combinations:8tasks x4resource states x2
sizes; title/status/counter bounds, gates, repeat purchase and reload. Necessary
core regressions also passed34586 economy/432000 navigation/52 session checks.
Allfixtures use private .tooling QA profiles; currentuser save never reset.

OldWindows-v0.3 player46276 was left responding, currentEditor43948 ready;
no freshWindows build this slice. No blocked or running asset/compile operation.
Library receipts are in ignored Deliverables/library-delivery.json; alloldIDs
preserved. New evidence is ui-v0.4b-*, foot-mask-v0.4b-* underEvidence.

Next visual work: restrained separate arms and planted turns; stool/bed spacing.
Larger stool reserve moves must preserve legacy saved heroine coordinates:
GameModel.IsPlausible currently validates them against the current geometry,
so a geometry-only move could reject an otherwise validsave. Plan compatible
layout validation/migration before moving reservedzones; keep cash/parts/owned.
Then actualProfiler/nativePlayer workload, broader final regression and fresh
Windows build/delivery. Window still ends14:05UTC, reserve13:15 onward.

Optional parent art batch:10 aligned transparent128x192 files: front/back
head+torso without arms(2), upperarmL/R(4), forearm+handL/R(4), same neutral
adult blackbob/creamtee/bluejeans/whitesneakers. Source neutralLibraryID
libfile_8be9a414782c8191b8e526f808e53eba; exactsource headtop12,sole178,center64;
shoulders(50,60)/(85,62), elbows(51,84)/(87,86), wrists(49,105)/(88,108).
Use2px joint overlap, no matte/glow/room/UI or duplicate body pixels.

Static before/after galleries deliberately show all9symbol textures for optical
review; actual spin reward captures and the runtime clip use the current pool.
New Library verified IDs: compact libfile_444f808987b481919535a5cfd8f7ed69;
tall libfile_5aaacdece308819199ade7294c1c3fee; rewards
libfile_ca9c488e58388191ad285f981dd3b87f; current runtimeGIF
libfile_64e549ca1cc08191b60fe4aba934c545. Native metadata verified.


## Bounded motion/save checkpoint v0.4c (2026-10-06)

10 aligned128x192 source cutouts (front/back torso, upperarms and forearms)
are inArt/animation/*-rig-*.png. Rest reconstruction is pixelexact; forearms
have no blue trousers. SourceRGB unchanged; parent needs no new art batch.
UpperBodyRig counter-swings upperarms5.5degrees max with restrained elbow
response. Far arm behind torso/near arm infront, stable shoulder/elbow pivots.
Turn pauses walking.36s, head leads at.10s/body switches at.20s; sourceviews
remain discrete. Mirror pivot67 sits between support legs. Toe masks preserved.

New2:3 PNGs initially autoimported asCubemap despite Default texturetype;
actualrender exposed missingupperbody. Fixed PixelAssetImporter to force
TextureImporterShape.Texture2D, reimported all10; Resources/directload prove2D.
Rig now rejects missing textures and MotionAudit checks6 renderedimages.
Final actual screenshot/clip shows complete body; ignore earlier bad private
frames, which were replaced before delivery. No nativePlayer validation yet.

Stool moved to(1.9,2.3), raw reserve1.75..2.05/2.15..2.45. Bed/stool opaque
silhouette overlap eliminated. Save schema4 validates v2/v3 against previous
reserve, then deterministically projects only blocked feet to nearest free
point. Maximum policy.75m; tested maximum.491m,5628oldvalid gridpoints,1531
relocated. Cash/parts/owned/free/recovery/spins and stablequests preserved;
raw invalid IDs/NaN/out-of-room remain rejected; reload and idempotence pass.
User save never reset or rewritten byQA; allfixtures use privateX profiles.
Do not run oldv0.3 and new schema4 Players together against the same save.

READY/INSTALL TASK copy now reads RESOLVING during pendingreels; after8/8
expiredtoasts default to YOUR ROOM IS COMPLETE. UI Current() captures new
ui-v0.4c filenames; prior Library identities preserved.848UI assertions,
34586economy,432000nav,52session,20998layoutmigration assertions pass.
Motion before/after12s480x960 with actual advancing framecounts, armphase,
turnseen and jointgap checks. Encoded GIF frames/durations are separately
recorded in motion-v0.4c-checkpoint.json. This is Editor diagnostic capture,
not PlayerFPS/realhardwaretouch. UIcallback clip includes SPIN/manualBUY.

Next slice: measured nativeDevelopmentPlayer profiling in a newbuilddir,
with ZTM_QA_SAVE_DIR explicitly set toX/.tooling/perf-qa-save (now enabled
only forEDITOR orDEVELOPMENT_BUILD). Keep oldPlayer46276/userprofile intact.
Capture actualProfiler trace plus steadywalking and burstspin/purchase samples;
report framepercentiles, allocation spikes, paint/animation/navigation costs
and memory with source/evidence. Fix measured bottlenecks only. Do not enable
runtimeMCP/network inrelease. Final regression/build reserved13:15..14:05UTC.

v0.4c Library delivery (all metadata verified):
- Full actual runtime comparison: libfile_d9d203b3a0848191a875420f5f77f44f
- Tracked arm/leg close comparison: libfile_34d459fe4fdc8191bdf54bbe7c43a6e3
- Busy/completed UI state copy: libfile_44b4c59fa62c8191be9186736e2ec95f
Both GIFs decode144frames/12000ms; source144captures each. Before actual
frameCount1..621,after1..602. After101armphasebins, jointgapmax0.6777units.
Current SPIN/manualBUY clip132captures,11.002s,frameCount78..628,2.193m;
repeatedspin/buy,reload,completedhint,resetcancel/confirm passed privately.
Existing v0.3 Player46276 kept responding and brought foreground on owner
request 10:09UTC. No fresh native build or native Profiler result this slice.

## Slice4 native checkpoint v0.4d

See NATIVE_PERFORMANCE_AUDIT.md and native-v0.4d-* evidence. Slice3 committed
102b85a0d0af774a33a5157fcec53cb8f418f7a9. Three bounded reviewer visual fixes
precede all valid native measurements. Rejected hidden/IME/early-show attempts
remain privately under .tooling, excluded from benchmarks. Valid baseline/trace
and optimized Development Players rendered all phases, private saves unchanged
actual userhash;16spins/8buys/25guards each. Measured quest-cache allocation
reduction 21.76%; timing limits/no physicaltouch/release/Android
claims documented. RawProfiler imported596 mainthreadframes. All applicable
model/session/walk/migration/UIchecks and finalUIcallback run passed. No native
Player left running; original46276 preserved. Finalreleasewindow13:15..14:05UTC.

Slice4 Library metadata verified: visual-fixes-v0.4d.gif
libfile_ac20453a80348191a8dbd9ae2d228a0e (144decoded frames/12s).
Native rawProfiler+CSV+reports+reproduction tools ZIP
libfile_7d3e055a70c481919fb0ab5df110cb78. Source code commit
1b0e48c534f5525a5b36477117d728e3be6c0bef. User save SHA unchanged,
originalPlayer46276 responding, own EditorPlaymodefalse, no native testPlayer.
## Slice5 reliability checkpoint v0.4e

Direct owner request to open prototype restored existing v0.3 Player46276,
HWND61870534; no duplicate user-profile instance. User save SHA unchanged.
Native panel lifetime leak proved:10destroyed GameViews left10PanelSettings.
GameView now owns/detaches/destroys runtime panel; final native10cycles leave0,
with10SPIN/BUY/repeat checks each and unchanged nonblack360x720 render.
Final testexe:Builds/Windows-dev-v0.4d-reliability/ZeroToMillionaireDev.exe.
ComputeBuffer warning persists afterfix: finalizer stack only, allocationowner
unresolved. No engine edits/suppression/ForgiveLeaks; bounded investigationdone.
Actual OS mouse blocked by inputdesktop Screen-saver vs processdesktop Default,
foreground0/SetForegroundfalse. NoSendInputattempt/no physicaltouch claim.
Observer guard confirmed fail-before-launch. Actualpress/drag/cancel/rapid/menu/
focus/resize/relaunch pointer cases pending; do not unlock or clickotherwindows.
15min reliability-before native private soak finished53579frames/57cycles,
967spins/460buys/1427guards/57reloads/158.061m,errors0. Late managedheapreserve
expanded6.387MiB; no leakfreedom claim. NATIVE_RELIABILITY_AUDIT.md and compact
Evidence/native-v0.4e-* CSV/JSON include scope/timing/active-desktop limitations.
Soak predates lifetimefix; finalcode has targeted native lifetime/UIregression.
Art/rig frozen. Final release/delivery remains13:15..14:05UTC; Androiddeferred.
Slice5 source commit e0e4c602a5b93f6d1e2828279ea3df9fca9214d9; clean source.
Library reliability-v0.4e-evidence.zip ID libfile_21f26cbb303c8191a7e88a9de5db4403,
341291bytes; metadata verified; SHA256 8CC07C67790E86AC1AEF8F99F5813A58BD7ABFD104524E8373570CF3D24EBC9E.

Slice6 baseline: source ca4e19ec8aa70bf1670bb0b5a1c80cc178ad1f03; clean Release Windows-rc-v0.4f and separate instrumented Release audit twin built with None. Exact clean binary QA/hooks absent, private frame/render/walk/completion/relaunch proven; Twin16spins/8buys/25guards. UTF9passed, synthetic pointer19passed; proved down-face feedback fixed. ComputeBuffer warning remains at shutdown. See NATIVE_RELEASE_VALIDATION.md. New authorized onboarding/story/slow curve proposed in ONBOARDING_PACING_PROPOSAL.md; implement next, codefreeze13:15, final14:05.


Slice7 feature checkpoint 2026-10-06 13:02UTC: implemented authorized skippable/resumable Russian heroine tutorial, help-only replay, four finite saved milestone comments, schema5 exact stored requirements, future slow curve, legacy current task readiness preserved. Original user Player46276 not closed/reset; owner request to open restored nonminimized window61870534, but foreground false/inputdesktop Screen-saver. No security/input bypass. User save unchanged.
16 critical EditMode tests pass; full core107769 assertions,1000 seeded complete levels49..60/p50=53/p95=57; first buy p50=3/p95=4. 432000 walking samples,52 session checks,21106 layout checks. Actual Editor UI initial/context/purchase/skip/replay/reload/reset/story passes615frames/10.71s;360x640,360x800,480x960,44high speech buttons. All Russian literal glyphs present; OFL derivative only aliases existing capitals Pe/O, no outline/art changes. See ONBOARDING_PACING_VALIDATION.md.
Four Library actual frames already saved and metadata verified: initial libfile_f2b56587f63c8191b5f40441512a77f5; context libfile_1f1b3b62e3e881919e21b892771d49ec; surfaces libfile_c7b3136f336c8191ae0df4a3df326bcd; complete libfile_fac7f9f37ba08191943a3edffd23aa5c. Prepared helper unavailable before writes (Windows prepare_uploads absent); supported ordered direct fallback succeeded.
Next: clean None Release v0.5 + separate audit twin, actual full new level (no completion fixture), private clean relaunch and portrait/performance/cleanup, archive/source/Library by14:05UTC. Art/rig frozen; Android deferred; ComputeBuffer warning remains unsuppressed.


Final validation checkpoint v0.5 2026-10-06 13:36UTC: clean Windows Release built from frozen game source17f525238b1925819b857f9bfd31cb5ec4062126; named final folder Builds/Windows-FINAL-v0.5 copies all runtime bytes identically, excludes Burst DoNotShip and adds Russian notes. Exact clean fresh/relaunch/complete/old-save processes passed; six more persisted dialogue fixtures passed. Actual synthetic Windows PostMessage did not invoke buttons while Screen-saver active, no hardware/click success claimed.
Native full curve53spins/8manual buys/61repeat guards,135s/8087sequentialframes, complete/reload/resize/feetclear. Frame median16.67ms,p95progression17.44,max52.84;95sprogressionphase includes post-completion idle. Native test-only Release v0.5b (unchanged game methods plus excluded controller)23dialogue checks/488frames: skip/resume/help replay/no rewards, busy story queue/seen suppression/latest collapse/reset. Exact clean binary action clicks remain unrun; its persisted-state interpretation is validated.
Final actual Player initial Librarylibfile_b251433e616481919930631b881aee5e; complete libfile_21f33ccc01fc8191b1294e282c610ce9; synthetic native-validation GIFlibfile_05375b8f46148191ae9958efffa70fa8,96PNGcaptures/74decodedmergedframes/7.68s. All Librarymetadata verified. See NATIVE_RELEASE_VALIDATION_V05.md/RUN_WINDOWS_V05.md. No actionable visual findings in independent parent review.
Original Player46276/window61870534 and save SHA4B752F42C8E3354B7926DAF797E19AF60BC5FEE9ECB01BDEA2F144FB6724162F preserved; no user-profile duplicate, unlock or cursor injection. ComputeBuffer warning remains unsuppressed; Android deferred; all owned QA processes normally closed. Final source and Windows archives/IDs/hashes pending packaging; receipt will be in ignored Deliverables/final-v0.5-library.json. Late HEAD consists only docs/testing/packaging; production files diff against17f5252 is empty.


Final delivery 2026-10-06 13:47UTC: both complete archives uploaded successfully (no split): Windows libfile_07492a6a2c3081918854b57dea6bcaa7,37000732bytes,SHA2d5f663eaccbd9eb4005576a162b4815fc17b2c33693e2ed1a905c2640fba2c0; source libfile_c07541e7be2081918c874786d94d95cb,31679877bytes,SHAfd34be4fb29c625266abb5ef470a0198b89a45f6f8c15afea2b8eabe99918bfc. Metadata and transferred byte hashes verified. Source archive snapshot48daf9f9e2c62f49e42b97582c88fcb544fedeef; game build17f525238b1925819b857f9bfd31cb5ec4062126; final local docs HEAD follows delivery receipt commit.
Both CRCs/extractions passed;158Windows files and source code hash-match; all6refs present, private/caches excluded. Extracted Windows archive launched867frames and closed normally, QA hooks absent. Final nativeimages/GIF LibraryIDs above remainverified. Canonical runnablefolderWindows-FINAL-v0.5. DELIVERY_V05.md is concise full artifact/hash/limitation record, ignored Deliverables/final-v0.5-library.json has machine receipt. Keep original userPlayer/save; no QAPlayer remains. Final Git clean after receipt commit; no pushes. Parent read-only review accepted all4onboarding images.


Delivery correction: actual main-chat full ZIP attachment was rejected as over its limit despite successful Library retention. Both originals preserved; source and Windows split to `.zip.001/.002/.003`≤15,000,000bytes. Installed7-Zip extracted both streams successfully; all source/ref/runtime hashes verified. Six new Library files/metadata/partSHA verified; exact IDs in DELIVERY_V05.md and ignored Deliverables/final-v0.5-split-library.json. Parent owns final main-chat attachment; do not claim that Library creation proves it succeeded. No rebuild/code changes.
