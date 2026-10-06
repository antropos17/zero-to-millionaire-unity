# Verification receipt - v0.2

Final code passed 47,594 economy assertions and 1,000 complete levels:
18/23/25/28 spins (min/median/p95/max). Checks cover single-counter rewards,
active-pool transitions, cash-only and fragment-only purchase rejection,
prerequisites, repeat/concurrent BUY, nonnegative resources, starter/recovery,
legacy save migration, malformed saves, backup recovery, relaunch and reset.

Walking passed 432,000 samples across 12 seeded runs: 1,560 destinations and
1,843.7m travelled. Bounds, furniture and door swing remained clear. Speed,
stalled frame integration, exact corner crossing and no teleport were checked.
The discovered corner-cutting issue was fixed with exact segment/AABB checks.

Final Unity runtime checks passed at 360x640, 360x800 and 480x960. Live spin
coroutine committed once; repeated BUY handlers completed all eight upgrades;
completion persisted; reset cancellation and confirmed disk reset behaved
correctly. Screenshot files are real editor renders of staged progression.
All four final captures were visually inspected with the adult-sized heroine.

The final walking demo records actual GameView updates: 100 frames over 11.9s,
2.11m travelled, every feet sample clear, all four phase markers visited and
position restored from disk. The neutral cutout rig swings separate left/right
legs continuously. GIF and filmstrip were rebuilt from the final captures and
visually inspected. Editor frame samples: median 7.73ms, p95 27.53ms. These are
editor measurements on this desktop, not an Android benchmark.

Windows v0.2 built successfully. The new executable was launched in a visible,
responsive ZERO TO MILLIONAIRE window (PID 46968; verified HWND 64033870).
The old own v0.1 player was closed normally before opening v0.2 so it could save.
No gameplay exception appeared in the new desktop launch log.

Android build and signed APK inspection are recorded separately in
Evidence/android-v0.2-build.txt and Evidence/android-v0.2-verification.json.
See the final build receipt for exact APK size/hash and permissions.

Official Unity MCP: Unity CLI 1.0.0-beta.12 with com.unity.pipeline 0.8.0-exp.1.
Project-targeted stdio initialize and tools/list returned 160 tools; actual
get_build_settings and get_runtime_pipeline_settings reads succeeded. Runtime
pipeline enableInBuilds was false. Global Codex MCP is registered; the existing
chat tool surface needs a new session/reload to expose the new server natively.
No Assistant legal acceptance, AI cloud access or network policy change occurred.

No connected Android device or emulator was available. Physical touch, Android
installation/launch and Android performance remain unrun. Tests invoked the same
handlers as the controls, without simulating a physical touchscreen. Font and
some decorative UI artwork use live controls approximating the references.
The gait uses generated neutral sprite parts and procedural leg rotation; the
four-column drafts had duplicate poses and are not presented as finished frames.

Preserved v0.1 APK/source delivery. No store publication, login, ads or payments.

Final Android result: build succeeded in 4m13.6s; actual APK 24,021,296 bytes.
SHA256: e4dcf6ebc573867fa716089926c97d021c13738bb9e8a0df150ad69dc7bb652c.
Version 0.2.0/code 2; com.prototypeworkspace.zerotomillionaire; ARM64;
min API26 / target API36 / portrait. APK v2 signature verified with one signer,
same certificate as v0.1 (supports update). No INTERNET or storage permissions.
Only the app's internal DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION was present.
ADB returned no devices. Build summary total includes native symbol outputs;
the number above is the actual installable APK. Split ZIP extraction with the
installed 7-Zip is hash checked; it does not alter the signed APK.
