from pathlib import Path
import re, hashlib, json, shutil, uuid
from fontTools.ttLib import TTFont

root=Path(r'X:\Projects\ZERO-TO-MILLIONAIRE')
runtime=root/'Assets/Scripts/Runtime'
def write(path,text):path.write_text(text,encoding='utf-8',newline='\n')
def digest(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def rename_font(source,destination,style):
    font=TTFont(source)
    substitutions={}
    for feature in font['GSUB'].table.FeatureList.FeatureRecord:
        if feature.FeatureTag=='tnum':
            for index in feature.Feature.LookupListIndex:
                for table in font['GSUB'].table.LookupList.Lookup[index].SubTable:
                    substitutions.update(getattr(table,'mapping',{}))
    cmap=font.getBestCmap()
    mapped={cp:substitutions.get(cmap[cp],cmap[cp]) for cp in range(48,58)}
    widths={font['hmtx'][glyph][0] for glyph in mapped.values()}
    assert len(widths)==1, ('Existing tabular glyphs required',widths)
    for table in font['cmap'].tables:
        if table.isUnicode():table.cmap.update(mapped)
    family='ZTM Readable'
    names={1:family,2:style,3:family+' '+style+' v0.8',4:family+' '+style,6:'ZTMReadable-'+style,16:family,17:style}
    for record in font['name'].names:
        if record.nameID in names:record.string=names[record.nameID].encode(record.getEncoding())
    font.save(destination)
    loaded=TTFont(destination);assert len({loaded['hmtx'][loaded.getBestCmap()[cp]][0] for cp in range(48,58)})==1
    return {'source_file':str(source),'source_sha256':digest(source),'output':destination.name,'output_sha256':digest(destination),'tabular_advance':next(iter(widths))}

fonts=root/'Assets/Resources/Fonts'
installed=Path(r'X:\Programs\Unity\6000.3.21f1\Editor\Data\Resources\Fonts')
license_text=(installed/'Inter-LICENSE.txt').read_text(encoding='utf-8')
assert 'SIL OPEN FONT LICENSE Version 1.1' in license_text
provenance=[rename_font(installed/'Inter-Regular.ttf',fonts/'ZTMReadable-Regular.ttf','Regular'),rename_font(installed/'Inter-SemiBold.ttf',fonts/'ZTMReadable-Semibold.ttf','SemiBold')]
shutil.copyfile(installed/'Inter-LICENSE.txt',fonts/'INTER-OFL.txt')
write(fonts/'READABLE-MODIFICATION.txt','''ZTM Readable v0.8, derived from the OFL-licensed Inter fonts already supplied
with Unity 6000.3.21f1. No software or system fonts were installed.
Original copyright and complete SIL OFL 1.1 are in INTER-OFL.txt.
Official project: https://rsms.me/inter/
Changes: renamed family to ZTM Readable; mapped U+0030..U+0039 to the existing
tnum glyphs so runtime UI counters have fixed-width digits without relying on
unsupported CSS font-feature settings. Letter outlines and metrics unchanged.
Regular and SemiBold are separate real font resources. Pixelify remains only
for the game's pixel wordmark. Body, dialogue, controls and counters use the
clearer face. No claim of endorsement by the original authors.
'''+json.dumps(provenance,indent=2)+'\n')
template=(fonts/'PixelifySans.ttf.meta').read_text(encoding='utf-8')
for file in ['ZTMReadable-Regular.ttf','ZTMReadable-Semibold.ttf']:
    meta=fonts/(file+'.meta')
    if not meta.exists():write(meta,re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid4().hex,template))

write(runtime/'UiTypography.cs','''using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    public static class UiTypography
    {
        public const int Title = 16, Meta = 12, Counter = 13, Body = 15;
        public const int DialogueBody = 15, SmallAction = 14;
        static Font font, emphasisFont, brandFont;
        public static Font Font => font != null ? font : (font = Resources.Load<Font>("Fonts/ZTMReadable-Regular"));
        public static Font EmphasisFont => emphasisFont != null ? emphasisFont : (emphasisFont = Resources.Load<Font>("Fonts/ZTMReadable-Semibold"));
        public static Font BrandFont => brandFont != null ? brandFont : (brandFont = Resources.Load<Font>("Fonts/PixelifySans"));

        public static void Apply(VisualElement element, int size, Color color, bool emphasis)
        {
            var selected = emphasis ? EmphasisFont : Font;
            element.style.unityFont = selected;
            element.style.unityFontDefinition = FontDefinition.FromFont(selected);
            element.style.fontSize = size;
            element.style.color = color;
            element.style.unityFontStyleAndWeight = FontStyle.Normal;
        }
        public static void Brand(VisualElement element)
        {
            element.style.unityFont = BrandFont;
            element.style.unityFontDefinition = FontDefinition.FromFont(BrandFont);
        }
    }
}
''')

