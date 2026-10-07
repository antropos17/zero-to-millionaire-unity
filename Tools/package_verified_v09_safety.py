from pathlib import Path
import hashlib,json,subprocess,shutil,zipfile,xml.etree.ElementTree as ET

ROOT=Path(r'X:\Projects\ZERO-TO-MILLIONAIRE')
BUILD_SOURCE='457a13f5ecab649aafb371873b1ba9bb301536fe'
BASE=ROOT/'Builds/Windows-v0.9-candidate-3'
OUT=ROOT/'Deliverables/Final-v09-candidate-3'
OLD=ROOT/'Deliverables/Archive-v0.9-candidate-1'
STAGE=Path(r'C:\Users\murtu\Documents\Codex\2026-10-06\task\.tooling\v09-safety-fix-1')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git','-c','safe.directory=X:/Projects/ZERO-TO-MILLIONAIRE','-C',str(ROOT),*args],stderr=subprocess.DEVNULL)
def write(p,data):p.write_text(json.dumps(data,indent=2),encoding='utf8')

assert not OUT.exists(), 'Immutable final delivery root required'
assert 'source='+BUILD_SOURCE in (ROOT/'Evidence/Windows-v0.9-candidate-3-build.txt').read_text(encoding='utf8')
assert ET.parse(ROOT/'Evidence/v09-final-safety-editmode-1.xml').getroot().attrib['result']=='Passed'
assert json.loads((ROOT/'Evidence/native-v09-safe-final-1-summary.json').read_text(encoding='utf-8-sig'))['status']=='PASS'
assert json.loads((ROOT/'Evidence/v09-candidate3-compiled-safety-checks.json').read_text(encoding='utf8'))['status']=='PASS'
assert 'RESULT PASS' in (ROOT/'Evidence/v09-ui-checks-5.txt').read_text(encoding='utf8')

# Restore only importer whitespace after exact semantic comparison; retain all
# unrelated untracked evidence and all old immutable builds.
for rel in git('diff','--name-only','--','Assets/Resources/Art').decode().splitlines():
 p=ROOT/rel;before=git('show',BUILD_SOURCE+':'+rel);current=p.read_bytes()
 normalized=lambda b:'\n'.join(x.rstrip()for x in b.decode('utf8').splitlines())
 assert rel.endswith('.meta')and normalized(current)==normalized(before),'Semantic art edit'
 assert p.read_bytes()==current;p.write_bytes(before)
subprocess.run(['git','-c','safe.directory=X:/Projects/ZERO-TO-MILLIONAIRE','-C',str(ROOT),'diff',BUILD_SOURCE,'--exit-code','--','Assets','ProjectSettings','Packages'],check=True,capture_output=True)

review={'status':'PASS','productionSource':BUILD_SOURCE,'reviewedLibraryId':'libfile_6b8a9986d3d48191a2cdd90cda4ee05e','reviewedVersion':1,'scope':'Independent targeted review of fail-closed migration, exact recovery exception, no-op save-error retention and shop prerequisite text; confirmed by parent coordinator'}
write(ROOT/'Evidence/v09-final-independent-review.json',review)
opening={'scope':'User-requested normal visible launch AFTER private automated QA; no input dispatched by executor','pid':51356,'openedUtc':'2026-10-07T06:38:19Z','profile':'ordinary user profile; no --ztm-private-profile option','diskMigrationObserved':False,'readOnlyCheck':'2026-10-07 after opening, before final packaging','primarySchema':5,'backupSchema':5,'primaryModifiedUtc':'2026-10-07T04:32:06.5827786Z','backupModifiedUtc':'2026-10-07T04:32:05.0049446Z','preSchema6SnapshotDirectoryPresent':False,'resourceValuesReadOrExported':False,'userSaveWrittenByExecutor':False,'caveat':'No disk migration at this observation. A later normal user action, pause or quit can commit the normal supported migration. Existing files were not changed by the read-only check.'}
write(ROOT/'Evidence/v09-user-requested-opening-context.json',opening)

