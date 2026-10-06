# v0.6 motion and typography review checkpoint

Canonical project: X:\Projects\ZERO-TO-MILLIONAIRE. Windows remains primary.
This is a bounded review checkpoint; feature freeze remains 17:15 UTC.

The normal spin now presents the already committed 5x3 grid as five staggered
columns (80 ms apart, 600 ms travel, 140 ms settle; whole travel 1.06 s), then
300 ms real matching/triple emphasis and a 600 ms cash count. Cosmetic symbols
never consume gameplay random draws. STOP and a menu request skip presentation
without rolling or paying again, including requests in the initial frame.

The existing cream / dark forest / terracotta skin and separate source sprites
remain. Four small reward pixels travel from a collected symbol toward its meter;
manual installation reveals the bought surface/object over 220 ms. No cascades,
near-win alterations, multiplier features, extra rooms, ads or networking added.

Raised button faces ease down over 80 ms and up over 120 ms. A persisted calm
presentation option removes strips and reward particles, resolves cash in 160 ms
and makes presses immediate. Its presentation-v1.json is independent of schema5
player progress and follows the same private QA profile resolution.

Typography changes preserve the existing font outlines: dialogue text 13 px,
small feedback 11 px, action captions 11 px; menu help uses regular weight at
14 px. Large counters and the main button keep their hierarchy. All Russian
labels remain localized; the brand title remains ZERO TO MILLIONAIRE.

Actual before/final capture results have identical cash, fragments, ownership,
quests, grid and tutorial. Only walking position differs. The comparison GIF
uses recorded Editor pixels selected by capture timestamps, without interpolation.
Prepared fixtures and synthetic UI callbacks are explicitly distinguished from
physical input and real-device coverage. GIF is 9 seconds, 720 x 744; individual
360 x 720 PNG frames accompany it. Evidence/motion-comparison-v06-provenance.json
records source frame indices and SHA256.

Required final checks remain: clean Windows release launch/relaunch, final
regressions, private save/recovery, narrow/tall sizing, completion and reset,
performance evidence, source/build packaging, and preservation of user save.
Android packaging is deferred by the latest owner instruction. v0.5 release and
v0.6 safety artifacts remain immutable. WebGL remains QA-only with browser
persistence unverified; no compatibility/security work or publication is planned.