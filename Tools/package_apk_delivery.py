"""Delivery packaging only: keep the existing signed APK unchanged."""
from pathlib import Path
import hashlib,json,zipfile

root=Path(__file__).resolve().parents[1]
apk=root/'Builds'/'Android'/'zero-to-millionaire-logic-v0.1.apk'
expected='9163171c0c43e0e164497814d7843296f7612b63cf5af0e89a9349f51724f887'
original_hash=hashlib.sha256(apk.read_bytes()).hexdigest()
assert original_hash==expected, 'Original APK differs from verified build'
output=root/'Deliverables'/'zero-to-millionaire-android-v0.1.zip'
with zipfile.ZipFile(output,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as archive:
    archive.write(apk,apk.name)
with zipfile.ZipFile(output,'r') as archive:
    assert archive.testzip() is None, 'ZIP CRC failed'
    extracted_hash=hashlib.sha256(archive.read(apk.name)).hexdigest()
assert extracted_hash==expected, 'Extracted APK differs from verified build'
assert hashlib.sha256(apk.read_bytes()).hexdigest()==expected, 'Original APK changed'
receipt={'file':output.name,'zip_bytes':output.stat().st_size,'apk_bytes':apk.stat().st_size,
         'extracted_apk_sha256':extracted_hash,'zip_sha256':hashlib.sha256(output.read_bytes()).hexdigest(),
         'under_20mb_decimal':output.stat().st_size<20000000,'original_unchanged':True}
(root/'Deliverables'/'android-zip-verification.json').write_text(json.dumps(receipt,indent=2))
print(json.dumps(receipt))
