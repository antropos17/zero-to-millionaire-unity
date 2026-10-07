from pathlib import Path
import hashlib,json,re,subprocess,zipfile,shutil
from PIL import Image,ImageSequence
from fontTools.ttLib import TTFont

root=Path(r'X:\Projects\ZERO-TO-MILLIONAIRE')
def git(*args):return subprocess.check_output(['git',*args],cwd=root)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def put(p,text):p.write_text(text,encoding='utf-8',newline='\n')
assert git('rev-parse','HEAD').decode().strip()=='16f35a9a42ffdc39d529380dbfc7b57bdceca4f6','Unexpected concurrent source commit'

# Restore only proven importer whitespace, preserving every semantic change.
for raw in git('diff','--name-only','--','Assets/Resources/Art','Assets/Resources/Fonts/PixelifySans.ttf.meta').decode().splitlines():
    assert raw.endswith('.meta'),raw
    previous=git('show','HEAD:'+raw);p=root/raw
    normalize=lambda b:b'\n'.join(l.rstrip() for l in b.splitlines())
    assert normalize(previous)==normalize(p.read_bytes()),'Semantic resource edit: '+raw
    p.write_bytes(previous)

checks={
 'focus':('v071-focus-intent-after3.txt','RESULT PASS assertions=12'),
 'core':('v08-model-session-checks-1.txt','PASS 107769'),
 'critical':('critical-v08-1-editmode.txt','passed=24 failed=0'),
 'layout':('v08-english-layout-checks-2.txt','RESULT PASS 2624'),
 'dialogues':('v08-dialogue-checks-2.txt','RESULT PASS checks=540 failures=0'),
 'input':('v08-compact-input-checks-1.txt','RESULT PASS checks=256'),
 'complete_level':('final-v08-level-checks.txt','RESULT PASS'),
 'capture':('v08-after-2.txt','PASS graphics-enabled Editor capture'),
 'motion':('motion-interaction-v08-checks.txt','skipped count-up lands on committed cash total')}
for kind,(name,expected) in checks.items():
    text=(root/'Evidence'/name).read_text(encoding='utf-8');assert expected in text and '\nFAIL ' not in text and 'RESULT FAIL' not in text,(kind,name)

font_rows=[]
for p in sorted((root/'Assets/Resources/Fonts').glob('ZTMReadable-*.ttf')):
    f=TTFont(p);digits=[f['hmtx'][f.getBestCmap()[cp]][0] for cp in range(48,58)];assert len(set(digits))==1
    font_rows.append({'file':p.name,'bytes':p.stat().st_size,'sha256':sha(p),'tabular_digit_advances':digits,'missing_english_glyphs':[chr(c) for c in range(32,127) if c not in f.getBestCmap()]})
assert all(not r['missing_english_glyphs'] for r in font_rows)
assert not any(re.search('[\u0400-\u04ff]',p.read_text(encoding='utf-8')) for p in (root/'Assets/Scripts').rglob('*.cs'))
put(root/'Evidence/v08-font-validation.json',json.dumps(font_rows,indent=2))

gifs=[]
for name,expected in [('v08-spin-before-after.gif',71),('v08-repair-before-after.gif',23)]:
    p=root/'Evidence'/name
    with Image.open(p) as im:
        assert 3<=im.n_frames<=expected and im.size==(720,668)
        duration=0
        for frame in ImageSequence.Iterator(im):
            frame.load();duration+=frame.info.get('duration',0)
        assert duration==expected*50,(name,duration,expected*50)
        encoded=im.n_frames
    gifs.append({'file':name,'source_frames':expected,'encoded_frames':encoded,'duration_ms':duration,'identical_frames_may_be_combined_by_gif_encoder':True,'sha256':sha(p),'decoded_all_frames':True})
put(root/'Evidence/v08-gif-validation.json',json.dumps(gifs,indent=2))
references=[{'file':p.name,'bytes':p.stat().st_size,'sha256':sha(p)} for p in sorted((root/'references').glob('*.png'))]
assert len(references)==6
put(root/'Evidence/v08-reference-integrity.json',json.dumps(references,indent=2))

