# ZERO TO MILLIONAIRE — v0.7 project brief

One offline portrait level: the same fixed 9m² ruined room, adult black-bob heroine, cream/forest/terracotta UI and separate generated pixel art faithful to all six approved references. Unity 6000.3.21f1/C# was selected because this computer already had Unity and Android tooling; Godot/export templates were absent at the initial inspection. Android v0.7 is deferred under the subsequent desktop priority.

The 5x3 reels show cash and the currently needed repair/item. Each matching item cell contributes one fragment to that saved task. Excess rolls become cash; once both goals are met, the player must explicitly purchase before the next task starts. Each of eight upgrades has its own stored fragment count and cash requirement. Successful disk save precedes cosmetic spin/purchase changes; failure rolls back. The completed room stays modest.

New-save cash/fragment goals, in order:
| Upgrade | Cash | Parts |
|---|---:|---:|
| Floor patches | 60 | 20 |
| Surface repair | 115 | 30 |
| Cheap wallpaper | 155 | 38 |
| Door repair | 195 | 44 |
| Window repair | 235 | 50 |
| Mattress | 300 | 58 |
| Lamp | 315 | 62 |
| Stool | 355 | 70 |

A paid spin costs5; three starter spins and an explicit free recovery prevent a zero-cash deadlock. Base cash payout12, +2 per cash icon and +10 per vertical triple. The finite curve completes 1,000 seeded runs in49–60 spins (median53), giving each visible renovation several spins. Values are tunable in Assets/Scripts/Core/Economy.cs. Legacy saves retain their stored legacy requirements and all progress.

v0.7 improves font metrics, counter clarity, text hierarchy and accessible focus/keyboard behavior without changing the economy, save schema, art or room geometry. Repairs, items, story progress and settings persist locally. Reset requires confirmation. Full validation and honest coverage limits are in FINAL_VALIDATION_V07.md; Russian launch steps are in RUN_WINDOWS_V07.md.

Ready criteria: runnable complete level, both-resource guards and repeated-tap safety, per-task bars, purchase-only visible changes, disk persistence/reset confirmation, narrow/tall sizing, basic performance evidence, source/assets/reference delivery and honest Editor/native/device distinctions. No purchases, ads, network requirement, expansion, publishing or external Git push.
