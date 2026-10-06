# Native performance/input checkpoint v0.4d

Measured Unity6000.3.21f1 Development Mono/D3D11 Player on Windows11,
Core Ultra9 185H / RTX4060 Laptop /32GB. Original v0.3 Player and idle Editor
remained open. Private audit JSON profiles, actual SaveStore.path verification;
user save SHA256 unchanged before/after, originalPID46276 responding.
Target60FPS, vSync0 only in auditPlayer; original vSync1/target60 recorded.
Final ordinary game window is independently verified by drawcalls in >90% of
every phase and actual rendered screenshots. Hidden attempts are excluded.

Seed57 workload:5s warm,10s idle walk,20s continuous callback spins/manual
buys,5s ready final task,10s completed walk,10s actual portrait resizing.
All valid runs complete16spins/8buys/25 repeat guards, private reload and
clear feet. Callback input is NavigationSubmitEvent; physicaltouch/OSpointer,
Android, long thermal endurance and uninstrumented Release are untested.

| Workload | Before median/p95/p99/max ms | After median/p95/p99/max ms | After >33.33ms / >50ms |
|---|---|---|---|
| idle_walk | 16.66 / 17.54 / 18.33 / 19.79 | 16.67 / 17.22 / 17.76 / 18.29 | 0 / 0 |
| progression_spins_buys | 16.67 / 17.87 / 22.13 / 43.43 | 16.67 / 17.47 / 18.28 / 40.27 | 1 / 0 |
| ready_final_task | 16.67 / 17.74 / 18.42 / 21.23 | 16.67 / 17.42 / 18.31 / 20.97 | 0 / 0 |
| completed_walk | 16.67 / 17.34 / 18.20 / 34.04 | 16.66 / 17.44 / 17.88 / 21.82 | 0 / 0 |
| resize_portraits | 16.66 / 17.32 / 18.05 / 27.50 | 16.67 / 17.28 / 17.92 / 25.71 | 0 / 0 |

A measured redundant quest rebuild at reel completion prompted a bounded
cache keyed by active/next quest and cash/parts. Same balances keep elements;
busy wording and BUY gates still update. Full actual resource/quest changes
rebuild as before. Progression allocated 7,033,645 -> 5,503,247 bytes
(21.76% fewer). ZTM.DrawQuest aggregate time
29.00
-> 14.44ms.
Allocation peak remains 329780bytes; no claim that all
upgrade peaks disappeared. A single timing pair has driver/scheduling noise.
Navigation/pose costs stay small; no pathfinding/art/texture rewrite justified.

ProfilerRecorder LastValue describes the preceding completed frame. Stopwatch
intervals include limiter/waits/scheduling and audit overhead; CPU Main Thread
Frame Time is separately recorded, unlike inclusive MainThread. Nested markers
overlap and must not be summed. Fixed sample arrays allocate before measured
phases; action assertions add snapshots. Separate trace run uses same workload,
binary profiler enabled only seconds15..25; its timing is separate from baseline.
[ProfilerRecorder API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html)
and [binary log API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Profiling.Profiler-enableBinaryLog.html)
were checked against6000.3 documentation. GPU/render/memory availability is
recorded per phase; absent/zero-only counters are unavailable, never invented.
Unity imported the raw trace:596 valid mainthreadframes, exact receipt and
selected native samples in Evidence/native-v0.4d-profiler-import.json/CSV.

The shutdown log warns about ComputeBuffer cleanup. Prototype code constructs
no ComputeBuffer; origin is not isolated, and60seconds does not establish
long-session leak freedom. Development builds include Unity's standard local
Profiler discovery/connection. Release audit code and QA hooks are excluded;
runtimeMCP remains disabled. No game network/monetization features were added.

Visual fixes predate every valid baseline: original source-alpha252..253 was
wrongly removed from two elbow overlap rows; those exact source pixels were
restored, preserving outer outline. Mattress footprint ordering now keeps
exposed side/front feet ahead of the long mattress. Facing source swap waits
for zero-strength neutral/planted feet, without changing the route reserves
or schema4 migration. Motion:144captures/12.002s,frameCount1..604,
102armbins, maxgap0.678UIunit. No new art batch needed.

Verification after cache:34586economy,432000walk,52session,20998migration,
848UIassertions. Latest callback UIclip132captures/11.000s,frameCount80..671:
manualBUY/repeat/reload/busycopy/completedhint/resetcancel/confirmedreset pass.
Own QA Playmode stopped; all private nativePlayers exited.

Reproduce from project root with UnityEditor connected:
1. Build Assets/Editor/NativeAuditBuild.BuildBaseline with fresh permitted tag.
   Existing build directories are guarded against overwrite.
2. powershell -File Tools/launch_native_audit.ps1 -RunName NEW_RUN -BuildTag optimized
   Optional -Trace true. New private output required. It opens an interactive
   Unity window, sets process-local QA variables and exits automatically after60s.
3. python Tools/summarize_native_audit.py NEW_RUN
   Prefix the run name with rendered to require drawcalls/nonblack screenshots.
Do not launch new schema4 and oldv0.3 against the same live user profile.
Current testexe:Builds/Windows-dev-v0.4d-optimized/ZeroToMillionaireDev.exe.
This is a Development audit build; final releasebuild/delivery remains for
the reserved13:15..14:05UTC window. No APK/release build claimed for this slice.