put(root/'PROJECT_BRIEF_V08.md','''# ZERO TO MILLIONAIRE v0.8 visual checkpoint

User direction: English interface/tutorial, clearer type and more animation.
One offline portrait level, one fixed 3x3m room. Existing six-reference pixel
room, adult black-bob heroine and independent item/symbol assets are preserved.
No room expansion, luxury, timers, networking, login, ads, purchases or publishing.

Unity 6000.3.21f1/C# continues because this verified project, official integration
and Windows exporter are already available. No new software or terms accepted.
This milestone is a runnable Editor project and visual review checkpoint. The
new Windows release follows review; Android packaging remains deferred under
the latest owner direction. The existing v0.7 build and user game are preserved.

Economy is unchanged from v0.7 (tunable in Core/Economy.cs). Spin: 5 coins,
12 base payout, +2 per coin symbol, +10 per matching three-symbol column.
Three free starter spins and transparent FREE SPIN recovery prevent deadlock.
Parts: 20/30/38/44/50/58/62/70. Prices: 60/115/155/195/235/300/315/355.
Each matching current symbol gives exactly one part. Every upgrade still needs
BOTH its own parts and coins, followed by explicit BUY. Stable quest IDs and
schema5 data preserve old prices, excess parts, owned items and tutorial state.
1000 simulations completed in 49–60 spins (median53). The actual Editor run
completed in52 spins with8 manual buys and4 explicitly dismissed story comments.

Ready criteria for every follow-up: complete runnable fixed-room level; new
Windows build and APK when included in the agreed scope/tooling; correct touch
spin cash/parts; both-resource/repeat/overdraw guards; visible purchase changes;
disk save and actual relaunch; confirmed reset; compact/tall portrait sizing;
performance; explicit Editor/native/device/unrun coverage; six reference/resource
identity preservation; fresh final checks, screenshots, paths and install steps.
''')
put(root/'ASSET_MANIFEST_V08.md','''# v0.8 assets and presentation

Existing independent room layers, repaired door/window overlays, heroine rig,
mattress/lamp/stool and nine item symbols retain their approved v0.7 pixels.
The full project and all six reference PNGs are included in the checkpoint ZIP.
Their SHA-256 evidence is in Evidence/v08-reference-integrity.json; approved
Library identities remain recorded in ASSET_MANIFEST.md. No new flattened art.

Readable text: Resources/Fonts/ZTMReadable-Regular.ttf and
ZTMReadable-Semibold.ttf, derivatives of the installed OFL-licensed Inter faces.
Copyright and full license: INTER-OFL.txt. Provenance/changes:
READABLE-MODIFICATION.txt. Existing tabular digit glyphs are mapped by default;
letter outlines are unchanged. Actual normal/semibold resources are used.
The licensed Pixelify wordmark remains. All player-facing strings are English.
Official source: https://rsms.me/inter/

Motion: eased press with small release rebound; matched reel symbols lift/settle;
eight small reward pixels arc towards the current parts bar; destination gently
grows vertically instead of flashing opacity; repair crossfade and soft dust;
furniture settles five pixels during the existing reveal; dialogue enters in0.16s.
Transactions still save before all cosmetic frames. Existing reel/reward/reveal
durations and input rules are retained. CALM disables new travel/settle effects.
Idle effects do not repaint continuously; temporary transforms are cleared.

Ready: readable English at320x640/360x640/360x800/480x960; fixed-width counters
including int.MaxValue;44-unit buttons; real-frame review GIFs; stable symbol
identity, layer alignment, heroine grounding and fixed9m² footprint.
''')
put(root/'V08_VISUAL_CHECKPOINT.md','''# v0.8 English and animation checkpoint

Open Evidence/v08-room-before-after.png, v08-menu-before-after.png and
v08-tutorial-before-after.png for typography. Watch v08-spin-before-after.gif
and v08-repair-before-after.gif for real motion. Both columns are actual Unity
captures at approximately20fps. The baseline has v0.7 visuals plus the isolated
focus fix; no generated/tweened game frames were added.

Authoritative validation:

| Check | Result |
|---|---|
| Obsolete deferred focus vs pointer/navigation/disposal |12 PASS|
| Core economy, sessions, migration, walking |107769 economy;1000 levels;52 sessions;20998 layout;432000 walk samples PASS|
| Unity critical EditMode |24/24 PASS|
| English resource bars, all8 quests, max legacy balances, four sizes |2624 PASS,160 combinations|
| Five tutorial steps, four stories, five replay pages, menu/reset |540 PASS,64 actual layouts|
| Panel picks/44-unit edges, paired Tab, held input, rollback/retry/reset |256 PASS,three sizes|
| STOP/SKIP, duplicate awards, disk during animation, calm setting |30 PASS|
| Fresh full level, every purchase, disk and actual scene reload, reset |52 spins;8 buys;4 stories;456909 checks PASS|
| Editor timing during actual level |median16.67ms;p95 17.90ms;7763 frames|

Timing includes screenshot pauses and runs alongside the user game; it is an
Editor observation, not a standalone or device benchmark. Input is synthetic
UI Toolkit routing; no OS input was sent to the user's game. New executable,
APK, standalone/process relaunch and extraction checks follow visual review.

Earlier results are retained honestly: the first large-balance layout run failed
21 clipping checks, fixed by widening full cash counters; final run passed.
One combined test start overlapped EditMode teardown with Playmode. The older
large usability stress runner then failed an immediate fresh-key assertion and
timed out; its reports are not counted as PASS. The bounded compact runner uses
the actual runtime clock and independently passes hold/new key, purchase,
truthful errors, retry, modal and reset scenarios. Its result is authoritative.
The pre-existing Unity ComputeBuffer finalizer diagnostic stays unsuppressed;
the bounded allocation-ownership scan is not a native allocation-stack capture.

Run in Editor: use Unity6000.3.21f1, open the project folder, open
Assets/Scenes/Level1.unity and press Play. A normal run uses the local player
save; all QA runs used separate .tooling profiles. Avoid a normal Editor session
if the user is simultaneously playing the old executable with the same save.
For isolated review, set ZTM_QA_SAVE_DIR to a fresh local directory before
launching Editor. Current shipped Windows run remains:
Builds/Windows-v0.7-candidate-1/ZeroToMillionaire.exe.

Next slice: independent review of the English/font/motion checkpoint, then a
separate clean v0.8 Windows build, private native launch/quit/relaunch, ZIP/source
identity and extraction checks. Preserve v0.7 and the active user session.
''')

