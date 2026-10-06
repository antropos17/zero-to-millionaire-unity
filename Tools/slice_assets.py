"""Normalize explicitly authorized generated atlases; originals remain unchanged."""
from pathlib import Path
import hashlib, json
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'asset-source'
ART = ROOT / 'Assets' / 'Resources' / 'Art'
records = []

def visible_crop(image):
    alpha = image.getchannel('A')
    bounds = alpha.point(lambda p: 255 if p > 128 else 0).getbbox()
    if not bounds:
        raise ValueError('Empty generated sprite')
    image = image.crop(bounds)
    image.putalpha(image.getchannel('A').point(lambda p: p if p >= 16 else 0))
    return image, bounds

def write(image, relative, source, bounds):
    path = ART / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)
    alpha = image.getchannel('A')
    assert alpha.getextrema()[0] == 0 and alpha.getextrema()[1] == 255
    records.append({'file': str(path.relative_to(ROOT)), 'source': source, 'source_bounds': bounds,
                    'size': image.size, 'rgba': True, 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()})

room_name = 'exec-0a67835e-5076-4f83-be77-df00043ad0f6.png'
atlas = Image.open(SOURCE / room_name).convert('RGBA')
normalized = []
for i, name in enumerate(['ruined', 'patches', 'surfaces', 'wallpaper', 'repaired']):
    cell = (round(i * atlas.width / 5), 0, round((i + 1) * atlas.width / 5), atlas.height)
    crop, bbox = visible_crop(atlas.crop(cell))
    # Uniform scale, same footprint; bottom center pivot, never stretch an axis.
    scale = min(460 / crop.width, 460 / crop.height)
    scaled = crop.resize((round(crop.width * scale), round(crop.height * scale)), Image.Resampling.NEAREST)
    canvas = Image.new('RGBA', (512, 512))
    offset = ((512 - scaled.width) // 2, 490 - scaled.height)
    canvas.alpha_composite(scaled, offset)
    normalized.append((canvas, crop, scale, offset))
    write(canvas, f'room/{name}.png', room_name, [cell, bbox])

# Independent repaired opening overlays from the generated fifth room. Full-room
# canvas shares the room pivot, so buying one does not switch the other opening.
canvas, crop, scale, offset = normalized[4]
for name, polygon in [
    ('door-overlay', [(312, 94), (375, 122), (375, 310), (312, 276)]),
    ('window-overlay', [(50, 112), (125, 77), (125, 180), (50, 216)])
]:
    mask = Image.new('L', crop.size); ImageDraw.Draw(mask).polygon(polygon, fill=255)
    overlay = crop.copy(); overlay.putalpha(mask)
    overlay = overlay.resize((round(crop.width * scale), round(crop.height * scale)), Image.Resampling.NEAREST)
    output = Image.new('RGBA', (512, 512)); output.alpha_composite(overlay, offset)
    write(output, f'room/{name}.png', room_name, polygon)

object_name = 'exec-d0de5681-7375-4430-b693-9d5a23bd6b37.png'
atlas = Image.open(SOURCE / object_name).convert('RGBA')
for name, bounds, max_edge in [
    ('heroine', (150, 0, 570, 800), 384),
    ('mattress-dressed', (570, 0, 1358, 680), 384),
    ('stool', (160, 780, 600, 1159), 160),
    ('lamp', (840, 630, 1200, 1159), 256)
]:
    crop, inner = visible_crop(atlas.crop(bounds))
    scale = min(1, max_edge / max(crop.size))
    crop = crop.resize((round(crop.width * scale), round(crop.height * scale)), Image.Resampling.NEAREST)
    padded = Image.new('RGBA', (crop.width + 4, crop.height + 4)); padded.alpha_composite(crop, (2, 2))
    write(padded, f'objects/{name}.png', object_name, [bounds, inner])

icon_name = 'exec-5acc7cd6-421a-40c8-807f-3421139fa2f8.png'
plain_name = 'exec-d92ab5f9-51ce-4be4-9aa9-2076bd4b17a6.png'
plain_atlas = Image.open(SOURCE / plain_name).convert('RGBA')
plain, plain_bounds = visible_crop(plain_atlas.crop((600, 200, 1300, 670)))
plain_scale = min(384 / plain.width, 384 / plain.height)
plain = plain.resize((round(plain.width * plain_scale), round(plain.height * plain_scale)), Image.Resampling.NEAREST)
plain_padded = Image.new('RGBA', (plain.width+4, plain.height+4)); plain_padded.alpha_composite(plain, (2,2))
write(plain_padded, 'objects/mattress.png', plain_name, [(600,200,1300,670), plain_bounds])

atlas = Image.open(SOURCE / icon_name).convert('RGBA')
# Use content boxes, not equal fifths: the car/chair at their boundary touch.
for name, bounds in [
    ('cash', (30, 220, 440, 565)), ('key', (490, 180, 845, 535)),
    ('shirt', (890, 200, 1280, 540)), ('chair', (1340, 180, 1655, 565)),
    ('car', (1720, 230, 2172, 535))
]:
    crop, inner = visible_crop(atlas.crop(bounds))
    scale = min(112 / crop.width, 112 / crop.height)
    crop = crop.resize((round(crop.width * scale), round(crop.height * scale)), Image.Resampling.NEAREST)
    padded = Image.new('RGBA', (128, 128)); padded.alpha_composite(crop, ((128 - crop.width)//2, (128 - crop.height)//2))
    write(padded, f'symbols/{name}.png', icon_name, [bounds, inner])

(ROOT / 'Evidence' / 'asset-validation.json').write_text(json.dumps(records, indent=2))
sheet = Image.new('RGB', (1024, 800), '#f9eed5')
for i, (image, _, _, _) in enumerate(normalized):
    thumb = image.resize((200, 200), Image.Resampling.NEAREST)
    sheet.paste(thumb, (i * 202, 0), thumb)
for i, name in enumerate(['heroine', 'mattress-dressed', 'stool', 'lamp']):
    im = Image.open(ART / 'objects' / f'{name}.png'); im.thumbnail((210, 310), Image.Resampling.NEAREST)
    sheet.paste(im, (40 + i * 250, 240), im)
for i, name in enumerate(['cash','key','shirt','chair','car']):
    im = Image.open(ART / 'symbols' / f'{name}.png')
    sheet.paste(im, (35 + i * 200, 650), im)
sheet.save(ROOT / 'Evidence' / 'reusable-assets-contact-sheet.png')
print(json.dumps({'separate_assets':len(records), 'alpha_verified':True, 'uniform_room_scale':True}))
