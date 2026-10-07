# v0.10 optional decor: isolated domain checkpoint

Date: 2026-10-07. Branch: `prototype/v0.10-decor-domain`. Base: `e3dff7b7ce3187d173fc1ad4ce24e64dbeb22c58`. Obtain this checkpoint's exact identity with `git rev-parse HEAD`; the review archive records it in `review-manifest.json`.

This adds a separate schema7 model/session/store and tests. It does not bind schema7 to GameView, change the current schema6 save version, migrate a normal user profile, or replace the usable Windows slice1 build. The existing tracked runtime code is unchanged, allowing normal line-ending conversion. Unity importer-only whitespace edits were restored after the private editor exited. Unrelated untracked evidence remains untouched.

## Implemented rules

- Base room completion stays 8/8. After all eight purchases, an explicit optional start enables a separate four-item stage in the same room. Starting grants no money, resets no counters, and purchases nothing.
- Ordered items: windowsill plant (90 coins / 18 parts), small storage crate (120 / 22), simple curtains (150 / 26), small wall shelf (180 / 30). Each has its own parts and ownership. These values are provisional tuning, not screenshot-derived balances.
- One current item; its exact visible symbol earns one part. The 5x3 result uses cash 0 and the current decor symbol 9–12. Surplus symbols convert to cash; a missing matching roll gains a visible matching final cell. The first eight symbol identities remain reserved for renovation.
- Both coins AND parts are required for explicit purchase. Ownership advances once, resources cannot overdraw, future items cannot accumulate hidden parts, and repeated/concurrent commands cannot double-buy.
- Resources and counters remain shared. Paid spins use the existing 5/10/15 coin stakes; cash scales 1/2/3, parts do not. Free spins pay at 1x. Below the minimum 5-coin cost an explicit free recovery is available. Higher selected stakes can be lowered when the minimum stake is affordable.
- Session changes are prepared in a clone, saved, then published. Failed saves retain model/resources. Spin presentation holds a command lease; stake, buy, recovery and reset cannot interrupt it. Normal reset requires an explicit confirmation flag.

## Save safeguards

`Level1StateV7` is a separate flat serializable DTO; the old Economy/PlayerState/GameSession/SaveStore remain schema6. Schema7 adds only optional-stage fields to the shared base state. Strict JSON shape validation precedes Unity JsonUtility: bounded size/depth, exact mandatory keys/types/array lengths, duplicate/unknown field rejection, integer version, semantic validation and codec roundtrip checks.

Loading schemas1–6 does not write. Before the first schema7 commit, primary and backup are copied byte-for-byte (including BOM and absence metadata) under `level1-v1.json.pre-schema7/vN-SHA256/`, flushed and verified. Existing `.pre-schema6` history is retained. Exact snapshots never rotate; mismatches or inability to create/verify a copy refuse the migration. A remaining legacy backup is also protected before later rotation. A unique private temporary file is flushed, verified and replaced atomically. Both original streams are compared again before replacement; external changes refuse the command.

Future, unknown, unreadable or malformed schema7 input pauses writes and retains originals. Schema6 quest statuses must agree with canonical ownership/resources. A normal confirmed reset commits fresh state twice to update the rotating backup; a failure of the second write retains the first committed fresh state and a pending-backup notice. Recovery/reset of malformed or future saves is deliberately unavailable in this isolated module until a reviewed recovery flow is bound.

## Final verification

- .NET isolated decor runner: **94,300 assertions PASS**, 3,000 complete optional stages with base8/8 preserved. Recorded in `Evidence/v10-decor-dotnet-domain-final-1.txt`.
- Unity6000.3.21f1 and real JsonUtility: **94,300 assertions PASS**, same private scenarios. Recorded in `Evidence/v10-decor-unity-domain-2.txt`.
- Existing schema6 regression suite PASS: 107,769 economy; 432,000 motion samples; 52 session; 20,998 layout; 370,867 stake/schema6; 155 durable snapshot; 209 profile/notification assertions. Recorded in `Evidence/v10-decor-base-regression-1.txt`.
- Tests cover dual-resource/current-item rules, invalid indices/state/random output, overflow, repeated and concurrent purchase, presentation lease, failed/no-op command retention, zero-cash recovery, 1x/2x/3x payouts, save/relaunch/full4/4/reset, schemas1–6 primary/backup/BOM, nonrotation, snapshot-root/commit/codec failures, tampered snapshots, unreadable originals, external/future saves and future backups.
- Simulated decor pacing: min11 / median15 / p95 16 / max19 spins after opt-in. This is model simulation, not a rendered usability or device performance measurement.

The .NET decor project has its own intermediate/output directories, preventing collision with `Checks.csproj`. Editor tests ran on private data beneath the project `.tooling` directory. No Android APK, native decor interaction, decor screen screenshot, or decor rendering/performance test exists at this checkpoint.

## Exact reproduction and preserved playable build

```powershell
dotnet run --project X:\Projects\ZERO-TO-MILLIONAIRE\Checks\DecorChecksV10.csproj -- X:\Projects\ZERO-TO-MILLIONAIRE\.tooling\decor-review-private
dotnet run --project X:\Projects\ZERO-TO-MILLIONAIRE\Checks\Checks.csproj -- X:\Projects\ZERO-TO-MILLIONAIRE\.tooling\base-review-private
```

For the Unity domain runner, use the project's installed Unity CLI and `-executeMethod ZeroToMillionaire.Editor.DecorDomainAuditV10.Run` with `unity run`, editor version6000.3.21f1 and `-nographics`. It writes only private test data/evidence and exits. Never point tests at a normal user save.

The usable partial build remains `Builds/Windows-v0.10-slice1-candidate-1/ZeroToMillionaire.exe`, built from `9df99128385c5c22d1bce5815bb269e1f23df91c`. Launch that EXE for the playable slice1; do not represent this domain branch as complete v0.10. Its code/render review Library identity is `libfile_c3a48686e5e48191b74aeecdad3fd6f8`, version1, unchanged by this checkpoint. Screenshots and checks for that build are documented in `V10_SLICE1_CURRENT_CHECKPOINT.md`; new screenshots would misrepresent unbound decor.

## Pending integration and asset blocker

The native decor master ZIP `libfile_070c2230eba8819193e98d307bbad59a` could not be materialized: the documented supported route and one explicitly local retry both returned HTTP403 with fresh unexpired transfers. No further automatic retry or alternate download route is attempted. See `DECOR_TRANSFER_BLOCKER.md` for the exact bounded evidence. The parent is awaiting the user's local ZIP path. No master has been inspected, cleaned, cropped, scaled, substituted or integrated here.

After that handoff: inspect masters and resolve the inconsistent window-fit anchor, preserve transparency and separate reusable objects/symbols, then bind the optional START/shop/progress/purchase/room placement and schema7 store. Bind tutorial/heroine-position commands and the recovery UX deliberately; those session APIs are not yet present. Permit stake selection after base completion. Repeat real rendered narrow/tall sizing, purchase/relaunch/reset, full optional completion and performance checks against the integrated final code. Android tooling/export remains deferred. Stop dependent work until the asset handoff.
