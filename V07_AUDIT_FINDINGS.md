# v0.7 initial findings checkpoint — before production changes

Baseline game source: 6bec3763f688e44cf4266044fd995a945f96ebed.
Delivered source/docs/tests HEAD: 252f2dd609376a9c68fb1cfcdab44cb12290ac22.
Working branch: prototype/v0.7-readability-input. No new timer.
No production scripts, fonts, gameplay art, economy, scene, schema or packages changed
for this checkpoint. Only an Editor-only audit and evidence were added. Prior releases
and actual user save remain authoritative and preserved.

## Chosen skills and tools

- Official Unity plugin 0.1.8-beta ui-uitk: current C# UI Toolkit structure, flex/picking,
  Painter2D skin, text and event rules; read uss-guide, common-issues, painter2d.
- Official unity-cli: Editor/Pipeline discovery, playmode-verification-loop, tests/build
  guidance, diagnostics. Existing Unity Test Framework and project ModelChecks /
  SessionChecks / SaveRecoveryTests / layout audits remain the regression tools.
  Existing ProfilerRecorder/NativeMetrics can measure affected work without installing
  another profiler or framework. Source/Editor timing will not be called device FPS.
- Local better-typography: measured role hierarchy, readable body copy, wrapping,
  numeric metrics and contrast. Local better-accessibility: keyboard paths, visible
  focus, modal focus, target geometry and redundant state cues. Web-specific CSS/ARIA
  rules are adapted to native UITK, not pasted into it.
- Existing ARCHITECTURE_REVIEW.md / AGENTS.md establish UI -> GameSession -> IStateStore /
  SaveStore. No online backend, account, payment or multiplayer plugin is justified.
  The larger refactor agent workflow was inspected but not selected for this bounded pass.
- CLI 1.0.0-beta.12 self-update --check reports installed version is current. No update,
  installation, terms, grants or consent change performed.
- Actual MCP stdio initialize/tools-list/tools-call succeeded: unity-mcp 1.0.0-beta.12,
  160 tools, harmless eval read confirms X:\PROJECTS\ZERO-TO-MILLIONAIRE,
  Unity6000.3.21f1, Assets/Scenes/Level1.unity, playing=false and isolated profile.
  Evidence: v07-mcp-read.json and v07-mcp-catalog.json. Existing MCP processes preserved.

## Confirmed actionable findings

| Severity | Location | Before | Proposed bounded correction | Why |
|---|---|---|---|---|
| MEDIUM | Assets/Resources/Fonts/PixelifySans.ttf; GameView.cs counters | Digit1 advance404/1000em, other digits586/1000em; numbers are proportional | Deliberate tabular digit metrics in the licensed complete bundled font, or a measured single-font numeric treatment | Numeric columns/values should align consistently; current fixed containers prevent container movement, but glyph columns are uneven |
| MEDIUM | GameView.cs Text()/ButtonText(); DialoguePresenter.cs | Many gameplay labels10–12 logical units, broad default Bold; dialogue body13 regular/menu body14 regular | Central semantic type roles; increase useful small labels/controls selectively, keep display/title style and actual 3-size fit | Small Cyrillic is dense, and weights obscure hierarchy; blanket scaling would overflow the compact menu |
| MEDIUM | PixelUi.cs Skin()/Draw() | Pointer pressed bevel exists, but focus has no visible indication; actual SPIN crops change0 pixels at all3 sizes | Visible pixel-style focus and restrained hover; keyboard pressed feedback without adding a second action handler | Focused control is invisible to keyboard users |
| MEDIUM | GameView.cs ShowSettings/HideSettings/ShowReset/HideReset | Menu opens with focus none; Escape leaves menu open; closing does not restore trigger focus | Scoped modal focus entry/traversal, Escape cancellation, safe focus restoration; keep background disabled | Keyboard flow is incomplete |
| MEDIUM | GameView.cs failed transaction paths / ShowSettings / saveLabel | Simulated purchase I/O failure shows transient toast; opening menu hides it and still says progress stored locally | One consistent save-status refresh; retain actionable error until a successful retry; audit destructive late callbacks | Actual persistent status does not expose the failure |
| LOW | PixelUi.cs terracotta/disabled faces | Cream versus active light band contrast3.929:1, base4.319:1; grey3.768:1 before disabled tint | Scoped palette/text treatment for small action captions and readable unavailable reasons | Small BUY captions need better separation; these are measurements, not a platform compliance verdict |

