"""Crop approved generated key poses into reusable rig parts and anchored idle."""
from pathlib import Path
from PIL import Image,ImageFilter
import json

root=Path(__file__).resolve().parents[1]
idle=Image.open(root/'asset-source-v0.2-idle/exec-745de9ab-14e8-4227-baa1-fcfd91270c84.png').convert('RGBA')
sources=[Image.open(root/'asset-source-v0.2-walk'/name).convert('RGBA') for name in [
    'exec-3211cb7a-72a8-4750-8762-d7d0bc3feb7d.png','exec-c0c10161-231a-4911-a7da-1c1f01f280ff.png']]

def isolated(image):
    # The neutral file has a low-alpha generated glow. Keep the single opaque
    # silhouette and one antialias pixel, without baking in a matte rectangle.
    w,h=image.size
    values=image.getchannel('A')
    mask=bytearray(1 if a>200 else 0 for a in values.getdata())
    largest=[]
    for seed in range(len(mask)):
        if not mask[seed]: continue
        stack=[seed]; mask[seed]=0; component=[]
        while stack:
            p=stack.pop(); component.append(p); x=p%w; y=p//w
            for q in (p-1 if x else -1,p+1 if x<w-1 else -1,p-w if y else -1,p+w if y<h-1 else -1):
                if q>=0 and mask[q]: mask[q]=0; stack.append(q)
        if len(component)>len(largest): largest=component
    assert len(largest)>10000,'No separable opaque character silhouette'
    selected=bytearray(w*h)
    for p in largest: selected[p]=255
    selected=Image.frombytes('L',(w,h),bytes(selected)).filter(ImageFilter.MaxFilter(3))
    alpha=bytes(a if keep else 0 for a,keep in zip(values.getdata(),selected.getdata()))
    image.putalpha(Image.frombytes('L',(w,h),alpha))
    return image.crop(image.getchannel('A').getbbox())

def anchored(image):
    crop=isolated(image)
    scale=166/crop.height
    resized=crop.resize((round(crop.width*scale),166),Image.Resampling.NEAREST)
    head=resized.crop((0,0,resized.width,50)).getchannel('A').getbbox()
    head_centre=(head[0]+head[2])/2
    canvas=Image.new('RGBA',(128,192))
    canvas.alpha_composite(resized,(round(64-head_centre),12))
    return canvas

def legs_only(full):
    image=full.crop((0,102,128,192))
    # Keep generated trousers/shoes, excluding detached hand fragments from the
    # stride key pose. Skin belongs to the stable neutral torso/arms layer.
    pixels=list(image.getdata()); w,h=image.size
    for y in range(min(43,h)):
        for x in range(w):
            r,g,b,a=pixels[y*w+x]
            if r>70 and r>g*1.07 and g>b*1.12: pixels[y*w+x]=(r,g,b,0)
    image.putdata(pixels)
    mask=bytearray(1 if p[3]>100 else 0 for p in pixels)
    biggest=[]
    for seed in range(len(mask)):
        if not mask[seed]:continue
        stack=[seed];mask[seed]=0;component=[]
        while stack:
            p=stack.pop();component.append(p);x=p%w;y=p//w
            for q in (p-1 if x else -1,p+1 if x<w-1 else -1,p-w if y else -1,p+w if y<h-1 else -1):
                if q>=0 and mask[q]:mask[q]=0;stack.append(q)
        if len(component)>len(biggest):biggest=component
    selected=set(biggest)
    image.putalpha(Image.frombytes('L',(w,h),bytes(p[3] if i in selected else 0 for i,p in enumerate(pixels))))
    return image

out=root/'Assets/Resources/Art/animation';out.mkdir(exist_ok=True)
receipt=[]
contact_sheet=Image.new('RGBA',(512,384))
for view,name in enumerate(['front','back']):
    neutral=anchored(idle.crop((view*idle.width//2,0,(view+1)*idle.width//2,idle.height)))
    contact=anchored(sources[view].crop((0,0,sources[view].width//4,sources[view].height)))
    neutral.save(out/(name+'-idle.png'))
    leg_parts=[legs_only(contact),legs_only(neutral)]
    torso=neutral.copy(); torso_alpha=list(torso.getchannel('A').getdata())
    for y in range(90):
        for x in range(128):
            if leg_parts[1].getpixel((x,y))[3]:torso_alpha[(y+102)*128+x]=0
    torso.putalpha(Image.frombytes('L',(128,192),bytes(torso_alpha)))
    for pose in range(2):
        torso.save(out/(name+f'-{pose}-upper.png'))
        leg_parts[pose].save(out/(name+f'-{pose}-legs.png'))
    # Production rig uses the consistent neutral legs, rather than the wide
    # draft stride poses. Retain overlap above the knee to hide joint seams.
    body=neutral.copy(); body_alpha=list(body.getchannel('A').getdata())
    for y in range(8,90):
        for x in range(128):
            if leg_parts[1].getpixel((x,y))[3]:body_alpha[(y+102)*128+x]=0
    body.putalpha(Image.frombytes('L',(128,192),bytes(body_alpha)));body.save(out/(name+'-body.png'))
    for side in ['left','right']:
        leg=leg_parts[1].copy();pixels=list(leg.getdata())
        for i,pixel in enumerate(pixels):
            if (i%128<66) != (side=='left'):pixels[i]=(pixel[0],pixel[1],pixel[2],0)
        leg.putdata(pixels);leg.save(out/(name+'-leg-'+side+'.png'))
    for phase in range(4):
        tile=Image.new('RGBA',(128,192))
        leg=leg_parts[phase%2].copy()
        if phase>=2:leg=leg.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
        tile.alpha_composite(leg,(0,102));tile.alpha_composite(torso,(0,0))
        contact_sheet.alpha_composite(tile,(phase*128,view*192))
    receipt.append({'view':name,'canvas':[128,192],'feet':[64,178],
        'cutout_split_y':102,'source_key_poses':2,'gait_phases':4,
        'alternation':'production: two independent neutral leg cutouts rotate oppositely at hip; continuous distance-driven gait; draft contact candidates retained only for comparison'})
contact_sheet.save(root/'Evidence/cutout-gait-v0.2.png')
(root/'asset-source-v0.2-idle/cutout-rig.json').write_text(json.dumps(receipt,indent=2))
print(json.dumps(receipt))
