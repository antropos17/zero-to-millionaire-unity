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
