# Offline architecture - verified desktop checkpoint

This project has no backend service. The application boundary is GameSession, with injected IStateStore and reward randomness. GameModel owns the finite eight-task domain rules; SaveStore owns bounded local JSON, raw schema validation, migration and atomic primary/backup persistence. GameView owns input, UI, movement and cosmetic animation.

Implemented and reproduced fixes:
- Reject malformed schema3 quest records before normalization. Duplicate, unknown, wrong-case, missing and invalid-enum IDs select the valid backup. Missing JSON arrays are no longer invented by constructor defaults.
- Publish a candidate only after its save succeeds. The spin result is committed before cosmetics; a presentation lease rejects repeat spin and purchase input.
- If reset commits its primary but its second backup write fails, keep the committed fresh live state and retry backup later.
- Check cash overflow using the net transaction in a wider intermediate.

Final code passed 34,586 economy assertions, 1,000 complete seeded levels, 432,000 walking samples and 52 targeted offline session checks, including an actual interrupted disk write and concurrent purchase taps. The fresh Unity run also passed the full actual UI callback flow: 16 spins, eight purchases, menu, reset cancellation/confirmation and recovery. This is editor-rendered/automated input evidence, not a physical Android device test.

All six approved references were inspected as actual pixels. Six matched 360x640 room stages and 360x800/480x960 live states were rendered. The neutral heroine now uses a uniform 212-unit canvas: source opaque height166 gives approximately100 pixels in the short viewport, matching the reference's approximately100-pixel adult figure. Foot pivot and all collision footprints are unchanged. Short-screen room size is280 square layout units; cash milestones now have connected octagonal markers.

The independent generated room art remains steeper than the references (near-square opaque bounds versus their flatter composition). The neutral walking pose and licensed Pixelify font also differ from the generated reference. The finite active/NEXT task panel is the approved gameplay adaptation. Separate art layers, sprites and item symbols remain editable; the prototype is not a flattened screenshot.

Current evidence: Evidence/unity-model-checks.txt, runtime-v0.3-qa.txt, unity-ui-v0.3-final-qa.log and v0.3-six-reference-review.png. The final Windows rebuild succeeded and the responding player is open. Launch kept87cash/9spins/2owned; the user has since actively advanced. A forced close was avoided; once the prior player was already closed, the unchanged build was reopened. Subsequent live input changed the state before comparison, so exact desktop-relaunch progress equality is unverified. No reset or old-save restoration was performed by the assistant. See windows-v0.3-startup.json and windows-v0.3-closeout.json. Android packaging is deferred; installed Android tooling and previous APKs are preserved.