for name in ('FINAL_VALIDATION_V09.md','V09_MIGRATION_ROLLBACK.md'):
 p=ROOT/name;s=p.read_text(encoding='utf8')
 if name=='FINAL_VALIDATION_V09.md':
  s=s.replace("Final Library archive replacements are held for the parent's independent source\nreview. Previously delivered candidate-1 archive versions must not be described\nas the updated safety build. RUN_WINDOWS_V09.md provides run/extraction steps.","Independent targeted review PASSED the exact production source above, including\nthe recovery exception and both UI fixes. Delivered Windows/source split archives\ncontain candidate-3 and are verified by CRC, rejoined SHA256 and actual7-Zip\nextraction with every file hash checked. RUN_WINDOWS_V09.md gives run steps.")
  s+='\nNormal launch used the user profile at the user\'s explicit request AFTER QA.\nA subsequent READ-ONLY check found both live files still schema5, unchanged\npre-launch timestamps and no pre-schema6 snapshot folder; no disk migration\nwas observed at that time. No input was sent to the opened window. Automated\nQA used only private profiles. Later normal actions/pause/quit can migrate.\n'
 else:
  s=s.replace("Final existing Library archive identities remain unchanged pending the parent's\nindependent review. The safety-review file is updated in place for that review.","Independent targeted review PASSED this exact source, including the narrowly\nscoped already-protected recovery-backup exception. Updated Windows/source\narchives contain candidate-3; split-file proofs record exact source and hashes.")
 p.write_text(s,encoding='utf8')

evidence_names={
 'v09-final-independent-review.json','v09-user-requested-opening-context.json',
 'v09-safety-dotnet-core-3.txt','v09-unity-core-checks-4.txt','v09-final-safety-editmode-1.xml',
 'v09-ui-checks-5.txt','v09-final-safety-rollback-checks.json','v09-candidate3-compiled-safety-checks.json',
 'v09-unity-stakes-pacing-4.csv','Windows-v0.9-candidate-3-build.txt',
 'final-v09-level-recheck-2-checks.txt','final-v09-level-recheck-2-completed-state.json',
 'final-v09-level-recheck-2-editor-timing.json','v09-final-safety-review-library.json'}
evidence_names.update(p.name for p in (ROOT/'Evidence').glob('native-v09-safe-final-1-*.json'))
evidence_names.update(p.name for p in (ROOT/'Evidence').glob('final-v09-level-recheck-2-*.png'))
for name in (
 'v09-qa5-main-low-coins-full-320x640.png','v09-qa5-main-low-coins-full-360x640.png',
 'v09-qa5-main-low-coins-full-360x800.png','v09-qa5-main-low-coins-full-480x960.png',
 'v09-qa5-main-low-coins-calm-320x640.png','v09-qa5-stake-picker-full-360x640.png',
 'v09-qa5-shop-ready-full-360x640.png','v09-qa5-shop-complete-bottom-full-360x640.png',
 'v09-qa5-stake-noop-save-failure-full-320x640.png','v09-qa5-migration-backup-failure-full-320x640.png',
 'v09-qa5-save-external-change-full-320x640.png'):
 evidence_names.add(name)
assert all((ROOT/'Evidence'/name).is_file()for name in evidence_names)
paths=['RUN_WINDOWS_V09.md','FINAL_VALIDATION_V09.md','V09_MIGRATION_ROLLBACK.md','Tools/CompiledReleaseAuditV09/Program.cs']+['Evidence/'+name for name in sorted(evidence_names)]
assert not git('diff','--cached','--name-only').strip(),'Pre-existing staged work preserved'
git('add','--',*paths,'Tools/package_verified_v09_safety.py')
git('commit','-m','Record reviewed v0.9 candidate3 verification and final run instructions')
HEAD=git('rev-parse','HEAD').decode().strip()
assert not git('diff','--name-only','--','Assets','ProjectSettings','Packages').strip()

OUT.mkdir();OLD.mkdir(exist_ok=True)
prior=json.loads((ROOT/'Deliverables/final-v09-library-saved.json').read_text(encoding='utf8'))
for row in prior['saved_files']:
 p=Path(row['canonical_windows_path'])
 if p.exists():
  saved=OLD/p.name
  if saved.exists():assert sha(saved)==sha(p)
  else:shutil.copy2(p,saved)
for p in (ROOT/'Deliverables').glob('zero-to-millionaire-*-v0.9.zip'):
 saved=OLD/p.name
 if saved.exists():assert sha(saved)==sha(p)
 else:shutil.copy2(p,saved)

for name in ('INTER-OFL.txt','READABLE-MODIFICATION.txt','OFL.txt','MODIFICATION.txt'):
 (BASE/'LICENSES').mkdir(exist_ok=True);shutil.copy2(ROOT/'Assets/Resources/Fonts'/name,BASE/'LICENSES'/name.upper())
