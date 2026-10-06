from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import shutil, json, hashlib

root = Path(__file__).resolve().parents[1]
ev = root / 'Evidence'
for source, target in [
    ('v0.3-start-360x640.png','ui-v0.3-spin-depth-after-full.png'),
    ('v0.3-spin-pressed-360x640.png','ui-v0.3-spin-depth-after-pressed-full.png'),
]:
    shutil.copyfile(ev / source, ev / target)
ref = root / 'references' / 'exec-2799bd70-2d1b-4e9c-8be5-9e81503711c5.png'
if not ref.exists():
    candidates = list((root / 'references').rglob(ref.name))
    assert len(candidates) == 1
    ref = candidates[0]
font = ImageFont.truetype(str(root / 'Assets/Resources/Fonts/PixelifySans.ttf'),18)
sheet = Image.new('RGB',(470,740),'#f9eeda')
draw = ImageDraw.Draw(sheet)
rows = [
    ('Approved reference',Image.open(ref).crop((340,1422,917,1646)).resize((450,176),Image.Resampling.NEAREST)),
    ('Previous button',Image.open(ev / 'ui-v0.3-spin-depth-before-full.png').crop((124,564,351,635)).resize((450,142),Image.Resampling.NEAREST)),
    ('New raised button',Image.open(ev / 'ui-v0.3-spin-depth-after-full.png').crop((124,564,351,635)).resize((450,142),Image.Resampling.NEAREST)),
    ('New pressed button',Image.open(ev / 'ui-v0.3-spin-depth-after-pressed-full.png').crop((124,564,351,635)).resize((450,142),Image.Resampling.NEAREST)),
]
y = 8
for title,pixels in rows:
    draw.text((10,y),title,font=font,fill='#142c28')
    sheet.paste(pixels,(10,y+25))
    y += pixels.height+35
sheet.save(ev / 'ui-v0.3-spin-depth-review.png')
report = { 'logical_screen':[360,640], 'spin_base_depth':10, 'pressed_face_travel':5,
    'source':'actual Unity UI render textures', 'reference':ref.name,
    'files':[] }
for name in ['ui-v0.3-spin-depth-review.png','ui-v0.3-spin-depth-after-full.png','ui-v0.3-spin-depth-after-pressed-full.png']:
    p = ev / name
    report['files'].append({'name':name,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
(ev/'ui-v0.3-spin-depth-review.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report))
