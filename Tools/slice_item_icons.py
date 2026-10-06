"""Normalize approved atlas into individual reusable symbols; no new illustration."""
from pathlib import Path
from PIL import Image
import json

root = Path(__file__).resolve().parents[1]
source = root / 'asset-source-v0.2' / 'exec-e26a679c-2641-4b96-a301-1b2c49b7212e.png'
image = Image.open(source).convert('RGBA')
w,h = image.size
alpha = image.getchannel('A')
mask = bytearray(1 if value > 128 else 0 for value in alpha.getdata())
components = []
for seed in range(len(mask)):
    if not mask[seed]: continue
    stack=[seed]; mask[seed]=0; count=0
    x0=w; y0=h; x1=y1=0
    while stack:
        p=stack.pop(); x=p%w; y=p//w; count+=1
        x0=min(x0,x); x1=max(x1,x); y0=min(y0,y); y1=max(y1,y)
        for q in (p-1 if x else -1,p+1 if x<w-1 else -1,p-w if y else -1,p+w if y<h-1 else -1):
            if q>=0 and mask[q]: mask[q]=0; stack.append(q)
    if count>10000: components.append((x0,y0,x1+1,y1+1,count))
assert len(components)==12, components
# Natural atlas rows are not exact rectangular cells: use full connected bounds
# to avoid clipping the wide mattress or lamp that crosses nominal row edges.
components.sort(key=lambda box: (box[1]+box[3])/2)
ordered=[]
for row in range(4): ordered.extend(sorted(components[row*3:row*3+3],key=lambda b:b[0]))
names=['cash','floorpatch','wallrepair','wallpaper','door','window','mattress','lamp','stool','door-owned','window-owned','mattress-owned']
out = root / 'Assets' / 'Resources' / 'Art' / 'item-symbols'
out.mkdir(exist_ok=True)
receipt=[]
sheet=Image.new('RGBA',(384,512))
for index,(name,box) in enumerate(zip(names,ordered)):
    x0,y0,x1,y1,_=box
    crop=image.crop((max(0,x0-3),max(0,y0-3),min(w,x1+3),min(h,y1+3)))
    clean=crop.getchannel('A').point(lambda value: 0 if value<16 else value)
    crop.putalpha(clean)
    ratio=min(112/crop.width,112/crop.height)
    scaled=crop.resize((max(1,round(crop.width*ratio)),max(1,round(crop.height*ratio))),Image.Resampling.NEAREST)
    tile=Image.new('RGBA',(128,128))
    tile.alpha_composite(scaled,((128-scaled.width)//2,(128-scaled.height)//2))
    tile.save(out/(name+'.png'))
    sheet.alpha_composite(tile,((index%3)*128,(index//3)*128))
    receipt.append({'name':name,'source_bounds':[x0,y0,x1,y1],'size':[128,128]})
sheet.save(root/'Evidence'/'item-symbols-v0.2.png')
(root/'asset-source-v0.2'/'icon-slicing.json').write_text(json.dumps(receipt,indent=2))
print(json.dumps({'icons':len(receipt),'true_alpha':True,'output':str(out)}))
