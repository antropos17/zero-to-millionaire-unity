from pathlib import Path
import hashlib,json,subprocess
root=Path(__file__).resolve().parents[1]
source=subprocess.check_output(["git","-C",str(root),"rev-parse","HEAD"],text=True).strip()
for kind,folder in (("candidate","Windows-rc-v0.5"),("audit","Windows-release-audit-v0.5")):
    base=root/"Builds"/folder
    files=[]
    for p in sorted(base.rglob("*")):
        if p.is_file():files.append(dict(path=p.relative_to(base).as_posix(),bytes=p.stat().st_size,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
    canonical=json.dumps(files,sort_keys=True,separators=(",",":"))
    data=dict(kind=kind,source_commit=source,folder=folder,build_options="None",audit_define=kind=="audit",files=files,total_bytes=sum(f["bytes"] for f in files),manifest_sha256=hashlib.sha256(canonical.encode()).hexdigest())
    (root/"Evidence"/("release-v0.5-"+kind+"-hashes.json")).write_text(json.dumps(data,indent=2),encoding="utf-8")
    print(json.dumps({k:v for k,v in data.items() if k!="files"}))