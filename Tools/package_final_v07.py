from pathlib import Path
import argparse,json,hashlib,subprocess,zipfile
r=Path(__file__).resolve().parents[1];d=r/'Deliverables';frozen='dd74c983dcb22eaa7a03b54027dfa3b3aefe8926'
a=argparse.ArgumentParser();a.add_argument('--kind',choices=['windows','source'],required=True);kind=a.parse_args().kind
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
head=subprocess.check_output(['git','-C',str(r),'rev-parse','HEAD'],text=True).strip()
subprocess.run(['git','-C',str(r),'diff',frozen,'--exit-code','--','Assets/Scripts','Assets/Resources','Assets/Scenes','ProjectSettings','Packages'],capture_output=True,check=True)
tracked=[x.decode() for x in subprocess.check_output(['git','-C',str(r),'ls-files','-z']).split(b'\0') if x]
archive=d/('zero-to-millionaire-'+('complete-source' if kind=='source' else 'windows')+'-v0.7.zip')
assert not archive.exists(),'Immutable delivery exists'
selected=[]
if kind=='source':
 for rel in tracked:
  p=r/rel
  if not p.is_file() or any(x in rel.split('/') for x in ['bin','obj','_Recovery']):continue
  roots=rel.startswith(('Assets/','Packages/','ProjectSettings/','references/','Tools/','Checks/'))
  evidence=rel.startswith('Evidence/') and (('v07' in p.name or 'v0.7' in p.name) and p.suffix.lower() in {'.txt','.json','.xml','.png','.csv'}) and not any(x in p.name for x in ['before-','attempt','relaunch-first','-mcp-','after-spin-','usability-regression'])
  docs=('/' not in rel and p.suffix.lower()=='.md') or rel in {'.gitignore','.gitattributes'}
  if roots or evidence or docs:selected.append(p)
 metadata={'game_build_source':frozen,'source_docs_tests_head':head,'production_diff_against_frozen':'empty','private_saves_caches_tooling_excluded':True,'engine':'Unity 6000.3.21f1','native_validation':'actual Release pixels and private launch/quit/relaunch; physical OS input untested','android_v07':'deferred'}
 prefix='zero-to-millionaire-source-v0.7/'
else:
 base=r/'Builds/Windows-v0.7-candidate-1'
 assert (base/'RUN_WINDOWS_V07.md').is_file() and (base/'BUILD_IDENTITY.json').is_file()
 selected=[p for p in base.rglob('*') if p.is_file() and 'DoNotShip' not in str(p)]
 prefix='zero-to-millionaire-windows-v0.7/'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for p in sorted(selected):z.write(p,prefix+p.relative_to(r if kind=='source' else base).as_posix())
 if kind=='source':z.writestr(prefix+'SOURCE_VERSION.json',json.dumps(metadata,indent=2))
with zipfile.ZipFile(archive) as z:
 assert z.testzip() is None
 names=z.namelist();assert all('\\' not in n and '../' not in n and '/Library/' not in n and '/.tooling/' not in n for n in names)
 if kind=='source':
  for rel in ['Assets/Scenes/Level1.unity','Assets/Scripts/Runtime/GameView.cs','Assets/Scripts/Runtime/UiInputCoordinator.cs','Assets/Editor/FinalLevelAuditV07.cs','Assets/Resources/Fonts/OFL.txt','RUN_WINDOWS_V07.md','FINAL_VALIDATION_V07.md']:assert prefix+rel in names,rel
  assert len([n for n in names if n.startswith(prefix+'references/') and n.endswith('.png')])==6
 else:assert prefix+'ZeroToMillionaire.exe' in names and all('DoNotShip' not in n for n in names)
record={'kind':kind,'file':archive.name,'bytes':archive.stat().st_size,'sha256':sha(archive),'zip_crc_verified':True,'file_count':len(names),'game_build_source':frozen,'source_docs_tests_head':head}
(d/('final-v0.7-'+kind+'-delivery.json')).write_text(json.dumps(record,indent=2))
print(json.dumps(record))
