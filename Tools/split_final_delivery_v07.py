from pathlib import Path
import argparse,json,hashlib,subprocess,zipfile
root=Path(__file__).resolve().parents[1];delivery=root/'Deliverables'
a=argparse.ArgumentParser();a.add_argument('--kind',choices=['windows','source'],required=True);kind=a.parse_args().kind
record=json.loads((delivery/('final-v0.7-'+kind+'-delivery.json')).read_text(encoding='utf-8'))
seven=Path(r'C:\Program Files\7-Zip\7z.exe');assert seven.is_file()
folder=root/'.tooling/final-v0.7-split-extraction';dest=folder/kind
assert not dest.exists() and dest.resolve().is_relative_to((root/'.tooling').resolve())
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
archive=delivery/record['file'];data=archive.read_bytes();assert hashlib.sha256(data).hexdigest()==record['sha256']
parts=[]
for index,offset in enumerate(range(0,len(data),15_000_000),1):
 part=delivery/(archive.name+f'.{index:03d}');assert not part.exists()
 part.write_bytes(data[offset:offset+15_000_000]);assert 0<part.stat().st_size<=15_000_000;parts.append(part)
assert hashlib.sha256(b''.join(x.read_bytes() for x in parts)).hexdigest()==record['sha256']
run=subprocess.run([str(seven),'x',str(parts[0]),'-o'+str(dest),'-y'],capture_output=True,text=True,encoding='utf-8',errors='replace')
(root/'Evidence'/('split-v07-'+kind+'-7zip.txt')).write_text(run.stdout+run.stderr,encoding='utf-8')
assert run.returncode==0,run.stdout+run.stderr
verified=0
with zipfile.ZipFile(archive) as z:
 assert z.testzip() is None
 for info in z.infolist():
  assert not info.is_dir()
  extracted=dest/info.filename;assert extracted.is_file()
  expected=hashlib.sha256(z.read(info)).hexdigest();assert sha(extracted)==expected,info.filename
  rel=Path(info.filename).relative_to(Path(info.filename).parts[0])
  if kind=='windows':assert sha(root/'Builds/Windows-v0.7-candidate-1'/rel)==expected,rel
  elif rel.as_posix()!='SOURCE_VERSION.json':assert sha(root/rel)==expected,rel
  verified+=1
base=dest/('zero-to-millionaire-'+('source' if kind=='source' else 'windows')+'-v0.7')
if kind=='source':
 assert len(list((base/'references').glob('*.png')))==6
 metadata=json.loads((base/'SOURCE_VERSION.json').read_text())
 assert metadata['game_build_source']==record['game_build_source'] and metadata['source_docs_tests_head']==record['source_docs_tests_head']
else:assert (base/'ZeroToMillionaire.exe').is_file() and (base/'RUN_WINDOWS_V07.md').is_file()
assert sha(archive)==record['sha256']
report={'format':'standard 7-Zip split ZIP byte stream .zip.001/.002/...','max_part_bytes':15000000,'kind':kind,'archive':record['file'],'archive_sha256':record['sha256'],'source_docs_tests_head':record['source_docs_tests_head'],'game_build_source':record['game_build_source'],'parts':[{'file':p.name,'bytes':p.stat().st_size,'sha256':sha(p)} for p in parts],'7zip_exit_code':0,'extracted_all_files_hash_verified':verified}
(delivery/('final-v0.7-'+kind+'-split-verification.json')).write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report))
