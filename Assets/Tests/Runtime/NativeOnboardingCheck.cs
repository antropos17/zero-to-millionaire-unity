#if ZTM_NATIVE_ONBOARDING_CHECK && ZTM_RELEASE_AUDIT && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire {
 // Test-only controller; omitted from the delivered clean Release. Frozen game methods are unchanged.
 public sealed class NativeOnboardingCheck:MonoBehaviour {
  GameView view;string output,savePath;StringBuilder checks=new StringBuilder();int assertions,firstFrame,captures;float nextCapture;bool recording;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Attach(){if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ZTM_ONBOARDING_CHECK_DIR")))new GameObject("Private Native Onboarding Check").AddComponent<NativeOnboardingCheck>();}
  VisualElement UI=>view.GetComponent<UIDocument>().rootVisualElement;
  Label Words=>UI.Q<Label>("dialogue-text");
  bool Visible=>UI.Q<VisualElement>("heroine-dialogue").resolvedStyle.display==DisplayStyle.Flex;
  void Check(bool ok,string message){assertions++;checks.AppendLine((ok?"PASS ":"FAIL ")+message);if(!ok)throw new InvalidOperationException(message);}
  static bool SameResources(PlayerState a,PlayerState b){if(a.cash!=b.cash||a.spins!=b.spins||a.freeSpins!=b.freeSpins||a.recoveries!=b.recoveries)return false;for(int i=0;i<8;i++)if(a.fragments[i]!=b.fragments[i]||a.purchased[i]!=b.purchased[i])return false;return true;}
  static bool SameTutorial(TutorialState a,TutorialState b)=>a.version==b.version&&a.step==b.step&&a.storySeenMask==b.storySeenMask&&a.completed==b.completed&&a.skipped==b.skipped;
  IEnumerator Start(){
   output=Path.GetFullPath(Environment.GetEnvironmentVariable("ZTM_ONBOARDING_CHECK_DIR"));savePath=Path.Combine(output,"save","level1-v1.json");
   var requested=Environment.GetEnvironmentVariable("ZTM_QA_SAVE_DIR");if(string.IsNullOrEmpty(requested)||Path.GetFullPath(Path.Combine(requested,"level1-v1.json"))!=savePath)throw new InvalidOperationException("private QA profile required");
   Directory.CreateDirectory(output);if(File.Exists(Path.Combine(output,"native-onboarding-checks.txt")))throw new InvalidOperationException("immutable new output required");
   view=UnityEngine.Object.FindFirstObjectByType<GameView>();var saves=(SaveStore)typeof(GameView).GetField("saves",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(view);var actual=(string)typeof(SaveStore).GetField("path",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(saves);
   Check(Path.GetFullPath(actual)==savePath,"actual native store private");Check(!Debug.isDebugBuild,"BuildOptions.None Release, separate test-only binary");
   QualitySettings.vSyncCount=0;Application.targetFrameRate=60;view.SetQaState(new PlayerState(),0);view.QaSave();yield return new WaitForSecondsRealtime(.6f);firstFrame=Time.frameCount;recording=true;nextCapture=0;
   IEnumerator sequence=Sequence();bool error=false;
   while(true){bool more=false;try{more=sequence.MoveNext();}catch(Exception e){checks.AppendLine("ERROR "+e);Debug.LogException(e);error=true;}if(error||!more)break;yield return sequence.Current;}
   recording=false;checks.AppendLine("RESULT "+(error?"FAIL":"PASS")+" assertions="+assertions+" actual_frames="+(Time.frameCount-firstFrame)+" captures="+captures+"; synthetic UIDocument callbacks, no physical input; separate test-only Release binary; frozen game code17f5252");File.WriteAllText(Path.Combine(output,"native-onboarding-checks.txt"),checks.ToString());Debug.Log("ZTM_NATIVE_ONBOARDING "+(error?"FAIL":"PASS"));Application.Quit(error?2:0);
  }
  IEnumerator Sequence(){
   Check(Visible&&Words.text==TutorialPolicy.Steps[0],"fresh introduction rendered");Shot("initial");var before=view.Model.Snapshot;view.QaClick("tutorial-next");yield return new WaitForSecondsRealtime(.35f);
   Check(SameResources(before,view.Model.Snapshot)&&TutorialPolicy.Step(view.Model.Snapshot)==1,"NEXT saves invitation without resources");view.QaSave();File.Copy(savePath,Path.Combine(output,"resume-step1.json"));
   UnityEngine.Object.Destroy(view.gameObject);yield return null;view=new GameObject("Private reload GameView").AddComponent<GameView>();yield return new WaitForSecondsRealtime(.6f);
   Check(Visible&&Words.text==TutorialPolicy.Steps[1]&&view.Model.Snapshot.tutorial.step==1,"in-process save reload resumes invitation");Shot("resume");
   before=view.Model.Snapshot;view.QaClick("tutorial-skip");yield return new WaitForSecondsRealtime(.3f);Check(view.Model.Snapshot.tutorial.skipped&&!Visible&&SameResources(before,view.Model.Snapshot),"SKIP persists and cannot grant resources");File.Copy(savePath,Path.Combine(output,"skipped.json"));
   UnityEngine.Object.Destroy(view.gameObject);yield return null;view=new GameObject("Private skipped reload GameView").AddComponent<GameView>();yield return new WaitForSecondsRealtime(.6f);Check(view.Model.Snapshot.tutorial.skipped&&!Visible,"skipped introduction stays absent after load");
   before=view.Model.Snapshot;view.QaClick("menu");view.QaClick("tutorial-replay");yield return new WaitForSecondsRealtime(.35f);
   for(int i=0;i<5;i++){Check(Visible&&SameResources(before,view.Model.Snapshot)&&SameTutorial(before.tutorial,view.Model.Snapshot.tutorial),"help-only replay step"+i+" keeps resources and saved tutorial");if(i==3)Check(Words.text!=TutorialPolicy.Steps[3],"replay buy help does not claim resources already ready");if(i==1)Shot("replay");view.QaClick("tutorial-next");yield return new WaitForSecondsRealtime(.35f);}
   Check(!Visible&&SameResources(before,view.Model.Snapshot),"replay closes with no rewards");
   var state=new PlayerState{cash=1000,spins=12};state.purchased[0]=state.purchased[1]=true;state.tutorial.completed=true;state.fragments[2]=37;view.SetQaState(state,0);view.QaBeginSpin();yield return new WaitForSecondsRealtime(.08f);
   Check(view.QaSpinning&&!Visible&&view.Model.Snapshot.tutorial.storySeenMask==0,"pending milestone waits during spin lease");yield return new WaitForSecondsRealtime(1.9f);
   Check(!view.QaSpinning&&Visible&&Words.text==TutorialPolicy.Stories[0]&&view.Model.Snapshot.tutorial.storySeenMask==1,"first story shown once after reel/reward feedback");Shot("queued-milestone");File.Copy(savePath,Path.Combine(output,"story-seen.json"));
   before=view.Model.Snapshot;view.QaClick("tutorial-next");yield return new WaitForSecondsRealtime(.35f);Check(!Visible&&SameResources(before,view.Model.Snapshot),"closing story cannot grant rewards");
   UnityEngine.Object.Destroy(view.gameObject);yield return null;view=new GameObject("Private milestone reload GameView").AddComponent<GameView>();yield return new WaitForSecondsRealtime(.5f);Check(!Visible&&view.Model.Snapshot.tutorial.storySeenMask==1,"seen milestone stays suppressed after load");
   state=new PlayerState{cash=777,spins=53};for(int i=0;i<8;i++)state.purchased[i]=true;state.tutorial.completed=true;state.tutorial.storySeenMask=1;before=state.Clone();view.SetQaState(state,0);yield return new WaitForSecondsRealtime(.4f);
   Check(Visible&&Words.text==TutorialPolicy.Stories[3]&&view.Model.Snapshot.tutorial.storySeenMask==15&&SameResources(before,view.Model.Snapshot),"multiple pending milestones collapse to latest completed story, no reward");Shot("latest-milestone");File.Copy(savePath,Path.Combine(output,"latest-seen.json"));
   view.QaClick("tutorial-next");yield return new WaitForSecondsRealtime(.3f);Check(!Visible,"no backlog barrage after final story");
   view.QaShowReset();view.QaCancelReset();Check(view.Model.Complete&&view.Model.Snapshot.cash==777,"reset cancel preserves complete resources");view.QaShowReset();view.QaConfirmReset();yield return new WaitForSecondsRealtime(.35f);Check(!view.Model.Complete&&view.Model.Snapshot.cash==0&&view.Model.Snapshot.freeSpins==3&&Visible&&Words.text==TutorialPolicy.Steps[0],"confirmed reset clears replay/story and returns saved intro");Shot("reset-intro");Check(Time.frameCount-firstFrame>100,"actual native advancing frames");
  }
  void Shot(string name){nextCapture=Time.unscaledTime+1f/12;ScreenCapture.CaptureScreenshot(Path.Combine(output,"native-"+name+".png"));checks.AppendLine("CAPTURE "+name+" frame="+Time.frameCount);}
  void LateUpdate(){if(!recording||Time.unscaledTime<nextCapture)return;nextCapture=Time.unscaledTime+1f/12;string dir=Path.Combine(output,"frames");Directory.CreateDirectory(dir);ScreenCapture.CaptureScreenshot(Path.Combine(dir,captures.ToString("D4")+".png"));captures++;}
 }
}
#endif
