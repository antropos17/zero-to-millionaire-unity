from pathlib import Path
import json,hashlib,subprocess,zipfile
r=Path(__file__).resolve().parents[1];d=r/'Deliverables';frozen='6bec3763f688e44cf4266044fd995a945f96ebed'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
head=subprocess.check_output(['git','-C',str(r),'rev-parse','HEAD'],text=True).strip()
subprocess.run(['git','-C',str(r),'diff',frozen,'--exit-code','--','Assets/Scripts/Core','Assets/Scripts/Runtime','Assets/Resources/Art','Assets/Resources/Fonts','ProjectSettings','Packages'],capture_output=True,check=True)
tracked=[x.decode() for x in subprocess.check_output(['git','-C',str(r),'ls-files','-z']).split(b'\0') if x]
docs={'README.md','AGENTS.md','PROJECT_BRIEF.md','ASSET_MANIFEST.md','ARCHITECTURE_REVIEW.md','ONBOARDING_PACING_VALIDATION.md','RUN_WINDOWS_V06.md','FINAL_VALIDATION_V06.md','V06_SAFETY_CHECKPOINT.md','V06_MOTION_CHECKPOINT.md','V06_REVIEW_FIXES.md','V06_TYPOGRAPHY_REVIEW.md'}
selected=[]
for rel in tracked:
 p=r/rel
 if not p.is_file() or any(x in rel.split('/') for x in ['bin','obj','_Recovery']):continue
 roots=rel.startswith(('Assets/','Packages/','ProjectSettings/','references/','Tools/'))
 evidence=rel.startswith('Evidence/') and (('v06' in p.name or 'v0.6' in p.name) and p.suffix.lower() in {'.txt','.json','.xml','.png','.gif','.csv'})
 if roots or evidence or rel in docs or rel in {'.gitignore','.gitattributes'}:selected.append(p)
assert not any('PerformanceTestRun' in p.name for p in selected)
source=d/'zero-to-millionaire-complete-source-v0.6.zip';windows=d/'zero-to-millionaire-windows-v0.6.zip'
assert not source.exists() and not windows.exists(),'Immutable delivery exists'
base=r/'Builds'/'Windows-v0.6-final';assert (base/'ZeroToMillionaire.exe').is_file();assert (base/'RUN_WINDOWS_V06.md').is_file()
metadata={'game_build_source':frozen,'source_docs_tests_head':head,'production_diff_against_frozen':'empty','private_saves_caches_tooling_excluded':True,'engine':'Unity6000.3.21f1','native_render_input':'unverifiedScreen-saver','android_latest':'deferred'}
with zipfile.ZipFile(source,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for p in sorted(selected):z.write(p,'zero-to-millionaire-source-v0.6/'+p.relative_to(r).as_posix())
 z.writestr('zero-to-millionaire-source-v0.6/SOURCE_VERSION.json',json.dumps(metadata,indent=2))
with zipfile.ZipFile(windows,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for p in sorted(base.rglob('*')):
  if p.is_file() and 'DoNotShip' not in str(p):z.write(p,'zero-to-millionaire-windows-v0.6/'+p.relative_to(base).as_posix())
records=[]
for p,kind in [(source,'source'),(windows,'windows')]:
 with zipfile.ZipFile(p) as z:
  assert z.testzip() is None;names=z.namelist();assert all('\\' not in n and '../' not in n and '/Library/' not in n and '/.tooling/' not in n for n in names)
  if kind=='source':
   pre='zero-to-millionaire-source-v0.6/'
   for rel in ['Assets/Scenes/Level1.unity','Assets/Scripts/Runtime/ReelMotion.cs','Assets/Scripts/Runtime/PixelEffects.cs','Assets/Scripts/Runtime/GameView.cs','Assets/Editor/FinalLevelAuditV06.cs','Assets/Resources/Fonts/OFL.txt','RUN_WINDOWS_V06.md','FINAL_VALIDATION_V06.md']:assert pre+rel in names,rel
   assert len([n for n in names if n.startswith(pre+'references/') and n.endswith('.png')])==6
  else:assert 'zero-to-millionaire-windows-v0.6/ZeroToMillionaire.exe' in names and all('DoNotShip' not in n for n in names)
 records.append({'kind':kind,'file':p.name,'bytes':p.stat().st_size,'sha256':sha(p),'zip_crc_verified':True,'game_build_source':frozen,'source_docs_tests_head':head})
(d/'final-v0.6-delivery.json').write_text(json.dumps(records,indent=2));print(json.dumps(records))