for name in ('RUN_WINDOWS_V09.md','FINAL_VALIDATION_V09.md','V09_MIGRATION_ROLLBACK.md'):shutil.copy2(ROOT/name,BASE/name);shutil.copy2(ROOT/name,OUT/name)
def shippable(p):return not any(x in str(p).lower()for x in ('donotship','dontship','burstdebuginformation'))
runtime=[p for p in sorted(BASE.rglob('*'))if p.is_file()and shippable(p)]
assert not (BASE/'BUILD_IDENTITY.json').exists()
identity={'source_commit':BUILD_SOURCE,'source_docs_tests_head':HEAD,'engine':'Unity6000.3.21f1','bundle_version':'0.9.0','options':'None','audit_define':False,'build_guid':'d7ac4afa7c304fd89be0a0ae8dec2849','executable':'ZeroToMillionaire.exe','file_count':len(runtime),'total_bytes':sum(p.stat().st_size for p in runtime),'files':[{'path':p.relative_to(BASE).as_posix(),'bytes':p.stat().st_size,'sha256':sha(p)}for p in runtime]}
write(BASE/'BUILD_IDENTITY.json',identity);write(ROOT/'Evidence/windows-v09-candidate-3-hashes.json',identity)
source_version={'game_build_source':BUILD_SOURCE,'source_docs_tests_head':HEAD,'production_diff':'empty versus compiled candidate-3 source','engine':'Unity6000.3.21f1','independent_source_review':'PASS version1 safety ZIP','private_saves_caches_tooling_excluded':True,'native_input':'NOT_RUN','android':'deferred','approved_reference_count':6}
results=[]
for kind in ('windows','source'):
 name='zero-to-millionaire-'+('windows'if kind=='windows'else'complete-source')+'-v0.9.zip';archive=OUT/name
 prefix='zero-to-millionaire-'+('windows'if kind=='windows'else'source')+'-v0.9/'
 if kind=='windows':files=[(p,p.relative_to(BASE).as_posix())for p in BASE.rglob('*')if p.is_file()and shippable(p)]
 else:
  files=[]
  for rel in git('ls-files','-z').decode().split('\0'):
   if not rel:continue
   p=ROOT/rel
   if not p.is_file()or any(x in rel.split('/')for x in ('bin','obj','_Recovery')):continue
   roots=rel.startswith(('Assets/','Packages/','ProjectSettings/','references/','Tools/','Checks/'))
   docs=('/'not in rel and p.suffix=='.md')or rel in ('.gitignore','.gitattributes')
   ev=rel.startswith('Evidence/')and p.name in evidence_names
   if roots or docs or ev:files.append((p,rel))
 with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=9)as z:
  for p,rel in sorted(files,key=lambda x:x[1]):z.write(p,prefix+rel)
  if kind=='source':z.writestr(prefix+'SOURCE_VERSION.json',json.dumps(source_version,indent=2));z.writestr(prefix+'README_DELIVERY.md','Read RUN_WINDOWS_V09.md and FINAL_VALIDATION_V09.md for this reviewed candidate-3. Close every old game/Editor using the user profile before the first updated launch. Open Assets/Scenes/Level1.unity in Unity6000.3.21f1 for source review. All six approved references and separately reusable assets are included; private saves, engine caches, credentials and logs are excluded. Android packaging is deferred.\n')
 with zipfile.ZipFile(archive)as z:
  assert z.testzip()is None;names=z.namelist()
  assert all('/.tooling/'not in n and'/Library/'not in n and'../'not in n and'\\'not in n and not n.endswith('.log')for n in names)
  if kind=='source':
   assert len([n for n in names if n.startswith(prefix+'references/')and n.endswith('.png')])==6
   for rel in git('ls-tree','-r','--name-only',BUILD_SOURCE,'--','Assets/Scripts','Assets/Resources','Assets/Scenes','ProjectSettings','Packages').decode().splitlines():
    assert z.read(prefix+rel)==git('show',BUILD_SOURCE+':'+rel),'Source archive drift: '+rel
 record={'kind':kind,'file':archive.name,'bytes':archive.stat().st_size,'sha256':sha(archive),'files':len(names),'buildSource':BUILD_SOURCE,'sourceHead':HEAD,'independentReview':'PASS','candidate':'Windows-v0.9-candidate-3'}
 data=archive.read_bytes();parts=[]
 for i,offset in enumerate(range(0,len(data),15_000_000),1):
  part=OUT/(archive.name+f'.{i:03d}');part.write_bytes(data[offset:offset+15_000_000]);parts.append(part)
 assert len(parts)==(3 if kind=='windows'else 2),'Unexpected part count; retain existing Library identities'
 assert all(0<p.stat().st_size<=15_000_000 for p in parts)
 assert hashlib.sha256(b''.join(p.read_bytes()for p in parts)).hexdigest()==record['sha256']
 extracted=ROOT/'.tooling/final-v09-candidate-3-extraction'/kind
 assert not extracted.exists()and extracted.resolve().is_relative_to((ROOT/'.tooling').resolve())
 extract=subprocess.run([r'C:\Program Files\7-Zip\7z.exe','x',str(parts[0]),'-o'+str(extracted),'-y'],capture_output=True,text=True,encoding='utf8',errors='replace')
 (ROOT/'Evidence'/('split-v09-candidate3-'+kind+'-7zip.txt')).write_text(extract.stdout+extract.stderr,encoding='utf8');assert extract.returncode==0
 with zipfile.ZipFile(archive)as z:
  for member in z.infolist():
   target=extracted/member.filename;assert target.is_file()and sha(target)==hashlib.sha256(z.read(member)).hexdigest(),member.filename
 record.update(parts=[{'file':p.name,'bytes':p.stat().st_size,'sha256':sha(p)}for p in parts],zipCrc=True,sevenZipExtractedFilesVerified=len(names),exactCompiledCandidateAndSourceVerified=True)
 write(OUT/('final-v0.9-'+kind+'-split-verification.json'),record);results.append(record);print(json.dumps(record),flush=True)

