# ZERO TO MILLIONAIRE - first playable v0.3

Unity 6000.3.21f1 / C# was selected from installed tooling. Godot and Android
export templates were absent in checked locations; Unity included SDK, NDK,
OpenJDK and Android export. This is a choice for this computer and task.

One fixed 9m² room, one portrait offline single-player level. Start wrecked and
unfurnished. Purchase patches, repaired surfaces, cheap wallpaper, door, window,
plain mattress, small lamp and wooden stool in order. The completed room remains
modest. Each visible change happens only after the explicit BUY transaction.

The nine reusable reel symbols are cash and eight matching upgrade items.
Only cash and the current needed item appear. Each visible noncash cell awards
one part to that task; locked and owned items do not earn. Each upgrade retains
its own saved parts, goal and cash price. BUY & INSTALL requires both resources
and only the first unfinished task can be purchased. Repeated taps cannot
double-spend. A compact next-task preview and room N/8 checkmarks show progress.

Costs/goals (cash/fragments): patches 18/8, surfaces 55/12, wallpaper 80/14,
door 65/12, window 60/12, mattress 110/16, lamp 55/12, stool 45/12. Total cash
cost is 488. A spin pays 12 base cash plus 2 per cash icon and 10 per vertical
triple. Paid spin costs 5; the player starts at zero with three free spins.
A visible free recovery is available below five cash with no allowance left.
These values are tunable in Economy.cs; 1,000 simulated finite-task levels took
14-21 spins, median 17, p95 18. No timers, payment, ads, login or network requirement.

The adult heroine walks at up to 0.52m/s between random reachable destinations,
pauses briefly, uses front/back generated neutral cutouts and opposite continuous
leg rotations. This is a procedural 2D cutout gait, not 24 painted frames.
A 0.34m clearance radius and expanded obstacles protect walls, furniture and the
door swing. Exact segment/AABB tests prevent corner clipping. Placement reserves
item footprints before purchase. Feet and furniture use room projection and
floor depth. Position and progression are saved locally; v1/v2 saves migrate to
schema3 while retaining all unspent resources, excess stock and out-of-order
purchases. Reset requires confirmation.

All six approved Library screenshots were downloaded using the unchanged helper
on native WSL storage with verified identity/version xattrs, then inspected.
Parent generated new reusable RGBA art; runtime room layers, heroine parts,
objects and item icons are independent assets. See ASSET_MANIFEST.md.

The official Unity plugin, Unity CLI and com.unity.pipeline are installed.
Project-targeted stdio MCP initialized, listed 160 tools and completed harmless
Editor reads. No legacy Assistant terms were accepted. Pipeline is disabled in
player builds. This existing chat does not hot-load the newly registered server;
a new Codex session can expose it directly. See README.md.

Ready criteria for every follow-up: runnable level and Android APK when possible;
spin cash/matching fragments; separate bars/prices; both-resource BUY and repeated
tap guards; purchased changes and position survive relaunch; full completion;
confirmed reset; narrow/tall portrait layout; movement and obstacle clearance;
actual animation render; basic performance evidence; explicit separation of
model/editor/desktop/device coverage. No publication, external push or payments.
# v0.3 finite renovation tasks (approved follow-up)

The same eight upgrades are now eight finite tasks. One current task sits above
the reels with an exact item icon, a parts x/y bar, a cash available/required bar,
and an explicit BUY & INSTALL button. One compact locked NEXT preview shows what
comes after it. The room displays N/8 completed tasks with drawn checkmarks; 8/8
ends the level and disables further collection. No new room, level, currency,
login, network service or real-money purchase is added.

Stable IDs: floor-patches, surface-repair, cheap-wallpaper, door-repair,
window-repair, first-mattress, small-lamp, wooden-stool. Each saved quest has
locked, collecting, ready-to-buy or completed status. Readiness is recalculated
from both balances. The first unfinished ID activates even if a legacy save has
later purchases. Schema 1/2 migration retains every owned improvement, cash,
allowance, spin count and unspent/excess part. Only the displayed bar is capped.

Reels use cash plus the current needed item. Each visible matching icon awards
one part to that task only. When its goal is filled, extra item rolls in that
spin become visible cash icons, and future reels use cash while more cash is
needed. Once both resources are ready, collection pauses until manual purchase.
Completed and locked tasks never earn new parts. A matching icon with +n moves
from the reels toward the task card after collection.

Spin rewards and the exact result grid are saved before the cosmetic animation.
A purchase saves both debits, ownership and the next task before changing room
art. A failed save restores the live model. Repeated taps and installation during
spin animation are guarded. Starter spins and explicit free recovery prevent a
zero-cash lock. Reset still requires confirmation.

Prices and goals remain tunable in Economy.cs: total 488 cash and 98 parts;
three starter spins, bet 5, base cash 12, +2 per cash icon and +10 per vertical
triple. Focused collection finishes 1,000 seeded model runs in 14-21 spins
(median 17, p95 18). Old excess stock is kept rather than converted or deleted.
These values are prototype balance, not values inferred from reference images.

Research links supplied by parent for this approved adaptation:
https://dreamgames.helpshift.com/hc/en/3-royal-match/faq/15-what-are-tasks-and-how-do-i-complete-them/
https://dreamgames.helpshift.com/hc/en/3-royal-match/faq/20-how-can-i-see-area-progress/
https://support.traveltowngame.com/hc/en-us/articles/4833678076434-What-are-Orders
https://support.coinmastergame.com/hc/en-us/articles/360022217400-How-Do-I-Build-My-Village

Ready criteria remain: runnable full first level; final Android APK when tooling
permits; both-resource and repeated-tap guards; visible purchases and local saves
survive relaunch; confirmed reset; narrow/tall portrait layouts; basic performance
check; final-code verification and exact run/install steps. Editor/render-texture
evidence must remain distinct from unrun physical Android touch/device tests.
Parent requested actual active-task and 3D-button preview before final packaging.
