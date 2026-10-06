from pathlib import Path
from PIL import Image,ImageDraw
import bisect,json,hashlib
r=Path(r'X:\Projects\ZERO-TO-MILLIONAIRE');ev=r/'Evidence';tags=['final','review'];times={t:json.loads((ev/f'polish-motion-v06-{t}-times.json').read_text())['seconds'] for t in tags}
def idx(tag,t):
 a=times[tag];i=bisect.bisect_left(a,t);return min(range(max(0,i-1),min(len(a),i+1)),key=lambda j:abs(a[j]-t))
def frame(tag,t):return Image.open(r/'.tooling'/f'polish-motion-v06-{tag}'/'frames'/f'{idx(tag,t):04}.png').convert('RGB')
frames=[];mapping=[]
for k in range(150):
 t=k*.06;c=Image.new('RGB',(720,744),'#142c28');d=ImageDraw.Draw(c);d.text((8,6),'BEFORE review fixes',fill='#f9edd9');d.text((368,6),'AFTER phase copy + room hold',fill='#f9edd9');c.paste(frame('final',t),(0,24));c.paste(frame('review',t),(360,24));frames.append(c);mapping.append({'t':t,'before':idx('final',t),'after':idx('review',t)})
p=ev/'review-motion-comparison-v06.gif';frames[0].save(p,save_all=True,append_images=frames[1:],duration=60,loop=0,optimize=True)
a=json.loads((ev/'polish-motion-v06-final-state.json').read_text());b=json.loads((ev/'polish-motion-v06-review-state.json').read_text());a.pop('heroineU');a.pop('heroineV');b.pop('heroineU');b.pop('heroineV');assert a==b
with Image.open(p) as g:n=g.n_frames;duration=sum((g.seek(i) or g.info['duration']) for i in range(n));assert duration==9000 and n>100
for label,t in [('reward',2.35),('room-hold',4.55),('story',5.3)]:frame('review',t).save(ev/f'review-motion-v06-{label}.png')
(ev/'review-motion-comparison-v06-provenance.json').write_text(json.dumps({'source':'Actual Editor captures; synthetic UI callbacks; prepared final-task fixture; no OS/physical input','before_tag':'final','after_tag':'review','decoded_frames':n,'duration_ms':duration,'mapping':mapping,'equal_economy_grid_quests_tutorial':True,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()},indent=2))
print(json.dumps({'bytes':p.stat().st_size,'frames':n,'duration_ms':duration,'equal_result':True,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}))