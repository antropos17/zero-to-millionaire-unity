# ZERO TO MILLIONAIRE v0.9 validation

Release: X:\Projects\ZERO-TO-MILLIONAIRE\Builds\Windows-v0.9-candidate-1\ZeroToMillionaire.exe
Build source: f2a04fedc065e0506a181ac77dac47b0c5a799c0; Unity6000.3.21f1; version0.9.0; clean Release, no audit define.

Virtual stakes1x/2x/3x cost5/10/15 coins and scale all cash rewards. Parts stay
one per matching visible icon; starter/recovery free spins reward1x. Choice is
persisted, locked during spins, and each spin captures its immutable multiplier.
SHOP uses the same GameSession purchase path, shows eight existing item icons,
room effects, both requirements and quest gates. It cannot buy a future task.

Final applicable checks:

- .NET:107769 economy assertions,52 session,20998 layout migration,432000 walk samples.
- Unity JSON:107769 economy,52 session,21106 layout migration,432000 walk samples.
- Both:370857 new stake/schema6 assertions and3000 full-level simulations each.
- Pacing1000 seeds/tier:1x mean53.333 spins;2x/3x53.277; every median53 and p9557.
  Higher stakes preserved the part curve; mean final coins236.076/2065.596/3900.283.
- Unity Test Framework24/24 PASS.
- Rendered UI657 PASS across320x640,360x640,360x800,480x960 and full/calm modes.
  Actual UITK synthetic pointer pick/down/up and keyboard/navigation, modal focus,
  no click-through, paid3x/free1x, shop guards, repeated STOP/SKIP/BUY and persistence.
- Fresh actual Editor full level:55 spins,8 explicit SHOP pointer-event buys,
 4 stories,592752 checks; room changes, scene reload and confirmed/cancelled reset.
- Editor full-level timing8413 actual frames:median16.66ms,p9517.78ms,max252.52ms.
  Includes screenshot pauses; this is NOT a native/device benchmark.
-8 hidden private native Release processes PASS for schema5 migration, stake3,
 fresh/complete load, normal self-quit and relaunch. These windows did not render;
 zero draw counters and black captures were rejected. No native pixels/performance
 or physical input coverage is claimed from hidden launches.

The first catalog test exposed retained bottom scroll on reopen; opening SHOP
now resets and scrolls to the current task. The final four-size/two-mode run passed.
An initial overflow-test expected an all-cash grid; its expectation was corrected
for the existing guaranteed matching-icon rule. No economics were weakened.

Native pointer/keyboard and visible-window rendering remain unverified in this
pass. Installed Computer Use guidance requires “Use node_repl JavaScript for all
Computer Use actions”; node_repl is not exposed. No alternate OS input/window
message helper was used. See the installed skill at
C:/Users/murtu/.codex/plugins/cache/openai-bundled/computer-use/26.930.61225/skills/computer-use/SKILL.md.
The existing ComputeBuffer shutdown warning remains unsuppressed and unresolved.
No new lead, engine upgrade or warning suppression was introduced.

Old releases, all approved art pixels, unrelated files/processes and the user's
ordinary save were preserved. Schema6 is read by v0.9; older releases cannot read
upgraded saves. Close any older game before normally launching v0.9. Actual user
save migration was not performed by QA. Android/APK/device testing remains deferred.

Read RUN_WINDOWS_V09.md for extraction, launch, source and isolated review steps.
No installer, backend, purchases, cashout, advertisement, publish or push was added.
