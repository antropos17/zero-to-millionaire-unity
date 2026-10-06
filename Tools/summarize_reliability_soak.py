"""Summarize completed private native soak; bucket quantiles are not global frame quantiles."""
import csv,json,statistics,sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
run=root/'.tooling'/sys.argv[1]
info=json.loads((run/'soak-summary.json').read_text(encoding='utf-8'))
if not info.get('finished'):raise SystemExit('Soak is incomplete: no final claims')
rows=list(csv.DictReader((run/'soak-trends.csv').open(encoding='utf-8')))
for row in rows:
    for k,v in row.items():row[k]=float(v)
previous=0
for row in rows:row['bucket_seconds']=row['seconds']-previous;previous=row['seconds']
def period(a,b):
    values=[r for r in rows if a<=r['seconds']<=b]
    keys=('median_ms','p95_ms','p99_ms')
    return dict(window_seconds=[a,b],buckets=len(values),
      mean_bucket_quantiles_ms={k:statistics.mean(r[k] for r in values) for k in keys},
      maximum_ms=max(r['max_ms'] for r in values),over33=sum(int(r['over33_ms']) for r in values),over50=sum(int(r['over50_ms']) for r in values),
      allocated_bytes_per_second=sum(r['allocated_bytes'] for r in values)/sum(r['bucket_seconds'] for r in values),
      memory_mib={k:dict(first=values[0][k]/1048576,last=values[-1][k]/1048576,min=min(r[k] for r in values)/1048576,max=max(r[k] for r in values)/1048576) for k in ('system_used','unity_used','unity_reserved','gc_used','gc_reserved')},
      gc0_collections_delta=int(values[-1]['gc_collections0']-values[0]['gc_collections0']))
log=(run/'Player.log').read_text(encoding='utf-8',errors='replace')
result=dict(summary=info,phase_early=period(120,300),phase_late=period(600,info['seconds']),
    total_over33=sum(int(r['over33_ms']) for r in rows),total_over50=sum(int(r['over50_ms']) for r in rows),total_maximum_ms=max(r['max_ms'] for r in rows),
    warning_count=log.count('GarbageCollector disposing of ComputeBuffer.'),
    all_memory_counters_available=all(r[k]>0 for r in rows for k in ('system_used','unity_used','unity_reserved','gc_used','gc_reserved')),
    quantile_scope='Means of per-10-second frame-loop quantiles, not whole-session frame quantiles.',
    display_scope='Native D3D11 Player/window visible on Default; input desktop Screen-saver. No full-session GPU/present or human pointer claim.',
    build_scope='reliability-before; fixed panel lifetime tested separately in final reliability build.')
out=root/'Evidence/native-v0.4e-soak-analysis.json'
out.write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))