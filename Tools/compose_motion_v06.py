from pathlib import Path
import json,bisect,hashlib,shutil
from PIL import Image,ImageDraw
root=Path(r'X:\Projects\ZERO-TO-MILLIONAIRE');ev=root/'Evidence'
tags=['before','final'];times={t:json.loads((ev/f'polish-motion-v06-{t}-times.json').read_text())['seconds'] for t in tags}
def index(tag,t):
 a=times[tag];i=bisect.bisect_left(a,t);return min(range(max(0,i-1),min(len(a),i+1)),key=lambda j:abs(a[j]-t))
def frame(tag,t):return Image.open(root/'.tooling'/f'polish-motion-v06-{tag}'/'frames'/f'{index(tag,t):04}.png').convert('RGB')
sample=[i*.06 for i in range(150)];mapping=[];frames=[]
for t in sample:
 c=Image.new('RGB',(720,744),'#142c28');d=ImageDraw.Draw(c);d.text((8,6),'BEFORE v0.6 safety',fill='#f9edd9');d.text((368,6),'AFTER v0.6 motion + type',fill='#f9edd9');c.paste(frame('before',t),(0,24));c.paste(frame('final',t),(360,24));frames.append(c);mapping.append({'t':t,'before':index('before',t),'final':index('final',t)})
p=ev/'motion-comparison-v06.gif';frames[0].save(p,save_all=True,append_images=frames[1:],duration=60,loop=0,optimize=True)
for label,t in [('travel',1.45),('match',2.25),('reward',2.6),('purchase',4.2),('complete',7.3)]:frame('final',t).save(ev/f'motion-v06-{label}.png')
c=Image.new('RGB',(720,744),'#142c28');d=ImageDraw.Draw(c);d.text((8,6),'BEFORE: actual captured frame',fill='#f9edd9');d.text((368,6),'AFTER: actual captured frame',fill='#f9edd9');c.paste(frame('before',.5),(0,24));c.paste(frame('final',.5),(360,24));c.save(ev/'type-comparison-v06.png')
a=json.loads((ev/'polish-motion-v06-before-state.json').read_text());b=json.loads((ev/'polish-motion-v06-final-state.json').read_text());a.pop('heroineU');a.pop('heroineV');b.pop('heroineU');b.pop('heroineV');assert a==b,'Economy/grid/quests changed between sequences'
with Image.open(p) as g:assert g.n_frames>100;decoded=g.n_frames
(ev/'motion-comparison-v06-provenance.json').write_text(json.dumps({'source':'Actual Unity Editor RenderTexture captures; prepared final-task fixture and synthetic UI callbacks; no physical input','sample_seconds':.06,'gif_frames':decoded,'gif_duration_ms':9000,'no_interpolation':True,'mapping':mapping,'result_economy_grid_quests_tutorial_equal':True,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()},indent=2))
print(json.dumps({'gif_bytes':p.stat().st_size,'decoded_frames':decoded,'equal_results':True,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}))