# Update the existing general code-review deliverable to the current source and
# reviewed final instructions. The separately versioned safety-review ZIP stays v1.
review_zip=ROOT/'Deliverables/Review-v09-457a13f5/zero-to-millionaire-v0.9-save-safety-review.zip'
with zipfile.ZipFile(review_zip)as z:entries={n:z.read(n)for n in z.namelist()if n not in ('READ_REVIEW.md','REVIEW_FILES.json')}
for name in ('RUN_WINDOWS_V09.md','FINAL_VALIDATION_V09.md','V09_MIGRATION_ROLLBACK.md'):entries[name]=(ROOT/name).read_bytes()
entries['SOURCE_VERSION.json']=json.dumps(source_version,indent=2).encode()
entries['READ_REVIEW.md']=('Reviewed v0.9 candidate-3 code subset. Production source '+BUILD_SOURCE+'. No art or executable is contained here; extract both complete-source split parts for the Unity project. Read RUN_WINDOWS_V09.md and close all old versions before first normal launch. Independent narrow review PASSED the version1 safety ZIP.\n').encode()
entries['REVIEW_FILES.json']=json.dumps([{'path':n,'bytes':len(b),'sha256':hashlib.sha256(b).hexdigest()}for n,b in sorted(entries.items())],indent=2).encode()
with zipfile.ZipFile(OUT/'zero-to-millionaire-v0.9-code-review.zip','w',zipfile.ZIP_DEFLATED,compresslevel=9)as z:
 for n,b in sorted(entries.items()):z.writestr(n,b)
with zipfile.ZipFile(OUT/'zero-to-millionaire-v0.9-code-review.zip')as z:assert z.testzip()is None

image_sources={
 'v09-qa2-main-low-coins-full-320x640.png':'v09-qa5-main-low-coins-full-320x640.png',
 'v09-qa2-stake-picker-full-360x640.png':'v09-qa5-stake-picker-full-360x640.png',
 'v09-qa2-shop-ready-full-360x640.png':'v09-qa5-shop-ready-full-360x640.png',
 'final-v09-level-complete-360x640.png':'final-v09-level-recheck-2-complete-360x640.png',
 'v09-qa2-shop-complete-bottom-full-360x640.png':'v09-qa5-shop-complete-bottom-full-360x640.png'}
for name,src in image_sources.items():shutil.copy2(ROOT/'Evidence'/src,OUT/name)
replacements=[]
for row in prior['saved_files']:
 new=OUT/row['file_name']
 if new.exists():
  old=Path(row['canonical_windows_path'])
  if not old.exists()or sha(old)!=sha(new):replacements.append({'library_file_id':row['library_file_id'],'expected_current_version':row['current_version_number'],'file_name':row['file_name'],'artifact_type':row['library_artifact_type'],'local_path':str(new),'bytes':new.stat().st_size,'sha256':sha(new)})
assert len(replacements)>=10
write(OUT/'final-v09-delivery-summary.json',{'source':BUILD_SOURCE,'sourceHead':HEAD,'archives':results,'replacements':replacements,'oldCanonicalArtifactsPreservedAt':str(OLD),'normalOpening':opening,'independentReview':review})
write(STAGE/'final-packaging-result.json',{'source':BUILD_SOURCE,'sourceHead':HEAD,'output':str(OUT),'replacements':replacements,'archives':results})
print(json.dumps({'status':'VERIFIED','source':BUILD_SOURCE,'sourceHead':HEAD,'output':str(OUT),'replacementCount':len(replacements)}),flush=True)
