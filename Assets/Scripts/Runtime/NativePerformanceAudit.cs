#if UNITY_EDITOR || DEVELOPMENT_BUILD || ZTM_RELEASE_AUDIT
using Unity.Profiling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace ZeroToMillionaire
{
    public static class NativeMetrics
    {
        public static readonly ProfilerMarker Navigation=new ProfilerMarker("ZTM.Navigation");
        public static readonly ProfilerMarker Pose=new ProfilerMarker("ZTM.PoseAndDepth");
        public static readonly ProfilerMarker Refresh=new ProfilerMarker("ZTM.Refresh");
        public static readonly ProfilerMarker Quest=new ProfilerMarker("ZTM.DrawQuest");
        public static readonly ProfilerMarker Cell=new ProfilerMarker("ZTM.SetCell");
    }
}
#endif
#if (DEVELOPMENT_BUILD || ZTM_RELEASE_AUDIT) && !UNITY_EDITOR
namespace ZeroToMillionaire
{
    // Opt-in, local Development Player audit. All fixtures/save writes live
    // in a unique private directory; ordinary Release builds exclude this workload; a separately named Release audit twin opts in.
    public sealed class NativePerformanceAudit:MonoBehaviour
    {
        sealed class Metric {public string Name,Unit;public ProfilerRecorder Recorder;}
        [Serializable] sealed class Info
        {
            public string unity,os,cpu,gpu,graphicsApi,gpuDriver,savePath,persistentPath;
            public string counterTiming="ProfilerRecorder.LastValue is the completed preceding frame; wall interval uses Stopwatch timestamps. CSV frame labels the preceding frame.";
            public string input="UIDocument NavigationSubmitEvent callbacks, repeated attempts; no physical touch or hardware input claim.";
            public string workload="5s warm,10s idle walk,20s seeded continuous spin/manual purchase,5s ready final quest,10s completed walk,10s portrait resize360x640 then540x1080.";
            public string sourceCommit,startedUtc;
            public int cpuCount,ramMb,width,height,targetFrameRate,vsync,originalVsync,originalTarget,quality,firstFrame,lastFrame,sampleCount,spins,purchases,repeatChecks;
            public bool development,traceRequested,traceStarted,traceStopped,finished,privateProfileVerified,fullCompletion,reloadVerified,resizeFit,feetClear;
            public float travelMetres;public double seconds;
        }
        GameView view;VisualElement ui;string output,savePath;
        readonly List<Metric> metrics=new List<Metric>();
        readonly Stopwatch timer=new Stopwatch();
        const int Capacity=12000;
        readonly int[] frames=new int[Capacity],phaseIds=new int[Capacity],widths=new int[Capacity],heights=new int[Capacity];
        readonly double[] wallMs=new double[Capacity],elapsed=new double[Capacity];
        long[,] values;int count,phase=-1,previousPhase,originalVsync,originalTarget;double lastTime,nextAction;
        FloorPoint previousFeet;float distance;bool trace,traceStarted,traceStopped,readySet,completeBuy,secondResize,readyShot,completeShot;
        Info info;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartOptIn()
        {
            if(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ZTM_PERF_DIR")))return;
            var v=UnityEngine.Object.FindFirstObjectByType<GameView>();
            if(v==null)throw new InvalidOperationException("Audit requires Level1 GameView");
            v.gameObject.AddComponent<NativePerformanceAudit>();
        }
        IEnumerator Start()
        {
            output=Path.GetFullPath(Environment.GetEnvironmentVariable("ZTM_PERF_DIR"));
            savePath=Path.Combine(output,"save","level1-v1.json");
            string requested=Environment.GetEnvironmentVariable("ZTM_QA_SAVE_DIR");
            if(string.IsNullOrEmpty(requested)||Path.GetFullPath(Path.Combine(requested,"level1-v1.json"))!=savePath||
               savePath.StartsWith(Path.GetFullPath(Application.persistentDataPath),StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Audit refuses non-private profile");
            Directory.CreateDirectory(output);
            if(File.Exists(Path.Combine(output,"native-manifest.json")))throw new InvalidOperationException("Audit output must be new");
            view=GetComponent<GameView>();ui=GetComponent<UIDocument>().rootVisualElement;
            var store=(SaveStore)typeof(GameView).GetField("saves",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(view);
            string actual=(string)typeof(SaveStore).GetField("path",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(store);
            if(Path.GetFullPath(actual)!=savePath)throw new InvalidOperationException("GameView store is not private");
            originalVsync=QualitySettings.vSyncCount;originalTarget=Application.targetFrameRate;
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
            // Startup size comes from Player arguments; do not race DX11 startup with another resolution request.
            view.SetQaState(new PlayerState(),0);view.QaSave();
            trace=Environment.GetEnvironmentVariable("ZTM_PERF_TRACE")=="1";
            yield return new WaitForSecondsRealtime(2);
            InitMetrics();
            values=new long[Capacity,metrics.Count];
            info=new Info{unity=Application.unityVersion,os=SystemInfo.operatingSystem,cpu=SystemInfo.processorType,cpuCount=SystemInfo.processorCount,
                ramMb=SystemInfo.systemMemorySize,gpu=SystemInfo.graphicsDeviceName,gpuDriver=SystemInfo.graphicsDeviceVersion,
                graphicsApi=SystemInfo.graphicsDeviceType.ToString(),savePath=actual,persistentPath=Application.persistentDataPath,
                sourceCommit=Environment.GetEnvironmentVariable("ZTM_PERF_COMMIT"),startedUtc=DateTime.UtcNow.ToString("O"),
                development=UnityEngine.Debug.isDebugBuild,originalVsync=originalVsync,originalTarget=originalTarget,
                vsync=QualitySettings.vSyncCount,targetFrameRate=Application.targetFrameRate,quality=QualitySettings.GetQualityLevel(),
                width=Screen.width,height=Screen.height,traceRequested=trace,privateProfileVerified=true,feetClear=true,resizeFit=true,firstFrame=Time.frameCount};
            previousFeet=view.QaFeet;timer.Start();lastTime=0;previousPhase=0;
            UnityEngine.Debug.Log("ZTM_NATIVE_AUDIT_STARTED private="+actual);
        }
        void InitMetrics()
        {
            var handles=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(handles);
            var wanted=new HashSet<string>{"Main Thread","Render Thread","PlayerLoop","GC Allocated In Frame","GC Used Memory","GC Reserved Memory",
                "Total Used Memory","Total Reserved Memory","System Used Memory","CPU Main Thread Frame Time","CPU Render Thread Frame Time",
                "CPU Total Frame Time","GPU Frame Time","Draw Calls Count","Batches Count","Vertices Count","Gfx.WaitForPresentOnGfxThread",
                "WaitForTargetFPS","ZTM.Navigation","ZTM.PoseAndDepth","ZTM.Refresh","ZTM.DrawQuest","ZTM.SetCell"};
            var catalog=new StringBuilder("category,name,unit,selected\n");
            foreach(var handle in handles)
            {
                var d=ProfilerRecorderHandle.GetDescription(handle);
                bool uiMarker=d.Name.StartsWith("UIR.",StringComparison.Ordinal)||d.Name.StartsWith("UIElements.",StringComparison.Ordinal);
                bool select=wanted.Contains(d.Name)||(uiMarker&&metrics.Count<55);
                if(select)
                {
                    var rec=ProfilerRecorder.StartNew(d.Category,d.Name,1,ProfilerRecorderOptions.StartImmediately|
                        ProfilerRecorderOptions.WrapAroundWhenCapacityReached|ProfilerRecorderOptions.SumAllSamplesInFrame);
                    metrics.Add(new Metric{Name=d.Name,Unit=d.UnitType.ToString(),Recorder=rec});
                }
                if(select||d.Name.Contains("UIElements")||d.Name.StartsWith("UIR.")||d.Name.Contains("GC.Alloc"))
                    catalog.Append(Csv(d.Category.ToString())).Append(',').Append(Csv(d.Name)).Append(',').Append(d.UnitType).Append(',').Append(select).Append('\n');
            }
            File.WriteAllText(Path.Combine(output,"counter-catalog.csv"),catalog.ToString());
        }
        static string Csv(string s){return "\""+s.Replace("\"","\"\"")+"\"";}
        int Phase(double t){return t<5?0:t<15?1:t<35?2:t<40?3:t<50?4:5;}
        void LateUpdate()
        {
            if(info==null||info.finished)return;
            try
            {
                double t=timer.Elapsed.TotalSeconds;
                if(count>=Capacity)throw new InvalidOperationException("Capture capacity exceeded");
                frames[count]=Time.frameCount-1;phaseIds[count]=previousPhase;elapsed[count]=t;wallMs[count]=(t-lastTime)*1000;
                widths[count]=Screen.width;heights[count]=Screen.height;
                for(int m=0;m<metrics.Count;m++)values[count,m]=metrics[m].Recorder.Valid?metrics[m].Recorder.LastValue:-1;
                count++;lastTime=t;
                FloorPoint feet=view.QaFeet;distance+=FloorPoint.Distance(previousFeet,feet);previousFeet=feet;
                if(!RoomGeometry.IsFree(feet)){info.feetClear=false;throw new InvalidOperationException("Footprint blocked");}
                int current=Phase(t);
                if(current!=phase){phase=current;UnityEngine.Debug.Log("ZTM_PHASE "+phase+" at"+t.ToString("F3",CultureInfo.InvariantCulture));}
                if(trace&&t>=15&&!traceStarted)
                {
                    Profiler.logFile=Path.Combine(output,"progression-profiler");
                    Profiler.enableBinaryLog=true;Profiler.enabled=true;traceStarted=true;
                }
                if(traceStarted&&!traceStopped&&t>=25){Profiler.enabled=false;Profiler.enableBinaryLog=false;traceStopped=true;}
                if(phase==2&&t>=nextAction&&!view.QaSpinning)
                {
                    nextAction=t+.60;
                    int active=view.Model.ActiveQuest;
                    if(active>=0&&view.Model.CanBuy(active))
                    {
                        int owned=view.Model.PurchasedCount;int cash=view.Model.Snapshot.cash;
                        view.QaClick("buy-"+active);view.QaBuy(active);
                        if(view.Model.PurchasedCount!=owned+1||view.Model.Snapshot.cash!=cash-Economy.Upgrades[active].CashPrice)
                            throw new InvalidOperationException("Repeated purchase overdraw");
                        info.purchases++;info.repeatChecks++;
                    }
                    else if(view.Model.CanRecover)view.QaClick("recovery");
                    else if(view.Model.CanSpin)
                    {
                        int spins=view.Model.Snapshot.spins;view.QaClick("spin");view.QaClick("spin");
                        if(view.Model.Snapshot.spins!=spins+1)throw new InvalidOperationException("Spin callback/repeat guard");
                        info.spins++;info.repeatChecks++;
                    }
                }
                if(phase==3&&!readySet)
                {
                    if(view.QaSpinning)return;
                    var s=new PlayerState{cash=1000,freeSpins=0};for(int i=0;i<7;i++)s.purchased[i]=true;
                    s.fragments[7]=Economy.Upgrades[7].FragmentGoal;view.SetQaState(s,3);view.QaSave();readySet=true;
                }
                if(phase==3&&t>=37&&!readyShot){ScreenCapture.CaptureScreenshot(Path.Combine(output,"native-ready.png"));readyShot=true;}
                if(phase==4&&!completeBuy)
                {
                    int cash=view.Model.Snapshot.cash;view.QaClick("buy-7");view.QaBuy(7);
                    if(!view.Model.Complete||view.Model.Snapshot.cash!=cash-Economy.Upgrades[7].CashPrice)throw new InvalidOperationException("Final double-buy");
                    string notice;var loaded=new GameModel(new SaveStore(savePath,new UnityStateCodec()).Load(out notice));
                    info.reloadVerified=loaded.Complete&&loaded.Snapshot.cash==view.Model.Snapshot.cash;
                    if(!info.reloadVerified)throw new InvalidOperationException("Native private reload");
                    info.repeatChecks++;info.fullCompletion=true;completeBuy=true;
                }
                if(phase==4&&t>=43&&!completeShot){ScreenCapture.CaptureScreenshot(Path.Combine(output,"native-complete.png"));completeShot=true;}
                if(phase==5&&!secondResize)
                {
                    if(t<55&&Screen.width!=360)Screen.SetResolution(360,640,FullScreenMode.Windowed);
                    if(t>=55){Screen.SetResolution(540,1080,FullScreenMode.Windowed);secondResize=true;}
                }
                if(phase==5&&((t>52&&t<54)||t>57))
                {
                    var spin=ui.Q<Button>("spin");info.resizeFit&=spin!=null&&spin.worldBound.yMax<=ui.worldBound.yMax+1;
                }
                previousPhase=current;
                if(t>=60)Finish(null);
            }
            catch(Exception e){Finish(e.ToString());}
        }
        void Finish(string error)
        {
            Profiler.enabled=false;Profiler.enableBinaryLog=false;
            info.finished=error==null;info.traceStarted=traceStarted;info.traceStopped=traceStopped;info.sampleCount=count;
            info.lastFrame=Time.frameCount;info.travelMetres=distance;info.seconds=timer.Elapsed.TotalSeconds;
            timer.Stop();
            var csv=new StringBuilder(count*metrics.Count*10);
            csv.Append("frame,phase,seconds,wall_ms,width,height");
            foreach(var metric in metrics)csv.Append(',').Append(Csv(metric.Name+" ["+metric.Unit+"]"));
            csv.Append('\n');
            for(int i=0;i<count;i++)
            {
                csv.Append(frames[i]).Append(',').Append(phaseIds[i]).Append(',').Append(elapsed[i].ToString("F6",CultureInfo.InvariantCulture))
                    .Append(',').Append(wallMs[i].ToString("F6",CultureInfo.InvariantCulture)).Append(',').Append(widths[i]).Append(',').Append(heights[i]);
                for(int m=0;m<metrics.Count;m++)csv.Append(',').Append(values[i,m]);
                csv.Append('\n');
            }
            File.WriteAllText(Path.Combine(output,"native-frames.csv"),csv.ToString());
            File.WriteAllText(Path.Combine(output,"native-manifest.json"),JsonUtility.ToJson(info,true));
            if(error!=null)File.WriteAllText(Path.Combine(output,"native-error.txt"),error);
            foreach(var metric in metrics)metric.Recorder.Dispose();
            UnityEngine.Debug.Log("ZTM_NATIVE_AUDIT_FINISHED "+(error??"PASS"));
            Application.Quit(error==null?0:2);
        }
    }
}
#endif
