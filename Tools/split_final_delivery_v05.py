from pathlib import Path
import json,hashlib,subprocess
root=Path(__file__).resolve().parents[1];delivery=root/'Deliverables';records=json.loads((delivery/'final-v0.5-delivery.json').read_text(encoding='utf-8'));results=[]
seven=Path(r'C:\Program Files\7-Zip\7z.exe');assert seven.is_file()
folder=root/'.tooling/final-v0.5-split-7zip-extraction';assert not folder.exists();assert folder.resolve().is_relative_to((root/'.tooling').resolve());folder.mkdir()
for record in records:
 archive=delivery/record['file'];data=archive.read_bytes();assert hashlib.sha256(data).hexdigest()==record['sha256']
 parts=[]
 for index,offset in enumerate(range(0,len(data),15_000_000),1):
  part=delivery/(archive.name+f'.{index:03d}');assert not part.exists();part.write_bytes(data[offset:offset+15_000_000]);assert part.stat().st_size<=15_000_000;parts.append(part)
 assert len(parts)==3;assert hashlib.sha256(b''.join(x.read_bytes() for x in parts)).hexdigest()==record['sha256']
 dest=folder/('source' if 'source' in archive.name else 'windows')
 run=subprocess.run([str(seven),'x',str(parts[0]),'-o'+str(dest),'-y'],capture_output=True,text=True,encoding='utf-8',errors='replace')
 (root/'Evidence'/('split-v0.5-'+('source' if 'source' in archive.name else 'windows')+'-7zip.txt')).write_text(run.stdout+run.stderr,encoding='utf-8');assert run.returncode==0,run.stdout+run.stderr
 if 'source' in archive.name:
  base=dest/'zero-to-millionaire-first-playable';version=json.loads((base/'SOURCE_VERSION.json').read_text());assert version['source_docs_testing_head']=='48daf9f9e2c62f49e42b97582c88fcb544fedeef';assert len(list((base/'references').glob('*.png')))==6
  for branch in ['Core','Runtime']:
   for f in (root/'Assets/Scripts'/branch).glob('*.cs'):assert (base/'Assets/Scripts'/branch/f.name).read_bytes()==f.read_bytes()
  verified='source scripts plus all6references match'
 else:
  base=dest/'zero-to-millionaire-windows-v0.5';manifest=json.loads((root/'Evidence/release-v0.5-final-folder-hashes.json').read_text(encoding='utf-8'))
  for row in manifest['files']:
   f=base/row['path'];assert f.is_file() and hashlib.sha256(f.read_bytes()).hexdigest()==row['sha256'],row['path']
  verified='all158Windows files hash-match final manifest'
 assert hashlib.sha256(archive.read_bytes()).hexdigest()==record['sha256']
 results.append({'archive':record['file'],'original_zip_sha256':record['sha256'],'original_unchanged':True,'parts':[{'file':f.name,'bytes':f.stat().st_size,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()} for f in parts],'installed_7zip_exit_code':run.returncode,'extracted_verification':verified})
report={'format':'standard7-Zip split ZIP byte stream .zip.001/.002/.003','max_part_bytes':15_000_000,'reason':'Library retained full ZIPs, but actual main-chat attachment delivery rejected them as over its upload limit','main_chat_complete_archive_delivery':'failed; corrected multipart delivery required','archives':results,'instructions':'Download all three matching parts into one folder. Open .zip.001 in 7-Zip and Extract, or run 7z x archive.zip.001. Do not extract .002/.003 separately. Windows: open extracted ZeroToMillionaire.exe with companion folders.'}
(delivery/'final-v0.5-split-verification.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report))
