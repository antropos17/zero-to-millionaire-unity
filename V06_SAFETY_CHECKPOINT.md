# v0.6 safety and Russian UI checkpoint

Local branch prototype/v0.6-russian-ui. Production v0.5 folders/archives remain immutable. Windows is the main deliverable; requested typography/motion slice follows this checkpoint before17:15UTC feature freeze and18:05UTC delivery.

Installed Unity6000.3.21f1 modules: AndroidPlayer, WebGLSupport, windowsstandalonesupport. BuildPipeline reports Linux=false, WebGL=true. No module installation. WebGL QA will be isolated, served only on the cloud reviewer's localhost. Windows-only --ztm-private-profile is not a WebGL save flag; use a fresh browser profile/origin.

Reproduced on private data before fixing: step2 ring detached after an actual spin redraw;20/20 wrapped to29.25px within19.5px; unsupported schema6 was loaded as cash0 and automatically overwritten by Save(). Evidence/polish-v06-before-reproduction.txt and save-safety-v06-before.txt record these. No actual user file used.

SaveStore now blocks writes for unreadable or future progress. A valid supported backup still recovers; an older backup cannot silently replace a future primary. Explicit retry rereads, and confirmed fresh start preserves primary/backup byte streams as .recovery-TICKS before writing. Runtime displays an explicit recovery choice and disables play while blocked. Save schema/economy/quest IDs unchanged.

Story queue now pauses under menu/reset/recovery. Seen bits commit on meaningful dismissal; quitting an undismissed story keeps it pending. Failed Next/Skip/dismissal invokes clear Russian error feedback; retry is explicit, never a0.12s write loop. Core counted-store checks and actual Editor UI menu/reload fixtures cover these; runtime before/after proofs are distinguished from source-path findings.

Russian presentation labels/menu/quest titles/rewards/save feedback cover the existing flow. English brand remains. Pixel font outlines, art, rig, motion rules and raised-button styling are unchanged. All current Cyrillic literals are supported. Full counters have reserved NoWrap width; exceptionally large cash reserves112px.160 real target layouts across320x640,360x640,360x800,480x960 passed2624 assertions, including all8full goals, legacy intMax balances, both-resource gating and repeated purchase/reload.24 critical EditMode tests passed;14 dialogue persistence checks; economy107769 assertions/1000levels and navigation432000 samples remain passing. Onboarding menu/story/replay/reset callbacks passed666 advancing frames/11.41s. These are automated Editor callbacks, not physical/OS input.

Windows OS-pointer automation has sent no inputs: launch guards rejected recent external input twice, and a read-only4-second observation returned idle=0 at both samples. Desktop isDefault. User interaction cannot be ruled out, so safeguards are retained. ComputeBuffer shutdown warning remains unsuppressed; ownership still unproven. No engine edits or fake leak-freedom claim.

Current user save was read only and is preserved (hash93A5CDC9A2E7B5D7DA598926A413BFC967B0738E1A92D6269B59D1052C5A125E at this checkpoint). Never restore an older hash: the user may legitimately play. No user Player was closed by QA.

All8menu/confirmation layouts now pass; a measured360x640overflow was corrected by clamping the lower modal edge. Pending this bounded checkpoint: separate Windows/WebGL safety build and clean-native recovery/story quit+relaunch. Next requested slice: measured reel/result/reward motion, typography hierarchy and movement review with before/after rendered evidence. Final hardware/Android coverage remains unclaimed; Android packaging is deferred.
