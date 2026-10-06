from pathlib import Path
import hashlib,json,subprocess,zipfile
root=Path(__file__).resolve().parents[1];out=root/'Deliverables';out.mkdir(exist_ok=True)
source=subprocess.check_output(['git','-C',str(root),'rev-parse','HEAD'],text=True).strip()
subprocess.run(['git','-C',str(root),'diff','17f525238b1925819b857f9bfd31cb5ec4062126','--exit-code','--','Assets/Scripts/Core','Assets/Scripts/Runtime','Assets/Resources/Art','Assets/Resources/Fonts','ProjectSettings','Packages'],check=True,capture_output=True)
entries=subprocess.check_output(['git','-C',str(root),'ls-files','-z']).split(b'\0')
selected=[]
for entry in entries:
 if not entry:continue
 rel=entry.decode('utf-8');p=root/rel
 if not p.is_file():continue
 if '/' not in rel and p.suffix=='.md' and p.name not in {'README.md','AGENTS.md','PROJECT_BRIEF.md','ASSET_MANIFEST.md','ARCHITECTURE_REVIEW.md','ONBOARDING_PACING_VALIDATION.md','NATIVE_RELEASE_VALIDATION_V05.md','RUN_WINDOWS_V05.md'}:continue
 if rel.startswith('Assets/_Recovery') or any(x in rel.split('/') for x in ['bin','obj']):continue
 if rel.startswith('Evidence/') and 'v0.5' not in p.name:continue
 if p.suffix in ['.log','.bak','.tmp'] or 'error' in p.name.lower():continue
 selected.append(p)
for name in ['RUN_WINDOWS_V05.md','NATIVE_RELEASE_VALIDATION_V05.md']:
 p=root/name
 if p.exists() and p not in selected:selected.append(p)
for p in (root/'Evidence').glob('*v0.5*'):
 if p.is_file() and 'error' not in p.name.lower() and p not in selected:selected.append(p)
source_zip=out/'zero-to-millionaire-first-playable-source-v0.5.zip'
assert not source_zip.exists(),'Immutable source archive already exists'
with zipfile.ZipFile(source_zip,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for p in sorted(selected):z.write(p,'zero-to-millionaire-first-playable/'+p.relative_to(root).as_posix())
 z.writestr('zero-to-millionaire-first-playable/SOURCE_VERSION.json',json.dumps({'game_build_source':'17f525238b1925819b857f9bfd31cb5ec4062126','source_docs_testing_head':source,'production_diff_against_build_source':'empty','private_user_saves_and_coordination_docs':'excluded'},indent=2))
with zipfile.ZipFile(source_zip) as z:
 assert z.testzip() is None
 names=z.namelist();pre='zero-to-millionaire-first-playable/'
 for rel in ['Assets/Tests/Runtime/NativeOnboardingCheck.cs','Assets/Scenes/Level1.unity','Assets/Scripts/Core/Onboarding.cs','Assets/Scripts/Runtime/DialoguePresenter.cs','Assets/Tests/Editor/OnboardingTests.cs','ONBOARDING_PACING_VALIDATION.md','RUN_WINDOWS_V05.md','Assets/Resources/Fonts/OFL.txt','Assets/Resources/Fonts/MODIFICATION.txt']:
  assert pre+rel in names,rel
 assert len([x for x in names if x.startswith(pre+'references/') and x.endswith('.png')])==6
 assert all('\\' not in x and '.tooling/' not in x and '/Library/' not in x for x in names)
windows_zip=out/'zero-to-millionaire-windows-v0.5.zip';base=root/'Builds/Windows-FINAL-v0.5'
assert not windows_zip.exists(),'Immutable Windows archive already exists'
with zipfile.ZipFile(windows_zip,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for p in sorted(base.rglob('*')):
  if p.is_file():z.write(p,'zero-to-millionaire-windows-v0.5/'+p.relative_to(base).as_posix())
with zipfile.ZipFile(windows_zip) as z:
 assert z.testzip() is None
 assert 'zero-to-millionaire-windows-v0.5/ZeroToMillionaire.exe' in z.namelist()
 assert 'zero-to-millionaire-windows-v0.5/RUN_WINDOWS_V05.md' in z.namelist()
 assert all('DoNotShip' not in x for x in z.namelist())
records=[]
for p in [source_zip,windows_zip]:records.append({'file':p.name,'path':str(p),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'zip_crc_verified':True,'source_docs_testing_head':source,'game_build_source':'17f525238b1925819b857f9bfd31cb5ec4062126'})
(out/'final-v0.5-delivery.json').write_text(json.dumps(records,indent=2),encoding='utf-8');print(json.dumps(records))
