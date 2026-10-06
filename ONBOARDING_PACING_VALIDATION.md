# Onboarding and pacing v0.5

Scope: same offline fixed9m2 room, same approved separate art/rig. Five Russian heroine speech steps, existing portrait; Skip at any point. MENU / Repeat tutorial is help only and grants nothing. Four finite milestone comments after surfaces, wallpaper/door/window, mattress/lamp, and all8 purchases. Seen bits are saved before showing; completed/experienced old saves do not replay old milestones. No voice/network/payment.

Schema5 stores tutorial version/step/completed/skipped/storySeenMask and exact task cashPrice/fragmentGoal. Schema1..4 migration retains cash, all parts/surplus, ownership including out-of-order, current old task price/goal/readiness. Only future tasks use new values. Atomic dialogue writes cannot mint currency or parts; save failure leaves live state unchanged. Confirmed reset clears tutorial/story/replay; cancel preserves state.

New task pairs (parts/cash):20/60,30/115,38/155,44/195,50/235,58/300,62/315,70/355. BET5; base cash12, cash icon2, vertical triple10; three starter free spins, explicit one-spin recovery below5cash. Every collecting spin shows at least one needed part, replacing one cash cell before payout calculation, so each visible item remains exactly1part. Full parts pool becomes cash-only until purchase. Both resources and manual BUY remain mandatory. Values live in Economy.SlowCurve; future changes to stored requirements need explicit migration/versioning.

1000 independent seeded simulations: full-level49..60spins, mean53.333, p50=53, p95=57. First task min2,max5,p50=3,p95=4; later medians4/5/6/7/8/9/10spins. Time estimate depends on actual reading and player actions; no guaranteed duration or forced wait.

Unity Test Framework:16 passed,0 failed. Full core regression:107769 economy assertions,1000 complete levels;432000 navigation samples;52 offline session cases;21106 layout assertions. UI audit: actual callbacks in private Editor profile;625-ish advancing frames; initial/contextual/first-ready/manual-buy/repair/skip/replay/save/reset/milestones;360x640,360x800,480x960; both speech controls44high, measured text fits and avoids quest/reels. Synthetic pointer and final native results are added in NATIVE_RELEASE_VALIDATION_V05.md.

Font: local OFL derivative renamed ZTM Pixelify; missing Cyrillic capitals Pe and O alias existing identical Pi and O glyphs. No outline/art changes. Original copyright/OFL retained; all Russian source literals have glyphs.

Known limits: remaining task labels/help are English; heroine speaks via text/portrait, no voice. Active physical desktop/input is blocked by Windows Screen-saver; Editor event tests do not claim hardware input. Existing ComputeBuffer finalizer warning remains unsuppressed, allocation owner unresolved. Android packaging deferred by owner.

Library checkpoints (verified bytes/metadata):
- initial360x640: libfile_f2b56587f63c8191b5f40441512a77f5
- contextual480x960: libfile_1f1b3b62e3e881919e21b892771d49ec
- surfaces480x960: libfile_c7b3136f336c8191ae0df4a3df326bcd
- complete480x960: libfile_fac7f9f37ba08191943a3edffd23aa5c

Prepared Library helper failed before any write: WSL auth absent; actual Windows helper reported prepare_uploads unavailable. Ordered direct create fallback then supported metadata application succeeded. No credentials copied or bypassed.
