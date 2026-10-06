#if DEVELOPMENT_BUILD && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire
{
    public static class ReliabilityBoot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeScene()
        {
            if(Environment.GetEnvironmentVariable("ZTM_SHUTDOWN_DIAG")=="1"){
                UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.EnabledWithStackTrace);
                Application.SetStackTraceLogType(LogType.Warning,StackTraceLogType.Full);
                Debug.Log("ZTM_DIAG allocation stacks enabled process-locally; no ForgiveLeaks or warning suppression.");
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterScene()
        {
            string input=Environment.GetEnvironmentVariable("ZTM_INPUT_AUDIT_DIR"),soak=Environment.GetEnvironmentVariable("ZTM_SOAK_AUDIT_DIR"),lifecycle=Environment.GetEnvironmentVariable("ZTM_LIFECYCLE_AUDIT_DIR");
            if(!string.IsNullOrEmpty(input))new GameObject("Private Windows Input Observer").AddComponent<NativeInputObserver>();
            if(!string.IsNullOrEmpty(soak))new GameObject("Private Session Observer").AddComponent<NativeSessionSoak>();
            if(!string.IsNullOrEmpty(lifecycle))new GameObject("Private Panel Lifecycle Observer").AddComponent<NativePanelLifecycle>();
        }
        public static string VerifyProfile(GameView view,string output)
        {
            string expected=Path.GetFullPath(Path.Combine(output,"save","level1-v1.json"));
            var store=(SaveStore)typeof(GameView).GetField("saves",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(view);
            string actual=Path.GetFullPath((string)typeof(SaveStore).GetField("path",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(store));
            if(actual!=expected||actual.StartsWith(Path.GetFullPath(Application.persistentDataPath),StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Reliability observer refuses user profile");
            Directory.CreateDirectory(output);return actual;
        }
    }
    public sealed class NativeInputObserver:MonoBehaviour
    {
        [Serializable] sealed class ButtonInfo {public string name;public bool enabled,visible,pressed;public float x,y,w,h,travel;}
        [Serializable] sealed class StateInfo
        {
            public int frame,width,height,activeQuest;public double seconds;public bool focused,spinning,complete,settings,confirmation,privateProfile;
            public string savePath;public PlayerState state;public ButtonInfo[] buttons;
        }
        GameView view;VisualElement root;string output,savePath;double started,next;
        readonly StringBuilder events=new StringBuilder("frame,time,event,target,pointer_id,pointer_type,buttons,x,y\n");
        void Start()
        {
            view=UnityEngine.Object.FindFirstObjectByType<GameView>();root=view.GetComponent<UIDocument>().rootVisualElement;
            output=Path.GetFullPath(Environment.GetEnvironmentVariable("ZTM_INPUT_AUDIT_DIR"));
            savePath=ReliabilityBoot.VerifyProfile(view,output);started=Time.realtimeSinceStartupAsDouble;
            string fixture=Environment.GetEnvironmentVariable("ZTM_INPUT_FIXTURE")??"fresh";
            var state=new PlayerState();
            if(fixture=="ready"){state.cash=1000;state.freeSpins=0;state.fragments[0]=Economy.Upgrades[0].FragmentGoal;}
            else if(fixture!="fresh"&&fixture!="load")throw new InvalidOperationException("Unknown private fixture");
            if(fixture!="load"){view.SetQaState(state,0);view.QaSave();}
            root.RegisterCallback<PointerDownEvent>(e=>Record("down",e),TrickleDown.TrickleDown);
            root.RegisterCallback<PointerUpEvent>(e=>Record("up",e),TrickleDown.TrickleDown);
            root.RegisterCallback<PointerCancelEvent>(e=>Record("cancel",e),TrickleDown.TrickleDown);
            root.RegisterCallback<PointerCaptureOutEvent>(e=>events.Append(Time.frameCount).Append(',').Append(Time.realtimeSinceStartupAsDouble-started).Append(",capture_out,").Append((e.target as VisualElement)?.name).Append(",").Append(e.pointerId).Append(",,,,\n"),TrickleDown.TrickleDown);
            Debug.Log("ZTM_INPUT_OBSERVER private="+savePath+" fixture="+fixture+"; all test actions come from Windows pointer, observer dispatches no action.");
        }
        void Record<T>(string kind,PointerEventBase<T> e) where T:PointerEventBase<T>,new()
        {
            events.Append(Time.frameCount).Append(',').Append((Time.realtimeSinceStartupAsDouble-started).ToString("F6",CultureInfo.InvariantCulture))
                .Append(',').Append(kind).Append(',').Append((e.target as VisualElement)?.name).Append(',').Append(e.pointerId).Append(',')
                .Append(e.pointerType).Append(',').Append(e.pressedButtons).Append(',').Append(e.position.x.ToString("F3",CultureInfo.InvariantCulture))
                .Append(',').Append(e.position.y.ToString("F3",CultureInfo.InvariantCulture)).Append('\n');
        }
        void LateUpdate()
        {
            if(view==null||Time.realtimeSinceStartupAsDouble<next)return;next=Time.realtimeSinceStartupAsDouble+.08;
            var buttons=new List<ButtonInfo>();
            float scale=Screen.width/Mathf.Max(1,root.worldBound.width);
            foreach(var button in root.Query<Button>().ToList()){
                if(string.IsNullOrEmpty(button.name))continue;
                bool visible=true;for(VisualElement p=button;p!=null;p=p.parent)if(p.resolvedStyle.display==DisplayStyle.None){visible=false;break;}
                Rect b=button.worldBound;
                buttons.Add(new ButtonInfo{name=button.name,enabled=button.enabledInHierarchy,visible=visible,pressed=button.ClassListContains("pressed"),
                    x=(b.x-root.worldBound.x)*scale,y=(b.y-root.worldBound.y)*scale,w=b.width*scale,h=b.height*scale,travel=PixelUi.FaceTravel(button)});
            }
            var state=new StateInfo{frame=Time.frameCount,seconds=Time.realtimeSinceStartupAsDouble-started,width=Screen.width,height=Screen.height,focused=Application.isFocused,
                activeQuest=view.Model.ActiveQuest,spinning=view.QaSpinning,complete=view.Model.Complete,savePath=savePath,privateProfile=true,state=view.Model.Snapshot,
                settings=root.Q<VisualElement>("settings").resolvedStyle.display!=DisplayStyle.None,confirmation=root.Q<VisualElement>("reset-confirmation").resolvedStyle.display!=DisplayStyle.None,
                buttons=buttons.ToArray()};
            string path=Path.Combine(output,"observer-state.json"),tmp=path+".tmp";
            try{File.WriteAllText(tmp,JsonUtility.ToJson(state,true));if(File.Exists(path))File.Replace(tmp,path,null);else File.Move(tmp,path);}
            catch(IOException){ /* A short-lived external read can hold the file; next sample retries. */ }
        }
        void OnApplicationQuit()
        {
            if(!string.IsNullOrEmpty(output))File.WriteAllText(Path.Combine(output,"pointer-events.csv"),events.ToString());
        }
    }
    public sealed class NativePanelLifecycle:MonoBehaviour
    {
        IEnumerator Start()
        {
            string output=Path.GetFullPath(Environment.GetEnvironmentVariable("ZTM_LIFECYCLE_AUDIT_DIR"));
            GameView live=UnityEngine.Object.FindFirstObjectByType<GameView>();
            ReliabilityBoot.VerifyProfile(live,output);
            var rows=new StringBuilder("cycle,attached_panels,after_destroy_panels,owned_panel_destroyed,spin_callback,manual_buy\n");
            int startPanels=Resources.FindObjectsOfTypeAll<PanelSettings>().Length,orphans=0,spins=0,buys=0;
            for(int cycle=0;cycle<10;cycle++){
                yield return null;yield return null;
                int attached=Resources.FindObjectsOfTypeAll<PanelSettings>().Length;
                PanelSettings owned=live.GetComponent<UIDocument>().panelSettings;
                live.SetQaState(new PlayerState(),0);live.QaSave();
                live.QaClick("spin");live.QaClick("spin");
                bool spin=live.Model.Snapshot.spins==1&&live.QaSpinning;if(spin)spins++;
                while(live.QaSpinning)yield return null;
                var ready=new PlayerState{cash=1000,freeSpins=0};ready.fragments[0]=Economy.Upgrades[0].FragmentGoal;
                live.SetQaState(ready,0);live.QaClick("buy-0");live.QaBuy(0);
                bool buy=live.Model.PurchasedCount==1&&live.Model.Snapshot.cash==1000-Economy.Upgrades[0].CashPrice;if(buy)buys++;
                if(cycle==0){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"native-live.png"));}
                Destroy(live.gameObject);live=null;yield return null;yield return null;
                int after=Resources.FindObjectsOfTypeAll<PanelSettings>().Length;bool destroyed=owned==null;if(!destroyed)orphans++;
                rows.Append(cycle).Append(',').Append(attached).Append(',').Append(after).Append(',').Append(destroyed).Append(',').Append(spin).Append(',').Append(buy).Append('\n');
                if(cycle<9)live=new GameObject("Private Lifecycle Game").AddComponent<GameView>();
            }
            File.WriteAllText(Path.Combine(output,"lifecycle.csv"),rows.ToString());
            string summary="{\"cycles\":10,\"start_panels\":"+startPanels+",\"final_panels\":"+Resources.FindObjectsOfTypeAll<PanelSettings>().Length+",\"orphan_panels\":"+orphans+",\"spin_checks\":"+spins+",\"buy_checks\":"+buys+",\"private_profile\":true,\"input\":\"UI NavigationSubmitEvent callbacks; no OS pointer\"}";
            File.WriteAllText(Path.Combine(output,"lifecycle-summary.json"),summary);Debug.Log("ZTM_PANEL_LIFECYCLE "+summary);
            yield return null;Application.Quit(spins==10&&buys==10?0:2);
        }
    }
    public sealed class NativeSessionSoak:MonoBehaviour
    {
        [Serializable] sealed class Summary
        {
            public string input="Automated NavigationSubmitEvent callbacks for soak; separate Windows pointer audit supplies OS input evidence.",savePath,startedUtc;
            public int secondsRequested,frames,cycles,spins,buys,resets,repeatChecks,reloadChecks,buckets,errors,originalVsync,originalTarget;
            public float distance;public double seconds;public bool privateProfile,finished;
        }
        GameView view;string output,savePath;Summary info;double start,nextAction,nextBucket,last;FloorPoint prior;
        int frameCount,bucketFrames;float distance;bool finished;
        readonly double[] frameMs=new double[2048];readonly StringBuilder rows=new StringBuilder("seconds,frames,cycles,spins,buys,median_ms,p95_ms,p99_ms,max_ms,over33_ms,over50_ms,allocated_bytes,system_used,unity_used,unity_reserved,gc_used,gc_reserved,gc_collections0\n");
        ProfilerRecorder alloc,system,used,reserved,gcUsed,gcReserved;long allocated;
        void Start()
        {
            view=UnityEngine.Object.FindFirstObjectByType<GameView>();output=Path.GetFullPath(Environment.GetEnvironmentVariable("ZTM_SOAK_AUDIT_DIR"));
            savePath=ReliabilityBoot.VerifyProfile(view,output);
            int duration=900;int.TryParse(Environment.GetEnvironmentVariable("ZTM_SOAK_SECONDS")??"900",out duration);
            if(duration<60||duration>1200)throw new InvalidOperationException("Soak must be60..1200seconds");
            info=new Summary{savePath=savePath,secondsRequested=duration,startedUtc=DateTime.UtcNow.ToString("O"),privateProfile=true,
                originalVsync=QualitySettings.vSyncCount,originalTarget=Application.targetFrameRate};
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
            view.SetQaState(new PlayerState(),0);view.QaSave();
            alloc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);
            system=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"System Used Memory",1);
            used=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"Total Used Memory",1);
            reserved=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"Total Reserved Memory",1);
            gcUsed=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Used Memory",1);
            gcReserved=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Reserved Memory",1);
            start=last=Time.realtimeSinceStartupAsDouble;nextBucket=start+10;prior=view.QaFeet;
            Debug.Log("ZTM_SOAK_START "+duration+"s private="+savePath);
        }
        void LateUpdate()
        {
            if(info==null||finished)return;
            try{
                double now=Time.realtimeSinceStartupAsDouble,t=now-start,dt=(now-last)*1000;last=now;
                frameCount++;if(bucketFrames>=frameMs.Length)throw new InvalidOperationException("Unexpected frame burst");frameMs[bucketFrames++]=dt;
                if(alloc.Valid)allocated+=alloc.LastValue;
                if(!RoomGeometry.IsFree(view.QaFeet))throw new InvalidOperationException("Soak feet blocked");
                distance+=FloorPoint.Distance(prior,view.QaFeet);prior=view.QaFeet;
                if(t>=3&&now>=nextAction&&!view.QaSpinning){
                    nextAction=now+.6;
                    if(view.Model.Complete){
                        string notice;var disk=new GameModel(new SaveStore(savePath,new UnityStateCodec()).Load(out notice));
                        if(!disk.Complete||disk.Snapshot.cash!=view.Model.Snapshot.cash)throw new InvalidOperationException("Soak completion reload");
                        info.cycles++;info.reloadChecks++;
                        view.QaClick("menu");view.QaClick("reset");view.QaClick("keep-room");
                        if(!view.Model.Complete)throw new InvalidOperationException("Soak reset cancel changed model");
                        view.QaClick("menu");view.QaClick("reset");view.QaClick("confirm-reset");
                        if(view.Model.PurchasedCount!=0||view.Model.Snapshot.freeSpins!=3)throw new InvalidOperationException("Soak confirmed reset");
                        info.resets++;prior=view.QaFeet;
                    }else{
                        int quest=view.Model.ActiveQuest;
                        if(view.Model.CanBuy(quest)){
                            int count=view.Model.PurchasedCount,cash=view.Model.Snapshot.cash;
                            view.QaClick("buy-"+quest);view.QaBuy(quest);
                            if(view.Model.PurchasedCount!=count+1||view.Model.Snapshot.cash!=cash-Economy.Upgrades[quest].CashPrice)throw new InvalidOperationException("Soak double purchase");
                            info.buys++;info.repeatChecks++;
                        }else if(view.Model.CanRecover)view.QaClick("recovery");
                        else if(view.Model.CanSpin){
                            int spins=view.Model.Snapshot.spins;view.QaClick("spin");view.QaClick("spin");
                            int changed=view.Model.Snapshot.spins-spins;if(changed<0||changed>1)throw new InvalidOperationException("Soak rapid spin");
                            info.spins+=changed;info.repeatChecks++;
                        }
                    }
                }
                if(now>=nextBucket){Bucket(t);nextBucket=now+10;}
                if(t>=info.secondsRequested)Finish(null);
            }catch(Exception e){info.errors++;Finish(e.ToString());}
        }
        static string N(double value){return value.ToString("F6",CultureInfo.InvariantCulture);}
        static long V(ProfilerRecorder r){return r.Valid?r.LastValue:-1;}
        void Bucket(double seconds)
        {
            Array.Sort(frameMs,0,bucketFrames);int over33=0,over50=0;for(int i=0;i<bucketFrames;i++){if(frameMs[i]>1000.0/30)over33++;if(frameMs[i]>50)over50++;}
            rows.Append(N(seconds)).Append(',').Append(bucketFrames).Append(',').Append(info.cycles).Append(',').Append(info.spins).Append(',').Append(info.buys).Append(',')
                .Append(N(frameMs[bucketFrames/2])).Append(',').Append(N(frameMs[(int)((bucketFrames-1)*.95)])).Append(',').Append(N(frameMs[(int)((bucketFrames-1)*.99)])).Append(',')
                .Append(N(frameMs[bucketFrames-1])).Append(',').Append(over33).Append(',').Append(over50).Append(',').Append(allocated).Append(',').Append(V(system)).Append(',')
                .Append(V(used)).Append(',').Append(V(reserved)).Append(',').Append(V(gcUsed)).Append(',').Append(V(gcReserved)).Append(',').Append(GC.CollectionCount(0)).Append('\n');
            info.frames=frameCount;info.seconds=seconds;info.distance=distance;info.buckets++;
            File.WriteAllText(Path.Combine(output,"soak-trends.csv"),rows.ToString());File.WriteAllText(Path.Combine(output,"soak-summary.json"),JsonUtility.ToJson(info,true));
            bucketFrames=0;allocated=0;
        }
        void Finish(string error)
        {
            if(finished)return;finished=true;
            if(bucketFrames>0)Bucket(Time.realtimeSinceStartupAsDouble-start);
            info.finished=error==null;
            File.WriteAllText(Path.Combine(output,"soak-summary.json"),JsonUtility.ToJson(info,true));
            if(error!=null)File.WriteAllText(Path.Combine(output,"soak-error.txt"),error);
            alloc.Dispose();system.Dispose();used.Dispose();reserved.Dispose();gcUsed.Dispose();gcReserved.Dispose();
            Debug.Log("ZTM_SOAK_FINISH "+(error??"PASS")+" cycles="+info.cycles);
            Application.Quit(error==null?0:2);
        }
    }
}
#endif
