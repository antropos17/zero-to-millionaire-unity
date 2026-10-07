from pathlib import Path
import json,hashlib
from PIL import Image,ImageDraw,ImageFont,ImageChops

root=Path(r'X:\Projects\ZERO-TO-MILLIONAIRE')
evidence=root/'Evidence'
font=ImageFont.truetype(str(root/'Assets/Resources/Fonts/ZTMReadable-Semibold.ttf'),15)
after='v08-after-2'
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def panel(a,b,labels):
    out=Image.new('RGB',(720,668),'#f9edd9');out.paste(a,(0,28));out.paste(b,(360,28));d=ImageDraw.Draw(out)
    d.text((12,6),labels[0],font=font,fill='#142c28');d.text((372,6),labels[1],font=font,fill='#142c28')
    return out

rows=[]
for sequence in ['spin','repair']:
    before=sorted((root/'.tooling/v08-before-1/frames').glob(sequence+'-*.png'))
    current=sorted((root/'.tooling'/after/'frames').glob(sequence+'-*.png'))
    assert before and current,(sequence,len(before),len(current))
    frames=[]
    for i in range(max(len(before),len(current))):
        with Image.open(before[min(i,len(before)-1)]) as a,Image.open(current[min(i,len(current)-1)]) as b:
            frames.append(panel(a.convert('RGB'),b.convert('RGB'),('BEFORE · v0.7','AFTER · v0.8')))
    destination=evidence/('v08-'+sequence+'-before-after.gif')
    assert not destination.exists(),'Immutable GIF exists'
    frames[0].save(destination,save_all=True,append_images=frames[1:],duration=50,loop=0,optimize=False)
    rows.append({'file':destination.name,'bytes':destination.stat().st_size,'sha256':sha(destination),'frames':len(frames),'before_frames':len(before),'after_frames':len(current),'source':'Actual Unity offscreen captures at approximately 20 fps; side-by-side composition only; no generated/tweened game frames. Last source frame held if lengths differ.'})
for name in ['room','menu','tutorial','ready','installed']:
    with Image.open(evidence/('v08-before-1-'+name+'.png')) as a,Image.open(evidence/(after+'-'+name+'.png')) as b:
        p=evidence/('v08-'+name+'-before-after.png');panel(a.convert('RGB'),b.convert('RGB'),('BEFORE · v0.7','AFTER · v0.8')).save(p)
        rows.append({'file':p.name,'bytes':p.stat().st_size,'sha256':sha(p)})
path=evidence/'v08-visual-comparison-manifest.json';path.write_text(json.dumps(rows,indent=2),encoding='utf-8')
print(json.dumps(rows,indent=2))
