from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib

root=Path(__file__).resolve().parents[1]
ev=root/'Evidence'
font=ImageFont.truetype(str(root/'Assets/Resources/Fonts/PixelifySans.ttf'),16)
names=[('BEFORE - TASK 1 READY','v0.3-quest-buy-ready-360x640.png'),
       ('AFTER - ACTUAL INSTALL','v0.3-quest-after-buy-360x640.png')]
sheet=Image.new('RGB',(744,680),'#f9eeda');draw=ImageDraw.Draw(sheet)
for i,(title,name) in enumerate(names):
    draw.text((8+i*368,8),title,font=font,fill='#142c28')
    pixels=Image.open(ev/name);assert pixels.size==(360,640)
    sheet.paste(pixels,(8+i*368,32))
target=ev/'v0.3-quest-flow-review.png';sheet.save(target)
report={'source':'actual Unity UI render textures; second panel follows live BUY callback',
        'before':{'cash':18,'parts':8,'completed':0,'active':'floor-patches'},
        'after':{'cash':0,'parts':0,'completed':1,'active':'surface-repair'},
        'file':target.name,'sha256':hashlib.sha256(target.read_bytes()).hexdigest()}
(ev/'v0.3-quest-flow-review.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report))
