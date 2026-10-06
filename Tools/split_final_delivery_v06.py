from pathlib import Path
import json,hashlib,subprocess,zipfile
root=Path(__file__).resolve().parents[1];delivery=root/'Deliverables'
records=json.loads((delivery/'final-v0.6-delivery.json').read_text(encoding='utf-8'))
seven=Path(r'C:\Program Files\7-Zip\7z.exe');assert seven.is_file()
folder=root/'.tooling/final-v0.6-split-extraction';assert not folder.exists()
assert folder.resolve().is_relative_to((root/'.tooling').resolve());folder.mkdir()
results=[]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for record in records:
 archive=delivery/record['file'];data=archive.read_bytes();assert hashlib.sha256(data).hexdigest()==record['sha256']
 parts=[]
 for index,offset in enumerate(range(0,len(data),15_000_000),1):
  part=delivery/(archive.name+f'.{index:03d}');assert not part.exists()
  part.write_bytes(data[offset:offset+15_000_000]);assert 0<part.stat().st_size<=15_000_000;parts.append(part)
 assert hashlib.sha256(b''.join(x.read_bytes() for x in parts)).hexdigest()==record['sha256']
 dest=folder/record['kind']
 run=subprocess.run([str(seven),'x',str(parts[0]),'-o'+str(dest),'-y'],capture_output=True,text=True,encoding='utf-8',errors='replace')
 (root/'Evidence'/('split-v06-'+record['kind']+'-7zip.txt')).write_text(run.stdout+run.stderr,encoding='utf-8')
 assert run.returncode==0,run.stdout+run.stderr
 verified=0
 with zipfile.ZipFile(archive) as z:
  assert z.testzip() is None
  for info in z.infolist():
   assert not info.is_dir()
   extracted=dest/info.filename;assert extracted.is_file()
   expected=hashlib.sha256(z.read(info)).hexdigest();assert sha(extracted)==expected,info.filename
   rel=Path(info.filename).relative_to(Path(info.filename).parts[0])
   if record['kind']=='windows':assert sha(root/'Builds/Windows-v0.6-final'/rel)==expected,rel
   elif rel.as_posix()!='SOURCE_VERSION.json':assert sha(root/rel)==expected,rel
   verified+=1
 base=dest/('zero-to-millionaire-'+('source' if record['kind']=='source' else 'windows')+'-v0.6')
 if record['kind']=='source':
  assert len(list((base/'references').glob('*.png')))==6
  metadata=json.loads((base/'SOURCE_VERSION.json').read_text())
  assert metadata['game_build_source']==record['game_build_source']
  assert metadata['source_docs_tests_head']==record['source_docs_tests_head']
 else:assert (base/'ZeroToMillionaire.exe').is_file() and (base/'RUN_WINDOWS_V06.md').is_file()
 assert sha(archive)==record['sha256']
 results.append({'kind':record['kind'],'archive':record['file'],'original_zip_sha256':record['sha256'],'original_unchanged':True,'parts':[{'file':p.name,'bytes':p.stat().st_size,'sha256':sha(p)} for p in parts],'installed_7zip_exit_code':0,'extracted_all_files_hash_verified':verified})
report={'format':'standard7-Zip split ZIP byte stream .zip.001/.002/...','max_part_bytes':15000000,'archives':results,'instructions':'Download every matching part into one folder; open .zip.001 in 7-Zip and Extract; do not extract .002 or later separately. Launch extracted ZeroToMillionaire.exe with its companion files.'}
(delivery/'final-v0.6-split-verification.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report))