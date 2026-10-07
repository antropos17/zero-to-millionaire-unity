using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ZeroToMillionaire.Editor
{
    [InitializeOnLoad] public static class ProfileSectionsAuditV10
    {
        const string Key="ZTM_PROFILE_SECTIONS_V10_6";
        static GameView view;static RenderTexture target;static FileStream lease;
        static int phase,scenario,warm,assertions,frames,phaseFrame;static long savedRevision;static int savedSpins;
        static double started,at,previousTick;
        static readonly System.Collections.Generic.List<double> intervals=new System.Collections.Generic.List<double>();
        static byte[] localOriginal;static string savedName;static bool savedCalm;
        static readonly StringBuilder Log=new StringBuilder();
        static readonly int[] Widths={320,360,360,480},Heights={640,640,800,960};
        static string Root=>Directory.GetParent(Application.dataPath).FullName;
        static string SaveDir=>Path.Combine(Root,".tooling","v10-ui-checks-6","save");
        static VisualElement UI=>view.GetComponent<UIDocument>().rootVisualElement;
        static VisualElement Focus=>UI.panel.focusController.focusedElement as VisualElement;
        static GameSession Session=>(GameSession)typeof(GameView).GetField("session",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(view);
        static ProfileSectionsAuditV10(){EditorApplication.update+=Tick;}
        public static void BatchStart()
        {
            if(EditorApplication.isPlaying||Directory.Exists(SaveDir))throw new Exception("Fresh audit profile required");
            Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",SaveDir);
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();
        }
        static void Check(bool ok,string message){assertions++;if(!ok)throw new Exception(message);Log.AppendLine("PASS "+message);}
        static void At(int p){phase=p;warm=0;phaseFrame=Time.frameCount;at=EditorApplication.timeSinceStartup;}
        static bool Warm()=>Time.frameCount-phaseFrame>=6&&EditorApplication.timeSinceStartup-at>.22;
        static PlayerState Fixture(int owned=0){var s=new PlayerState{cash=80,freeSpins=0};s.tutorial.completed=true;s.tutorial.storySeenMask=15;for(int i=0;i<owned;i++)s.purchased[i]=true;if(owned<8)s.fragments[owned]=0;s.quests=Economy.QuestRecords(s);return s;}
        static bool InFocus(VisualElement e)=>Focus==e||e.Contains(Focus);
        static void Press(KeyCode key,char character='\0')
        {
            var e=Focus??UI;using(var evt=KeyDownEvent.GetPooled(new Event{type=EventType.KeyDown,keyCode=key,character=character})){evt.target=e;e.SendEvent(evt);}
            if(key==KeyCode.Tab)using(var evt=NavigationMoveEvent.GetPooled(NavigationMoveEvent.Direction.Next,EventModifiers.None)){evt.target=e;e.SendEvent(evt);}
        }
        static void Up(KeyCode key){var e=Focus??UI;using(var evt=KeyUpEvent.GetPooled(new Event{type=EventType.KeyUp,keyCode=key})){evt.target=e;e.SendEvent(evt);}}
        static void Submit(VisualElement e){using(var evt=NavigationSubmitEvent.GetPooled()){evt.target=e;e.SendEvent(evt);}}
        static void Tap(Button button)
        {
            Check(UiInputCoordinator.CanActivate(button),"enabled visible action "+button.name);var point=button.worldBound.center;var hit=UI.panel.Pick(point);
            Check(hit==button||button.Contains(hit),"actual panel picks "+button.name);
            using(var e=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=point})){e.target=hit;hit.SendEvent(e);}
            using(var e=PointerUpEvent.GetPooled(new Event{type=EventType.MouseUp,button=0,mousePosition=point})){e.target=button;button.SendEvent(e);}
        }
        static void Bounds(VisualElement e){var a=e.worldBound;var b=UI.worldBound;Check(a.width>0&&a.height>0&&a.xMin>=b.xMin-1&&a.yMin>=b.yMin-1&&a.xMax<=b.xMax+1&&a.yMax<=b.yMax+1,"within portrait "+e.name+" "+a);}
        static void Chrome()
        {
            Bounds(UI.Q<VisualElement>("sections-panel"));
            foreach(var b in UI.Q<VisualElement>("section-tabs").Query<Button>().ToList())
            {Bounds(b);Check(b.worldBound.width>=44&&b.worldBound.height>=44,"section touch44 "+b.name);var l=b.Q<Label>("button-caption");Check(l.MeasureTextSize(l.text,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined).x<=l.contentRect.width+1,"section text fits "+b.name);}
        }
        static void Capture(string name)
        {
            var old=RenderTexture.active;RenderTexture.active=target;var t=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);t.ReadPixels(new Rect(0,0,target.width,target.height),0,0);t.Apply();
            File.WriteAllBytes(Path.Combine(Root,"Evidence","v10-slice1-qa6-"+name+"-"+(scenario>=4?"calm":"full")+"-"+target.width+"x"+target.height+".png"),t.EncodeToPNG());UnityEngine.Object.Destroy(t);RenderTexture.active=old;
        }
        static void BindTarget()
        {
            if(target==null){int i=scenario%4;target=new RenderTexture(Widths[i],Heights[i],0,RenderTextureFormat.ARGB32);target.Create();}
            view.GetComponent<UIDocument>().panelSettings.targetTexture=target;
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying)return;
            try
            {
                if(started==0)started=EditorApplication.timeSinceStartup;if(previousTick>0&&intervals.Count<20000)intervals.Add((EditorApplication.timeSinceStartup-previousTick)*1000);previousTick=EditorApplication.timeSinceStartup;if(EditorApplication.timeSinceStartup-started>240)throw new Exception("Bounded UI timeout");
                if(view==null){view=UnityEngine.Object.FindFirstObjectByType<GameView>();if(view==null)return;BindTarget();if(frames==0)frames=Time.frameCount;}
                if(phase==0)
                {
                    if(target!=null){view.GetComponent<UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);target=null;}BindTarget();view.SetQaState(Fixture(),0);
                    var prefs=(PresentationPreferences)typeof(GameView).GetField("presentation",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(view);if(prefs.ReducedMotion!=(scenario>=4))Check(prefs.Toggle(),"private full/calm saved");PixelUi.ReducedMotion=prefs.ReducedMotion;
                    var local=(LocalPlayerStore)typeof(GameView).GetField("localPlayer",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(view);var p=local.Snapshot;p.notificationMode="on";Check(local.Save(p,out _),"private optional notices baseline");view.QaNotifications.SetOptional(true);
                    view.QaClick("menu");At(1);
                }
                else if(phase==1&&Warm())
                {
                    Chrome();Check(InFocus(UI.Q<Button>("tutorial-replay")),"settings focus scoped");Check(UI.Q<Button>("section-settings").ClassListContains("selected")&&UI.Q<Label>("section-active-settings").style.display==DisplayStyle.Flex,"persistent selected section independent of focus");
                    var motion=UI.Q<Button>("motion-toggle");motion.Focus();bool originalCalm=view.QaLocalPlayer.Calm;Press(KeyCode.Return);Submit(motion);for(int k=0;k<20;k++){Press(KeyCode.Return);Submit(motion);}Check(view.QaLocalPlayer.Calm!=originalCalm,"held Return changes setting once");
                    typeof(GameView).GetMethod("OnApplicationFocus",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(view,new object[]{false});Press(KeyCode.Return);Submit(motion);Up(KeyCode.Return);Check(view.QaLocalPlayer.Calm==originalCalm,"focus loss releases held activation key");Bounds(UI.Q<Button>("motion-toggle"));Bounds(UI.Q<Button>("notification-toggle"));Capture("settings");Tap(UI.Q<Button>("section-profile"));At(2);
                }
                else if(phase==2&&Warm())
                {
                    Chrome();var field=UI.Q<TextField>("profile-name");Bounds(field);Check(InFocus(field),"name field receives section focus");Check(UI.Q<Label>("profile-stats").text.Contains("Spins this save: 0")&&UI.Q<Label>("profile-stats").text.Contains("Room renovation: 0/8"),"honest current-save stats");
                    for(int i=0;i<3;i++)Check(UI.Q<Image>("profile-avatar-image-"+i).image!=null,"approved reusable profile badge "+i);
                    long revision=view.QaLocalPlayer.revision;int spins=view.Model.Snapshot.spins;Press(KeyCode.Space,' ');Submit(field);Up(KeyCode.Space);Press(KeyCode.Return,'\n');Submit(field);Up(KeyCode.Return);
                    Check(view.QaLocalPlayer.revision==revision&&view.Model.Snapshot.spins==spins,"editing Space/Enter does not save/spin");
                    field.value="<b>Bad</b>";Check(!UI.Q<Button>("save-profile").enabledInHierarchy,"invalid name explicit save gate");
                    field.value="Ирина Zoë";Tap(UI.Q<Button>("avatar-stool"));At(3);
                }
                else if(phase==3&&Warm())
                {
                    Bounds(UI.Q<Button>("save-profile"));Check(UI.Q<Button>("avatar-stool").ClassListContains("selected")&&UI.Q<Label>("avatar-selected-stool").style.display==DisplayStyle.Flex,"persistent selected badge independent of focus");Bounds(UI.Q<Label>("avatar-selected-stool"));Capture("profile-draft");Tap(UI.Q<Button>("save-profile"));savedName="Ирина Zoë";savedRevision=view.QaLocalPlayer.revision;
                    Check(view.QaLocalPlayer.displayName==savedName&&view.QaLocalPlayer.avatarId=="stool","explicit profile commit");
                    var disk=new LocalPlayerStore(SaveDir,new UnityLocalPlayerCodec());Check(disk.Snapshot.SameValues(view.QaLocalPlayer),"profile persisted via Unity JSON");
                    lease=new FileStream(Path.Combine(SaveDir,"local-player-v1.json"),FileMode.Open,FileAccess.Read,FileShare.None);
                    UI.Q<TextField>("profile-name").value="Retry "+scenario;Tap(UI.Q<Button>("save-profile"));At(4);
                }
                else if(phase==4&&Warm())
                {
                    Check(view.QaLocalPlayer.displayName==savedName&&view.QaLocalPlayer.revision==savedRevision,"failed profile callback preserves committed display");Check(UI.Q<Label>("section-error").text.Contains("not saved"),"profile failure visible in shared shell");
                    UI.Q<TextField>("profile-name").value=savedName;Tap(UI.Q<Button>("save-profile"));Check(UI.Q<Label>("section-error").text.Contains("not saved"),"profile no-op retains failed-write message");Capture("profile-failure");Tap(UI.Q<Button>("section-settings"));At(5);
                }
                else if(phase==5&&Warm())
                {
                    bool mode=view.QaLocalPlayer.Notifications;Tap(UI.Q<Button>("notification-toggle"));Check(view.QaLocalPlayer.Notifications==mode,"failed setting stays at saved choice");Bounds(UI.Q<Label>("section-error"));Capture("settings-failure");lease.Dispose();lease=null;
                    Tap(UI.Q<Button>("notification-toggle"));Check(!view.QaLocalPlayer.Notifications&&UI.Q<Label>("section-error").style.display==DisplayStyle.None,"actual successful settings commit clears local error");
                    At(50);
                }
                else if(phase==50&&Warm()){Tap(UI.Q<Button>("reset"));At(6);}
                else if(phase==6&&Warm())
                {
                    Check(InFocus(UI.Q<Button>("keep-room")),"nested reset safe focus");Press(KeyCode.Escape);At(7);
                }
                else if(phase==7&&Warm())
                {
                    Check(InFocus(UI.Q<Button>("reset"))&&view.Model.Snapshot.cash==80,"cancel reset returns to settings preserving resources");
                    for(int i=0;i<12;i++)view.QaClick(i%2==0?"section-shop":"section-profile");At(8);
                }
                else if(phase==8&&Warm())
                {
                    Check(InFocus(UI.Q<TextField>("profile-name")),"rapid section changes cancel stale focus");Press(KeyCode.Tab);Check(InFocus(UI.Q<Button>("avatar-heroine")),"Tab reaches badge from editable field");
                    UI.Q<TextField>("profile-name").value="Unsaved draft";Press(KeyCode.Escape);At(9);
                }
                else if(phase==9&&Warm())
                {
                    Check(InFocus(UI.Q<Button>("menu")),"Back/Escape restores opener");Check(view.QaLocalPlayer.displayName==savedName,"closing cancels unsaved name draft");
                    lease=new FileStream(Path.Combine(SaveDir,"level1-v1.json.tmp"),FileMode.OpenOrCreate,FileAccess.Write,FileShare.None);view.QaClick("spin");Check(Session.Error.Length>0&&view.Model.Snapshot.spins==0,"failed game write awards no spin");At(10);
                }
                else if(phase==10&&Warm())
                {
                    Check(!view.QaLocalPlayer.Notifications&&UI.Q<Label>("critical-status").style.display==DisplayStyle.Flex&&UI.Q<Label>("critical-status").text.Contains("Could not save"),"muted reward notices cannot hide game save failure");Bounds(UI.Q<Button>("spin"));Bounds(UI.Q<Label>("critical-status"));Capture("room-critical-muted");
                    Check(Array.FindIndex(view.QaNotifications.History,s=>s.StartsWith("Spin "))<0,"no fictitious failed spin in session log");lease.Dispose();lease=null;view.QaClick("spin");for(int i=0;i<12;i++)view.QaClick("spin");At(11);
                }
                else if(phase==11&&!view.QaSpinning&&Warm())
                {
                    savedSpins=view.Model.Snapshot.spins;savedCalm=view.QaLocalPlayer.Calm;Check(savedSpins==1&&view.QaNotifications.PendingCount==0,"repeated skip awards/logs one muted spin");Check(Array.FindAll(view.QaNotifications.History,s=>s.StartsWith("Spin ")).Length==1,"one committed spin history entry");
                    Check(UI.Q<Label>("critical-status").style.display==DisplayStyle.None,"successful game commit clears game error");
                    view.GetComponent<UIDocument>().panelSettings.targetTexture=null;view=null;SceneManager.LoadScene("Level1");At(12);
                }
                else if(phase==12&&Warm())
                {
                    Check(view.QaLocalPlayer.displayName==savedName&&view.QaLocalPlayer.avatarId=="stool"&&view.QaLocalPlayer.Calm==savedCalm&&!view.QaLocalPlayer.Notifications,"actual scene reload restores complete local profile/settings");Check(view.Model.Snapshot.spins==savedSpins&&view.QaNotifications.History.Length==0,"saved game survives reload; session log starts empty");
                    view.QaClick("menu");view.QaClick("section-profile");At(13);
                }
                else if(phase==13&&Warm())
                {
                    Check(UI.Q<TextField>("profile-name").value==savedName&&UI.Q<Label>("profile-stats").text.Contains("Spins this save: 1"),"reloaded name/current-save stat rendered");Capture("profile-reloaded");
                    view.QaClick("section-settings");view.QaClick("reset");At(14);
                }
                else if(phase==14&&Warm())
                {
                    Tap(UI.Q<Button>("confirm-reset"));Check(view.Model.Snapshot.spins==0&&view.Model.Snapshot.freeSpins==3&&view.QaLocalPlayer.displayName==savedName,"confirmed reset preserves local profile/settings");Check(view.QaNotifications.History.Length==1&&view.QaNotifications.History[0].StartsWith("Fresh start"),"confirmed reset clears old history then records actual reset");
                    view.SetQaState(Fixture(8),3);view.QaClick("menu");view.QaClick("section-profile");At(15);
                }
                else if(phase==15&&Warm())
                {
                    Check(view.Model.Complete&&view.Model.PurchasedCount==8&&UI.Q<Label>("profile-stats").text.Contains("Room renovation: 8/8"),"basic completion remains unchanged");Capture("profile-complete");
                    Press(KeyCode.Escape);var ready=Fixture();ready.fragments[0]=20;ready.quests=Economy.QuestRecords(ready);view.SetQaState(ready,0);view.QaClick("shop");At(16);
                }
                else if(phase==16&&Warm())
                {
                    Chrome();Bounds(UI.Q<Button>("shop-buy-0"));Check(UI.Q<Button>("shop-buy-0").enabledInHierarchy,"shared shop both-resource current gate");
                    for(int i=1;i<8;i++)Check(!UI.Q<Button>("shop-buy-"+i).enabledInHierarchy,"shared shop earlier-task gate"+i);
                    var retired=UI.Q<Button>("shop-buy-0");Tap(retired);for(int i=0;i<30;i++)Submit(retired);
                    Check(view.Model.PurchasedCount==1&&view.Model.Snapshot.cash==20&&view.Model.Snapshot.fragments[0]==0,"shared shop repeated pointer/submit debits both once");At(17);
                }
                else if(phase==17&&Warm())
                {
                    Check(InFocus(UI.Q<Button>("shop")),"shared shop purchase restores shop opener");Capture("shop-purchase-room");
                    view.QaClick("menu");At(18);
                }
                else if(phase==18&&Warm())
                {
                    string file=Path.Combine(SaveDir,"local-player-v1.json");localOriginal=File.ReadAllBytes(file);File.WriteAllText(file,"{\"version\":2,\"revision\":1}");
                    Tap(UI.Q<Button>("motion-toggle"));Check(UI.Q<Label>("section-error").text.Contains("changed on disk"),"external future local file blocks runtime setting commit");At(19);
                }
                else if(phase==19&&Warm())
                {
                    Check(!UI.Q<Button>("motion-toggle").enabledInHierarchy&&!UI.Q<Button>("notification-toggle").enabledInHierarchy,"blocked local data disables setting changes");UI.Q<ScrollView>("settings-scroll").ScrollTo(UI.Q<Button>("retry-local-data"));At(20);
                }
                else if(phase==20&&Warm())
                {
                    Bounds(UI.Q<Button>("retry-local-data"));Tap(UI.Q<Button>("retry-local-data"));Check(UI.Q<Label>("section-error").text.Contains("Original files kept"),"retry protects unsupported local original and surfaces read error");
                    File.WriteAllBytes(Path.Combine(SaveDir,"local-player-v1.json"),localOriginal);At(21);
                }
                else if(phase==21&&Warm())
                {
                    Capture("local-data-protected");Tap(UI.Q<Button>("retry-local-data"));Check(UI.Q<Button>("motion-toggle").enabledInHierarchy&&view.QaLocalPlayer.displayName==savedName,"local retry restores current known profile after file correction");
                    Check(view.Model.PurchasedCount==1&&view.Model.Snapshot.cash==20,"local retry preserves game resources");Press(KeyCode.Escape);
                    var local=(LocalPlayerStore)typeof(GameView).GetField("localPlayer",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(view);var prefs=local.Snapshot;prefs.notificationMode="on";Check(local.Save(prefs,out _),"optional notice regression enabled");view.QaNotifications.SetOptional(true);
                    var near=Fixture();near.fragments[0]=19;near.quests=Economy.QuestRecords(near);view.SetQaState(near,0);view.QaClick("spin");view.QaClick("menu");At(22);
                }
                else if(phase==22&&!view.QaSpinning&&Warm())
                {
                    Check(view.Model.CanBuy(0)&&Array.FindAll(view.QaNotifications.Pending,n=>n.Event==SessionEvent.Ready&&n.ItemId==0).Length==1,"actual spin queued typed current-item Ready while menu prevents drain");
                    view.QaClick("section-shop");At(23);
                }
                else if(phase==23&&Warm())
                {
                    var retired=UI.Q<Button>("shop-buy-0");Tap(retired);for(int k=0;k<20;k++)Submit(retired);
                    Check(view.Model.PurchasedCount==1,"immediate purchase installs before Ready queue drains");
                    Check(Array.FindAll(view.QaNotifications.Pending,n=>n.Event==SessionEvent.Ready&&n.ItemId==0).Length==0,"committed purchase invalidates queued Ready");
                    Check(Array.FindAll(view.QaNotifications.Pending,n=>n.Event==SessionEvent.Purchase&&n.ItemId==0).Length==1,"exactly one typed purchase notice pending");At(24);
                }
                else if(phase==24)
                {
                    var toast=UI.Q<VisualElement>("toast");var label=toast.Q<Label>();bool visible=toast.style.display==DisplayStyle.Flex;
                    Check(!visible||!label.text.Contains("ready to buy"),"no stale Ready prompt after installed item");
                    if(visible&&label.text.Contains("installed"))
                    {
                        Check(Array.FindAll(view.QaNotifications.History,s=>s.Contains("installed")).Length==1,"one committed purchase history entry");Capture("ready-invalidated-purchase");
                        Log.AppendLine("SCENARIO PASS "+target.width+"x"+target.height+" calm="+(scenario>=4));if(++scenario<8)At(0);else Finish(true);
                    }
                    else if(EditorApplication.timeSinceStartup-at>8)throw new Exception("Purchase notice not shown after obsolete Ready removal");
                }
            }
            catch(Exception e){Log.AppendLine("FAIL phase="+phase+" scenario="+scenario+" "+e);Finish(false);}
        }
        static void Finish(bool pass)
        {
            lease?.Dispose();lease=null;SessionState.SetBool(Key,false);Log.AppendLine("RESULT "+(pass?"PASS":"FAIL")+" assertions="+assertions+" runtimeFrames="+(Time.frameCount-frames)+" elapsed="+(EditorApplication.timeSinceStartup-started).ToString("F2")+"; actual offscreen Editor panels; synthetic UITK pointer/key/navigation; private save only; no OS input");
            intervals.Sort();if(intervals.Count>0)Log.AppendLine("Editor tick intervals (including captures/reloads): samples="+intervals.Count+" medianMs="+intervals[intervals.Count/2].ToString("F2")+" p95Ms="+intervals[(int)((intervals.Count-1)*.95)].ToString("F2")+" maxMs="+intervals[intervals.Count-1].ToString("F2")+"; not native/device frame benchmark");
            File.WriteAllText(Path.Combine(Root,"Evidence","v10-slice1-ui-checks-6.txt"),Log.ToString());
            if(target!=null){if(view!=null)view.GetComponent<UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);}
            EditorApplication.ExitPlaymode();EditorApplication.delayCall+=()=>EditorApplication.Exit(pass?0:1);
        }
    }
}
