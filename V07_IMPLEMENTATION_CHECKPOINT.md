# v0.7 implementation checkpoint — typography, focus and offline save status

User scope remains open. The only intervening instruction was «открой прототип» (open the prototype); no pause/cancellation was requested. Windows v0.6 was launched as PID46592. No automated OS input was sent to that session, and it was not closed by this work. Its save was never used for QA or restored to an older version.

## Implemented

- One complete Cyrillic pixel font remains in use. Digit1 now shares the other digits' advance width across weights400/550/700; the same outline is centered. Existing Cyrillic cmap, family identity, OFL license and reference art are retained.
- Typography roles: titles16, metadata12, counters13, menu body15, dialogue14. Menu help has three concise paragraphs. Dialogue space increased from135 to150 units. Main SPIN hierarchy is retained.
- Real Button controls retain native Clickable activation, with a visible gold focus ring, restrained hover outline and pressed feedback. Action targets have a45-unit minimum so fractional pixel snapping remains at least44 units in tested layouts. Captions pass pointer hits through.
- One disposable input coordinator per document scopes Tab/Shift+Tab to visible enabled buttons; modals have initial focus, Escape cancellation and trigger-focus restoration. Reset starts on safe Cancel. Recovery cannot be dismissed through Escape. Held Enter/Space cannot perform a second transaction after animation or a failed write; detached/hidden controls are inert.
- Save failures remain visible in the menu, with a truthful highlighted status, until a successful write/reload. The injected failure case preserves cash/ownership; a fresh retry succeeds exactly once.
- UnityStateCodec moved into its own file with identical class text. Core economy, GameSession, SaveStore, schema, scenes, packages and PNG art did not change. Existing offline transaction locking, candidate-save-before-publish, rollback and recovery protections remain the backend.

## Final-code checks completed

- Unity recompilation:0 errors,0 warnings.
- UsabilityRegressionV07:PASS995 assertions,347 advancing runtime frames,42.84s. Actual offscreen Editor pixels; UI Toolkit keyboard/navigation and pick/PointerDown/Up/Cancel events, all synthetic. Three sizes360x640,360x800,480x960; focus/Tab/Escape, both-resource gates, failed-write status, held keys past animation, pointer purchase, repeated/retired/hidden actions, reset confirmation and scene recreation.
- RussianLayoutAuditV07:PASS2624 assertions,160 combinations, all8 tasks, four sizes320x640/360x640/360x800/480x960. Full counters including intMax, task titles,44-unit minimum targets, purchase gating, repeated purchase and disk reload.
- Existing model/session/navigation/migration regressions:107769 economy assertions,1000 completed levels (49–60 spins, median53,p9557),432000 motion samples,52 offline session checks and21106 layout/migration assertions. Core simulation380ms; this is not native rendering performance.
- Unity Test Framework EditMode:24 passed,0 failed,0 skipped.
- All145 characters extracted from production string literals are present in the font. Rendered1111111111 and8888888888 widths match. FontTools confirms equal digit advances at weights400/550/700.
- Palette contrast calculations: cream/forest12.771:1; cream/lightest active terracotta face4.570:1; cream/darkest face5.299:1. These are source-color calculations, not a full accessibility certification.
- Focused SPIN crops now differ by1152/1264/2384 pixels across the three sizes; before each difference was0. Actual before/after main, menu and dialogue comparisons accompany the source review.

Earlier attempts are preserved. Attempt1 found a short menu target; attempt2 identified43.5-unit fractional snapping despite a44-unit style, corrected with45 minimum; attempt3 passed functionality then found an index error in the new recreation harness. Final corrected suite passed; no economic/backend defect was found or invented.

## Isolation and current limitations

QA ran only in .tooling/v07-input-regression/save, .tooling/russian-layout-v07-save and .tooling/v07-model-checks, owned Editor PID16444. The active user save changed while the separately launched v0.6 session was running; current bytes were preserved, never rolled back. Evidence/v07-isolation-checkpoint.json records the hashes without save contents.

This is a source/Editor-review checkpoint. No Windows v0.7 candidate has been built yet, and no OS keyboard/mouse, real Android device, APK export or native v0.7 performance claim is made. The requested next slice is independent code/image review, then a separate Windows v0.7 Release candidate and isolated native verification. Old v0.6 release remains intact. Android remains deferred. No new timer, server, account, network dependency, purchase, ad, payment, publishing, external push or software install.

## Reproduce in the existing Editor

Canonical project: X:/Projects/ZERO-TO-MILLIONAIRE; Unity6000.3.21f1. Never aim QA at the user's save.

1. Stop only the owned audit Editor's Playmode, then call ZeroToMillionaire.Editor.UsabilityRegressionV07.Start(). It supplies its own private profile and writes Evidence/v07-usability-regression-checks.txt.
2. Call ZeroToMillionaire.Editor.RussianLayoutAuditV07.Start() outside Playmode. It supplies another private profile and writes Evidence/v07-layout-checks.txt.
3. Run ZeroToMillionaire.ModelChecks.Run(new ZeroToMillionaire.UnityStateCodec(), a NEW isolated directory). Run CriticalTestRunner.Start with a NEW report tag to preserve prior evidence.

Tools used: existing official Unity CLI/MCP and Unity UI Toolkit/unity-cli skills, local better-typography and better-accessibility guidance, FontTools/Pillow for font/evidence verification, and Library for review delivery. No new plugin needed. The larger refactor skill was inspected but not invoked; a wholesale or multi-agent rewrite was inappropriate for these reproduced findings.
