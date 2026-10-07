using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire.Editor
{
    [InitializeOnLoad] public static class StakeShopAuditV09
    {
        const string Key = "ZTM_STAKE_SHOP_V09";
        static GameView view; static RenderTexture target;
        static int phase, scenario, warm, assertions, frame0;
        static double started, at;
        static PlayerState before; static FileStream failedWriteLease; static byte[] externalOriginal;
        static readonly StringBuilder Log = new StringBuilder();
        static readonly int[] Widths = {320,360,360,480}, Heights = {640,640,800,960};
        static string Root => Directory.GetParent(Application.dataPath).FullName;
        static string SaveDir => Path.Combine(Root, ".tooling", "v09-ui-checks-5", "save");
        static VisualElement UI => view.GetComponent<UIDocument>().rootVisualElement;
        static string Focused => (UI.panel.focusController.focusedElement as VisualElement)?.name ?? "<none>";
        static GameSession Session => (GameSession)typeof(GameView).GetField("session", BindingFlags.Instance|BindingFlags.NonPublic).GetValue(view);
        static StakeShopAuditV09() { EditorApplication.update += Tick; }
        public static void Start()
        {
            if (EditorApplication.isPlaying || Directory.Exists(SaveDir)) throw new Exception("Fresh owned Editor/profile required");
            Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR", SaveDir);
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");
            SessionState.SetBool(Key,true); EditorApplication.EnterPlaymode();
        }
        static void Check(bool ok,string message) { assertions++; if(!ok)throw new Exception(message); Log.AppendLine("PASS " + message); }
        static void At(int p) { phase=p; warm=0; at=EditorApplication.timeSinceStartup; }
        static bool Warm() => ++warm>18 && EditorApplication.timeSinceStartup-at>.25;
        static PlayerState Fixture(int cash,int parts,int free=0,int owned=0)
        {
            var s=new PlayerState{cash=cash,stakeMultiplier=3,freeSpins=free};s.tutorial.completed=true;s.tutorial.storySeenMask=15;
            for(int i=0;i<owned;i++)s.purchased[i]=true;
            if(owned<8)s.fragments[owned]=parts;
            s.quests=Economy.QuestRecords(s);return s;
        }
        static void Set(PlayerState state) { view.SetQaState(state,0); }
        static PlayerState Disk() { return new SaveStore(Path.Combine(SaveDir,"level1-v1.json"),new UnityStateCodec()).Load(out _); }
        static bool SameResources(PlayerState a,PlayerState b)
        {
            if(a.cash!=b.cash||a.spins!=b.spins||a.freeSpins!=b.freeSpins||a.stakeMultiplier!=b.stakeMultiplier)return false;
            for(int i=0;i<8;i++)if(a.fragments[i]!=b.fragments[i]||a.purchased[i]!=b.purchased[i])return false;
            for(int i=0;i<15;i++)if(a.lastGrid[i]!=b.lastGrid[i])return false;return true;
        }
        static void Capture(string name)
        {
            var old=RenderTexture.active;RenderTexture.active=target;var t=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
            t.ReadPixels(new Rect(0,0,target.width,target.height),0,0);t.Apply();
            File.WriteAllBytes(Path.Combine(Root,"Evidence","v09-qa5-"+name+"-"+(scenario>=4?"calm":"full")+"-"+target.width+"x"+target.height+".png"),t.EncodeToPNG());
            UnityEngine.Object.Destroy(t);RenderTexture.active=old;
        }
        static void Submit(Button button) { using(var e=NavigationSubmitEvent.GetPooled()){e.target=button;button.SendEvent(e);} }
        static void Press(KeyCode code,bool shift=false)
        {
            var original=(UI.panel.focusController.focusedElement as VisualElement)??UI;
            using(var e=KeyDownEvent.GetPooled(new Event{type=EventType.KeyDown,keyCode=code,modifiers=shift?EventModifiers.Shift:EventModifiers.None})){e.target=original;original.SendEvent(e);}
            if(code==KeyCode.Tab)using(var e=NavigationMoveEvent.GetPooled(shift?NavigationMoveEvent.Direction.Previous:NavigationMoveEvent.Direction.Next,EventModifiers.None)){e.target=original;original.SendEvent(e);}
        }
        static void Up(KeyCode code) { using(var e=KeyUpEvent.GetPooled(new Event{type=EventType.KeyUp,keyCode=code})){e.target=(UI.panel.focusController.focusedElement as VisualElement)??UI;((VisualElement)e.target).SendEvent(e);} }
        static void Tap(Button button)
        {
            var point=button.worldBound.center;var hit=UI.panel.Pick(point);
            Check(hit==button||button.Contains(hit),"actual panel picks " + button.name);
            using(var e=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=point})){e.target=hit;hit.SendEvent(e);}
            using(var e=PointerUpEvent.GetPooled(new Event{type=EventType.MouseUp,button=0,mousePosition=point})){e.target=button;button.SendEvent(e);}
        }
        static void Bounds(VisualElement element)
        {
            var b=element.worldBound; var r=UI.worldBound;
            Check(b.width>0&&b.height>0&&b.xMin>=r.xMin-1&&b.yMin>=r.yMin-1&&b.xMax<=r.xMax+1&&b.yMax<=r.yMax+1,"within portrait " + element.name + " " + b);
        }
        static void Targets(VisualElement element)
        {
            foreach(var b in element.Query<Button>().ToList())
            {
                if(!UiInputCoordinator.CanActivate(b))continue;
                Check(b.worldBound.width>=44&&b.worldBound.height>=44,"target44 " + b.name);
                var caption=b.Q<Label>("button-caption");if(caption==null||caption.style.display==DisplayStyle.None)continue;
                var text=caption.MeasureTextSize(caption.text,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined);
                Check(text.x<=caption.contentRect.width+1,"caption fits " + b.name);
            }
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying)return;
            try
            {
                if(started==0)started=EditorApplication.timeSinceStartup;
                if(EditorApplication.timeSinceStartup-started>150)throw new Exception("Bounded UI audit timeout phase="+phase);
                if(view==null){view=UnityEngine.Object.FindFirstObjectByType<GameView>();if(view==null)return;frame0=Time.frameCount;At(0);}
                if(phase==0)
                {
                    if(target!=null){view.GetComponent<UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);}
                    int size=scenario%4;target=new RenderTexture(Widths[size],Heights[size],0,RenderTextureFormat.ARGB32);target.Create();view.GetComponent<UIDocument>().panelSettings.targetTexture=target;
                    var prefs=(PresentationPreferences)typeof(GameView).GetField("presentation",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(view);
                    if(prefs.ReducedMotion!=(scenario>=4)){Check(prefs.Toggle(),"private calm preference saved");PixelUi.ReducedMotion=prefs.ReducedMotion;}
                    Set(Fixture(8,0));At(1);
                }
                else if(phase==1&&Warm())
                {
                    Bounds(UI.Q<Button>("spin"));Targets(UI.Q<VisualElement>("game"));
                    Check(!UI.Q<Button>("spin").enabledInHierarchy&&UI.Q<Label>("bet-state").text=="LOW COINS","3x unaffordable display and gate");
                    Capture("main-low-coins");Tap(UI.Q<Button>("bet-panel"));At(2);
                }
                else if(phase==2&&Warm())
                {
                    Bounds(UI.Q<VisualElement>("stake-overlay-panel"));Targets(UI.Q<VisualElement>("stake-overlay"));
                    Check(Focused=="stake-3","stake modal initial current choice focus");
                    Press(KeyCode.Tab);Check(Focused=="close-stakes","stake Tab scoped");Press(KeyCode.Tab,true);Check(Focused=="stake-3","stake ShiftTab scoped");
                    int spins=view.Model.Snapshot.spins;Submit(UI.Q<Button>("spin"));Check(view.Model.Snapshot.spins==spins,"stake modal no submit click-through");
                    var hit=UI.panel.Pick(UI.Q<Button>("spin").worldBound.center);Check(!UI.Q<VisualElement>("game").Contains(hit),"stake backdrop intercepts underlying pointer");
                    Capture("stake-picker");view.QaClick("stake-1");At(3);
                }
                else if(phase==3&&Warm())
                {
                    Check(view.Model.SelectedStake==1&&Disk().stakeMultiplier==1,"stake selection saved through real UI callback/Unity JSON");
                    Check(UI.Q<Button>("spin").enabledInHierarchy&&Focused=="bet-panel","lower affordable stake and trigger focus restored");
                    before=view.Model.Snapshot;
                    failedWriteLease=new FileStream(Path.Combine(SaveDir,"level1-v1.json.tmp"),FileMode.OpenOrCreate,FileAccess.Write,FileShare.None);
                    view.QaClick("spin");
                    Check(Session.Error.Length>0&&SameResources(before,view.Model.Snapshot),"actual failed spin preserves committed resources");
                    view.QaClick("bet-panel");At(12);
                }
                else if(phase==12&&Warm())
                {
                    string error=Session.Error,notice=Session.Notice;Tap(UI.Q<Button>("stake-1"));
                    Check(Session.Error==error&&Session.Notice==notice&&SameResources(before,view.Model.Snapshot),"current stake no-op keeps failed-write status and performs no transaction");
                    view.QaClick("menu");At(13);
                }
                else if(phase==13&&Warm())
                {
                    Check(UI.Q<Label>("save-status").text.Contains("Could not save"),"displayed persistent failure survives current-stake no-op");
                    Bounds(UI.Q<Label>("save-status"));Capture("stake-noop-save-failure");
                    view.QaClick("close-settings");view.QaClick("bet-panel");At(14);
                }
                else if(phase==14&&Warm())
                {
                    Tap(UI.Q<Button>("stake-3"));
                    Check(Session.Error.Length>0&&view.Model.SelectedStake==1&&UI.Q<VisualElement>("stake-overlay").style.display==DisplayStyle.Flex,"different stake failed commit keeps old choice/modal");
                    Check(SameResources(before,view.Model.Snapshot),"failed different-stake write changes no resource");
                    failedWriteLease.Dispose();failedWriteLease=null;At(15);
                }
                else if(phase==15&&Warm())
                {
                    Tap(UI.Q<Button>("stake-3"));
                    Check(Session.Error==""&&view.Model.SelectedStake==3&&Disk().stakeMultiplier==3,"actual successful different stake clears failure after commit");
                    view.QaClick("menu");At(16);
                }
                else if(phase==16&&Warm())
                {
                    Check(!UI.Q<Label>("save-status").text.Contains("Could not"),"only committed selection clears visible failure");
                    view.QaClick("close-settings");
                    var legacy=Fixture(80,0);legacy.version=5;legacy.stakeMultiplier=0;
                    File.WriteAllText(Path.Combine(SaveDir,"level1-v1.json"),new UnityStateCodec().Encode(legacy));
                    Check(Session.TryReload(),"private legacy profile reloaded for backup-failure display");
                    typeof(GameView).GetMethod("Refresh",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(view,null);
                    string snapshotRoot=Path.Combine(SaveDir,"level1-v1.json.pre-schema6");
                    if(Directory.Exists(snapshotRoot)){
                        string history=Path.Combine(SaveDir,"private-snapshot-history-"+scenario);
                        string scope=Path.GetFullPath(SaveDir)+Path.DirectorySeparatorChar;
                        Check(Path.GetFullPath(snapshotRoot).StartsWith(scope,StringComparison.OrdinalIgnoreCase)&&Path.GetFullPath(history).StartsWith(scope,StringComparison.OrdinalIgnoreCase),"private snapshot relocation stays in owned audit save root");
                        Directory.Move(snapshotRoot,history);
                    }
                    File.WriteAllText(snapshotRoot,"private backup blocker");
                    view.QaClick("bet-panel");At(17);
                }
                else if(phase==17&&Warm())
                {
                    before=view.Model.Snapshot;Tap(UI.Q<Button>("stake-3"));
                    Check(Session.Error==SaveStore.MigrationSnapshotError&&SameResources(before,view.Model.Snapshot),"required migration backup failure blocks real UI preference commit");
                    view.QaClick("close-stakes");view.QaClick("menu");At(18);
                }
                else if(phase==18&&Warm())
                {
                    Check(UI.Q<Label>("save-status").text=="Could not protect the old save. No changes saved.","migration-copy failure shown explicitly in menu");
                    Bounds(UI.Q<Label>("save-status"));Capture("migration-backup-failure");
                    view.QaClick("close-settings");
                    File.Move(Path.Combine(SaveDir,"level1-v1.json.pre-schema6"),Path.Combine(SaveDir,"private-backup-blocker-"+scenario));
                    Check(Session.TrySelectStake(3),"private migration-copy retry actually commits");
                    var legacy=Fixture(80,0);legacy.version=5;legacy.stakeMultiplier=0;
                    externalOriginal=Encoding.UTF8.GetBytes(new UnityStateCodec().Encode(legacy));
                    File.WriteAllBytes(Path.Combine(SaveDir,"level1-v1.json"),externalOriginal);
                    Check(Session.TryReload(),"legacy loaded before external-change UI check");
                    typeof(GameView).GetMethod("Refresh",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(view,null);
                    var foreign=Fixture(444,0);foreign.version=7;
                    File.WriteAllText(Path.Combine(SaveDir,"level1-v1.json"),new UnityStateCodec().Encode(foreign));
                    view.QaClick("bet-panel");At(19);
                }
                else if(phase==19&&Warm())
                {
                    before=view.Model.Snapshot;Tap(UI.Q<Button>("stake-3"));
                    Check(Session.Error==SaveStore.SaveChangedNotice&&SameResources(before,view.Model.Snapshot),"external future save blocks real UI commit without changing resources");
                    view.QaClick("close-stakes");view.QaClick("menu");At(20);
                }
                else if(phase==20&&Warm())
                {
                    Check(UI.Q<Label>("save-status").text=="Save changed. Close and reopen the game.","external-change refusal gives actionable visible message");
                    Bounds(UI.Q<Label>("save-status"));Capture("save-external-change");view.QaClick("close-settings");
                    File.WriteAllBytes(Path.Combine(SaveDir,"level1-v1.json"),externalOriginal);
                    Set(Fixture(60,20));view.QaClick("shop");At(4);
                }
                else if(phase==4&&Warm())
                {
                    var shop=UI.Q<VisualElement>("shop-overlay");Bounds(UI.Q<VisualElement>("shop-overlay-panel"));Targets(shop);
                    Check(Focused=="close-shop","shop focus safe close");
                    Check(shop.Query<VisualElement>().Where(x=>x.name!=null&&x.name.StartsWith("shop-item-")).ToList().Count==8,"eight catalog entries");
                    for(int i=0;i<8;i++){Check(shop.Q<Image>("shop-icon-"+i).image!=null,"exact existing item icon"+i);Check(shop.Q<Button>("shop-buy-"+i).enabledInHierarchy==(i==0),"shop current-only both-resource gate"+i);Check(shop.Q<Label>("shop-benefit-"+i).text.Contains(view.Model.Definition(i).Description),"room improvement preview"+i);}
                    for(int i=1;i<8;i++)Check(shop.Q<Label>("shop-unlock-"+i).text=="Complete earlier tasks first.","accurate prerequisite for each locked future task"+i);
                    Press(KeyCode.Tab);Check(Focused=="shop-buy-0","shop Tab reaches current purchase");Press(KeyCode.Tab,true);Check(Focused=="close-shop","shop ShiftTab wraps within modal");
                    Bounds(shop.Q<Button>("shop-buy-0"));before=view.Model.Snapshot;Submit(UI.Q<Button>("spin"));Check(SameResources(before,view.Model.Snapshot),"shop no background transaction");Capture("shop-ready");
                    var retired=shop.Q<Button>("shop-buy-0");Tap(retired);for(int i=0;i<30;i++)Submit(retired);At(5);
                }
                else if(phase==5&&Warm())
                {
                    Check(view.Model.PurchasedCount==1&&view.Model.Snapshot.cash==0&&view.Model.Snapshot.fragments[0]==0,"shop explicit install debits both once");
                    Check(SameResources(view.Model.Snapshot,Disk()),"shop purchase persisted");
                    Check(UI.Q<VisualElement>("shop-overlay").style.display==DisplayStyle.None&&Focused=="shop","install returns to changed room/focus");
                    Capture("shop-installed");Set(Fixture(59,20));view.QaClick("shop");At(6);
                }
                else if(phase==6&&Warm())
                {
                    Check(!UI.Q<Button>("shop-buy-0").enabledInHierarchy,"shop parts-only remains disabled");before=view.Model.Snapshot;view.QaClick("shop-buy-0");Check(SameResources(before,view.Model.Snapshot),"no overdraw from unmet shop cash");
                    Press(KeyCode.Escape);At(7);
                }
                else if(phase==7&&Warm())
                {
                    Check(Focused=="shop"&&UI.Q<VisualElement>("shop-overlay").style.display==DisplayStyle.None,"Escape closes shop/restores trigger");
                    Set(Fixture(1000,0));before=view.Model.Snapshot;view.QaClick("spin");
                    var after=view.Model.Snapshot;int cashIcons=0,triples=0,parts=0;foreach(int icon in after.lastGrid){if(icon==0)cashIcons++;else parts++;}
                    for(int x=0;x<5;x++)if(after.lastGrid[x]==after.lastGrid[x+5]&&after.lastGrid[x]==after.lastGrid[x+10])triples++;
                    Check(after.spins==1&&after.cash==before.cash-15+3*(12+2*cashIcons+10*triples)&&after.fragments[0]==parts,"real paid3x spin cash scaling/parts consistency");
                    Check(!UI.Q<Button>("bet-panel").enabledInHierarchy&&!UI.Q<Button>("shop").enabledInHierarchy,"stake and shop lock during spin");
                    Check(!Session.TrySelectStake(1)&&view.Model.SelectedStake==3,"session locks captured stake");
                    for(int i=0;i<30;i++)view.QaClick("spin");Check(view.Model.Snapshot.spins==1,"STOP/SKIP repeats award once during presentation");At(8);
                }
                else if(phase==8&&!view.QaSpinning&&Warm())
                {
                    Check(SameResources(view.Model.Snapshot,Disk()),"paid3x survives disk reload");
                    Set(Fixture(0,0,1));before=view.Model.Snapshot;view.QaClick("spin");
                    var after=view.Model.Snapshot;int cashIcons=0,triples=0;foreach(int icon in after.lastGrid)if(icon==0)cashIcons++;
                    for(int x=0;x<5;x++)if(after.lastGrid[x]==after.lastGrid[x+5]&&after.lastGrid[x]==after.lastGrid[x+10])triples++;
                    Check(after.cash==12+2*cashIcons+10*triples&&after.freeSpins==0&&after.stakeMultiplier==3,"free spin remains1x with3x selected");
                    if(view.QaSpinning)Check(UI.Q<Label>("bet-state").text=="FREE · 1x","captured free stake stays explicit during presentation");
                    view.QaClick("spin");At(9);
                }
                else if(phase==9&&!view.QaSpinning&&Warm())
                {
                    Set(Fixture(61,0,0,8));view.QaClick("shop");At(10);
                }
                else if(phase==10&&Warm())
                {
                    for(int i=0;i<8;i++)Check(UI.Q<Label>("shop-state-"+i).text=="INSTALLED"&&!UI.Q<Button>("shop-buy-"+i).enabledInHierarchy,"owned shop terminal gate"+i);
                    UI.Q<ScrollView>("shop-catalog").ScrollTo(UI.Q<VisualElement>("shop-item-7"));At(11);
                }
                else if(phase==11&&Warm())
                {
                    Check(UI.Q<ScrollView>("shop-catalog").scrollOffset.y>0,"catalog scroll reaches later items");Capture("shop-complete-bottom");view.QaClick("close-shop");
                    Log.AppendLine("SCENARIO PASS " + target.width+"x"+target.height+" calm="+(scenario>=4));
                    if(++scenario<8)At(0);else Finish(true);
                }
            }
            catch(Exception e){Log.AppendLine("FAIL phase="+phase+" scenario="+scenario+" "+e);Debug.LogException(e);Finish(false);}
        }
        static void Finish(bool pass)
        {
            failedWriteLease?.Dispose();failedWriteLease=null;
            SessionState.SetBool(Key,false);Log.AppendLine("RESULT "+(pass?"PASS":"FAIL")+" assertions="+assertions+" runtimeFrames="+(Time.frameCount-frame0)+" elapsed="+(EditorApplication.timeSinceStartup-started).ToString("F2")+"; actual offscreen Editor pixels/Unity JSON; synthetic UITK pointer/navigation/key events; no OS input");
            File.WriteAllText(Path.Combine(Root,"Evidence","v09-ui-checks-5.txt"),Log.ToString());
            if(target!=null){if(view!=null)view.GetComponent<UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);}
            EditorApplication.ExitPlaymode();EditorApplication.delayCall+=()=>EditorApplication.Exit(pass?0:1);
        }
    }
}
