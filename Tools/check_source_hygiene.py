"""Check only intended Git source paths; never print a matched secret."""
from pathlib import Path
import json,re,subprocess,sys
root=Path(__file__).resolve().parents[1]
mode=['--cached'] if '--staged' in sys.argv else ['--others','--exclude-standard']
reply=subprocess.run(['git','-C',str(root),'ls-files','-z',*mode],capture_output=True,check=True)
names=sorted({n.decode('utf-8') for n in reply.stdout.split(b'\0') if n})
patterns={
    'private_key':re.compile(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----'),
    'signed_url':re.compile(rb'(?i)(?:X-Amz-Signature=[0-9a-f]{32,}|[?&](?:sig|access_token|auth_token|token)=[A-Za-z0-9%_.+-]{16,})'),
    'api_key':re.compile(rb'sk-(?:proj-|svcacct-)?[A-Za-z0-9_-]{32,}'),
    'bearer_value':re.compile(rb'(?i)\bBearer\s+[A-Za-z0-9._-]{20,}'),
    'secret_assignment':re.compile(rb"""(?i)(?:api[_-]?key|access[_-]?token|client[_-]?secret)\s*[=:]\s*["']?[A-Za-z0-9._/-]{24,}"""),
}
text_ext={'.cs','.md','.py','.ps1','.json','.toml','.yaml','.yml','.asset','.meta','.unity','.txt','.xml','.config','.csproj','.gitignore','.gitattributes'}
findings=[];checked=0;large=[]
for name in names:
    path=root/name
    if not path.is_file():continue
    if name.split('/')[0] in {'.tooling','.library-helper','.library-upload-helper','Library','Temp','Builds','Deliverables','UserSettings'}:
        findings.append({'file':name,'kind':'generated_or_private_path_in_source'});continue
    if path.suffix.lower() not in text_ext and path.name not in {'.gitignore','.gitattributes'}:continue
    if path.stat().st_size>8*1024*1024:
        large.append(name);continue
    data=path.read_bytes();checked+=1
    for kind,pattern in patterns.items():
        if pattern.search(data):findings.append({'file':name,'kind':kind})
receipt={'intended_files':len(names),'text_files_checked':checked,'findings':findings,'large_text_skipped':large,'passed':not findings and not large,'mode':'staged' if '--staged' in sys.argv else 'untracked_intended'}
out=root/'Evidence/source-hygiene-x.json'
out.write_text(json.dumps(receipt,indent=2))
print(json.dumps(receipt))
sys.exit(0 if receipt['passed'] else 1)
