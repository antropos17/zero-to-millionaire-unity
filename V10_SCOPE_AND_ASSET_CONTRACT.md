# v0.10 scope and asset contract

Baseline: X:/Projects/ZERO-TO-MILLIONAIRE, commit
6d632e6e3ddae56e144724c539bd8978fbb6cc9c; reviewed v0.9 production
457a13f5ecab649aafb371873b1ba9bb301536fe. Initial read-only Git check found
no tracked changes. Untracked deliveries/evidence remain untouched. No running
ZeroToMillionaire process was found at this inspection; no process/window input
or user-save mutation is part of this checkpoint.

English UI, offline single-player, same adult black-bob heroine, cream/forest/
terracotta pixel style, one fixed3x3m room. The original eight-task renovation
remains8/8 complete for completed saves. Optional decor is a separate category.
There is no room expansion, account/login, subscription, payment, OS notification,
email, backend, plugin installation or publication in this slice. Windows remains
the owner's priority; Android packaging is deferred.

## Cohesive feature slice

1. Local profile: editable1-20-character display name; selectable Heroine, Lamp
   and Stool avatars from existing approved textures. The heroine portrait uses
   the same front-idle texture UV already used by DialoguePresenter. Avatar
   choice affects the profile badge. Show actual saved coins, spins in this save,
   renovation N/8 and optional decor N/4. Do not infer lifetime earnings/playtime
   or manufacture historical activity from migrated data. Disable rich text for
   user names; validate lengths, whitespace/control characters and glyph support.
2. Sections: one shared section panel with ROOM / SHOP / PROFILE / SETTINGS
   tabs. ROOM closes the panel. Existing SHOP and MENU entries open the same
   panel at Shop and Settings. Switching a tab changes the current section with
   immediate focus; Back/Escape returns to Room and restores focus. Scrollable
   content keeps320x640 usable, rather than reducing the existing room/reels to
   add a permanent bottom navigation row.
3. Settings: existing FULL/CALM motion preference and persisted reward-toast
   visibility. Replay tutorial and confirmed room reset remain in Game Data.
   No audio files were found in Assets; no audio/volume toggle is presented.
   Profile/settings writes publish only after atomic file success and retain
   the old visible preference on failure. A failed no-op does not clear errors.
4. Notifications: optional transient toasts and a bounded20-entry THIS SESSION
   activity list in Profile. Log actual committed spins, purchases and newly
   unlocked/ready tasks; deduplicate using session generation plus committed
   action identity. On reload establish a fresh baseline and show no old reward
   toast; on confirmed reset clear old queue/history. Reset cancellation preserves
   it. This session scope avoids fabricated historical stats and avoids making
   notification persistence a second part of a cash transaction. Critical save
   errors remain visible independently of the optional-toast setting; failed
   transactions produce no reward or installed-item entry. Current error rows
   may be shown for this session but are not claimed to be saved transactions.
5. Animations:120-160ms section opacity entrance with stable hit bounds; brief
   purchase reveal/highlight of the installed item's own sprite; toast entrance/
   exit. Focus and navigation act immediately. Cancel/replace transitions on
   rapid switching. No additional busy/input-lock state. CALM applies final
   positions/opacity immediately with a quiet static highlight; existing spin,
   walking and reward presentation keep their established calm equivalents.

Local profile/name/avatar/reward-toast preference lives in a small versioned
local-player-v1.json beside the private/current profile. Keep the existing
presentation-v1.json motion preference and import its current value. Draft
profile fields apply on SAVE PROFILE; leaving the panel cancels unsaved drafts.
Settings toggles use saved data, never an unsaved name draft. There is no profile
service or new notion of an account. Local configuration must preserve originals
and report read/write errors rather than silently overwriting unknown versions.

## Optional decor rules

Shop groups: RENOVATION and OPTIONAL COMFORT. Decor requires all eight base
upgrades. A completed old save still says ROOM8/8 COMPLETE; a separate START
DECORATING action enables the optional target. It never silently makes the old
story incomplete. One optional target at a time, in the order below, with the
same explicit BUY & INSTALL cash-plus-matching-parts transaction path. Each item
has its own parts counter/progress and price. A purchase changes the room only
after saving; repeated taps cannot double-buy or overdraw.

| Stable ID | Item | Provisional coins | Matching parts | Room placement |
| --- | --- | --- | --- | --- |
| plant | Windowsill plant |90|18|Small terracotta pot on repaired left-window sill|
| crate | Small storage crate |120|22|Narrow floor strip between existing lamp and mattress head|
| curtains | Simple curtains |150|26|Cheap muted forest cloth around the existing window|
| shelf | Small wall shelf |180|30|Plain board/two worn books on right wall, left of door, above head height|

These tunable prices preserve a modest room and extend the existing parts loop.
Paid stakes retain5/10/15 cost and existing cash scaling; one matching icon is
one part. Free starter/recovery spins remain cost0/reward1x. Preserve a recovery
path when cash is below the minimum spin cost. Verify pacing after implementation;
do not claim these provisional values are final measured balance.

Keep the original8 base purchased/fragments/quest IDs intact; add separate
decorPurchased[4], decorFragments[4] and decorStarted fields. New reel symbol IDs
9-12 append after cash0 and base1-8. GameModel's base completion semantics stay
separate from optional completion. Resource commands share the existing guarded
candidate-save-publish path, including rollback on write failure.

