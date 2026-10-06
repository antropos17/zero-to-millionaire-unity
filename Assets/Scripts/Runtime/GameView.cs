using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    public sealed class UnityStateCodec : IStateCodec
    {
        public string Encode(PlayerState state) { return JsonUtility.ToJson(state, true); }
        public PlayerState Decode(string text) { return JsonUtility.FromJson<PlayerState>(text); }
    }

    public sealed class GameView : MonoBehaviour
    {
        private static readonly Color Cream = new Color32(249, 238, 218, 255);
        private static readonly Color Forest = new Color32(20, 44, 40, 255);
        private static readonly Color Rust = new Color32(186, 75, 45, 255);
        private static readonly Color Sand = new Color32(202, 169, 125, 255);
        private GameModel model;
        private SaveStore saves;
        private IRandomSource random;
        private VisualElement root, room, upgradeRows, confirmation, main;
        private VisualElement roomStage;
        private Label cashLabel, statusLabel, roomLabel, pageLabel, saveLabel;
        private Label roomProgress, rewardAmount;
        private VisualElement roomTicks, rewardFly;
        private Image rewardIcon;
        private int rewardQuest=-1,rewardParts;
        private float rewardStarted;
        private Vector2 rewardStart;
        private bool rewardPending;
        private Button spinButton, recoveryButton, previous, next;
        private readonly Image[] cells = new Image[15];
        private readonly Texture2D[] reelTextures = new Texture2D[Economy.ReelSymbolCount];
        private readonly string[] symbolNames = { "cash", "floorpatch", "wallrepair", "wallpaper", "door", "window", "mattress", "lamp", "stool" };
        private RoomWanderer wanderer;
        private HeroineVisual heroine;
        private int drawnRoom = -1;
        private readonly List<RoomLayer> roomLayers = new List<RoomLayer>();
        private int lastDepthBand = -1;
        private sealed class RoomLayer
        { public VisualElement Element; public float Depth; }
        private int page;
        private VisualElement header, pathbar, grid, spinRow, settings, toast, completion;
        private Label betLabel, freeLabel;
        private Button menuButton;
        private static Font pixelFont;
        private float toastUntil;
        private bool spinning;
        private string saveNotice = "Saved locally";
        private float spinReadyAt;

        public GameModel Model { get { return model; } }

        void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            random = new SeededRandom(unchecked(Environment.TickCount));
            string saveDirectory = Application.persistentDataPath;
#if UNITY_EDITOR
            string qaDirectory = Environment.GetEnvironmentVariable("ZTM_QA_SAVE_DIR");
            if (!string.IsNullOrEmpty(qaDirectory)) saveDirectory = qaDirectory;
#endif
            saves = new SaveStore(Path.Combine(saveDirectory, "level1-v1.json"), new UnityStateCodec());
            string notice;
            model = new GameModel(saves.Load(out notice));
            wanderer = new RoomWanderer(new FloorPoint(model.Snapshot.heroineU, model.Snapshot.heroineV), new SeededRandom(unchecked(Environment.TickCount + 17)));
            if (!string.IsNullOrEmpty(notice)) saveNotice = notice;
            var panel = ScriptableObject.CreateInstance<PanelSettings>();
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(360, 640);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 0;
            panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("GameTheme");
            var doc = gameObject.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            root = doc.rootVisualElement;
            page = model.Complete ? 3 : model.PurchasedCount / 2;
            BuildUI();
            Refresh();
        }

        static VisualElement Box(string name, Color color)
        {
            var v = new VisualElement { name = name };
            v.style.backgroundColor = color;
            return v;
        }
        static Label Text(string text, int size, Color color)
        {
            var label = new Label(text);
            label.style.fontSize = size; label.style.color = color;
            if (pixelFont == null) pixelFont = Resources.Load<Font>("Fonts/PixelifySans");
            label.style.unityFont = pixelFont;
            label.style.unityFontDefinition = FontDefinition.FromFont(pixelFont);
            label.style.paddingTop=label.style.paddingBottom=label.style.marginTop=label.style.marginBottom=0;
            label.style.paddingLeft=label.style.paddingRight=label.style.marginLeft=label.style.marginRight=0;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.whiteSpace = WhiteSpace.Normal;
            return label;
        }
        static Button Action(string text, Action action, Color color)
        {
            var button = new Button(action);
            button.style.height = 44; button.style.fontSize = 16;
            button.style.unityFont = Resources.Load<Font>("Fonts/PixelifySans");
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.color = Cream; button.style.whiteSpace = WhiteSpace.NoWrap;
            button.style.marginLeft = button.style.marginRight = button.style.marginTop = button.style.marginBottom = 0;
            button.style.paddingLeft = button.style.paddingRight = 4;
            PixelUi.Skin(button, color == Rust ? PixelUi.Finish.Rust : color==Color.gray?PixelUi.Finish.Grey:PixelUi.Finish.Forest);
            var caption=Text(text,16,Cream);caption.name="button-caption";caption.pickingMode=PickingMode.Ignore;
            caption.style.unityTextAlign=TextAnchor.MiddleCenter;caption.style.flexGrow=1;caption.style.width=Length.Percent(100);button.Add(caption);
            return button;
        }
        static void ButtonText(Button button,string value,int size)
        {button.text="";var label=button.Q<Label>("button-caption");if(label!=null){label.text=value;label.style.fontSize=size;}}
        static void Border(VisualElement v)
        {
            v.style.borderLeftWidth = 2; v.style.borderRightWidth = 2;
            v.style.borderTopWidth = 2; v.style.borderBottomWidth = 2;
            v.style.borderLeftColor = Forest; v.style.borderRightColor = Forest;
            v.style.borderTopColor = Forest; v.style.borderBottomColor = Forest;
        }
        static void Row(VisualElement v) { v.style.flexDirection = FlexDirection.Row; v.style.alignItems = Align.Center; }

        static Image Icon(Texture2D texture, float size)
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
            roomProgress=Text("ROOM 0/8 DONE",12,Forest);roomProgress.name="room-progress";
            roomProgress.style.position=Position.Absolute;roomProgress.style.left=0;roomProgress.style.top=3;
            roomProgress.style.height=20;roomProgress.style.width=128;roomProgress.style.backgroundColor=Cream;room.Add(roomProgress);
            roomTicks=new VisualElement{name="room-task-ticks"};Row(roomTicks);roomTicks.style.position=Position.Absolute;
            roomTicks.style.left=0;roomTicks.style.top=24;roomTicks.style.width=120;roomTicks.style.height=11;room.Add(roomTicks);

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
            rewardFly=Frame("reward-to-task",PixelUi.Finish.Forest);Row(rewardFly);rewardFly.style.position=Position.Absolute;
            rewardFly.style.width=68;rewardFly.style.height=32;rewardFly.pickingMode=PickingMode.Ignore;
            rewardIcon=Icon(reelTextures[1],28);rewardAmount=Text("",16,Cream);rewardFly.Add(rewardIcon);rewardFly.Add(rewardAmount);
            rewardFly.style.display=DisplayStyle.None;root.Add(rewardFly);
            settings=Modal("settings");settings.Add(Text("MY ROOM",23,PixelUi.Hex(0xffc472)));
            saveLabel=Text("Saved locally",13,Cream);settings.Add(saveLabel);
            settings.Add(Text("Finish 8 tasks in this same room. Only the current task earns parts: 1 matching icon = 1 part. When parts are full, reels earn cash. BUY & INSTALL uses both.\n\nBET is 5. Spins give 12 base cash plus cash icons and triples. Starter and recovery spins are free.",14,Cream));
            var reset=Action("RESET ROOM",ShowReset,Rust);reset.name="reset";reset.style.marginTop=14;settings.Add(reset);
            var close=Action("BACK TO ROOM",HideSettings,Forest);close.name="close-settings";close.style.marginTop=8;settings.Add(close);
            settings.style.display=DisplayStyle.None;root.Add(settings);
            confirmation=Modal("reset-confirmation");confirmation.Add(Text("RESET THIS ROOM?",22,PixelUi.Hex(0xffc472)));
            confirmation.Add(Text("Cash, fragments and purchases will be cleared. You start with 3 free spins.",14,Cream));
            var controls=new VisualElement();Row(controls);controls.style.marginTop=14;
            var cancel=Action("KEEP ROOM",HideReset,Forest);cancel.name="keep-room";cancel.style.flexGrow=1;cancel.style.marginRight=8;
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
            // Short portrait keeps a large whole room; tall portrait restores
            // the reference's generous reel and SPIN proportions.
            float tall=Mathf.Clamp01((height-628)/160);
            header.style.height=46;pathbar.style.height=Mathf.Lerp(28,32,tall);
            grid.style.height=Mathf.Lerp(132,204,tall);
            spinRow.style.height=Mathf.Lerp(68,96,tall);
            upgradeRows.style.height=116;
            SizeRoomStage();
        }

        void DrawRoom(PlayerState s)
        {
            int signature=0;for(int i=0;i<s.purchased.Length;i++)if(s.purchased[i])signature|=1<<i;
            if (signature == drawnRoom && roomStage != null) return;
            drawnRoom = signature; lastDepthBand = -1; roomLayers.Clear();
            if (roomStage != null) roomStage.RemoveFromHierarchy();
            roomStage = new VisualElement { name = "room-stage" };
            roomStage.style.position = Position.Absolute; roomStage.pickingMode = PickingMode.Ignore;
            room.Insert(0,roomStage); SizeRoomStage();
            string background = s.purchased[2] ? "wallpaper" : s.purchased[1] ? "surfaces" : s.purchased[0] ? "patches" : "ruined";
            Texture2D roomTexture = Resources.Load<Texture2D>("Art/room/" + background);
            if (roomTexture != null)
            {
                var image = new Image { image = roomTexture, scaleMode = ScaleMode.ScaleToFit };
                image.style.width = Length.Percent(100); image.style.height = Length.Percent(100);
                roomStage.Add(image);
                if (s.purchased[3]) AddRoomOverlay("door-overlay");
                if (s.purchased[4]) AddRoomOverlay("window-overlay");
                // Floor footprints and render anchors share one metre-based layout.
                if (s.purchased[5]) AddFurniture(model.Complete ? "mattress-dressed" : "mattress", new FloorPoint(0.47f, 1.82f), 164, 0.98f);
                if (s.purchased[6]) AddFurniture("lamp", new FloorPoint(0.42f, 0.50f), 38, 0.98f);
                if (s.purchased[7]) AddFurniture("stool", new FloorPoint(1.23f, 2.45f), 45, 0.97f);
                heroine = new HeroineVisual(); roomStage.Add(heroine.Root);
                UpdateHeroine();
            }
            else
            {
                // A clearly labeled wireframe only until generated art arrives.
                var placeholder = Text("ART PENDING\n\nFIXED 9m² ROOM\n" + background.ToUpperInvariant() + "\n\n" + (s.purchased[5] ? "MATTRESS  " : "") + (s.purchased[6] ? "LAMP  " : "") + (s.purchased[7] ? "STOOL" : "EMPTY"), 19, Forest);
                placeholder.style.unityTextAlign = TextAnchor.MiddleCenter;
                placeholder.style.height = Length.Percent(100); Border(placeholder); roomStage.Add(placeholder);
            }
            roomLabel.text = "9m²";
        }
        void AddFurniture(string filename, FloorPoint feet, float width, float pivotY)
        {
            Texture2D texture = Resources.Load<Texture2D>("Art/objects/" + filename);
            if (texture == null) return;
            float height = width * texture.height / texture.width;
            float floorFront = RoomGeometry.PixelY(feet) + (filename.StartsWith("mattress") ? 37 : 0);
            var image = new Image { image = texture, scaleMode = ScaleMode.StretchToFill, pickingMode = PickingMode.Ignore };
            image.style.position = Position.Absolute;
            image.style.left = Length.Percent((RoomGeometry.PixelX(feet) - width / 2) / 512 * 100);
            image.style.top = Length.Percent((floorFront - height * pivotY) / 512 * 100);
            image.style.width = Length.Percent(width / 512 * 100); image.style.height = Length.Percent(height / 512 * 100);
            roomStage.Add(image); roomLayers.Add(new RoomLayer { Element = image, Depth = RoomGeometry.PixelY(feet) + (filename.StartsWith("mattress") ? 37 : 5) });
        }
        void Update()
        {
            if (toast != null && Time.unscaledTime > toastUntil) HideToast();
            UpdateRewardFlight();
            if (wanderer == null || model == null || heroine == null) return;
            wanderer.Step(Time.unscaledDeltaTime);
            model.SetHeroinePosition(wanderer.Position);
            UpdateHeroine();
        }
        void UpdateHeroine()
        {
            if (heroine == null || wanderer == null) return;
            FloorPoint feet = wanderer.Position;
            heroine.SetPose(feet, wanderer.Delta, wanderer.Walking, wanderer.Travelled);
            float depth = RoomGeometry.PixelY(feet);
            int band = 0; foreach (var layer in roomLayers) if (layer.Depth < depth) band++;
            if (lastDepthBand == band) return;
            lastDepthBand = band;
            roomLayers.Sort((a, b) => a.Depth.CompareTo(b.Depth));
            bool added = false;
            foreach (var layer in roomLayers)
            {
                if (!added && depth < layer.Depth) { heroine.Root.BringToFront(); added = true; }
                layer.Element.BringToFront();
            }
            if (!added) heroine.Root.BringToFront();
        }
        void SizeRoomStage()
        {
            if (roomStage == null) return;
            // Fit the opaque room bounds, omitting transparent canvas padding.
            float side = Mathf.Min(room.contentRect.width * 512f / 457f, room.contentRect.height * 512f / 460f);
            var panelSettings=GetComponent<UIDocument>().panelSettings;
            float pixelsPerUnit=(panelSettings.targetTexture!=null?panelSettings.targetTexture.width:Screen.width)/Mathf.Max(1,root.contentRect.width);
            side=Mathf.Floor(side*pixelsPerUnit)/pixelsPerUnit;
            roomStage.style.width = side; roomStage.style.height = side;
            roomStage.style.left = Mathf.Round((room.contentRect.width-side)/2*pixelsPerUnit)/pixelsPerUnit;
            roomStage.style.top = Mathf.Round(((room.contentRect.height-side*460f/512f)/2-side*30f/512f)*pixelsPerUnit)/pixelsPerUnit;
        }
        void AddRoomOverlay(string name)
        {
            var texture = Resources.Load<Texture2D>("Art/room/" + name);
            if (texture == null) return;
            var image = new Image { image = texture, scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
            image.style.position = Position.Absolute; image.style.left = 0; image.style.top = 0;
            image.style.width = Length.Percent(100); image.style.height = Length.Percent(100); roomStage.Add(image);
        }
        void AddLayer(string filename, float x, float y, float w, float h)
        {
            var texture = Resources.Load<Texture2D>("Art/objects/" + filename);
            if (texture == null) return;
            var image = new Image { image = texture, scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
            image.style.position = Position.Absolute; image.style.left = Length.Percent(x); image.style.top = Length.Percent(y);
            image.style.width = Length.Percent(w); image.style.height = Length.Percent(h); roomStage.Add(image);
        }

        void Refresh()
        {
            PlayerState s=model.Snapshot;
            cashLabel.text=s.cash.ToString();cashLabel.style.fontSize=s.cash.ToString().Length>6?16:26;
            saveLabel.text=saveNotice;betLabel.text=s.freeSpins>0||model.CanRecover?"0":"5";
            freeLabel.text=model.Complete?"ALL TASKS DONE":model.ActiveQuest>=0&&model.CanBuy(model.ActiveQuest)?"INSTALL TASK":s.freeSpins>0?"FREE SPINS "+s.freeSpins:model.CanRecover?"FREE RECOVERY":"FIXED BET";
            if(model.Complete)betLabel.text="0";
            spinButton.SetEnabled(!spinning&&model.CanSpin);ButtonText(spinButton,model.Complete?"COMPLETE":spinning?"...":"SPIN",model.Complete?26:45);
            ButtonText(menuButton,"MENU",10);ButtonText(recoveryButton,"FREE SPIN",24);
            spinButton.style.display=model.CanRecover?DisplayStyle.None:DisplayStyle.Flex;
            recoveryButton.style.display=model.CanRecover?DisplayStyle.Flex:DisplayStyle.None;recoveryButton.SetEnabled(!spinning);
            roomProgress.text="ROOM "+model.PurchasedCount+"/8 DONE";roomTicks.Clear();
            for(int i=0;i<8;i++){
                var tick=Box("room-task-"+i,s.purchased[i]?PixelUi.Hex(0xf6ad38):Sand);tick.style.flexGrow=1;tick.style.height=10;tick.style.marginRight=2;
                if(s.purchased[i])tick.generateVisualContent+=ctx=>{var p=ctx.painter2D;p.strokeColor=Forest;p.lineWidth=1.7f;p.BeginPath();p.MoveTo(new Vector2(3,5));p.LineTo(new Vector2(6,8));p.LineTo(new Vector2(10,2));p.Stroke();};
                roomTicks.Add(tick);
            }
            int[] marks={0,1000,10000,100000,1000000};
            for(int i=0;i<5;i++)pathbar.Q<VisualElement>("milestone-dot-"+i).style.backgroundColor=s.cash>=marks[i]?PixelUi.Hex(0xffb13b):Cream;
            DrawRoom(s);DrawQuest(s);
            int[] pool=model.ActivePool;for(int i=0;i<cells.Length;i++)SetCell(i,s.spins>0?s.lastGrid[i]:pool[i%pool.Length]);
            ApplySafeArea();
        }
        void DrawQuest(PlayerState s)
        {
            upgradeRows.Clear();int index=model.ActiveQuest;
            if(index<0){
                var done=Text("8/8 TASKS DONE",14,Forest);done.style.height=21;done.style.unityTextAlign=TextAnchor.MiddleCenter;upgradeRows.Add(done);
                completion=Frame("completion",PixelUi.Finish.Forest);completion.style.height=44;
                var title=Text("ROOM COMPLETE",23,PixelUi.Hex(0xffbf5c));title.style.flexGrow=1;title.style.unityTextAlign=TextAnchor.MiddleCenter;completion.Add(title);upgradeRows.Add(completion);
                var end=Text("A small room of your own.",13,Forest);end.style.height=21;end.style.unityTextAlign=TextAnchor.MiddleCenter;upgradeRows.Add(end);
                var nextDone=Text("NO MORE TASKS",11,Forest);nextDone.name="next-quest";nextDone.style.unityTextAlign=TextAnchor.MiddleCenter;upgradeRows.Add(nextDone);return;
            }
            var d=Economy.Upgrades[index];bool partsFull=s.fragments[index]>=d.FragmentGoal,cashFull=s.cash>=d.CashPrice,ready=model.CanBuy(index);
            var row=new VisualElement{name="active-quest"};Row(row);row.style.height=69;row.style.flexShrink=0;
            var well=Box("quest-item-well",PixelUi.Hex(0xc9a77c));well.style.width=48;well.style.height=55;well.style.marginRight=5;
            well.Add(Icon(reelTextures[index+1],47));row.Add(well);
            var info=new VisualElement();info.style.flexGrow=1;info.style.flexBasis=0;info.style.marginRight=4;
            string need=ready?"READY TO INSTALL":partsFull?"NEED CASH":cashFull?"COLLECT PARTS":"PARTS + CASH";
            var eyebrow=Text("TASK "+(index+1)+"/8  -  "+need,9,Forest);eyebrow.name="quest-status";eyebrow.style.height=12;eyebrow.style.whiteSpace=WhiteSpace.NoWrap;info.Add(eyebrow);
            var titleLabel=Text(d.ActionTitle.ToUpperInvariant(),14,Forest);titleLabel.name="quest-title";titleLabel.style.height=19;titleLabel.style.whiteSpace=WhiteSpace.NoWrap;info.Add(titleLabel);
            var parts=new VisualElement();Row(parts);parts.style.height=16;
            var partsTrack=new VisualElement{name="progress-track-"+index};Row(partsTrack);partsTrack.style.flexGrow=1;partsTrack.style.height=10;
            for(int block=0;block<d.FragmentGoal;block++){
                var segment=Box("fragment-"+index+"-"+block,s.fragments[index]>block?(partsFull?PixelUi.Hex(0x477650):PixelUi.Hex(0xf6ad38)):Sand);
                segment.style.height=9;segment.style.flexGrow=1;segment.style.flexBasis=0;segment.style.marginRight=1;partsTrack.Add(segment);
            }
            parts.Add(partsTrack);var count=Text(Mathf.Min(s.fragments[index],d.FragmentGoal)+"/"+d.FragmentGoal,11,Forest);count.name="fragment-count-"+index;
            count.style.width=38;count.style.unityTextAlign=TextAnchor.MiddleRight;parts.Add(count);info.Add(parts);
            var cash=new VisualElement();Row(cash);cash.style.height=17;cash.Add(Icon(reelTextures[0],17));
            var cashTrack=Box("cash-progress-track",Sand);cashTrack.style.height=7;cashTrack.style.flexGrow=1;cashTrack.style.marginLeft=2;
            var cashFill=Box("cash-progress-fill",cashFull?PixelUi.Hex(0x477650):PixelUi.Hex(0xf6ad38));cashFill.style.width=Length.Percent(Mathf.Clamp01((float)s.cash/d.CashPrice)*100);cashFill.style.height=Length.Percent(100);cashTrack.Add(cashFill);cash.Add(cashTrack);
            var cashCount=Text(s.cash+"/"+d.CashPrice,11,Forest);cashCount.name="cash-count";cashCount.style.width=64;cashCount.style.unityTextAlign=TextAnchor.MiddleRight;cash.Add(cashCount);info.Add(cash);row.Add(info);
            var buy=Action("BUY &\nINSTALL",()=>Buy(index),ready?Rust:Color.gray);buy.name="buy-"+index;
            buy.style.width=77;buy.style.height=48;buy.style.flexShrink=0;buy.style.paddingLeft=buy.style.paddingRight=2;
            var caption=buy.Q<Label>("button-caption");caption.style.fontSize=12;caption.style.whiteSpace=WhiteSpace.Normal;
            buy.SetEnabled(!spinning&&ready);row.Add(buy);upgradeRows.Add(row);
            int nextIndex=model.NextQuest;var preview=new VisualElement{name="next-quest"};Row(preview);preview.style.height=22;
            preview.style.borderTopWidth=1;preview.style.borderTopColor=Sand;
            if(nextIndex>=0){preview.Add(Icon(reelTextures[nextIndex+1],21));var nextTitle=Text("NEXT: "+Economy.Upgrades[nextIndex].ActionTitle.ToUpperInvariant(),11,Forest);nextTitle.style.flexGrow=1;preview.Add(nextTitle);preview.Add(Text("LOCKED",9,Forest));}
            else{var final=Text("FINAL TASK - FINISH YOUR ROOM",11,Forest);preview.Add(final);}
            upgradeRows.Add(preview);
            var legend=Text("1 ITEM ICON = 1 PART   BUY USES PARTS + CASH",9,Forest);legend.name="quest-legend";legend.style.height=13;legend.style.whiteSpace=WhiteSpace.NoWrap;upgradeRows.Add(legend);
        }
        void StartRewardFlight(SpinResult result)
        {
            rewardQuest=-1;for(int i=0;i<result.Fragments.Length;i++)if(result.Fragments[i]>0){rewardQuest=i;rewardParts=result.Fragments[i];break;}
            if(rewardQuest<0)return;
            for(int i=0;i<result.Grid.Length;i++)if(result.Grid[i]==rewardQuest+1){rewardStart=cells[i].worldBound.center-(Vector2)root.worldBound.position;break;}
            rewardIcon.image=reelTextures[rewardQuest+1];rewardAmount.text="+"+rewardParts;rewardPending=true;
        }
        void UpdateRewardFlight()
        {
            if(rewardFly==null)return;
            if(rewardPending){rewardPending=false;rewardStarted=Time.unscaledTime;rewardFly.style.display=DisplayStyle.Flex;rewardFly.BringToFront();}
            if(rewardFly.style.display==DisplayStyle.None)return;
            var target=root.Q<VisualElement>("quest-item-well");
            float t=(Time.unscaledTime-rewardStarted)/0.7f;
            if(t>=1||target==null){rewardFly.style.display=DisplayStyle.None;return;}
            Vector2 end=target.worldBound.center-(Vector2)root.worldBound.position;
            Vector2 point=Vector2.Lerp(rewardStart,end,Mathf.SmoothStep(0,1,t));
            rewardFly.style.left=point.x-34;rewardFly.style.top=point.y-16;
        }
        void SetCell(int i, int symbol)
        {
            cells[i].Clear();
            cells[i].image = reelTextures[symbol];
            if (reelTextures[symbol] == null)
            {
                var label = Text(symbolNames[symbol], 11, Cream); label.style.height = Length.Percent(100);
                label.style.unityTextAlign = TextAnchor.MiddleCenter; cells[i].Add(label);
            }
        }
        void BeginSpin()
        {
            if (spinning || Time.unscaledTime < spinReadyAt || !model.CanSpin) return;
            StartCoroutine(SpinRoutine());
        }
        IEnumerator SpinRoutine()
        {
            var before=model.Snapshot;SpinResult result;
            if(!model.TrySpin(random,out result))yield break;
            // Commit rewards and their exact grid before any cosmetic frames.
            if(!Persist()){model=new GameModel(before);Refresh();ShowToast("SAVE FAILED - TRY AGAIN");yield break;}
            spinning = true; Refresh();
            int[] pool = result.Pool;
            for (int frame = 0; frame < 6; frame++)
            {
                for (int i = 0; i < 15; i++) SetCell(i, pool[random.Next(pool.Length)]);
                yield return new WaitForSecondsRealtime(0.055f);
            }
            spinning = false; spinReadyAt = Time.unscaledTime + 0.12f; Refresh();
            int collected=0;foreach(int value in result.Fragments)collected+=value;
            ShowToast("+"+result.Payout+" CASH"+(collected>0?"  ·  +"+collected+" PARTS":""));StartRewardFlight(result);
        }
        void Buy(int index)
        {
            if(spinning)return;var before=model.Snapshot;if(!model.TryBuy(index))return;
            if(!Persist()){model=new GameModel(before);Refresh();ShowToast("SAVE FAILED - TRY AGAIN");return;}
            rewardPending=false;rewardFly.style.display=DisplayStyle.None;
            ShowToast(model.Complete?"8/8 DONE - ROOM COMPLETE":"TASK "+model.PurchasedCount+"/8 DONE - INSTALLED");
            page = model.Complete ? 3 : model.PurchasedCount / 2;
            Refresh();
        }
        void Recover() { if(spinning)return;var before=model.Snapshot;if(!model.GrantRecovery())return;if(!Persist()){model=new GameModel(before);Refresh();ShowToast("SAVE FAILED - TRY AGAIN");return;}ShowToast("FREE RECOVERY SPIN");Refresh(); }
        bool Persist() { string error;bool saved=saves.Save(model.Snapshot, out error);saveNotice=saved?"Saved locally · offline":error;return saved; }
        void ShowReset() { if (spinning) return; settings.style.display=DisplayStyle.None; main.SetEnabled(false); confirmation.style.display = DisplayStyle.Flex; confirmation.BringToFront(); }
        void HideReset() { confirmation.style.display = DisplayStyle.None; main.SetEnabled(true); }
        void ResetConfirmed()
        {
            if (spinning) return;
            var fresh = new GameModel(); string error;
            // A confirmed reset is committed twice so the backup also contains the reset.
            if (!saves.Save(fresh.Snapshot, out error) || !saves.Save(fresh.Snapshot, out error))
            { statusLabel.text = error; HideReset(); return; }
            model = fresh; page = 0; saveNotice = "Room reset · 3 free spins";
            wanderer = new RoomWanderer(new FloorPoint(1.85f, 1.45f), new SeededRandom(Environment.TickCount)); drawnRoom = -1;
            HideReset(); Refresh(); ShowToast("3 FREE SPINS · A FRESH START");
        }
        void OnApplicationPause(bool paused) { if (paused && model != null) Persist(); }
        void OnApplicationQuit() { if (model != null) Persist(); }
#if UNITY_EDITOR
        public void SetQaState(PlayerState state, int upgradePage)
        { HideReset();HideSettings();HideToast();rewardPending=false;rewardFly.style.display=DisplayStyle.None;PixelUi.SetPressed(spinButton,false);random=new SeededRandom(57);model = new GameModel(state); wanderer = new RoomWanderer(new FloorPoint(state.heroineU, state.heroineV), new SeededRandom(57)); drawnRoom = -1; page = upgradePage; spinning = false; statusLabel.text = ""; Refresh(); }
        public void QaClick(string name)
        {
            var button=root.Q<Button>(name);if(button==null||!button.enabledInHierarchy)return;
            for(VisualElement item=button;item!=null;item=item.parent)if(item.style.display==DisplayStyle.None)return;
            button.Focus();
            using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}
        }
        public void QaPressedSpin(){PixelUi.SetPressed(spinButton,true);}
        public void QaReleaseSpin(){PixelUi.SetPressed(spinButton,false);}
        public FloorPoint QaFeet { get { return wanderer.Position; } }
        public bool QaWalking { get { return wanderer.Walking; } }
        public int QaAnimationPhase { get { return heroine == null ? -1 : heroine.Phase; } }
        public void QaStepWalk(float seconds) { wanderer.Step(seconds); model.SetHeroinePosition(wanderer.Position); UpdateHeroine(); }
        public void QaSave() { Persist(); }
        public void QaBeginSpin() { BeginSpin(); }
        public bool QaSpinning { get { return spinning; } }
        public void QaBuy(int index) { Buy(index); }
        public void QaShowReset() { ShowReset(); }
        public void QaCancelReset() { HideReset(); }
        public void QaConfirmReset() { ResetConfirmed(); }
#endif
    }
}
