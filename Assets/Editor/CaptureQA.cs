using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire.Editor
{
    [InitializeOnLoad]
    public static class CaptureQA
    {
        const string Active = "ZTM_CAPTURE_ACTIVE";
        static RenderTexture target;
        static int phase, frames, captureIndex;
        static double spinStarted;
        static GameView view;
        static readonly StringBuilder Report = new StringBuilder();
        static readonly List<float> FrameTimes = new List<float>();
        static readonly HashSet<int> GaitPhases = new HashSet<int>();
        static FloorPoint demoStart;
        static double nextDemoFrame, demoStarted;
        static int demoFrames;
        static float demoDistance;
        static FloorPoint previousFeet;
        static double flowNextSpin;
        static double flowLastProgress;
        static int flowSpins, flowBuys;
        static bool busyCaptured;
        static bool rewardCaptured;
        static readonly (int width, int height, int count, int page, string name)[] Captures = {
            (360,640,0,0,"v0.3-quest-start-360x640"), (360,800,3,1,"v0.3-quest-wallpaper-360x800"),
            (480,960,6,2,"v0.3-quest-mattress-480x960"), (360,800,8,3,"v0.3-quest-complete-360x800"),
            (360,640,0,0,"v0.3-quest-buy-ready-360x640"), (360,640,0,0,"v0.3-quest-spin-pressed-360x640"),
            (360,640,0,0,"v0.3-quest-menu-360x640"), (360,640,0,0,"v0.3-quest-reset-confirmation-360x640"),
            (360,640,0,0,"v0.3-quest-progress-360x640"), (360,640,0,0,"v0.3-quest-cash-needed-360x640"),
            (360,640,1,0,"v0.3-quest-after-buy-360x640")
        };
        static CaptureQA() { EditorApplication.update += Update; }
        public static void Start()
        {
            BuildPrototype.PrepareAndCheck();
            SessionState.SetBool(Active,true);
            EditorApplication.EnterPlaymode();
        }
        static void Assert(bool value, string text)
        { if (!value) throw new Exception("QA FAILED: " + text); Report.AppendLine("PASS " + text); }
        static string Root { get { return Directory.GetParent(Application.dataPath).FullName; } }
        static void Update()
        {
            if (!SessionState.GetBool(Active,false) || !EditorApplication.isPlaying) return;
            try
            {
                if (view == null) view = UnityEngine.Object.FindFirstObjectByType<GameView>();
                if (view == null) return;
                var doc = view.GetComponent<UIDocument>();
                if (phase == 0)
                {
                    var c = Captures[captureIndex];
                    target = new RenderTexture(c.width,c.height,0,RenderTextureFormat.ARGB32);
                    target.Create(); doc.panelSettings.targetTexture = target;
                    var state = new PlayerState { cash = c.count == 0 ? 0 : 100, spins = c.count == 0 ? 0 : 1,freeSpins=c.count==0?3:0 };
                    for(int i=0;i<8;i++) { state.purchased[i] = i < c.count; state.fragments[i] = c.count==0?0:4+i; }
                    if(c.name.Contains("buy-ready")){state.cash=18;state.fragments[0]=8;}
                    if(c.name.Contains("progress")){state.cash=12;state.freeSpins=1;state.spins=2;state.fragments[0]=5;}
                    if(c.name.Contains("cash-needed")){state.cash=10;state.freeSpins=0;state.fragments[0]=8;state.spins=2;}
                    if(c.name.Contains("after-buy")){state=new PlayerState{cash=18};state.fragments[0]=8;}
                    var pool = Economy.ActivePool(state);
                    for(int i=0;i<15;i++) state.lastGrid[i]=pool[i%pool.Length];
                    view.SetQaState(state,c.page);
                    if(c.name.Contains("after-buy")){view.QaClick("buy-0");Assert(view.Model.PurchasedCount==1&&view.Model.ActiveQuest==1&&view.Model.Snapshot.cash==0,"after-buy screenshot follows actual explicit installation from same18cash/8parts state");}
                    if(c.name.Contains("spin-pressed"))view.QaPressedSpin();
                    if(c.name.Contains("menu")||c.name.Contains("reset-confirmation"))view.QaClick("menu");
                    if(c.name.Contains("reset-confirmation"))view.QaClick("reset");
                    frames=0; phase=1;
                }
                else if (phase == 1)
                {
                    if(frames>10 && Time.unscaledDeltaTime>0) FrameTimes.Add(Time.unscaledDeltaTime*1000);
                    if (++frames<30) return;
                    var c = Captures[captureIndex];
                    var root = doc.rootVisualElement;
                    var spin = root.Q<Button>("spin"); var menu = root.Q<Button>("menu");
                    Assert(spin.worldBound.height>0 && spin.worldBound.yMax <= root.worldBound.yMax+1, c.name+" spin fits viewport");
                    Assert(menu.worldBound.height>=43.5f && menu.worldBound.yMax<=root.worldBound.yMax+1,c.name+" 44-unit menu fits viewport");
                    Assert(root.Query<VisualElement>().Where(element=>element.name!=null&&element.name.StartsWith("cell-")).ToList().Count==15,c.name+" stable 5x3 grid");
                    Assert(root.Q<VisualElement>("reset-confirmation").style.display==(c.name.Contains("reset-confirmation")?DisplayStyle.Flex:DisplayStyle.None),c.name+" reset modal visibility correct");
                    var stage = root.Q<VisualElement>("room-stage");
                    Debug.Log(c.name+" stage world="+stage.worldBound+" layout="+stage.layout+" style="+stage.style.width+" x "+stage.style.height+" room="+root.Q<VisualElement>("room").contentRect+" root="+root.contentRect);
                    File.WriteAllBytes(Path.Combine(Root,"Evidence",c.name+"-diagnostic.png"),ReadTarget());
                    Assert(Math.Abs(stage.worldBound.width-stage.worldBound.height)<1,c.name+" room aspect preserved");
                    var caption=spin.Q<Label>("button-caption");
                    Assert(caption!=null&&caption.text==(view.Model.Complete?"COMPLETE":"SPIN")&&caption.worldBound.height>20,c.name+" visible primary button child caption");
                    Assert(caption.resolvedStyle.unityFontDefinition.font!=null&&caption.resolvedStyle.unityFontDefinition.font.name.Contains("Pixelify"),c.name+" actual Pixelify font bound");
                    if(!view.Model.Complete){
                        int active=view.Model.ActiveQuest;var d=Economy.Upgrades[active];
                        Assert(root.Q<VisualElement>("active-quest")!=null&&root.Q<Label>("quest-title").text==d.ActionTitle.ToUpperInvariant(),c.name+" exactly current finite task shown");
                        Assert(root.Query<Button>().Where(button=>button.name!=null&&button.name.StartsWith("buy-")).ToList().Count==1,c.name+" only one install control");
                        Assert(root.Q<Label>("fragment-count-"+active).text==Math.Min(view.Model.Snapshot.fragments[active],d.FragmentGoal)+"/"+d.FragmentGoal,c.name+" exact capped task display preserves saved surplus");
                        Assert(root.Q<Label>("cash-count").text==view.Model.Snapshot.cash+"/"+d.CashPrice,c.name+" cash available and required remain explicit");
                        Assert(root.Q<VisualElement>("next-quest")!=null,c.name+" single compact next preview");
                        var buy=root.Q<Button>("buy-"+active);
                        Assert(buy.worldBound.width>=44&&buy.worldBound.height>=44,c.name+" install touch target fits");
                        Assert(root.Q<VisualElement>("active-quest").worldBound.yMax<=root.Q<VisualElement>("reels").worldBound.yMin,c.name+" active task stays above reels");
                    }
                    Assert(root.Q<Label>("room-progress").text=="ROOM "+view.Model.PurchasedCount+"/8 DONE",c.name+" visible overall task completion");
                    if(c.name.Contains("buy-ready"))Assert(root.Q<Button>("buy-0").enabledInHierarchy&&root.Q<Button>("buy-0").Q<Label>("button-caption").text=="BUY &\nINSTALL",c.name+" ready explicit install caption and gate");
                    if(c.name.Contains("cash-needed"))Assert(!root.Q<Button>("buy-0").enabledInHierarchy&&root.Q<Label>("quest-status").text.Contains("NEED CASH")&&view.Model.ActivePool.Length==1,c.name+" full parts never masquerade as cash readiness");
                    var bytes = ReadTarget();
                    File.WriteAllBytes(Path.Combine(Root,"Evidence",c.name+".png"),bytes);
                    if(c.name.Contains("spin-pressed")){
                        Assert(Math.Abs(PixelUi.FaceTravel(spin)-5)<0.01f,"SPIN face travels5 pixels over fixed10-pixel base");
                        view.QaReleaseSpin();Assert(PixelUi.FaceTravel(spin)==0,"SPIN release returns face to raised position");
                    }
                    doc.panelSettings.targetTexture = null; target.Release(); UnityEngine.Object.Destroy(target); target=null;
                    captureIndex++;
                    if(captureIndex<Captures.Length) phase=0;
                    else {
                        view.SetQaState(new PlayerState(),0);spinStarted=EditorApplication.timeSinceStartup;view.QaBeginSpin();
                        string notice;var early=new SaveStore(Path.Combine(Environment.GetEnvironmentVariable("ZTM_QA_SAVE_DIR"),"level1-v1.json"),new UnityStateCodec()).Load(out notice);
                        Assert(view.QaSpinning&&early.spins==1&&early.cash==view.Model.Snapshot.cash,"spin rewards saved before cosmetic reel animation; relaunch cannot lose or double charge");
                        view.QaBuy(0);Assert(view.Model.PurchasedCount==0,"install blocked during unresolved reel animation");phase=2;
                    }
                }
                else if (phase == 2)
                {
                    if(EditorApplication.timeSinceStartup-spinStarted<1 || view.QaSpinning) return;
                    Assert(view.Model.Snapshot.spins==1 && view.Model.Snapshot.cash>=12,"runtime spin coroutine commits cash/fragments once");
                    var state = new PlayerState { cash=1000 };
                    for(int i=0;i<8;i++) state.fragments[i]=100;
                    view.SetQaState(state,0);
                    for(int i=0;i<8;i++) { view.QaBuy(i); view.QaBuy(i); }
                    Assert(view.Model.Complete,"runtime BUY handlers complete level without double-buy");
                    string notice;
                    var persisted = new SaveStore(Path.Combine(Environment.GetEnvironmentVariable("ZTM_QA_SAVE_DIR"),"level1-v1.json"),new UnityStateCodec()).Load(out notice);
                    Assert(new GameModel(persisted).Complete,"runtime full-level purchases reload from local disk");
                    int cash = view.Model.Snapshot.cash;
                    view.QaShowReset(); view.QaCancelReset();
                    Assert(view.Model.Complete && view.Model.Snapshot.cash==cash,"reset cancellation preserves room");
                    view.QaShowReset(); view.QaConfirmReset();
                    Assert(view.Model.Snapshot.cash==0 && view.Model.PurchasedCount==0 && view.Model.Snapshot.freeSpins==3,"confirmed runtime reset");
                    persisted = new SaveStore(Path.Combine(Environment.GetEnvironmentVariable("ZTM_QA_SAVE_DIR"),"level1-v1.json"),new UnityStateCodec()).Load(out notice);
                    Assert(persisted.cash==0 && !persisted.purchased[0] && persisted.freeSpins==3,"runtime confirmed reset reloads from disk");
                    target = new RenderTexture(480,960,0,RenderTextureFormat.ARGB32);
                    target.Create(); doc.panelSettings.targetTexture = target;
                    var home = new PlayerState { cash = 100 };
                    for(int i=0;i<8;i++) home.purchased[i]=true;
                    view.SetQaState(home,3);
                    demoStart = previousFeet = view.QaFeet;
                    demoStarted = nextDemoFrame = EditorApplication.timeSinceStartup;
                    Directory.CreateDirectory(Path.Combine(Root,"Evidence","walk-demo-frames-v0.3"));
                    phase=3;
                }
                else if (phase == 3)
                {
                    if (EditorApplication.timeSinceStartup < nextDemoFrame) return;
                    nextDemoFrame += 0.12;
                    var feet = view.QaFeet;
                    Assert(RoomGeometry.IsFree(feet),"rendered walk sample "+demoFrames+" feet clear");
                    demoDistance += FloorPoint.Distance(previousFeet,feet); previousFeet=feet;
                    if (view.QaWalking) GaitPhases.Add(view.QaAnimationPhase);
                    File.WriteAllBytes(Path.Combine(Root,"Evidence","walk-demo-frames-v0.3","frame-"+demoFrames.ToString("D3")+".png"),ReadTarget());
                    if (++demoFrames < 100) return;
                    Assert(demoDistance>1.5f,"actual editor game updates move heroine over time");
                    Assert(GaitPhases.Count==4,"actual visible gait visits four alternating cutout phases");
                    view.QaSave();
                    string notice;
                    var reloaded = new SaveStore(Path.Combine(Environment.GetEnvironmentVariable("ZTM_QA_SAVE_DIR"),"level1-v1.json"),new UnityStateCodec()).Load(out notice);
                    Assert(FloorPoint.Distance(new FloorPoint(reloaded.heroineU,reloaded.heroineV),view.QaFeet)<0.01f,"walking feet position survives disk reload");
                    Report.AppendLine("Actual rendered demo: "+demoFrames+" frames; "+(EditorApplication.timeSinceStartup-demoStarted).ToString("F1")+"s; travelled "+demoDistance.ToString("F2")+"m.");
                    doc.panelSettings.targetTexture=null; target.Release(); UnityEngine.Object.Destroy(target);target=null;
                    FrameTimes.Sort();
                    if(FrameTimes.Count>0) Report.AppendLine("Editor frame-time sample median_ms="+FrameTimes[FrameTimes.Count/2].ToString("F2")+" p95_ms="+FrameTimes[(int)(FrameTimes.Count*0.95)].ToString("F2")+"; no device benchmark.");
                    Report.AppendLine("Coverage: editor runtime rendered to texture, direct UI handlers; no physical touch/device test.");
                    target=new RenderTexture(360,640,0,RenderTextureFormat.ARGB32);target.Create();doc.panelSettings.targetTexture=target;
                    view.SetQaState(new PlayerState(),0);flowLastProgress=EditorApplication.timeSinceStartup;flowNextSpin=flowLastProgress+0.3;phase=4;
                }
                else if(phase==4)
                {
                    if(view.QaSpinning){if(!busyCaptured){Assert(!doc.rootVisualElement.Q<Button>("spin").enabledInHierarchy,"busy SPIN disabled");File.WriteAllBytes(Path.Combine(Root,"Evidence","v0.3-spin-busy-360x640.png"),ReadTarget());busyCaptured=true;}return;}
                    var fly=doc.rootVisualElement.Q<VisualElement>("reward-to-task");
                    if(!rewardCaptured&&fly!=null&&fly.resolvedStyle.display==DisplayStyle.Flex&&fly.worldBound.height>20){
                        Assert(fly.worldBound.center.y>doc.rootVisualElement.Q<VisualElement>("quest-item-well").worldBound.center.y,"actual matching item +n feedback travels from reels toward active task");
                        File.WriteAllBytes(Path.Combine(Root,"Evidence","v0.3-quest-collect-feedback-360x640.png"),ReadTarget());rewardCaptured=true;
                    }
                    if(EditorApplication.timeSinceStartup<flowNextSpin)return;
                    if(view.Model.Snapshot.spins>flowSpins){Assert(view.Model.Snapshot.spins==flowSpins+1,"live UI spin commits once "+view.Model.Snapshot.spins);flowSpins=view.Model.Snapshot.spins;flowLastProgress=EditorApplication.timeSinceStartup;}
                    while(!view.Model.Complete&&view.Model.CanBuy(view.Model.PurchasedCount)){
                        int item=view.Model.PurchasedCount,before=view.Model.Snapshot.cash;var button=doc.rootVisualElement.Q<Button>("buy-"+item);
                        Assert(button!=null&&button.enabledInHierarchy&&button.Q<Label>("button-caption").text=="BUY &\nINSTALL","live UI enables dual-resource BUY "+item);
                        view.QaClick("buy-"+item);view.QaBuy(item);
                        Assert(view.Model.PurchasedCount==item+1&&view.Model.Snapshot.cash==before-Economy.Upgrades[item].CashPrice,"live repeated BUY commits once "+item);flowBuys++;
                        string notice;var committed=new GameModel(new SaveStore(Path.Combine(Environment.GetEnvironmentVariable("ZTM_QA_SAVE_DIR"),"level1-v1.json"),new UnityStateCodec()).Load(out notice));
                        Assert(committed.PurchasedCount==view.Model.PurchasedCount&&committed.ActiveQuest==view.Model.ActiveQuest,"purchase and next stable quest committed before visuals "+item);
                    }
                    if(view.Model.Complete){flowNextSpin=EditorApplication.timeSinceStartup+0.2;phase=5;return;}
                    Assert(EditorApplication.timeSinceStartup-flowLastProgress<5,"live UI flow advances without stalling");
                    Assert(flowSpins<60,"live full-level pacing bound");string action=view.Model.CanRecover?"recovery":"spin";view.QaClick(action);view.QaClick(action);flowNextSpin=EditorApplication.timeSinceStartup+0.55;
                }
                else if(phase==5)
                {
                    if(EditorApplication.timeSinceStartup<flowNextSpin)return;
                    File.WriteAllBytes(Path.Combine(Root,"Evidence","v0.3-playthrough-complete-360x640.png"),ReadTarget());
                    Assert(flowBuys==8&&view.Model.Complete,"actual UI callback playthrough completes all eight purchases");
                    Assert(rewardCaptured,"matching item feedback rendered during actual level flow");
                    int cash=view.Model.Snapshot.cash;view.QaClick("menu");
                    Assert(doc.rootVisualElement.Q<VisualElement>("settings").style.display==DisplayStyle.Flex,"MENU opens actual settings");
                    view.QaClick("reset");Assert(doc.rootVisualElement.Q<VisualElement>("reset-confirmation").style.display==DisplayStyle.Flex,"settings RESET opens confirmation");
                    view.QaClick("keep-room");Assert(view.Model.Complete&&view.Model.Snapshot.cash==cash,"actual KEEP ROOM cancellation preserves completion and cash");
                    view.QaClick("menu");view.QaClick("reset");view.QaClick("confirm-reset");
                    Assert(view.Model.Snapshot.cash==0&&view.Model.PurchasedCount==0&&view.Model.Snapshot.freeSpins==3,"actual confirmed reset starts fresh");
                    view.SetQaState(new PlayerState {cash=0,freeSpins=0},0);view.QaClick("recovery");flowNextSpin=EditorApplication.timeSinceStartup+0.7;phase=6;
                }
                else if(phase==6)
                {
                    if(view.QaSpinning||EditorApplication.timeSinceStartup<flowNextSpin)return;
                    Assert(view.Model.Snapshot.spins==1&&view.Model.Snapshot.recoveries==1&&view.Model.Snapshot.cash>=12,"actual zero-cash recovery control grants and spins once");
                    Report.AppendLine("Actual UI callback level flow: "+flowSpins+" spins, "+flowBuys+" buys; menu, reset-cancel, confirmed-reset and recovery passed.");
                    doc.panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);target=null;
                    File.WriteAllText(Path.Combine(Root,"Evidence","runtime-v0.3-qa.txt"),Report.ToString());Debug.Log(Report.ToString());
                    SessionState.SetBool(Active,false);EditorApplication.Exit(0);
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e); File.WriteAllText(Path.Combine(Root,"Evidence","runtime-v0.3-qa.txt"),Report+"\n"+e);
                SessionState.SetBool(Active,false); EditorApplication.Exit(1);
            }
        }
        static byte[] ReadTarget()
        {
            var previous=RenderTexture.active; RenderTexture.active=target;
            var texture=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
            texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0); texture.Apply();
            byte[] bytes=texture.EncodeToPNG(); UnityEngine.Object.Destroy(texture); RenderTexture.active=previous;
            return bytes;
        }
    }
}
