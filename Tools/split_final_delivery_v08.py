from pathlib import Path
import argparse,json,hashlib,subprocess,zipfile
r=Path(__file__).resolve().parents[1];d=r/'Deliverables';a=argparse.ArgumentParser();a.add_argument('--kind',choices=['source','windows'],required=True);kind=a.parse_args().kind
record=json.loads((d/('final-v0.8-'+kind+'-delivery.json')).read_text());archive=d/record['file'];seven=Path(r'C:\Program Files\7-Zip\7z.exe');assert seven.is_file()
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
assert sha(archive)==record['sha256'];data=archive.read_bytes();parts=[];dest=r/'.tooling/final-v0.8-split-extraction'/kind;assert not dest.exists() and dest.resolve().is_relative_to((r/'.tooling').resolve())
for i,offset in enumerate(range(0,len(data),15_000_000),1):
 p=d/(archive.name+f'.{i:03d}');assert not p.exists();p.write_bytes(data[offset:offset+15_000_000]);parts.append(p)
assert all(0<p.stat().st_size<=15_000_000 for p in parts);assert hashlib.sha256(b''.join(p.read_bytes() for p in parts)).hexdigest()==record['sha256']
run=subprocess.run([str(seven),'x',str(parts[0]),'-o'+str(dest),'-y'],capture_output=True,text=True,encoding='utf-8',errors='replace');(r/'Evidence'/('split-v08-'+kind+'-7zip.txt')).write_text(run.stdout+run.stderr,encoding='utf-8');assert run.returncode==0,run.stdout+run.stderr
with zipfile.ZipFile(archive) as z:
 assert z.testzip() is None
 for entry in z.infolist():
  p=dest/entry.filename;assert p.is_file() and sha(p)==hashlib.sha256(z.read(entry)).hexdigest(),entry.filename
  rel=Path(entry.filename).relative_to(Path(entry.filename).parts[0])
  if kind=='windows':assert sha(r/'Builds/Windows-v0.8-candidate-1'/rel)==sha(p)
  elif rel.as_posix()!='SOURCE_VERSION.json':assert sha(r/rel)==sha(p),rel
 count=len(z.infolist())
result={**record,'format':'7-Zip split ZIP byte stream .zip.001/.002/...','max_part_bytes':15000000,'parts':[{'file':p.name,'bytes':p.stat().st_size,'sha256':sha(p)} for p in parts],'7zip_exit_code':0,'extracted_all_files_hash_verified':count}
(d/('final-v0.8-'+kind+'-split-verification.json')).write_text(json.dumps(result,indent=2));print(json.dumps(result))
