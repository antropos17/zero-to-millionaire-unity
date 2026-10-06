from pathlib import Path
import hashlib,json,zipfile

root=Path(__file__).resolve().parents[1]
output=root/'Deliverables';output.mkdir(exist_ok=True)
zip_path=output/'zero-to-millionaire-first-playable-source-v0.2.zip'
included=['Assets','Packages','ProjectSettings','Checks','Tools','Evidence','references']
excluded_parts={'bin','obj','_Recovery','checks-tmp','qa-save','qa-save-v0.2','walk-demo-frames','apk-split-extraction-v0.1','apk-split-extraction-v0.2','.library-helper','.library-upload-helper'}
with zipfile.ZipFile(zip_path,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as archive:
    for name in ['AGENTS.md','PROJECT_BRIEF.md','ASSET_MANIFEST.md','README.md','VERIFICATION.md','.gitignore']:
        p=root/name
        if p.is_file():archive.write(p,'zero-to-millionaire-first-playable/'+name)
    for folder in included:
        for p in (root/folder).rglob('*'):
            if not p.is_file() or any(part in excluded_parts for part in p.relative_to(root).parts):continue
            if p.suffix in {'.log','.bak','.tmp'}:continue
            archive.write(p,'zero-to-millionaire-first-playable/'+str(p.relative_to(root)).replace('\\','/'))
    for folder in ['asset-source','asset-source-v0.2','asset-source-v0.2-walk','asset-source-v0.2-idle']:
        for p in (root/folder).glob('*.json'):
            archive.write(p,'zero-to-millionaire-first-playable/'+str(p.relative_to(root)).replace(chr(92),'/'))
record={'file':zip_path.name,'bytes':zip_path.stat().st_size,'sha256':hashlib.sha256(zip_path.read_bytes()).hexdigest()}
(output/'source-v0.2-sha256.json').write_text(json.dumps(record,indent=2))
print(json.dumps(record))