write(runtime/'EnglishUi.cs','''namespace ZeroToMillionaire
{
    // Presentation text only. Stable quest IDs, prices and save data are unchanged.
    public static class EnglishUi
    {
        public const string RewardHint = "1 matching symbol = 1 part";
        public const string Help = "Eight upgrades, one small room. Spins give coins and parts for your current task. Extra symbols become coins.\\n\\nCollect BOTH the parts and coins shown, then press BUY to install the upgrade.\\n\\nA spin costs 5 coins and gives at least 12. Your first 3 spins are free. If you have fewer than 5 coins, FREE SPIN keeps you going.";
        static readonly string[] Titles = {
            "Patch floor", "Repair walls", "Wallpaper", "Repair door",
            "Repair window", "Add mattress", "Add lamp", "Add stool"
        };
        public static string QuestTitle(int index) => Titles[index];
        public static string Coins(int value) => value == 1 ? "coin" : "coins";
        public static string SaveNotice(string notice, bool failed) {
            if (notice == SaveStore.RecoveryNotice) return "Save unavailable. Original files are protected.";
            if (failed) return "Could not save. Please retry the action.";
            if (notice == "Save upgraded: all progress kept.") return "Save updated. All progress kept.";
            if (notice == "Recovered the previous local save.") return "Previous local save recovered.";
            if (notice == "Local save could not be read. Starting a new room.") return "Save unreadable. A new room was started.";
            if (notice == "Reset saved; backup update pending.") return "Reset saved. Backup update pending.";
            return "Saved on this computer · offline";
        }
    }
}
''')
old=runtime/'RussianUi.cs'
if old.exists():
    old.unlink()
    shutil.move(str(runtime/'RussianUi.cs.meta'),str(runtime/'EnglishUi.cs.meta'))

onboarding=root/'Assets/Scripts/Core/Onboarding.cs'
text=onboarding.read_text(encoding='utf-8')
text=re.sub(r'public static readonly string\[\] Steps=\{.*?\};','''public static readonly string[] Steps={
   "It is rough in here, but we can fix it little by little. Let us start with the floor.",
   "Spins give coins and parts for the current upgrade. Try your first free spin.",
   "Every upgrade needs BOTH coins and matching parts. Check the two progress bars below.",
   "You have enough to patch the floor. Press BUY whenever you are ready.",
   "That is better! Your next task is ready. Keep going at your own pace."};''',text,flags=re.S)
text=re.sub(r'public static readonly string\[\] Stories=\{.*?\};','''public static readonly string[] Stories={
   "The floor and walls feel solid again. Now I can picture what this room could become.",
   "What a difference! The room is ready. Next, I need somewhere to rest.",
   "A place to sleep and a little light. One simple stool will finish our small room.",
   "Nine square metres have become a cosy home. Thank you for helping me."};''',text,flags=re.S)
write(onboarding,text)

