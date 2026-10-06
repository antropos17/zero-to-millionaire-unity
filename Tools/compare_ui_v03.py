"""QA comparison only; the game remains live UI controls and separate sprites."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,re

r=Path(__file__).resolve().parents[1];e=r/'Evidence';font=ImageFont.truetype(str(r/'Assets/Resources/Fonts/PixelifySans.ttf'),18)
panes=[('REFERENCE',r/'references/exec-e75feb62-841f-4788-8ecb-d3ea6a8afa0b.png'),('BEFORE v0.2',e/'v0.2-start-360x640.png'),('AFTER v0.3',e/'v0.3-start-360x640.png')]
sheet=Image.new('RGB',(1100,680),(249,238,218));draw=ImageDraw.Draw(sheet)
for i,(label,p) in enumerate(panes):
    im=Image.open(p).convert('RGB').resize((360,640),Image.Resampling.NEAREST)
    x=i*370;sheet.paste(im,(x,32));draw.text((x+10,7),label,font=font,fill=(20,44,40))
sheet.save(e/'ui-v0.3-reference-before-after.png')
log=(e/'unity-ui-v0.3-qa.log').read_text(encoding='utf-8-sig',errors='replace')
measurements=[]
for name,x,y,w,h in re.findall(r'(v0\.3-[\w-]+) stage world=\(x:([\d.-]+), y:([\d.-]+), width:([\d.-]+), height:([\d.-]+)\)',log):
    width=float(w);height=float(h)
    measurements.append({'capture':name,'stage_pixels_in_panel':[width,height],
        'opaque_room_width_in_panel':round(width*457/512,1),'heroine_canvas_height_in_panel':round(height*162/512,1),
        'heroine_neutral_opaque_height_in_panel':round(height*162/512*166/192,1)})
receipt={'palette':{'cream':'#F9EEDA','forest':'#142C28','rust':'#BA4B2D'},
    'reference_sample_modes':{'cream':[249,237,217],'forest':[20,44,40],'rust':[186,75,45]},
    'layout_measurements':measurements,'reference_room_width_at360':round(656/941*360,1),
    'responsive_tradeoff':'At 360x640 the grid uses compact 44-unit rows and SPIN height68 so the entire room can grow proportionally. Taller screens restore grid height204 and SPIN96. Character/object ratios and floor projection are unchanged.',
    'font':'Pixelify Sans, unmodified OFL TTF, explicitly bound with FontDefinition.FromFont',
    'comparison_is_supporting_evidence_only':True}
(e/'ui-v0.3-comparison.json').write_text(json.dumps(receipt,indent=2));print(json.dumps(receipt))
