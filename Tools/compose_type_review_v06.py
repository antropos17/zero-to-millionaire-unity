from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
r=Path(r'X:\Projects\ZERO-TO-MILLIONAIRE');ev=r/'Evidence';receipt=[]
for role in ['dialogue','menu']:
 before=ev/f'type-v06-before-{role}-360x640.png';after=ev/f'review-v06-after-{role}-type-360x640.png'
 a=Image.open(before).convert('RGB');b=Image.open(after).convert('RGB');assert a.size==b.size==(360,640)
 c=Image.new('RGB',(720,664),'#142c28');d=ImageDraw.Draw(c);d.text((8,6),'BEFORE safety f69ae158',fill='#f9edd9');d.text((368,6),'AFTER review fixes',fill='#f9edd9');c.paste(a,(0,24));c.paste(b,(360,24));out=ev/f'type-{role}-comparison-v06.png';c.save(out)
 receipt.append({'role':role,'before_source_commit':'f69ae158279596c981a4008b97cc05eaa4a8d987','before_image':before.name,'after_image':after.name,'source':'Authentic actual Editor captures; before bytes exported from Git unchanged; prepared intro fixture; no OS input','comparison':out.name,'sha256':hashlib.sha256(out.read_bytes()).hexdigest()})
(ev/'type-roles-comparison-v06-provenance.json').write_text(json.dumps(receipt,indent=2))
print(json.dumps(receipt))