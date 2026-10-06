# ZERO TO MILLIONAIRE - playable v0.2

Offline portrait single-player prototype. One fixed 3x3m room progresses through
8 modest repairs/items. No ads, payments, login, publication or room expansion.
All graphics were newly generated for this project from the six approved refs.

SPIN shows 5x3 symbols. Each item icon credits ONLY its own fragment counter.
Cash icons pay +2; base payout is12, matching vertical triples add10. Paid spins
cost5; start with3 free spins. Free recovery grants1 spin only below5 cash with
no allowance. BUY needs BOTH the item's fragments and cash, plus the preceding
upgrade. It deducts once, then applies the visible change. Arrows show all8 rows.

The pool starts with cash, broken floor, cracked wall, door and window. Purchased
icons leave it. Surface repair replaces cracked wall with wallpaper; wallpaper
introduces mattress. Mattress enables lamp, then lamp enables stool. Owned rows
use repaired door/window and dressed mattress variants. Unspent progress stays.
Prices total488 cash, goals98 fragments; 1000 simulations complete in18-28 spins,
median23/p9525. Economy.cs is tunable; this is prototype pacing.

The adult heroine picks reachable floor destinations, accelerates, slows and
pauses. Generated neutral front/back art becomes a 2D rig: separate legs swing
oppositely with small lift, continuously driven by distance. Head/pelvis retain
their view, whole rig mirrors for leftward travel; idle feet are planted. Draft
stride sheets were rejected as a finished walk cycle. This is procedural limb
animation, not24 distinct generated frames. Furniture/door footprints reserve
safe placement zones before purchase, so items never spawn through the heroine.
The same metre geometry projects feet and controls depth order. Mattress along
left wall, lamp by its head, stool nearby; door/window and centre passage clear.

## Run

Use Unity6000.3.21f1; open Assets/Scenes/Level1.unity and press Play.
Installed editor: X:\Programs\Unity\6000.3.21f1\Editor\Unity.exe.
Windows: Builds/Windows-v0.2/ZeroToMillionaire.exe with companion files.
Android: Builds/Android/zero-to-millionaire-v0.2.apk, ARM64, Android8/API26+.
Prior v0.1 APK is preserved. For already authorized USB debugging:

```powershell
& 'X:\Programs\Unity\6000.3.21f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe' install -r '.\Builds\Android\zero-to-millionaire-v0.2.apk'
```

No phone was connected; physical Android/touch/battery tests remain unrun.
See VERIFICATION.md for actual editor/build coverage and Evidence for renders.

For split delivery, place zero-to-millionaire-android-v0.2.zip.001 and .002 in
the same directory, open .001 with 7-Zip and extract. The extracted APK is
byte-for-byte identical to the signed build. No APK conversion or merging needed.

```powershell
& 'C:\Program Files\7-Zip\7z.exe' x '.\zero-to-millionaire-android-v0.2.zip.001' '-o.\ZERO-TO-MILLIONAIRE-Android'
```

Build from the extracted project with the installed Android module:

```powershell
& 'X:\Programs\Unity\6000.3.21f1\Editor\Unity.exe' -batchmode -nographics -projectPath $PWD.Path -executeMethod ZeroToMillionaire.Editor.BuildPrototype.Android -quit -logFile "$PWD\Evidence\android-build.log"
```

## Saves and source

Existing level1-v1.json filename is retained, internal schema2. Migration keeps
v1 cash, purchases, ALL unspent fragments, spins and recovery allowance. Walking
feet position saves too. Actions/pause/quit commit locally; corrupt primary falls
back to validated backup. RESET asks confirmation and rewrites primary/backup.
Downgrading to v0.1 after a v2 save is not supported.

```powershell
dotnet run --project Checks/Checks.csproj -- Evidence/checks-tmp
```

Source ZIP includes production PNGs, code, six references, screenshots/GIF and
identity/crop receipts. Raw generated atlases remain local and in Library IDs
listed in ASSET_MANIFEST.md; needed only to rerun cropping, not to play/build.

## Unity MCP

Official unity@unity-agent-plugin0.1.8-beta installed. Official Unity CLI
1.0.0-beta.12 lives outside Unity's disposable Temp at sibling
unity-mcp-tools/bin/unity.exe. Local com.unity.pipeline0.8.0-exp.1 installed;
its runtime server is disabled in builds. No Assistant/cloud package, sign-in,
new agreement, credential grant or sandbox network-policy change used.

Codex stdio server unity-zero-to-millionaire is pinned to this project.
Initialize,160 live tools and a harmless build-settings read were verified.
Native tools in an existing chat need a session reload; Tools/verify_unity_mcp.py
can verify the standard protocol now. CLI binaries and global config backups
are outside the source archive. MCP receipts contain no instance-auth tokens.
