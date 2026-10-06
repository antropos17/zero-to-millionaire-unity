from pathlib import Path
import hashlib,json,zipfile

root=Path(__file__).resolve().parents[1];output=root/'Deliverables';output.mkdir(exist_ok=True)
zip_path=output/'zero-to-millionaire-first-playable-source-v0.3.zip'
excluded_parts={'bin','obj','_Recovery','checks-tmp','qa-save','qa-save-v0.2','qa-save-v0.3','walk-demo-frames','walk-demo-frames-v0.3','.library-helper','.library-upload-helper'}
def include(path):
    rel=path.relative_to(root)
    if not path.is_file() or any(x in excluded_parts for x in rel.parts):return False
    if path.suffix in {'.log','.bak','.tmp'}:return False
    if path.name=='update_ui_v03.py':return False # disposable one-shot migration, not a rebuild tool
    if rel.parts[0]=='Evidence':
        if path.suffix in {'.png','.gif'}:
            return (('v0.3' in path.name and 'first-' not in path.name and 'diagnostic' not in path.name)
                    or path.name in {'v0.2-start-360x640.png','item-symbols-v0.2.png'})
        return ('v0.3' in path.name or path.name.startswith('unity-mcp-') or path.name=='unity-model-checks.txt')
    return True
with zipfile.ZipFile(zip_path,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
    for name in ['AGENTS.md','PROJECT_BRIEF.md','ASSET_MANIFEST.md','README.md','VERIFICATION.md','.gitignore']:
        p=root/name
        if p.exists():z.write(p,'zero-to-millionaire-first-playable/'+name)
    for folder in ['Assets','Packages','ProjectSettings','Checks','Tools','Evidence','references']:
        for p in (root/folder).rglob('*'):
            if include(p):z.write(p,'zero-to-millionaire-first-playable/'+p.relative_to(root).as_posix())
    for folder in ['asset-source','asset-source-v0.2','asset-source-v0.2-walk','asset-source-v0.2-idle']:
        for p in (root/folder).glob('*.json'):z.write(p,'zero-to-millionaire-first-playable/'+p.relative_to(root).as_posix())
with zipfile.ZipFile(zip_path) as z:
    assert z.testzip() is None
    names=z.namelist();pre='zero-to-millionaire-first-playable/'
    for n in ['Assets/Scenes/Level1.unity','Assets/Scripts/Runtime/GameView.cs','Assets/Scripts/Runtime/PixelUi.cs','Assets/Resources/Fonts/OFL.txt','Evidence/runtime-v0.3-qa.txt','Evidence/windows-v0.3-startup.json']:
        assert pre+n in names,n
    assert len([n for n in names if n.startswith(pre+'references/') and n.endswith('.png')])==6
    assert all('\\' not in n for n in names)
assert zip_path.stat().st_size<20000000,'Source attachment exceeds20MB; review evidence selection'
record={'file':zip_path.name,'bytes':zip_path.stat().st_size,'sha256':hashlib.sha256(zip_path.read_bytes()).hexdigest(),'zip_crc_verified':True,'under20MB':True}
(output/'source-v0.3-sha256.json').write_text(json.dumps(record,indent=2));print(json.dumps(record))
