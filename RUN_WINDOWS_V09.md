# Run ZERO TO MILLIONAIRE v0.9

Close ALL older game versions and Editors using the player profile before the
FIRST launch of this updated v0.9. Older already-loaded code can overwrite live
save data. Do not run old and new games against one profile simultaneously.

The updated build protects the exact old primary/backup bytes before a schema1-5
to schema6 write in `level1-v1.json.pre-schema6/vN-FINGERPRINT/`. This verified
snapshot never rotates with `.bak`. If reading, classification or copying fails,
no live save is replaced. If legacy originals change after loading, close and
reopen the game. Read `V09_MIGRATION_ROLLBACK.md` for safe byte restoration.

Put every Windows split part `.zip.001`, `.zip.002`, `.zip.003` in one folder.
In 7-Zip choose Extract on `.zip.001`. Run `ZeroToMillionaire.exe` in the complete
extracted `zero-to-millionaire-windows-v0.9` folder. No installer, administrator,
network, login, real-money purchase or cashout is part of this game.

v0.9 defaults old saves to1x while preserving resources and room ownership.
Older releases cannot read a schema6 save. Keep immutable old builds and the
pre-migration snapshot; use updated v0.9 to continue the upgraded progress.
All automated QA used private profiles. A normal visible game launch was
subsequently performed at the user's explicit request.

Normal save directory:
`%USERPROFILE%/AppData/LocalLow/PrototypeWorkspace/ZERO TO MILLIONAIRE/`.

Tap the STAKE cost panel to choose1x/2x/3x. Paid spins cost5/10/15 virtual coins;
all coin rewards multiply by the selected paid stake. One matching visible item
icon remains one part. Free starter/recovery spins always cost0 and reward1x.
If the chosen paid stake is unaffordable, lower it. Below5coins, FREE SPIN provides
the existing transparent recovery. Choices lock during an active spin.

SHOP previews all eight upgrades, their exact item icons, room effects, prices,
parts and current/next/locked/installed states. Only the current task can be bought
when BOTH its coins and parts are sufficient. BUY & INSTALL applies the real room
change and closes SHOP. BACK/Escape closes the modal. Menu offers tutorial replay,
full/calm animation and a reset that requires confirmation.

Mouse/touch controls and Tab/Shift+Tab/Enter/Space/Escape use the same live buttons.
Actual Editor synthetic pointer/keyboard events and rendered layouts were checked;
physical native pointer/keyboard remain untested because supported Computer Use
`node_repl` is unavailable in the build executor.

Source: extract all source split parts with7-Zip. Open the project in the installed
Unity6000.3.21f1, open `Assets/Scenes/Level1.unity` and press Play. For isolated
Editor review set `ZTM_QA_SAVE_DIR` to a fresh absolute private directory before
launching Unity. Do not share the normal game profile with an Editor.

For private native verification on the connected project only, pass
`--ztm-private-profile X:\Projects\ZERO-TO-MILLIONAIRE\.tooling\NEW_PROFILE`.
The directory is validated against this executable's owning project's `.tooling`
tree. Optional `--ztm-private-auto-quit` requests a normal self-shutdown after the
private read-only screenshot/frame observation; it does not dispatch input or
economy actions. This flag is inactive for ordinary launches.

APK export and Android/device testing are deferred. No store publication occurred.

Verified local build: `Builds/Windows-v0.9-candidate-3/ZeroToMillionaire.exe`.
Production build source: `457a13f5ecab649aafb371873b1ba9bb301536fe`.
