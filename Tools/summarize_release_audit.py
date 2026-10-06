from pathlib import Path
import csv,json,statistics,hashlib,sys,shutil
root=Path(__file__).resolve().parents[1]
label=sys.argv[1]
folder=root/'.tooling'/label
manifest=json.loads((folder/'native-manifest.json').read_text(encoding='utf-8'))
assert manifest['finished'] and manifest['privateProfileVerified'] and manifest['fullCompletion'] and manifest['reloadVerified'] and manifest['resizeFit'] and manifest['feetClear'],manifest
rows=list(csv.DictReader((folder/'native-frames.csv').open(encoding='utf-8')))
assert len(rows)==manifest['sampleCount']
if True:
 for phase in range(1,6):
  frame_rows=[r for r in rows if int(r['phase'])==phase]
  assert sum(int(r['Draw Calls Count [Count]'])>0 for r in frame_rows)>len(frame_rows)*.90, 'No actual rendered draw calls: phase'+str(phase)
 from PIL import Image
 for name in ['native-ready.png','native-complete.png']:
  image=Image.open(folder/name).convert('RGB')
  assert len(image.getcolors(1000000))>100, 'Black/non-game screenshot'
assert all(int(rows[i+1]['frame'])==int(rows[i]['frame'])+1 for i in range(len(rows)-1))
def pct(vals,p):
 vals=sorted(vals);rank=(len(vals)-1)*p
 low=int(rank);hi=min(low+1,len(vals)-1)
 return vals[low]+(vals[hi]-vals[low])*(rank-low)
def stats(vals):
 return dict(n=len(vals),mean=statistics.fmean(vals),median=statistics.median(vals),p95=pct(vals,.95),p99=pct(vals,.99),max=max(vals),min=min(vals))
phases={1:'idle_walk',2:'progression_spins_buys',3:'ready_final_task',4:'completed_walk',5:'resize_portraits'}
result={'run':label,'manifest':manifest,'phases':{},'notes':[
 'Release audit twin (BuildOptions.None plus explicit ZTM_RELEASE_AUDIT), Mono/D3D11, target60/vSync0; not the clean candidate binary.',
 'Stopwatch intervals are end-to-end frame intervals; they include scheduling, limiter waits and instrumentation.',
 'ProfilerRecorder.LastValue labels completed preceding frame. Custom markers include whole methods; nested durations overlap.',
 'UIDocument NavigationSubmitEvent exercises real button callbacks; additional guard snapshots add allocations on action frames.',
 'Input desktop Screen-saver; active-display delivery, physical pointer, thermal endurance and Android are not measured. No Dev/Release A/B claim.',
 'A zero-only or absent metric is reported unavailable rather than fabricated. Resize contains real swapchain/layout transients.']}
for phase,name in phases.items():
 selected=[r for r in rows if int(r['phase'])==phase]
 wall=[float(r['wall_ms']) for r in selected]
 data={'frames':len(selected),'frame_intervals_ms':stats(wall),'over33_333ms':sum(v>1000/30 for v in wall),'over50ms':sum(v>50 for v in wall),'metrics':{}}
 for key in rows[0]:
  if ' [' not in key:continue
  vals=[int(r[key]) for r in selected]
  if not vals or min(vals)<0 or max(vals)==0:
   data['metrics'][key]={'available_nonzero':False};continue
  unit=key.rsplit(' [',1)[-1].rstrip(']')
  scale=1e-6 if unit=='TimeNanoseconds' else 1.0
  scaled=[v*scale for v in vals]
  sm=stats(scaled);sm['unit']='ms' if scale==1e-6 else unit;sm['nonzero_frames']=sum(v!=0 for v in vals)
  if 'GC Allocated In Frame' in key:sm['total_bytes']=sum(vals)
  data['metrics'][key]=sm
 result['phases'][name]=data
evidence=root/'Evidence'
(evidence/f'release-v0.4f-{label}-summary.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
for name in ['native-frames.csv','counter-catalog.csv','native-manifest.json','native-ready.png','native-complete.png']:
 src=folder/name
 if src.exists():shutil.copyfile(src,evidence/f'release-v0.4f-{label}-{name}')
traces=[]
for p in folder.glob('progression-profiler*'):
 traces.append({'path':str(p),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
if traces:(evidence/f'release-v0.4f-{label}-trace.json').write_text(json.dumps(traces,indent=2),encoding='utf-8')
for name,data in result['phases'].items():
 print(name,data['frames'],'wall_ms',dict((k,round(data['frame_intervals_ms'][k],3)) for k in ['median','p95','p99','max']),'stutters',data['over33_333ms'],data['over50ms'])
 for key,value in data['metrics'].items():
  if ('ZTM.' in key or 'GC Allocated In Frame' in key or 'UIR.' in key or 'Main Thread' in key) and 'median' in value:
   print(' ',key,'mean',round(value['mean'],4),'p95',round(value['p95'],4),'p99',round(value['p99'],4),'max',round(value['max'],4),value['unit'])
print('PASS native isolation/completion/reload/resize/feet and sequential actual frameCount',manifest['firstFrame'],manifest['lastFrame'],'samples',len(rows),'tracefiles',len(traces))
