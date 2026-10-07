# Run ZERO TO MILLIONAIRE v0.9

Put every Windows split part `.zip.001`, `.zip.002`, `.zip.003` in one folder.
In 7-Zip choose Extract on `.zip.001`. Run `ZeroToMillionaire.exe` in the complete
extracted `zero-to-millionaire-windows-v0.9` folder. No installer, administrator,
network, login, real-money purchase or cashout is part of this game.

Close any older game instance before normally launching v0.9: they use the same
player save directory. v0.9 reads old saves without erasing progress, defaults
the new stake choice to1x and upgrades its schema on the next committed action.
Older releases cannot read a schema6 save; keep immutable old builds but use v0.9
for continuing upgraded progress. Do not run old and new versions against one
save simultaneously. No live user save was migrated by QA.

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
