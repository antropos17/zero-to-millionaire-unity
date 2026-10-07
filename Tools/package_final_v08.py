from pathlib import Path
import argparse,json,hashlib,subprocess,zipfile
r=Path(__file__).resolve().parents[1];d=r/'Deliverables'
a=argparse.ArgumentParser();a.add_argument('--kind',choices=['source','windows'],required=True);a.add_argument('--build-source',required=True);o=a.parse_args()
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git','-C',str(r),*args])
head=git('rev-parse','HEAD').decode().strip();assert len(o.build_source)==40
subprocess.run(['git','-C',str(r),'diff',o.build_source,'--exit-code','--','Assets/Scripts','Assets/Resources','Assets/Scenes','ProjectSettings','Packages'],capture_output=True,check=True)
archive=d/('zero-to-millionaire-'+('complete-source' if o.kind=='source' else 'windows')+'-v0.8.zip');assert not archive.exists(),'Immutable delivery exists'
if o.kind=='source':
 tracked=[x.decode() for x in git('ls-files','-z').split(b'\0') if x];files=[]
 for rel in tracked:
  p=r/rel
  if not p.is_file() or any(x in rel.split('/') for x in ['bin','obj','_Recovery']):continue
  roots=rel.startswith(('Assets/','Packages/','ProjectSettings/','references/','Tools/','Checks/'))
  docs=('/' not in rel and p.suffix.lower()=='.md') or rel in {'.gitignore','.gitattributes'}
  ev=rel.startswith('Evidence/') and ('v08' in p.name or 'v0.8' in p.name) and p.suffix.lower() in {'.txt','.json','.xml','.png','.gif','.csv'}
  excluded=any(x in p.name for x in ['v08-reviewed-after2-','v08-reviewed-after3-','v08-reviewed-after4-','v08-entrance--','v08-entrance-.','v08-suite-1-interrupted'])
  if roots or docs or (ev and not excluded):files.append((p,rel))
 prefix='zero-to-millionaire-source-v0.8/'
 identity={'game_build_source':o.build_source,'source_docs_tests_head':head,'production_diff_against_build':'empty','engine':'Unity 6000.3.21f1','private_saves_caches_tooling_excluded':True,'android_v08':'deferred','input_coverage':'synthetic real UITK Editor events; no physical OS/device input','native_coverage':'clean Windows Release render and private save relaunch; see final validation'}
else:
 base=r/'Builds/Windows-v0.8-candidate-1';assert (base/'BUILD_IDENTITY.json').is_file() and (base/'RUN_WINDOWS_V08.md').is_file()
 files=[(p,p.relative_to(base).as_posix()) for p in base.rglob('*') if p.is_file() and 'DoNotShip' not in str(p)]
 prefix='zero-to-millionaire-windows-v0.8/'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for p,rel in sorted(files,key=lambda x:x[1]):z.write(p,prefix+rel)
 if o.kind=='source':z.writestr(prefix+'SOURCE_VERSION.json',json.dumps(identity,indent=2))
with zipfile.ZipFile(archive) as z:
 assert z.testzip() is None;names=z.namelist();assert all('\\' not in n and '../' not in n and '/Library/' not in n and '/.tooling/' not in n for n in names)
 if o.kind=='source':
  for rel in ['Assets/Scenes/Level1.unity','Assets/Scripts/Runtime/DialoguePresenter.cs','Assets/Editor/DialogueEntranceAuditV08.cs','Assets/Resources/Fonts/INTER-OFL.txt','FINAL_VALIDATION_V08.md']:assert prefix+rel in names,rel
  assert len([n for n in names if n.startswith(prefix+'references/') and n.endswith('.png')])==6
 else:assert prefix+'ZeroToMillionaire.exe' in names
record={'kind':o.kind,'file':archive.name,'bytes':archive.stat().st_size,'sha256':sha(archive),'zip_crc_verified':True,'file_count':len(names),'game_build_source':o.build_source,'source_docs_tests_head':head}
(d/('final-v0.8-'+o.kind+'-delivery.json')).write_text(json.dumps(record,indent=2))
print(json.dumps(record))
