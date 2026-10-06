from pathlib import Path

r=Path(__file__).resolve().parents[1]
p=r/'Assets/Scripts/Runtime/GameView.cs'
s=p.read_text(encoding='utf-8-sig')
s=s.replace('new Color32(249, 238, 213, 255)','new Color32(249, 238, 218, 255)')
s=s.replace('new Color32(21, 49, 42, 255)','new Color32(20, 44, 40, 255)')
s=s.replace('new Color32(193, 75, 37, 255)','new Color32(186, 75, 45, 255)')
s=s.replace('private int page;','private int page;\n        private VisualElement header, pathbar, grid, spinRow, settings, toast, completion;\n        private Label betLabel, freeLabel;\n        private Button menuButton;\n        private static Font pixelFont;\n        private float toastUntil;')
s=s.replace('panel.referenceResolution = new Vector2Int(480, 960);','panel.referenceResolution = new Vector2Int(360, 640);')
s=s.replace('            BuildUI();','            page = model.Complete ? 3 : model.PurchasedCount / 2;\n            BuildUI();',1)
s=s.replace('            label.style.fontSize = size; label.style.color = color;','            label.style.fontSize = size; label.style.color = color;\n            if (pixelFont == null) pixelFont = Resources.Load<Font>("Fonts/PixelifySans");\n            label.style.unityFont = pixelFont;')
start=s.index('        static Button Action(');end=s.index('        static void Border(',start)
s=s[:start]+'''        static Button Action(string text, Action action, Color color)
        {
            var button = new Button(action) { text = text };
            button.style.height = 44; button.style.fontSize = 16;
            button.style.unityFont = Resources.Load<Font>("Fonts/PixelifySans");
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.color = Cream; button.style.whiteSpace = WhiteSpace.NoWrap;
            button.style.marginLeft = button.style.marginRight = button.style.marginTop = button.style.marginBottom = 0;
            button.style.paddingLeft = button.style.paddingRight = 4;
            PixelUi.Skin(button, color == Rust ? PixelUi.Finish.Rust : PixelUi.Finish.Forest);
            return button;
        }
''' + s[end:]
start=s.index('        void BuildUI()');end=s.index('        void DrawRoom(',start)
s=s[:start]+'''        static Image Icon(Texture2D texture, float size)
        {
            var image = new Image { image=texture,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore };
            image.style.width=size;image.style.height=size;image.style.flexShrink=0;return image;
        }
        static VisualElement Frame(string name, PixelUi.Finish finish)
        { var v=new VisualElement { name=name }; PixelUi.Skin(v,finish);return v; }
        void BuildUI()
        {
            root.style.backgroundColor=Cream;root.style.alignItems=Align.Center;
            pixelFont=Resources.Load<Font>("Fonts/PixelifySans"); root.style.unityFont=pixelFont;
            main=Box("game",Cream);main.style.width=Length.Percent(100);main.style.maxWidth=450;
            main.style.height=Length.Percent(100);main.style.paddingLeft=10;main.style.paddingRight=10;
            root.Add(main);
            for(int i=0;i<reelTextures.Length;i++)reelTextures[i]=Resources.Load<Texture2D>("Art/item-symbols/"+symbolNames[i]);

            header=new VisualElement { name="header" };Row(header);header.style.flexShrink=0;
            var title=Frame("title-panel",PixelUi.Finish.Title);title.style.flexGrow=1;title.style.flexBasis=0;
            title.style.height=Length.Percent(100);title.style.marginRight=10;title.style.paddingLeft=6;
            var first=Text("ZERO TO",16,Cream);first.style.height=19;first.style.marginTop=3;
            var second=Text("MILLIONAIRE",20,PixelUi.Hex(0xffb758));second.style.height=22;
            title.Add(first);title.Add(second);header.Add(title);
            var bank=Frame("cash-panel",PixelUi.Finish.Forest);Row(bank);bank.style.flexGrow=1;bank.style.flexBasis=0;
            bank.style.height=Length.Percent(100);bank.style.paddingLeft=7;bank.style.paddingRight=7;
            bank.Add(Icon(reelTextures[0],35));
            var bankInfo=new VisualElement();bankInfo.style.flexGrow=1;
            var cashTitle=Text("CASH",13,Cream);cashTitle.style.height=15;bankInfo.Add(cashTitle);
            cashLabel=Text("0",26,Cream);cashLabel.style.height=27;cashLabel.style.unityTextAlign=TextAnchor.MiddleRight;bankInfo.Add(cashLabel);
            bank.Add(bankInfo);header.Add(bank);main.Add(header);

            pathbar=new VisualElement { name="cash-path" };Row(pathbar);pathbar.style.flexShrink=0;
            pathbar.style.justifyContent=Justify.SpaceBetween;
            string[] names={"START","1K","10K","100K","1M"};
            for(int i=0;i<5;i++) {
                var step=new VisualElement { name="milestone-"+i };step.style.width=50;step.style.alignItems=Align.Center;
                var dot=Box("milestone-dot-"+i,i==0?PixelUi.Hex(0xffb13b):Cream);dot.style.width=12;dot.style.height=12;Border(dot);dot.style.borderTopLeftRadius=3;dot.style.borderTopRightRadius=3;dot.style.borderBottomLeftRadius=3;dot.style.borderBottomRightRadius=3;
                step.Add(dot);var label=Text(names[i],11,Forest);label.style.unityTextAlign=TextAnchor.MiddleCenter;step.Add(label);pathbar.Add(step);
            }
            main.Add(pathbar);

            room=Box("room",Cream);room.style.flexGrow=1;room.style.flexShrink=1;room.style.minHeight=140;
            room.style.overflow=Overflow.Hidden;room.RegisterCallback<GeometryChangedEvent>(_=>SizeRoomStage());main.Add(room);
            roomLabel=Text("9m²",15,Forest);roomLabel.name="room-area";roomLabel.style.position=Position.Absolute;
            roomLabel.style.right=0;roomLabel.style.top=3;roomLabel.style.width=44;roomLabel.style.height=23;
            roomLabel.style.backgroundColor=Sand;roomLabel.style.unityTextAlign=TextAnchor.MiddleCenter;room.Add(roomLabel);
            menuButton=Action("MENU",ShowSettings,Forest);menuButton.name="menu";menuButton.style.fontSize=10;
            menuButton.style.position=Position.Absolute;menuButton.style.right=0;menuButton.style.top=29;menuButton.style.width=44;room.Add(menuButton);
            previous=Action("<",()=>{page=Mathf.Max(0,page-1);Refresh();},Forest);previous.name="previous-upgrades";
            next=Action(">",()=>{page=Mathf.Min(3,page+1);Refresh();},Forest);next.name="next-upgrades";
            foreach(var arrow in new[]{previous,next}){arrow.style.position=Position.Absolute;arrow.style.bottom=2;arrow.style.width=44;}
            previous.style.left=0;next.style.right=0;room.Add(previous);room.Add(next);
            pageLabel=Text("",10,Forest);pageLabel.style.position=Position.Absolute;pageLabel.style.right=49;pageLabel.style.bottom=3;
            room.Add(pageLabel);

            upgradeRows=Frame("upgrades",PixelUi.Finish.Cream);upgradeRows.style.flexShrink=0;
            upgradeRows.style.paddingLeft=5;upgradeRows.style.paddingRight=5;upgradeRows.style.paddingTop=5;
            upgradeRows.style.marginBottom=4;main.Add(upgradeRows);
            grid=Frame("reels",PixelUi.Finish.Forest);grid.style.flexShrink=0;grid.style.paddingLeft=5;grid.style.paddingRight=5;
            grid.style.paddingTop=5;grid.style.paddingBottom=5;grid.style.marginBottom=5;
            for(int y=0;y<3;y++){
                var line=new VisualElement();Row(line);line.style.flexGrow=1;line.style.flexBasis=0;
                for(int x=0;x<5;x++){
                    var cell=Frame("cell-"+(y*5+x),PixelUi.Finish.Slot);cell.style.flexGrow=1;cell.style.flexBasis=0;
                    cell.style.height=Length.Percent(100);cell.style.marginLeft=1;cell.style.marginRight=1;cell.style.marginTop=1;cell.style.marginBottom=1;
                    cell.style.paddingLeft=4;cell.style.paddingRight=4;cell.style.paddingTop=4;cell.style.paddingBottom=4;
                    var image=new Image {scaleMode=ScaleMode.ScaleToFit,name="symbol",pickingMode=PickingMode.Ignore};
                    image.style.width=Length.Percent(100);image.style.height=Length.Percent(100);cells[y*5+x]=image;cell.Add(image);line.Add(cell);
                }grid.Add(line);
            }main.Add(grid);

            spinRow=new VisualElement {name="spin-row"};Row(spinRow);spinRow.style.flexShrink=0;
            var bet=Frame("bet-panel",PixelUi.Finish.Forest);bet.style.width=108;bet.style.height=Length.Percent(100);bet.style.marginRight=7;
            var betTitle=Text("BET",16,Cream);betTitle.style.unityTextAlign=TextAnchor.MiddleCenter;betTitle.style.height=22;bet.Add(betTitle);
            var value=new VisualElement();Row(value);value.style.justifyContent=Justify.Center;value.style.height=30;
            value.Add(Icon(reelTextures[0],30));betLabel=Text("5",24,Cream);betLabel.style.marginLeft=5;value.Add(betLabel);bet.Add(value);
            freeLabel=Text("",11,PixelUi.Hex(0xffce76));freeLabel.style.unityTextAlign=TextAnchor.MiddleCenter;freeLabel.style.flexGrow=1;bet.Add(freeLabel);spinRow.Add(bet);
            var actionArea=new VisualElement();actionArea.style.flexGrow=1;actionArea.style.height=Length.Percent(100);
            spinButton=Action("SPIN",BeginSpin,Rust);spinButton.name="spin";spinButton.style.height=Length.Percent(100);spinButton.style.fontSize=45;
            recoveryButton=Action("FREE SPIN",()=>{Recover();BeginSpin();},Rust);recoveryButton.name="recovery";recoveryButton.style.height=Length.Percent(100);recoveryButton.style.fontSize=27;
            actionArea.Add(spinButton);actionArea.Add(recoveryButton);spinRow.Add(actionArea);main.Add(spinRow);

            toast=Frame("toast",PixelUi.Finish.Forest);toast.style.position=Position.Absolute;toast.style.left=16;toast.style.right=16;
            toast.style.top=Length.Percent(32);toast.style.paddingLeft=8;toast.style.paddingRight=8;toast.style.height=32;toast.pickingMode=PickingMode.Ignore;
            statusLabel=Text("",13,Cream);statusLabel.style.unityTextAlign=TextAnchor.MiddleCenter;toast.Add(statusLabel);root.Add(toast);toast.style.display=DisplayStyle.None;
            settings=Modal("settings");settings.Add(Text("MY ROOM",23,PixelUi.Hex(0xffc472)));
            saveLabel=Text("Saved locally",13,Cream);settings.Add(saveLabel);
            settings.Add(Text("Match item fragments and collect cash. BUY uses both.\n\nBET is 5. Each spin gives 12 base cash, plus cash icons and matching reels. Free starter and recovery spins need no payment.",14,Cream));
            var reset=Action("RESET ROOM",ShowReset,Rust);reset.name="reset";reset.style.marginTop=14;settings.Add(reset);
            var close=Action("BACK TO ROOM",HideSettings,Forest);close.name="close-settings";close.style.marginTop=8;settings.Add(close);
            settings.style.display=DisplayStyle.None;root.Add(settings);
            confirmation=Modal("reset-confirmation");confirmation.Add(Text("RESET THIS ROOM?",22,PixelUi.Hex(0xffc472)));
            confirmation.Add(Text("Cash, fragments and purchases will be cleared. You start with 3 free spins.",14,Cream));
            var controls=new VisualElement();Row(controls);controls.style.marginTop=14;
            var cancel=Action("KEEP ROOM",HideReset,Forest);cancel.style.flexGrow=1;cancel.style.marginRight=8;
            var confirm=Action("RESET",ResetConfirmed,Rust);confirm.name="confirm-reset";confirm.style.flexGrow=1;
            controls.Add(cancel);controls.Add(confirm);confirmation.Add(controls);root.Add(confirmation);confirmation.style.display=DisplayStyle.None;
            root.RegisterCallback<GeometryChangedEvent>(_=>ApplySafeArea());
        }
        VisualElement Modal(string name)
        {
            var modal=Frame(name,PixelUi.Finish.Forest);modal.style.position=Position.Absolute;
            modal.style.left=16;modal.style.right=16;modal.style.top=Length.Percent(26);
            modal.style.paddingLeft=15;modal.style.paddingRight=15;modal.style.paddingTop=15;modal.style.paddingBottom=15;return modal;
        }
        void ShowSettings(){if(spinning)return;HideToast();main.SetEnabled(false);settings.style.display=DisplayStyle.Flex;settings.BringToFront();}
        void HideSettings(){settings.style.display=DisplayStyle.None;main.SetEnabled(true);}
        void ShowToast(string text){statusLabel.text=text;toastUntil=Time.unscaledTime+3;toast.style.display=DisplayStyle.Flex;toast.BringToFront();}
        void HideToast(){toast.style.display=DisplayStyle.None;}
        void ApplySafeArea()
        {
            float width=root.contentRect.width;if(width<=0)return;
            float scale=width/Mathf.Max(1,Screen.width);Rect safe=Screen.safeArea;
            float top=Mathf.Max(6,(Screen.height-safe.yMax)*scale),bottom=Mathf.Max(6,safe.yMin*scale);
            main.style.paddingTop=top;main.style.paddingBottom=bottom;
            float height=Mathf.Max(560,root.contentRect.height-top-bottom);
            header.style.height=46;pathbar.style.height=32;
            grid.style.height=Mathf.Clamp(height*0.265f,162,204);
            spinRow.style.height=Mathf.Clamp(height*0.13f,78,96);
            upgradeRows.style.height=model!=null&&model.Complete?124:104;
            SizeRoomStage();
        }

''' + s[end:]
# room.Clear must preserve independent overlay controls.
s=s.replace('            room.Clear();','            if (roomStage != null) roomStage.RemoveFromHierarchy();')
s=s.replace('            room.Add(roomStage); SizeRoomStage();','            room.Insert(0,roomStage); SizeRoomStage();')
old='            roomLabel.text = model.Complete ? "9m² · LEVEL 1 COMPLETE · A modest home" : "9m² · " + model.PurchasedCount + "/8 upgrades · Same room throughout";'
assert old in s
s=s.replace(old,'            roomLabel.text = "9m²";')
s=s.replace('            if (wanderer == null || model == null || heroine == null) return;','            if (toast != null && Time.unscaledTime > toastUntil) HideToast();\n            if (wanderer == null || model == null || heroine == null) return;')
start=s.index('        void Refresh()');end=s.index('        void SetCell(',start)
s=s[:start]+'''        void Refresh()
        {
            PlayerState s=model.Snapshot;
            cashLabel.text=s.cash.ToString();cashLabel.style.fontSize=s.cash.ToString().Length>6?16:26;
            saveLabel.text=saveNotice;betLabel.text=s.freeSpins>0||model.CanRecover?"0":"5";
            freeLabel.text=s.freeSpins>0?"FREE SPINS "+s.freeSpins:model.CanRecover?"FREE RECOVERY":"FIXED BET";
            spinButton.SetEnabled(!spinning&&model.CanSpin);spinButton.text=spinning?"...":"SPIN";
            spinButton.style.display=model.CanRecover?DisplayStyle.None:DisplayStyle.Flex;
            recoveryButton.style.display=model.CanRecover?DisplayStyle.Flex:DisplayStyle.None;recoveryButton.SetEnabled(!spinning);
            previous.SetEnabled(page>0);next.SetEnabled(page<3);pageLabel.text=(page*2+1)+"-"+(page*2+2)+" / 8";
            int[] marks={0,1000,10000,100000,1000000};
            for(int i=0;i<5;i++)pathbar.Q<VisualElement>("milestone-dot-"+i).style.backgroundColor=s.cash>=marks[i]?PixelUi.Hex(0xffb13b):Cream;
            DrawRoom(s);upgradeRows.Clear();
            string[] titles={"FLOOR","SURFACES","WALLPAPER","DOOR","WINDOW","MATTRESS","LAMP","STOOL"};
            for(int rowIndex=0;rowIndex<2;rowIndex++){
                int index=page*2+rowIndex;var d=Economy.Upgrades[index];bool owned=s.purchased[index],ready=model.CanBuy(index);
                var row=new VisualElement {name="upgrade-"+index};Row(row);row.style.height=47;row.style.paddingRight=1;
                if(rowIndex==0){row.style.borderBottomWidth=1;row.style.borderBottomColor=Sand;}
                string ownedIcon=index==3?"door-owned":index==4?"window-owned":index==5&&model.Complete?"mattress-owned":symbolNames[index+1];
                var well=Box("item-well-"+index,PixelUi.Hex(0xc9a77c));well.style.width=52;well.style.height=41;well.style.marginRight=5;
                var itemIcon=Icon(owned?Resources.Load<Texture2D>("Art/item-symbols/"+ownedIcon):reelTextures[index+1],43);well.Add(itemIcon);row.Add(well);
                var info=new VisualElement();info.style.flexGrow=1;info.style.flexBasis=0;info.style.marginRight=5;
                var label=Text(titles[index],14,Forest);label.style.height=20;info.Add(label);
                var progressLine=new VisualElement();Row(progressLine);progressLine.style.height=20;
                var progress=new VisualElement {name="progress-track-"+index};Row(progress);progress.style.flexGrow=1;progress.style.height=11;
                for(int block=0;block<d.FragmentGoal;block++){
                    var segment=Box("fragment-"+index+"-"+block,owned||s.fragments[index]>block?PixelUi.Hex(0xf6ad38):Sand);
                    segment.style.height=10;segment.style.flexGrow=1;segment.style.flexBasis=0;segment.style.marginRight=1;progress.Add(segment);
                }
                progressLine.Add(progress);var counter=Text((owned?d.FragmentGoal:s.fragments[index])+"/"+d.FragmentGoal,12,Forest);counter.name="fragment-count-"+index;
                counter.style.width=37;counter.style.unityTextAlign=TextAnchor.MiddleRight;progressLine.Add(counter);info.Add(progressLine);row.Add(info);
                var price=Frame("price-"+index,PixelUi.Finish.Forest);Row(price);price.style.width=61;price.style.height=36;price.style.marginRight=4;price.style.paddingLeft=3;
                price.Add(Icon(reelTextures[0],23));var priceText=Text(owned?"PAID":d.CashPrice.ToString(),owned?12:19,Cream);priceText.name="cash-price-"+index;priceText.style.flexGrow=1;priceText.style.unityTextAlign=TextAnchor.MiddleCenter;price.Add(priceText);row.Add(price);
                var buy=new Button(()=>Buy(index)){text=owned?"OWNED":ready?"BUY":"LOCKED",name="buy-"+index};
                buy.style.width=66;buy.style.height=44;buy.style.fontSize=14;buy.style.unityFont=pixelFont;buy.style.unityFontStyleAndWeight=FontStyle.Bold;buy.style.color=Cream;
                buy.style.marginTop=buy.style.marginBottom=buy.style.marginLeft=buy.style.marginRight=0;buy.style.paddingLeft=buy.style.paddingRight=2;
                PixelUi.Skin(buy,owned?PixelUi.Finish.Owned:ready?PixelUi.Finish.Rust:PixelUi.Finish.Grey);
                buy.SetEnabled(!spinning&&ready);row.Add(buy);upgradeRows.Add(row);
            }
            if(model.Complete){completion=Frame("completion",PixelUi.Finish.Forest);completion.style.height=21;
                var completeText=Text("LEVEL 1 COMPLETE",16,PixelUi.Hex(0xffbf5c));completeText.style.unityTextAlign=TextAnchor.MiddleCenter;completion.Add(completeText);upgradeRows.Add(completion);}
            int[] pool=model.ActivePool;for(int i=0;i<cells.Length;i++)SetCell(i,s.spins>0?s.lastGrid[i]:pool[i%pool.Length]);
            ApplySafeArea();
        }
''' + s[end:]
s=s.replace('                statusLabel.text = "+" + result.Payout + " CASH · cost " + result.Cost + " · " + result.Triples + " matching reels. Fragments collected.";','                int collected=0;foreach(int value in result.Fragments)collected+=value;\n                ShowToast("+"+result.Payout+" CASH  ·  "+collected+" FRAGMENTS");')
# SpinResult field may differ; validate against actual declaration before running.
s=s.replace('            Persist(); statusLabel.text = Economy.Upgrades[index].Title + " purchased. Room updated.";\n            if (model.Complete) statusLabel.text = "LEVEL 1 COMPLETE · Your same 9m² room is now a modest home.";','            Persist(); ShowToast(model.Complete?"LEVEL 1 COMPLETE":Economy.Upgrades[index].Title.ToUpperInvariant()+" BOUGHT");\n            page = model.Complete ? 3 : model.PurchasedCount / 2;')
s=s.replace('statusLabel.text = "One free recovery spin. No ads or purchases.";','ShowToast("FREE RECOVERY SPIN");')
s=s.replace('void ShowReset() { if (spinning) return; main.SetEnabled(false); confirmation.style.display = DisplayStyle.Flex; }','void ShowReset() { if (spinning) return; settings.style.display=DisplayStyle.None; main.SetEnabled(false); confirmation.style.display = DisplayStyle.Flex; confirmation.BringToFront(); }')
s=s.replace('            statusLabel.text = "Start again: spin, collect fragments, then BUY."; HideReset(); Refresh();','            HideReset(); Refresh(); ShowToast("3 FREE SPINS · A FRESH START");')
p.write_text(s,encoding='utf-8')
print('Applied live UI v0.3; economy, navigation and save code unchanged')