Decor requires schema7. Schema6 migration preserves stake1/2/3, coins, free spins,
spins/recoveries, all base arrays, last grid, tutorial and heroine position; new
decor fields start empty/inactive. Older valid schemas remain supported. Strict
required-field validation applies to newly written schema7; malformed/future data
must fail closed. Retain current verified originals/pending-migration mechanism:
protect exact schema6 primary/backup before first schema7 write under
level1-v1.json.pre-schema7/v6-FINGERPRINT/. Existing pre-schema6 snapshots remain
intact. Close all old games/Editors before a later normal v0.10 launch. No schema7
write to the real user profile is authorized as an automated test.

## Asset contract for parent generation

Need EIGHT separately reusable transparent RGBA PNGs, with hard pixel edges and
the approved six-reference palette, scale, lighting and isometric directions.
Use existing room/wallpaper.png and the current lamp/stool/item symbols as local
placement/style references. No composite gameplay screenshot, room background,
heroine, captions, borders or extra objects may be baked into these assets.

Room canvas is512x512. Floor projection is x=256+68*(u-v),
y=260+(100/3)*(u+v), with0<=u,v<=3m. Use exact transparent padding/canvas
alignment for wall/sill assets; the integration renders these behind the heroine.

| Requested PNG | Shape/placement contract |
| --- | --- |
| decor/plant-room.png |512x512 transparent layer; only a small potted plant. Pot foot near(188,236), about26px wide and40px total height; sits on sill, not floating in the window glass. Final bounds must be checked against repaired window overlay.|
| decor/crate-room.png |Tightly padded independent transparent sprite. Render about46px wide; modest38x16x25cm wooden crate. Floor footprint u=.10..48,v=.78..94; anchor(.29,.86), projected centre(217.2,298.3), front edge about+9px. Ground-front pivot supplied/measured from alpha; no arbitrary stretch.|
| decor/curtains-room.png |512x512 transparent layer; only cheap muted forest curtains fitted to the existing left window, roughly window region x=95..213,y=153..284. Cloth follows the sloped isometric frame; no floor puddle or opaque replacement of the whole window. Plant remains visible in front.|
| decor/shelf-room.png |512x512 transparent layer; only plain wall shelf and two worn books. On right wall left of door, about x=280..326,y=136..175, board anchor around(301,162). Mounted above head height; no new floor footprint.|
| item-symbols/plant.png |128x128 individual transparent slot/shop icon; exact existing item-symbol canvas dimensions and outline/pixel vocabulary; same plant identity.|
| item-symbols/crate.png |128x128 individual transparent matching icon; same narrow crate.|
| item-symbols/curtains.png |128x128 individual transparent matching icon; recognizable cheap cloth pair.|
| item-symbols/shelf.png |128x128 individual transparent matching icon; same plain shelf/books.|

Coordinates are integration targets, not proof a generated image already fits.
Parent may propose corrected alpha bounds/pivots after inspecting generated
pixels. The existing room itself must stay fixed. New sheets/avatars are not
needed; profile uses approved existing art. Generation may supply larger
pixel-aligned masters with final-canvas exports, but all eight final files must be
separate, transparent and visually inspected before claiming completed art.

The crate's padded collision region for radius.34m is u=-.24..82,v=.44..1.28.
For legal u>=.34, its part below v=.61 is inside the existing lamp exclusion;
its part above v=.61 is inside the existing mattress exclusion. Thus this chosen
footprint can keep existing legal feet/pathing unchanged. Validate that assertion
against actual RoomGeometry plus sampled paths before integrating. Wall/sill
decor has no floor collider. Test new alpha overlap/depth with the heroine and
all existing furniture; final pixel placement may require adjustment inside
these reserved regions, not relocation of the room or heroine.

Asset ready criteria: all8 actual files present; correct dimensions/alpha;
no baked room/hero/UI; style faithful to all6 approved refs; integer/nearest
sampling; correct icon identity; accurate ground pivot and depth; composed room
checked at320x640,360x640,360x800,480x960 in full/calm modes; existing feet remain
clear; art described as pending until these checks pass. No unrelated sprite is
stretched/tinted into a new object or labelled final.

## Implementation slices and ready criteria

Slice1: local profile store/settings/shared section shell/session notifications
and animations. Keep schema6 gameplay unchanged; test configuration no-op/error
rollback, text limits/glyphs, tab/focus/back/Escape, notification dedup, critical
errors with optional toasts off, reset/reload and all four portrait sizes.

Slice2: schema7 optional decor state and shared spin/purchase guard, while parent
generates PNGs. Verify schemas1-6, exact snapshots and fault paths; completed6
saves remain8/8 and no extra target starts automatically. Test affordable and
unaffordable stakes, independent part counts, repeated/concurrent buys, blocked
saves and full optional completion. Pending art is labelled and not delivered as
finished. Test actual geometry, not only manual placement assumptions.

Slice3: integrate inspected8 PNGs, room/purchase reveal/depth, usable shop groups,
and final fresh+schema6-complete playthrough. Build clean Windows candidate,
native private save/relaunch checks, source/archive extraction and Library IDs.
Preserve old candidates and all user files. No normal launch into the user profile
until authorized and all old versions are closed.

Every follow-up carries: runnable fixed-room level, correct touch spin cash/parts,
both-resource explicit purchase, no double-buy/overdraw, persistent visible room
changes, full base+optional completion, local profile/settings reload, confirmed
reset, narrow/tall layout, modal focus/back, notification dedup/stale handling,
transaction rollback/migration snapshots and basic performance evidence. Distinguish
Editor/synthetic events/private native runs from physical input/device tests.
Computer Use still requires unavailable node_repl; no alternate OS UI helper.
No new feature category or optional cleanup is needed for this bounded v0.10.

Initial proposed-crate geometry check PASS:361201 sampled floor points,71069
existing legal points,0 lost. This uses actual unchanged v0.9 RoomGeometry;
asset composition, alpha depth and native input remain untested.
