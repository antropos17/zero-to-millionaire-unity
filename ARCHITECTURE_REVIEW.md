# Offline architecture review — migration checkpoint

The backend is local game code and storage. There is no server or network service.

## Current boundaries

- Core/Economy.cs: typed Symbol/ReelSymbol enums, immutable upgrade definitions,
  stable saved IDs, costs/goals and active task/pool rules.
- Core/GameModel.cs: validated state, finite quest progression, cash/part
  transactions, repeated-buy guards, migrations and snapshotted spin pool/grid.
  Core has no Unity dependency and runs in the .NET check executable.
- Core/SaveStore.cs: codec boundary, bounded local JSON, flush + atomic replace,
  backup recovery and version migration.
- Runtime/GameView.cs: live UI, room art, input, cosmetic spin/reward flight and
  movement presentation. It currently also coordinates save and rollback.

The X copy passed existing 34,586 economy assertions / 1,000 complete seeded
levels and 432,000 walking samples. Exact source/asset hashes matched before
tooling changes. The latest rendered quest run remains the prior successful
17-spin/8-purchase UI evidence; this checkpoint does not claim a new UI rebuild.

## Concrete next changes

1. Extract the repeated mutate/save/rollback code for spin, BUY and recovery from
   GameView into one small offline session, with an injectable save interface.
   This makes save-failure, close/reload and repeated input tests independent of
   Unity. Keep domain rules in GameModel and cosmetics in GameView.

2. Check persisted schema3 quest-ID array shape before canonical migration.
   Currently Migrate rebuilds quest records before IsValid checks them. A malformed
   raw record list can therefore be normalized rather than selecting the valid
   backup. Reproduce with a disk fixture, reject duplicates/unknown/missing IDs,
   and still derive readiness from balances while preserving legacy owned stock.

3. Test reset when its second save fails. Current GameView checks two saves in one
   condition; a first committed reset followed by failure can leave the live model
   describing the old state. Define the primary-commit outcome explicitly without
   inventing a distributed transaction system.

4. Check the cash overflow boundary against the net transaction. Current spin
   validation uses gross payout before subtracting the bet; valid near-limit net
   values may be rejected. Use a wider intermediate and a targeted numeric test.

These are inspection findings, not all reproduced failures yet. No speculative
layers, additional currencies, hosted API or anti-cheat system are proposed.

## Next verification boundary

Implement those small changes on X, then rerun migration/resource gates/repeated
taps, save-failure/relaunch/mid-spin/reset/recovery, full finite UI playthrough and
movement clearance. Compare actual live empty/collecting/ready/completed,
pressed/disabled/menu states to all6 references at matched resolution; measure
room/heroine scale, fonts, spacing and button depth. Return the requested visual
checkpoint before rebuilding and opening the final Windows portrait player.
Android packaging is deferred; installed Android capability is retained.