## Verified existing behavior to retain

- Main target geometry already meets project44x44 aim: MENU44x44, BUY77x48,
  SPIN225x68 at360x640 and225x96 at360x800. At480x960 panel logical size is360x720
  and raster scaling is4/3; MENU44.25x44.25 and BUY77.25x48. These are logical units,
  not a claimed Unity/Windows requirement. Child captions already use PickingMode.Ignore.
  Edge/child/icon/adjacent-target tests still need to be added in implementation.
- Bundled ZTM Pixelify contains all checked Cyrillic uppercase/lowercase, Ё/ё, Й/й,
  digits and /×²—–•…«»; no missing glyph in that corpus. Font has wght400–700.
  Runtime PanelSettings uses ScaleWithScreenSize, reference360x640, width matching;
  textSettings is default/null. Plan deliberate complete setup, not unnecessary fallback
  chains. Complete production-string corpus and actual max currency will be checked later.
- Cream/forest palette ratio12.771:1 is strong; preserve it.
- GameSession already locks actions, commits candidate save before publishing, rejects
  repeat purchase and holds a spin presentation lease. SaveStore bounds JSON to64KiB,
  validates raw versions/IDs/shape, atomically replaces primary with backup, protects
  future saves and retains original files for explicit recovery reset. No rewrite needed.
- Injected failed purchase kept60cash,0owned. Thus the reproduced issue is reporting,
  not an observed debit/rollback defect. Final affected persistence/transaction regressions
  will be rerun after any corrections.
- Long-delayed hidden reset control did NOT reproduce an accidental reset: retained focus
  cleared to null before delayed event test, cash60 stayed60. Do not report this as a bug.
  Immediate stale events / repeated submit / hold keys remain acceptance tests.

## Actual baseline evidence and protection

Editor-only UsabilityAuditV07 compiled with0errors/0warnings. Real offscreen output
captured at360x640,360x800,480x960: main, focused SPIN, menu and dialogue, plus failure
menu and exact SPIN crops. 98 actual advancing runtime frames recorded. UI/keyboard
callbacks are synthetic; there is no OS-pointer, native Player or device pass here.
Evidence/v07-before-usability-audit.txt and v07-before-font-metrics.json record geometry,
font metrics, contrast and byte/pixel comparison. Focus crops have0changed pixels at
all sizes. Images were visually inspected.

Our audit Editor PID16444 / Pipeline7800 is retained ready; private saves only under
.tooling/v07-audit-private and .tooling/v07-before-ui. Initial real-user save SHA256:
1bc60cbf31ef17176b0b6870fee9485b318c30a330e1b61b90ea9110d19bc1ca.
No actual game Player was running at initial inspection; no user process was closed.
Current read-only input desktop check: Default. This permits evaluating guarded isolated
pointer tests later, not a claim that input has been tested; still require owned window,
focus, inactivity and external-input guards. Never dismiss screensaver/change security.

## Next implementation slice / done criteria

Implement focused type roles/numeric treatment, focus/hover/pressed/disabled feedback,
modal input/focus coordination and save-status reporting. Preserve domain/save schema and
balance unless a specific regression proves a needed fix. Test exactly-one action, held /
repeated / late inputs, insufficient resources, modal click-through, UI recreation and
subscriptions, saves/migrations/future/corrupt/write interruption. Compare matching actual
before/after frames at all3 sizes, build separate Windows-v0.7 candidate. Native rendering
and actual pointer tests must be labelled separately and guarded; otherwise state blocked.
Clean local Git and runnable candidate; complete source/assets/references plus Windows
archives, every Library delivery part<=15000000bytes. Android remains deferred.

Official references checked:
https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-fallback-font.html
https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-focus-order.html
https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-Events-Dispatching.html