# v0.7 final validation

Final game/build source: dd74c983dcb22eaa7a03b54027dfa3b3aefe8926. Unity 6000.3.21f1, Windows x64, clean Release (BuildOptions.None), version 0.7.0, build GUID 2e8a06664d1244b6b7d05853db57be4e. Assembly-CSharp.dll SHA256 cb9cdebe084ecf310bdb6e878042e3f67135d79787291037fd0578f8de726bf5. Production scripts, scenes, resources, packages and project settings match that source. Later commits contain Editor checks, tooling, documentation and evidence. SOURCE_VERSION.json records the exact source archive documentation/tests HEAD.

Runnable folder: Builds/Windows-v0.7-candidate-1. This candidate is the final delivered binary; it is not rebuilt or modified during packaging. Windows-v0.6-final and previous releases remain preserved. RUN_WINDOWS_V07.md supplies Russian launch instructions. Full source includes independent production art, font/OFL license, all six approved references and Editor checks. No ads, payments, login, server or publication.

## Applicable checks

| Criterion | Final evidence and scope |
|---|---|
| Spin, cash and per-upgrade fragments | Fresh end-to-end Editor scene; 55 real spins, eight manual purchase callbacks, 47,492 assertions. No fixture state setter. Synthetic UI callbacks, actual scene/render/save code. |
| Both-resource gates, repeated taps, rollback | Final usability suite: 1,790 assertions; repeated buys cannot debit twice. 24/24 critical EditMode tests. Paired Tab+navigation and navigation-only traversal, held Enter/Space, modal ownership, failed-write rollback and save-status feedback covered. |
| Visible repairs/items and full completion | Actual Editor frames for each of eight purchases, same 9m² room; completion and all installed changes survive scene recreation/save reload. |
| Save and reset | Full level checks include cancel/reset confirmation and reload of reset state. Native clean Release loads that separately copied confirmed-reset save, quits and relaunches: 0 cash, three free spins, zero purchases. Native reset-button physical input was not tested. |
| v0.6 save compatibility | Existing private v0.6 schema5 save copied to a separate directory. Actual Release quit/relaunch preserves cash170, spins15, two purchases, all fragments and undismissed story. User save was never used as a fixture. |
| Portrait layout | 2,624 assertions across 160 combinations: eight tasks, five states and four actual layouts (320x640, 360x640, 360x800, 480x960). Final dialogue/main/menu reviewed at 360x640, 360x800, 480x960. |
| Font/readability | All 145 production literal characters covered; digit advances tested through weight400/550/700 and actual rendered counters. Focus, hover, pressed and disabled states reviewed; enabled text contrast >=4.57. |
| Performance | Editor full scenario: 8,110 frames, median16.547ms, p95 20.348ms, max293.895ms including screenshot pauses. Native unfocused read-only rendering: >600 advancing frames and >600 nonzero draw samples per private relaunch. This is basic observation, not a device or interactive benchmark. |
| Extracted delivery | Final Windows split stream extracted with installed 7-Zip, exit0; every one of 158 files SHA256-matches the packaged candidate. Extracted executable actually launched, rendered fresh pixels, closed and relaunched twice with exit0. Fresh profile has 0 cash, three free spins, no purchases and introduction. |
| Release separation | Static IL and actual private read-only observer confirm development=false, no QA GameView methods and no automated harness types. Read-only observer cannot purchase, reset or send input; active only with explicit validated private-profile argument. |
| Offline progression | Unchanged model regression: 107,769 economy assertions, 1,000 completed levels (49–60 spins, median53, p95 57), 432,000 motion samples, 52 session checks and 21,106 migration/layout assertions. |

## Native input and limitations

Actual native screenshots are native pixels, not Editor screenshots. All three latest native cases (private v0.6, confirmed-reset state and final extracted executable) ran on the Default desktop, drew correctly, closed/relaunched with exit0 and preserved their own disk state. Windows were shown without activation. They were never verifiably foreground, so no OS keyboard or mouse input was sent. Physical native keyboard, pointer/touch, native gameplay completion and native reset confirmation remain untested. Editor input tests use real UI Toolkit event paths but are synthetic. No Android v0.7 build, emulator or real-device test is claimed; Android remains deferred.

A first hidden-only legacy harness rendered no GPU pixels (black screenshot) and was rejected. The supported ShowWindow(SW_SHOWNOACTIVATE) read-only harness then passed both rendered runs. Failed evidence remains local and is not presented as success.

Unity still logs the known ComputeBuffer disposal warning on shutdown. Some unfocused Direct3D11 runs also log presentation timestamp/input-latency warnings. These were not suppressed or fixed; they limit performance interpretation.

The user profile was checked only by hash. Final hash equals the pre-test baseline: 785be6ebbcbaee012786e88d3acf7b67c45ddddea1faf0d87de67083a5d2cfbc. No user reset, fixture write or user Player automation occurred.

## Reproduce / open source

Run the extracted ZeroToMillionaire.exe beside ZeroToMillionaire_Data and companion files. Installation and internet are unnecessary for gameplay. Keep every archive part in the same directory and extract only .zip.001 through 7-Zip.

Open the complete-source folder in Unity 6000.3.21f1, then Assets/Scenes/Level1.unity. The source project may require the existing Unity package cache/registry to import Editor packages; this is distinct from offline runtime. Relevant checks are Assets/Tests/Editor (run through CriticalTestRunner.Start), UsabilityRegressionV07, RussianLayoutAuditV07 and FinalLevelAuditV07; use their documented guarded Editor entry points. Native read-only checks use Tools/native_readonly_v07.ps1 with a NEW private Prefix and explicit executable. Never pass the actual user profile to a test.
