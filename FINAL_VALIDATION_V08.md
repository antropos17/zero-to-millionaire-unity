# v0.8 final Windows validation

Windows Release: Builds/Windows-v0.8-candidate-1/ZeroToMillionaire.exe.
Game/build source46efc6581ff8a0c5e3eda661587f30fd49e010dc; Unity6000.3.21f1;
BuildOptions.None; audit defineFalse; bundleVersion0.8.0. Build succeeded in35.34s.
The source/runtime/room/font/scene/package/settings diff against that commit is
empty. BUILD_IDENTITY.json and windows-v08-candidate-1-hashes.json record bytes.
Font OFL licenses and modification provenance are shipped with the executable.

The six-reference art, English typography and purposeful motion passed the
parent's independent visual review. The source review's interrupted dialogue
entrance was reproduced and fixed; see V08_REVIEW_FIXES.md and before/after
entrance reports. Focus cancellation was preserved. No economy/save/schema
change, network/login, payments/ads, publication or external push was added.

Final corrected code:12 focus checks;24 interrupted entrance checks;326 broad
input checks;30 STOP/SKIP/calm checks;24/24 Critical EditMode PASS. The broad
timeout is now explained and corrected as an old-instance reacquisition loop
in the test's deferred scene-load handling, not a stuck second spin. Its final
326-check run passed in16.39s. Earlier failures are retained and not called PASS.
Previously passed English/layout checks:2624/160 combinations at320x640,
360x640,360x800,480x960; dialogue540/64 layouts. Fonts have full ASCII and fixed
digit advances. Core checks:107769 economy assertions,1000 completed levels,
52 sessions,20998 migrations/layout cases and432000 walking samples.

Fresh final actual Editor progression passed39054 checks:50 spins,8 manual BUY
transactions,4 story dismissals. Grid cash/parts, starter spins, both-resource
purchase gates, repeated buys, each disk commit, actual complete-room scene
reload, calm preference, cancelled reset and confirmed reset reload all passed.
No fixture setter was used in that progression. Editor timing:7451 samples,
median16.58ms,p95 21.73ms,max1749.66ms with screenshot pauses;128.03s total.
This timing is an Editor observation, not a device/gameplay benchmark.

Clean native Release: eight successful launch/quit/relaunch processes for
fresh360x640, actual Editor-completed state480x960, old private test-save360x800,
and actual confirmed-reset state320x640. All produced actual D3D11 pixels,
advancing frames, nonzero draw counts and walking. Exact cash/parts/grid/owned
state survives process relaunch. The complete state retained50 spins,61 coins,
8 owned upgrades and all15 story bits; the confirmed reset retained0 coins,
3 free spins and fresh tutorial. Development/QA action harnesses are absent.
Basic native idle intervals were approximately4.16ms median and5.94–6.08ms p95
over roughly1500–2100 samples per process. See native-v08-release-summary.json;
these are short idle observations, not a full gameplay/device performance test.

Input coverage is actual UITK panel picking and synthetic pointer/keyboard/
navigation events in the graphics Editor. Native processes used read-only
observation and no OS input or activation. Native transaction/full progression,
physical touch/keyboard/mouse, Androidv0.8 export and real devices are untested.
The pre-existing Unity ComputeBuffer finalizer warning remains unresolved and
unsuppressed; its native allocation owner has not been established. Existing
v0.7 builds and the user's live game/save were not targeted by QA.

Delivery: complete source and Windows ZIPs are split into standard .zip.001/
.002/... byte streams, each at most15000000 bytes. Extract .zip.001 with7-Zip
while all sibling parts are present. Split receipts contain archive/part hashes,
CRC and all extracted-file verification; an extracted Windows launch/relaunch
is checked separately. Source includes all six references, independent reusable
art, fonts/licenses, scenes, code, checks, actual screenshots/GIFs and documents;
private saves, caches and local tooling are excluded. See RUN_WINDOWS_V08.md.

Ready criteria remain one fixed9m² offline level; correct cash/parts; explicit
both-resource BUY, repeat/overdraw guard, visible persistent upgrades; complete
level; save relaunch and confirmed reset; compact/tall portrait/performance;
honest Editor/native/device coverage; exact final source/build/part identity;
APK when included in the agreed scope. Android is currently deferred.

Extracted delivery verification also passed:162 original build files hash-match after7-Zip split extraction, then two actual clean extracted-executable processes rendered and retained fresh resources. This makes10 successful native process runs in total. See native-v08-extracted-1-checks.json and split-v08-windows-7zip.txt.

