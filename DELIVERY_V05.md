# Final Windows handoff — v0.5

Game build source:17f525238b1925819b857f9bfd31cb5ec4062126. Source archive docs/testing snapshot:48daf9f9e2c62f49e42b97582c88fcb544fedeef. Later local docs HEAD contains delivery receipts only plus extracted-archive smoke helper/evidence; production game/art/font/package/settings diff against17f5252 remains empty.

Runnable final folder on X: `X:/Projects/ZERO-TO-MILLIONAIRE/Builds/Windows-FINAL-v0.5/ZeroToMillionaire.exe`. Extract the complete Windows ZIP and launch its EXE, keeping all adjacent files. RUN_WINDOWS_V05.md contains Russian notes. Verified extracted ZIP launched with private profile,867frames, QA hooks absent and normal exit. No installation or network required. Existing v0.3 remains running; close it manually before switching ordinary profiles.

| Artifact | Confirmed Library ID | Bytes | SHA256 |
|---|---|---:|---|
| zero-to-millionaire-windows-v0.5.zip | libfile_07492a6a2c3081918854b57dea6bcaa7 | 37000732 | 2d5f663eaccbd9eb4005576a162b4815fc17b2c33693e2ed1a905c2640fba2c0 |
| zero-to-millionaire-first-playable-source-v0.5.zip | libfile_c07541e7be2081918c874786d94d95cb | 31679877 | fd34be4fb29c625266abb5ef470a0198b89a45f6f8c15afea2b8eabe99918bfc |

Both complete ZIPs were retained by Library, but actual main-chat attachment delivery rejected them as over its upload limit. Use the corrected six split parts below; Library retention alone does not confirm main-chat attachment success. CRC and actual extraction verified; all158Windows files hash-match the final manifest; source scripts match frozen code and all six approved reference PNGs are present. Source excludes caches, private logs/saves/coordination, credentials, signing keys and Git internals. Source assets remain separate editable images. Immutable v0.3/v0.4f and Android builds preserved.

Final Player images: initial `libfile_b251433e616481919930631b881aee5e`, complete `libfile_21f33ccc01fc8191b1294e282c610ce9`. Native validation GIF `libfile_05375b8f46148191ae9958efffa70fa8`:96actual PNGcaptures/74decodedmergedframes,7.68s,360x720,4071693bytes. GIF uses synthetic callbacks and private fixtures in separate test-only Release; not ordinary playthrough timing. All Library IDs and local metadata verified.

Four reviewed initial/context/milestone checkpoints remain `libfile_f2b56587f63c8191b5f40441512a77f5`, `libfile_1f1b3b62e3e881919e21b892771d49ec`, `libfile_c7b3136f336c8191ae0df4a3df326bcd`, `libfile_fac7f9f37ba08191943a3edffd23aa5c`.

Coverage:16critical tests,19synthetic pointer checks;1000seedcomplete levels49..60,median53,p9557,first purchasemedian3,p954. Native fresh actual progression53spins/8manual buys/61repeat guards/8087sequentialframes; reload/resize/walking passed. Exact clean Release persisted resume/skip/story cases6passed; synthetic native Release-test dialogue actions23passed. Neither synthetic UI callbacks nor state fixtures prove hardware input.

Remaining limits: Windows Screen-saver blocks physical/active-display validation; no unlock/security changes attempted. ComputeBuffer warning persists at shutdown, no suppression/leak-freedom claim. Mixed-language non-dialogue UI, text-only heroine speech, unsigned Windows build, inherited bundleVersion0.3.0. Android packaging deferred; no new APK/device/emulator/battery claim. Detailed boundaries: NATIVE_RELEASE_VALIDATION_V05.md.

## Corrected chat-size delivery

Each archive is a standard 7-Zip split byte stream in three parts, max15,000,000bytes. Original ZIPs unchanged. Installed 7-Zip `x archive.zip.001` returned0 for both streams; source scripts/six refs and all158Windows hashes matched. Main-chat attachment of these new parts is pending the parent delivery step.

| File | Bytes | Confirmed Library ID |
|---|---:|---|
| zero-to-millionaire-first-playable-source-v0.5.zip.001 | 15000000 | libfile_8d36109bd40c819181851d3c72a2222a |
| zero-to-millionaire-first-playable-source-v0.5.zip.002 | 15000000 | libfile_1d9eb349544c8191a7bf4c8c6e1129ba |
| zero-to-millionaire-first-playable-source-v0.5.zip.003 | 1679877 | libfile_bd08b98cf1d08191bed4e17747a65d1a |
| zero-to-millionaire-windows-v0.5.zip.001 | 15000000 | libfile_875aee4300e88191b842d97131cbdcdc |
| zero-to-millionaire-windows-v0.5.zip.002 | 15000000 | libfile_8a2eae55bfd48191bb623acd1ae7c236 |
| zero-to-millionaire-windows-v0.5.zip.003 | 7000732 | libfile_c0d5a34628588191a24acaba8ab940fd |

Download all three matching parts into the same folder. Open `.zip.001` in 7-Zip and Extract; do not extract `.002`/`.003` separately. The Windows extraction contains `ZeroToMillionaire.exe` and all companion folders. Do not rename the part stems.
