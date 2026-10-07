# v0.9 virtual stakes and room shop

Paid stakes 1x/2x/3x cost 5/10/15 virtual coins. The entire existing cash payout
(12 base + 2 per cash icon + 10 per vertical matching triple) is multiplied by
the paid stake. Matching visible item icons remain one icon = one fragment.
Goals/prices and the finite room task order remain unchanged. Higher stakes
increase coins without tripling renovation speed through hidden part rewards.

Three starter spins and each bounded free recovery always cost0/reward1x,
regardless of selected future paid stake. Selection persists in schema6.
Paid spins require the full selected cost; the stake panel shows balance/cost
and permits lowering the stake. Free recovery stays restricted to cash below5,
so selecting an unaffordable high stake cannot mint repeated free allowances.
Stake selection locks during presentation; each committed spin captures its
selection/effective reward multiplier, cost and payout as readonly values.

Schema1..5 migrations retain balances, parts, allowances, purchases and applicable
legacy task economics. Schema5 tutorial/story history remains exact. New selection
defaults1x. Schema6 missing/invalid selection is rejected before normalization and
uses the established recovery/backup path. Atomic save-before-cosmetics and rollback
remain GameSession responsibilities.

SHOP is a modal catalog for the same eight upgrades with their exact existing
icons, room-effect previews, current/next/locked/installed states, and coin/part
requirements. It uses GameView.Buy -> GameSession.TryPurchase; it cannot bypass
the single active task. A successful purchase closes SHOP to show the real change.
Main controls are disabled behind the modal, and UiInputCoordinator scopes focus,
Tab/Shift+Tab/submit/Escape and restoration to the actual overlay.

Balance verification compares 1000 same-seed full-level runs at each stake with
an explicit lower-affordable-stake policy. Numbers describe spins, not guaranteed
minutes. The baseline curve must retain at least90percent mean spin count at higher
stakes. Physical native input remains blocked by missing supported node_repl;
domain, Unity JSON, synthetic UITK events, actual rendering and native read-only
launch/relaunch coverage are reported separately. No real-money feature, new plugin,
engine update, publishing or external push is included.
