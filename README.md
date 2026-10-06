# ZERO TO MILLIONAIRE - desktop prototype v0.3

Offline portrait single-player Unity prototype. One fixed 3x3m (9m2) room starts wrecked and empty, then gains eight modest repairs/items: floor patches, surfaces, cheap wallpaper, door, window, mattress, lamp and stool. There is no room expansion, login, ads, payment or network requirement. All reusable art was generated separately for this project from the six approved references.

## Run

Open Builds/Windows-v0.3/ZeroToMillionaire.exe, keeping its companion folders beside it. Canonical project: X:/Projects/ZERO-TO-MILLIONAIRE. The latest Windows build is already open on the connected computer. To edit, open the project with installed Unity6000.3.21f1 at X:/Programs/Unity/6000.3.21f1/Editor/Unity.exe; load Assets/Scenes/Level1.unity and press Play.

## Play and balance

SPIN shows a 5x3 grid. There is one active task and one locked NEXT task. Every visible current-item symbol gives one part for that task; cash symbols pay +2, every spin pays base12, and matching vertical triples add10. Once the part goal is met, extra item rolls become visible cash. If both resources are ready, install before spinning again. Manual BUY & INSTALL requires both the task's parts and cash, deducts once and applies the visible repair/item. Completion8/8 ends collection.

Paid spins cost5. Three free starter spins and explicit free recovery below5cash with no allowance prevent deadlock. Prices total488cash; goals total98parts. Economy.cs holds tunable values. Across1000 seeded simulations completion takes14-21spins, median17/p9518. These values support a short playable prototype; reference picture balances are illustrative.

The adult heroine walks continuously between reachable destinations with grounded feet and reserved furniture/door clearance. Neutral front/back sprites are cut into separate limb pieces and animated procedurally; this is not24 hand-painted walk frames. Room surfaces, character, furniture, symbols and live controls remain separate editable assets.

## Save and reset

The existing local filename level1-v1.json is retained, now schema3. Windows saves in %USERPROFILE%/AppData/LocalLow/PrototypeWorkspace/ZERO TO MILLIONAIRE. Migration preserves legacy cash, ownership, allowances and all excess parts. Actions commit before cosmetic animation; failed writes preserve the prior live state. Invalid primary data selects a validated backup. RESET in MENU needs confirmation. Do not downgrade a schema3 save into old builds.

The fresh launch preserved87cash/9spins/2owned. The user then actively progressed to120cash/18spins/7owned. A forced close was avoided. When the user had already closed the player, the unchanged build was reopened. Live gameplay changed resource/quest fields before the after-load observation, so exact player-relaunch progress equality remains unverified. No save was reset or restored by the assistant. Independent disk/relaunch and failure tests passed.

## Verification and limits

Final code passed34,586 economy assertions,1000 full simulated levels,432,000 walking samples and52 offline session checks. Fresh actual Unity control callbacks completed all8purchases in16spins and checked repeated taps, save-before-cosmetics, recovery, menu and reset confirmation/cancellation. Portrait UI was rendered at360x640,360x800 and480x960. See VERIFICATION.md and Evidence/runtime-v0.3-qa.txt.

All six approved refs were inspected as actual pixels. Evidence/v0.3-six-reference-review.png pairs matched room stages. The heroine scale was corrected uniformly without changing collision footprints. Remaining visual differences: the generated room is steeper than the references; neutral walking poses and licensed Pixelify font are approximate. The finite active/NEXT task panel is the gameplay adaptation.

Android packaging is deferred by the desktop instruction. Installed SDK/NDK/JDK and earlier signed v0.1/v0.2 APKs remain unchanged. No physical Android device, emulator, touch or battery test is claimed. There is no fresh v0.3 APK.

## Source and local tooling

Run local model/session checks:
dotnet run --project Checks/Checks.csproj --no-restore -- Evidence/checks-tmp

Unity build method: ZeroToMillionaire.Editor.BuildPrototype.Windows. Keep build/cache TEMP and TMP under this project's Temp. Tools/package_project_v03.py makes the current source archive; do not run old Tools/package_project.py, which targets the old v0.2 archive.

The official Unity CLI1.0.0-beta.12 and project-pinned MCP use .tooling/unity-cli/bin/unity.exe with com.unity.pipeline0.8.0-exp.1. Runtime pipeline is disabled in builds. Tools/verify_unity_mcp.py verifies the standard local protocol. Private helpers, cache, credentials, user saves and build binaries are excluded from Git/source ZIP. This is a local repository with no remote or push.
