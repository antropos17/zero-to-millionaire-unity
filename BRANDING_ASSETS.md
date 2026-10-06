# Additional branding assets — 6 October 2026

Three original raster alternatives were generated with the built-in image tool
after visually inspecting all six approved room references. They use the game's
cream, forest-green, terracotta and amber palette. These files are saved in this
project and available for integration; the current runtime header still uses
live text. No existing art or source file was overwritten.

| File under `asset-source-v0.2-branding-root/` | Dimensions | Intended use |
| --- | --- | --- |
| `zero-to-millionaire-wordmark-v1.png` | 1983 × 793 | Wide title/header artwork |
| `room-emblem-v1.png` | 1254 × 1254 | Compact cutaway-room emblem |
| `zero-to-millionaire-seal-v1.png` | 1254 × 1254 | Alternate square title seal |
| `character-variants-v1.png` | 1227 × 1282 | Two alternative adult characters, front/back pose source atlas |

All three PNG files are RGBA with measured alpha extrema 0 and 255. The full
text on the wordmark and seal reads ZERO TO MILLIONAIRE. Inspect readability at
the final rendered size before replacing a header; cash, progress and controls
remain live UI. The room emblem is branding artwork, not a replacement room
background or an additional level.

The existing v0.2 item icons and character gait are separate runtime assets in
`Assets/Resources/Art/item-symbols/` and `Assets/Resources/Art/animation/`.

The additional character atlas is also measured RGBA with alpha extrema 0 and
255. It contains a teal-shirt male and a sage-cardigan female, each with front
and back pose rows. It is a concept source sheet: cell baselines, distinct gait
phases and cropped pivots require inspection before import. The generated
poses alone do not qualify a smooth animation. It does not change the active
heroine, collision geometry, save state or current animation rig.

SHA-256 of `character-variants-v1.png`:
`b706285f471414669ddadf8ce8d4de3e8eff0070631cfa82081b3bb94a46f5c4`.
