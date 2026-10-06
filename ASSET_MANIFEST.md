# Reusable asset manifest - v0.2

All six approved references were materialized with native Library metadata and
visually inspected before art work. No old project graphics. Live UI remains
cream/forest/terracotta with prototype typography/bevels.

| Family | Actually used reusable files | Ready criteria |
|---|---|---|
| Room | ruined, patches, surfaces, wallpaper; independent door/window overlays | Aligned512px transparent canvases, fixed footprint, no baked heroine/furniture/UI |
| Furniture | plain and dressed mattress, lamp, stool | Independent alpha; BUY-only changes; left-wall mattress and clear openings |
| Item symbols | cash, floorpatch, wallrepair, wallpaper, door, window, mattress, lamp, stool | Nine separate128px RGBA symbols; exact one-to-one credit |
| Owned icons | door-owned, window-owned, mattress-owned | Three useful state variants actually used in upgrade rows |
| Heroine rig | front/back body, leg-left, leg-right, idle | Eight transparent production files;128x192 canvas, stable feet(64,178), no matte; continuous opposing leg rotations, idle planted |
| UI | live UI Toolkit | Cash, separate bars/prices, explicit dual-resource BUY, confirmation reset, touch sizing |

Animation art sources were honestly reviewed. Original stride/knee sheets have
repeated poses and are retained as draft candidates. Production uses generated
neutral sprites as a small skeletal cutout rig, not a nominal24frame atlas.
Foreground isolation keeps the opaque silhouette plus1 antialias pixel and
removes the neutral source's low-alpha glow; raw originals remain unchanged.
Point filtering, no mipmaps or lossy compression. Actual rendered GIF is required
for gait/feet/joint review; frame-phase assertions alone do not establish quality.

Derived art: Assets/Resources/Art/room, objects, item-symbols, animation.
Input identity/crop receipts: asset-source*/identity-map.json, icon-slicing.json,
cutout-rig.json. Raw generated source atlases are local/Library, omitted from
source ZIP to avoid duplicating production PNGs. Style references are included.

Current generated inputs, all version0:

- Room stages: libfile_3146efc1d61c81918a57e22890beef9c.
- Refined heroine/furniture: libfile_52837629012c81918ea7030b828149fe.
- Plain mattress: libfile_2f2ff73fae1081919c512bdf08978db6.
- Twelve item icons: libfile_bc8c1aef287c819195f1e64ae6b0ac94.
- Neutral front/back (production rig): libfile_8be9a414782c8191b8e526f808e53eba.
- Front stride draft: libfile_d28d1a8536b88191bb9e5ce02343e11f.
- Back stride draft: libfile_e63e99b38b98819186349b3370e0e154.
- Original five generic icons from v0.1 (retained legacy candidates, not used in v0.2 reels): libfile_fbe134aaf93481918d7991b3424f9940.

Approved reference identities (all six visually inspected):

1. Ruined libfile_7a7cbf4a0f7481919d164be3932e4ed7.
2. Patches libfile_1dc46769dbfc8191bdbe2f4a248235f4.
3. Surfaces libfile_5e5c1e54e4908191b92941a836868c40.
4. Wallpaper libfile_7741c619bb5c81919e7cc726670e7564.
5. Corrected mattress libfile_9db10562c0008191849dceaaa25fb4db.
6. Modest finish libfile_c42c06e8a880819188661f4787c30cbf.

Floor projection:3x3m -> back(256,260), right(460,360), front(256,460), left(52,360).
0.34m swept-foot clearance; exact segment/expanded-obstacle intersection, grid
routes with safe smoothing. Door swing u1.75-2.90/v0-0.70; mattress u0.08-0.85,
v0.95-2.68; lamp base u0.28-0.55/v0.35-0.65; stool u1.08-1.38/v2.30-2.60.
Placement zones are reserved from the beginning to prevent purchase overlap.
Rendering:108x162 heroine canvas on512px room, feet pivot preserved; depth by
projected ground position. Same floor and heroine through the whole level.