source=['Assets/Scripts/Core/Onboarding.cs','Assets/Scripts/Runtime/UiTypography.cs','Assets/Scripts/Runtime/GameView.cs','Assets/Scripts/Runtime/DialoguePresenter.cs','Assets/Scripts/Runtime/PixelUi.cs','Assets/Scripts/Runtime/PixelEffects.cs','Assets/Scripts/Runtime/ReelMotion.cs','Assets/Scripts/Runtime/RussianUi.cs','Assets/Scripts/Runtime/RussianUi.cs.meta','Assets/Scripts/Runtime/EnglishUi.cs','Assets/Scripts/Runtime/EnglishUi.cs.meta']
new_editor=['VisualCheckpointV08','EnglishLayoutAuditV08','EnglishDialogueAuditV08','UsabilityRegressionV08','MotionInteractionAuditV08','FinalLevelAuditV08','V08RegressionSuite','CheckpointCaptureQueueV08','CompactInputAuditV08']
source += [f'Assets/Editor/{n}{ext}' for n in new_editor for ext in ['.cs','.cs.meta']]
source += [p.relative_to(root).as_posix() for p in (root/'Assets/Resources/Fonts').iterdir() if p.name.startswith(('ZTMReadable-','INTER-OFL','READABLE-MODIFICATION'))]
source += ['PROJECT_BRIEF_V08.md','ASSET_MANIFEST_V08.md','V08_VISUAL_CHECKPOINT.md','Tools/implement_english_v08.py','Tools/implement_motion_v08.py','Tools/compose_checkpoint_v08.py','Tools/package_checkpoint_v08.py']
source += [p.relative_to(root).as_posix() for p in (root/'Evidence').iterdir() if p.is_file() and (p.name.startswith(('v08-','final-v08-','critical-v08-','motion-interaction-v08-'))) and p.suffix in ['.txt','.json','.png','.gif','.xml']]
git('add','--',*source)

