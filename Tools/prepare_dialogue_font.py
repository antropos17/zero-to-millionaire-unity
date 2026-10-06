from pathlib import Path
from fontTools.ttLib import TTFont
import json
r=Path(r"X:\Projects\ZERO-TO-MILLIONAIRE")
p=r/"Assets/Resources/Fonts/PixelifySans.ttf"
f=TTFont(p); pi=f.getBestCmap()[0x03a0];latin_o=f.getBestCmap()[0x4f]
for table in f['cmap'].tables:
    if table.isUnicode():table.cmap[0x041f]=pi;table.cmap[0x041e]=latin_o
for record in f['name'].names:
    if record.nameID in (1,4,6,16):
        value='ZTMPixelify-Regular' if record.nameID==6 else 'ZTM Pixelify'
        record.string=value.encode(record.getEncoding())
f.save(p)
(r/'Assets/Resources/Fonts/MODIFICATION.txt').write_text('Pixelify Sans, SIL OFL1.1. Local derivative named ZTM Pixelify. Cyrillic capital O U+041E aliases existing Latin O U+004F; Cyrillic uppercase Pe U+041F aliases the existing visually identical Greek Pi U+03A0 glyph; no glyph outlines changed. Original copyright/license retained in OFL.txt.\n',encoding='utf-8')
print(json.dumps({'added':'U+041F','existing_glyph':pi,'outlines_unchanged':True}))