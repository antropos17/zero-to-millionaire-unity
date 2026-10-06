"""Package verified v0.3 APK; preserve signed bytes and existing v0.1 deliveries."""
from pathlib import Path
import hashlib,json,subprocess,zipfile

root=Path(__file__).resolve().parents[1]
apk=root/'Builds/Android/zero-to-millionaire-v0.3.apk'
verified=json.loads((root/'Evidence/android-v0.3-verification.json').read_text())
expected=verified['sha256']
assert hashlib.sha256(apk.read_bytes()).hexdigest()==expected
out=root/'Deliverables';out.mkdir(exist_ok=True)
archive_path=out/'zero-to-millionaire-android-v0.3.zip'
with zipfile.ZipFile(archive_path,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
    z.write(apk,apk.name)
with zipfile.ZipFile(archive_path) as z:
    assert z.testzip() is None
    assert hashlib.sha256(z.read(apk.name)).hexdigest()==expected
parts=[]
if archive_path.stat().st_size>=20000000:
    data=archive_path.read_bytes()
    for i,offset in enumerate(range(0,len(data),15000000),1):
        p=out/(archive_path.name+f'.{i:03d}');p.write_bytes(data[offset:offset+15000000])
        assert p.stat().st_size<20000000
        parts.append({'file':p.name,'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
    extraction=out/'apk-split-extraction-v0.3';extraction.mkdir(exist_ok=True)
    run=subprocess.run([r'C:\Program Files\7-Zip\7z.exe','x',str(out/parts[0]['file']),'-o'+str(extraction),'-y'],capture_output=True,text=True)
    assert run.returncode==0,run.stdout+run.stderr
    assert hashlib.sha256((extraction/apk.name).read_bytes()).hexdigest()==expected
assert hashlib.sha256(apk.read_bytes()).hexdigest()==expected
receipt={'apk_bytes':apk.stat().st_size,'apk_sha256':expected,'zip':archive_path.name,
         'zip_bytes':archive_path.stat().st_size,'parts':parts,'original_unchanged':True,
         'crc_and_extracted_sha256_verified':True,'actual_7zip_split_extraction_verified':bool(parts)}
(out/'android-v0.3-packaging.json').write_text(json.dumps(receipt,indent=2))
print(json.dumps(receipt))