receipt=root/'Evidence/source-hygiene-x.json';old_receipt=receipt.read_bytes() if receipt.exists() else None
scan=subprocess.run(['python',str(root/'Tools/check_source_hygiene.py'),'--staged'],cwd=root,capture_output=True,check=True)
put(root/'Evidence/source-hygiene-v08.json',receipt.read_text(encoding='utf-8'))
if old_receipt is not None:receipt.write_bytes(old_receipt)
git('add','--','Evidence/source-hygiene-v08.json')
assert git('branch','--show-current').decode().strip()=='prototype/v0.7.1-bug-polish'
git('branch','-m','prototype/v0.8-english-motion')
print(git('commit','-m','Add English UI, readable tabular typography and purposeful motion').decode())
head=git('rev-parse','HEAD').decode().strip()
assert not git('diff','--name-only','--','Assets/Scripts','Assets/Resources','Packages','ProjectSettings').decode().strip(),'Uncommitted production changes'

tracked=[v.decode() for v in git('ls-files','-z').split(b'\0') if v]
selected=[]
for rel in tracked:
    p=root/rel
    if not p.is_file() or any(x in rel.split('/') for x in ['bin','obj','_Recovery']):continue
    sources=rel.startswith(('Assets/','Packages/','ProjectSettings/','references/','Tools/','Checks/'))
    docs=('/' not in rel and p.suffix=='.md') or rel in {'.gitignore','.gitattributes'}
    evidence=rel.startswith('Evidence/') and p.name.startswith(('v08-','final-v08-','critical-v08-','motion-interaction-v08-','v071-focus-intent-after3')) and p.suffix in ['.txt','.json','.png','.gif','.xml']
    if sources or docs or evidence:selected.append(p)

destination=root/'Deliverables/zero-to-millionaire-v0.8-visual-checkpoint.zip'
assert not destination.exists(),'Immutable checkpoint exists'
prefix='zero-to-millionaire-v0.8-checkpoint/'
metadata={'source_head':head,'focus_fix_commit':'16f35a9a42ffdc39d529380dbfc7b57bdceca4f6','engine':'Unity6000.3.21f1','milestone':'English/font/motion visual review before build','new_native_build':'pending visual review','android':'deferred','synthetic_input_only':True,'private_saves_caches_tooling_excluded':True}
with zipfile.ZipFile(destination,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
    for p in sorted(selected):z.write(p,prefix+p.relative_to(root).as_posix())
    z.writestr(prefix+'CHECKPOINT_VERSION.json',json.dumps(metadata,indent=2))
with zipfile.ZipFile(destination) as z:
    assert z.testzip() is None
    names=z.namelist();assert all('\\' not in n and '../' not in n and '/Library/' not in n and '/.tooling/' not in n and '/Builds/' not in n for n in names)
    assert len([n for n in names if n.startswith(prefix+'references/') and n.endswith('.png')])==6
    for rel in ['Assets/Scenes/Level1.unity','Assets/Scripts/Runtime/EnglishUi.cs','Assets/Resources/Fonts/INTER-OFL.txt','Evidence/v08-spin-before-after.gif','Evidence/v08-compact-input-checks-1.txt']:
        assert prefix+rel in names,rel
    assert z.read(prefix+'Assets/Scripts/Runtime/GameView.cs')==(root/'Assets/Scripts/Runtime/GameView.cs').read_bytes()
record={'file':str(destination),'bytes':destination.stat().st_size,'sha256':sha(destination),'zip_crc_verified':True,'files':len(names),'source_head':head,'source_hygiene_passed':True}
put(root/'Deliverables/v08-visual-checkpoint-package.json',json.dumps(record,indent=2))
print(json.dumps(record,indent=2))
