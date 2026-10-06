"""Split the verified ZIP for delivery and verify extraction with installed 7-Zip."""
from pathlib import Path
import hashlib
import json
import subprocess

root = Path(__file__).resolve().parents[1]
delivery = root / 'Deliverables'
archive = delivery / 'zero-to-millionaire-android-v0.1.zip'
apk = root / 'Builds' / 'Android' / 'zero-to-millionaire-logic-v0.1.apk'
expected_apk = '9163171c0c43e0e164497814d7843296f7612b63cf5af0e89a9349f51724f887'
expected_zip = '0fa2607ee723cd873753ee8d517bdae44b4f4c5288c358bd139cbe10c90455fa'
assert hashlib.sha256(apk.read_bytes()).hexdigest() == expected_apk
data = archive.read_bytes()
assert hashlib.sha256(data).hexdigest() == expected_zip
part_size = 15_000_000
parts = []
for index, offset in enumerate(range(0, len(data), part_size), 1):
    part = delivery / (archive.name + f'.{index:03d}')
    part.write_bytes(data[offset:offset + part_size])
    assert part.stat().st_size < 20_000_000
    parts.append(part)
assert len(parts) == 2
reconstructed = b''.join(part.read_bytes() for part in parts)
assert hashlib.sha256(reconstructed).hexdigest() == expected_zip
output = root / 'Evidence' / 'apk-split-extraction-v0.1'
output.mkdir(exist_ok=True)
result = subprocess.run([
    r'C:\Program Files\7-Zip\7z.exe', 'x', str(parts[0]), '-o' + str(output), '-y'
], capture_output=True, text=True)
(root / 'Evidence' / 'apk-split-7zip.txt').write_text(result.stdout + result.stderr)
assert result.returncode == 0, result.stdout + result.stderr
extracted = output / apk.name
assert hashlib.sha256(extracted.read_bytes()).hexdigest() == expected_apk
assert hashlib.sha256(apk.read_bytes()).hexdigest() == expected_apk
receipt = {
    'parts': [{'file': part.name, 'bytes': part.stat().st_size,
               'sha256': hashlib.sha256(part.read_bytes()).hexdigest()} for part in parts],
    'format': '7-Zip split byte stream (.zip.001 + .zip.002)',
    'reconstructed_zip_sha256': expected_zip,
    'extracted_apk_sha256': expected_apk,
    'original_apk_unchanged': True,
    'installed_7zip_extract_exit_code': result.returncode,
    'extraction': 'Place both parts together; open .zip.001 in 7-Zip and Extract, or run 7z x .zip.001.'
}
(delivery / 'android-split-verification.json').write_text(json.dumps(receipt, indent=2))
print(json.dumps(receipt))
