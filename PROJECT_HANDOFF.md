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

## Verification and remaining work

Current quest code: 34,586 model assertions / 1,000 completed seeds (14–21 spins,
median17); 432,000 navigation samples. Live Editor UI flow17spins/8buys,
migration, resource gates, repeated taps, save-before-cosmetics, reset/recovery,
three portrait sizes and reward feedback passed. Editor-only performance is
7.18ms median/39.69ms p95; no physical-phone or emulator claim.

Windows-v0.3/ZeroToMillionaire.exe was built BEFORE depth/quest changes.
Android v0.2 and old source/splits remain immutable. Final v0.3 builds are pending.

Next bounded slices: extract save/rollback orchestration from GameView into a
small testable offline session; validate persisted stable-ID shapes before
canonical migration; inspect reset/save-failure and overflow edges. Recheck UI
against all6refs at matched size and report measurable scale/font/spacing/state
differences. Return visual/regression checkpoint, then final Windows executable and source.
Latest owner instruction prioritizes desktop; Android builds/splits are deferred.
Keep the Android toolchain and immutable old APKs; do not delete capability.

Commands from project root:
dotnet run --project Checks/Checks.csproj --no-restore -- Evidence/checks-tmp
python Tools/verify_unity_mcp.py get_build_settings
Unity batch build methods: ZeroToMillionaire.Editor.BuildPrototype.Windows
and .Android; final QA: ZeroToMillionaire.Editor.CaptureQA.Start with
ZTM_QA_SAVE_DIR set to X:\Projects\ZERO-TO-MILLIONAIRE\Evidence\qa-save-v0.3.
Direct every new build/cache TEMP/TMP to this project's Temp directory.
Never run old Tools/package_project.py: it overwrites the v0.2 archive.

Final acceptance: source and intended clean local Git on X, documented offline
architecture, full finite level, both-resource and repeated-tap guards, migration,
save/relaunch/closing midspin and confirmed reset, free recovery, randomized
movement clearance, actual live UI playthrough,3portrait sizes, visible responding
Windows player, verified source and confirmed Library IDs. Android packaging is
deferred by the latest desktop instruction. Preserve user progress and distinguish device/editor tests.
This file is a durable context summary; it does not claim chat compaction.
