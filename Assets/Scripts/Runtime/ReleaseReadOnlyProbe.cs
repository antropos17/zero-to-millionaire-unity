#if !UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire
{
 // Read-only instrumentation, activated solely by a validated explicit private profile.
 // No fixture setter, input dispatch or purchase/reset/recovery action exists here.
 public sealed class ReleaseReadOnlyProbe:MonoBehaviour
 {
  [Serializable]sealed class Evidence{
   public bool development,qaHooksPresent,automatedHarnessPresent,profileVerified,recoveryRequired,recoveryVisible;
   public string actualSavePath,unity,graphicsApi,questTitle,questStatus,dialogueText;public bool dialogueVisible;public int tutorialStep,taskCashPrice,taskFragmentGoal;
   public int firstFrame,lastFrame,samples,width,height,owned,activeQuest,drawCounterNonzero;
   public double seconds,medianMs,p95Ms,p99Ms,maxMs;public float distance;public bool complete,spinEnabled,buyEnabled;
   public PlayerState state;
  }
  GameView view;SaveStore store;Evidence info;string output;double start,last;int count,nonzero;float distance;FloorPoint prior;
  readonly double[] intervals=new double[12000];ProfilerRecorder draws;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void Attach(){if(RuntimePrivateProfile.SelectedDirectory!=null)new GameObject("Private read-only Release observer").AddComponent<ReleaseReadOnlyProbe>();}
  IEnumerator Start(){
   view=UnityEngine.Object.FindFirstObjectByType<GameView>();
   var saves=store=(SaveStore)typeof(GameView).GetField("saves",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(view);
   string actual=(string)typeof(SaveStore).GetField("path",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(saves);
   string expected=Path.Combine(RuntimePrivateProfile.SelectedDirectory,"level1-v1.json");
   if(Path.GetFullPath(actual)!=Path.GetFullPath(expected))throw new InvalidOperationException("Read-only observer refuses non-private store");
   output=Path.Combine(RuntimePrivateProfile.SelectedDirectory,"readonly");Directory.CreateDirectory(output);
   info=new Evidence{development=Debug.isDebugBuild,qaHooksPresent=typeof(GameView).GetMethod("SetQaState")!=null,
    automatedHarnessPresent=typeof(GameView).Assembly.GetType("ZeroToMillionaire.NativePerformanceAudit")!=null,profileVerified=true,
    actualSavePath=actual,unity=Application.unityVersion,graphicsApi=SystemInfo.graphicsDeviceType.ToString(),firstFrame=Time.frameCount};
   start=last=Time.realtimeSinceStartupAsDouble;var s=view.Model.Snapshot;prior=new FloorPoint(s.heroineU,s.heroineV);
   draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);
   yield return new WaitForSecondsRealtime(3);yield return new WaitForEndOfFrame();
   ScreenCapture.CaptureScreenshot(Path.Combine(output,"native-first.png"));Write();
  }
  void LateUpdate(){
   if(info==null)return;double now=Time.realtimeSinceStartupAsDouble;
   if(count<intervals.Length)intervals[count++]=(now-last)*1000;last=now;if(draws.Valid&&draws.LastValue>0)nonzero++;
   var s=view.Model.Snapshot;var feet=new FloorPoint(s.heroineU,s.heroineV);distance+=FloorPoint.Distance(prior,feet);prior=feet;
  }
  void Write(){
   if(info==null)return;var ui=view.GetComponent<UIDocument>().rootVisualElement;
   var sorted=new double[count];Array.Copy(intervals,sorted,count);Array.Sort(sorted);
   info.lastFrame=Time.frameCount;info.samples=count;info.seconds=Time.realtimeSinceStartupAsDouble-start;info.distance=distance;info.drawCounterNonzero=nonzero;
   if(count>0){info.medianMs=sorted[count/2];info.p95Ms=sorted[(int)((count-1)*.95)];info.p99Ms=sorted[(int)((count-1)*.99)];info.maxMs=sorted[count-1];}
   info.recoveryRequired=store.RecoveryRequired;info.recoveryVisible=ui.Q<VisualElement>("save-recovery")?.resolvedStyle.display==DisplayStyle.Flex;
   info.state=view.Model.Snapshot;info.owned=view.Model.PurchasedCount;info.activeQuest=view.Model.ActiveQuest;info.complete=view.Model.Complete;info.width=Screen.width;info.height=Screen.height;
   info.questTitle=ui.Q<Label>("quest-title")?.text;info.questStatus=ui.Q<Label>("quest-status")?.text;info.spinEnabled=ui.Q<Button>("spin").enabledInHierarchy;
   info.tutorialStep=TutorialPolicy.Step(info.state);info.dialogueText=ui.Q<Label>("dialogue-text")?.text;info.dialogueVisible=ui.Q<VisualElement>("heroine-dialogue")?.resolvedStyle.display==DisplayStyle.Flex; if(info.activeQuest>=0){info.taskCashPrice=view.Model.Definition(info.activeQuest).CashPrice;info.taskFragmentGoal=view.Model.Definition(info.activeQuest).FragmentGoal;}
   info.buyEnabled=info.activeQuest>=0&&(ui.Q<Button>("buy-"+info.activeQuest)?.enabledInHierarchy??false);
   File.WriteAllText(Path.Combine(output,"release-readonly.json"),JsonUtility.ToJson(info,true));
  }
  void OnApplicationQuit(){Write();draws.Dispose();Debug.Log("ZTM_RELEASE_READONLY frameCount="+Time.frameCount+"; no observer input dispatch.");}
 }
}
#endif