translations={
 'Маленькими шагами':'One small step', 'Далее':'Next', 'Пропустить обучение':'Skip tutorial', 'Готово':'Done', 'Закрыть':'Close',
 'Обучение — без наград':'Tutorial replay · no rewards', 'Наша комната':'Our small room',
 'Когда хватает монет и фрагментов, улучшение можно купить вручную.':'When BOTH bars are full, press BUY to install the upgrade yourself.',
 'Прогресс хранится на устройстве':'Saved on this computer · offline', 'МОНЕТЫ':'COINS', 'СТАРТ':'START', '1тыс':'1K', '10тыс':'10K', '100тыс':'100K','1млн':'1M',
 '9м':'9m', 'МЕНЮ':'MENU', 'КОМНАТА 0/8':'ROOM 0/8', 'СТАВКА':'SPIN COST', 'СПИН':'SPIN', 'БЕСПЛАТНО':'FREE SPIN',
 'МОЯ КОМНАТА':'MY ROOM', 'Повторить обучение':'Replay tutorial', 'АНИМАЦИЯ: СПОКОЙНАЯ':'ANIMATION: CALM','АНИМАЦИЯ: ПОЛНАЯ':'ANIMATION: FULL',
 'НАЧАТЬ ЗАНОВО':'START AGAIN','ВЕРНУТЬСЯ В КОМНАТУ':'BACK TO ROOM','НАЧАТЬ ЗАНОВО?':'START AGAIN?',
 'Монеты, фрагменты и улучшения будут сброшены. Вы снова получите 3 бесплатных спина.':'This resets your coins, parts and upgrades. You will get 3 free spins again.',
 'ОТМЕНА':'KEEP ROOM','СБРОСИТЬ':'RESET','СОХРАНЕНИЕ НЕДОСТУПНО':'SAVE UNAVAILABLE',
 'Прогресс пока не удалось прочитать. Исходные файлы сохранены, новые записи заблокированы. Попробуйте загрузить снова. Начало заново возможно только после подтверждения; исходные файлы останутся отдельными копиями.':'Your progress could not be read. Original files are protected and new saves are paused. Try loading again. Starting over requires confirmation and keeps a copy of the original files.',
 'ПОВТОРИТЬ ЗАГРУЗКУ':'RETRY LOAD','НЕ СОХРАНЕНО — ПОВТОРИТЕ':'NOT SAVED · PLEASE RETRY','НЕ ПРОЧИТАНО. ПОВТОРИТЬ':'LOAD FAILED · RETRY','НАСТРОЙКА НЕ СОХРАНЕНА':'SETTING NOT SAVED',
 'ВАША КОМНАТА ГОТОВА':'YOUR ROOM IS COMPLETE','ВСЁ ГОТОВО':'ALL DONE','МОЖНО КУПИТЬ':'READY TO BUY','БЕСПЛАТНО: ':'FREE: ','ЦЕНА СПИНА':'COINS PER SPIN',
 'ГОТОВО':'DONE','ПРОПУСТИТЬ':'SKIP','СТОП':'STOP','КОМНАТА ':'ROOM ','НУЖНЫ МОНЕТЫ':'NEED COINS','НУЖНЫ ФР.':'NEED PARTS','ФР. + МОНЕТЫ':'COINS + PARTS','ЗАДАНИЕ ':'',
 '8/8 ЗАДАНИЙ ВЫПОЛНЕНО':'8/8 UPGRADES COMPLETE','КОМНАТА ГОТОВА':'ROOM COMPLETE','Маленькая комната, которая стала домом.':'A small room that became a home.','Маленькая комната, которая стала вашей.':'A small room to call your own.','Уровень завершён':'Level complete',
 'КУПИТЬ':'BUY','ДАЛЕЕ: ':'NEXT: ','ЗАКРЫТО':'LOCKED','Последнее задание этой комнаты':'The final upgrade for this room',' фр.':' parts',
 'НАГРАДА':'REWARD','ВРАЩЕНИЕ':'SPINNING','8/8 — КОМНАТА ГОТОВА':'8/8 · ROOM COMPLETE',
 '/8 — ГОТОВО':'/8 · INSTALLED','БЕСПЛАТНЫЙ СПИН':'FREE SPIN','НАЧАЛО ЗАНОВО • 3 БЕСПЛАТНЫХ СПИНА':'FRESH START · 3 FREE SPINS',
 ' ФР.':' parts','КРУТИТСЯ':'SPINNING','НОВОЕ НАЧАЛО • 3 БЕСПЛАТНЫХ СПИНА':'FRESH START · 3 FREE SPINS',
 'ПОСЛЕДНЕЕ ЗАДАНИЕ ЭТОГО УРОВНЯ':'The final upgrade for this room','УРОВЕНЬ ЗАВЕРШЁН':'Level complete'
}
for name in ['GameView.cs','DialoguePresenter.cs']:
    path=runtime/name;text=path.read_text(encoding='utf-8')
    def replace_literal(match):
        value=match.group()[1:-1]
        if not re.search('[\u0400-\u04ff]',value):return match.group()
        if value not in translations:raise RuntimeError('Unmapped text '+name+': '+repr(value))
        return '"'+translations[value]+'"'
    text=re.sub(r'"(?:[^"\\]|\\.)*"',replace_literal,text).replace('RussianUi.','EnglishUi.')
    if name=='GameView.cs':
        text=text.replace('title.Add(first);title.Add(second);','UiTypography.Brand(first);UiTypography.Brand(second);title.Add(first);title.Add(second);')
        text=text.replace('Text("SPIN COST",16,Cream)','Text("SPIN COST",14,Cream)')
        text=text.replace('Text("FREE SPIN",22','Text("FREE SPIN",22')
        text=text.replace('ButtonText(menuButton,"MENU",12)','ButtonText(menuButton,"MENU",11)')
        text=text.replace('model.CanBuy(model.ActiveQuest)?"READY TO BUY"','model.CanBuy(model.ActiveQuest)?"PRESS BUY"')
        text=text.replace('cachedReady?"READY TO BUY"','cachedReady?"READY"').replace('ready?"READY TO BUY"','ready?"READY"')
        text=text.replace('ButtonText(recoveryButton,"FREE SPIN",22)','ButtonText(recoveryButton,"FREE SPIN",23)')
        text=text.replace('Text("ROOM COMPLETE",23','Text("ROOM COMPLETE",21')
    else:
        text=text.replace('var l=Text(text,12);','var l=Text(text,UiTypography.SmallAction);')
    write(path,text)
assert not any(re.search('[\u0400-\u04ff]',f.read_text(encoding='utf-8')) for f in (root/'Assets/Scripts').rglob('*.cs'))
print('English UI, all 5 tutorial steps/4 stories, licensed readable regular/semibold font with tabular digits installed as project assets.')
