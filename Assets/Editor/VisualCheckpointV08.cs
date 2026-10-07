using System;using System.IO;using System.Text;using UnityEditor;using UnityEngine;using UnityEngine.UIElements;
namespace ZeroToMillionaire.Editor {
 [InitializeOnLoad] public static class VisualCheckpointV08 {
  const string Key="ZTM_VISUAL_V08";static string Tag=>SessionState.GetString(Key+"tag","");static string Root=>Directory.GetParent(Application.dataPath).FullName;static GameView view;static RenderTexture rt;static int phase,warm,frame;static double at,lastFrame;static StringBuilder report=new StringBuilder();
  static VisualElement UI=>view.GetComponent<UIDocument>().rootVisualElement;
  static VisualCheckpointV08(){EditorApplication.update+=Tick;}
  public static void BatchStart(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"--ztm-visual-tag");var tag=i>=0?a[i+1]:"v08-after-1";if(File.Exists(Path.Combine(Root,"Evidence",tag+".txt")))throw new Exception("Immutable evidence exists");SessionState.SetString(Key+"tag",tag);SessionState.SetBool(Key,true);Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",Path.Combine(Root,".tooling",tag,"save"));Directory.CreateDirectory(Path.Combine(Root,".tooling",tag,"frames"));UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");EditorApplication.EnterPlaymode();}
  static void At(int next){phase=next;warm=0;at=EditorApplication.timeSinceStartup;}
  static bool Warm()=>++warm>18&&EditorApplication.timeSinceStartup-at>.30;
  static void Pointer(Button b,bool down){if(down){using(var e=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=b.worldBound.center})){e.target=b;b.SendEvent(e);}}else{using(var e=PointerUpEvent.GetPooled(new Event{type=EventType.MouseUp,button=0,mousePosition=b.worldBound.center})){e.target=b;b.SendEvent(e);}}}
  static void Capture(string path){var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);t.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.Destroy(t);RenderTexture.active=old;}
  static void Shot(string name)=>Capture(Path.Combine(Root,"Evidence",Tag+"-"+name+".png"));
  static void Frame(string sequence){if(EditorApplication.timeSinceStartup-lastFrame<.05)return;lastFrame=EditorApplication.timeSinceStartup;Capture(Path.Combine(Root,".tooling",Tag,"frames",sequence+"-"+(frame++).ToString("D4")+".png"));}
  static PlayerState Fixture(bool ready){var s=new PlayerState{cash=100,freeSpins=3};s.fragments[0]=ready?Economy.Upgrades[0].FragmentGoal:3;s.tutorial.completed=true;s.tutorial.storySeenMask=15;return s;}
  static void Tick(){if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying)return;try{
   if(view==null){view=UnityEngine.Object.FindFirstObjectByType<GameView>();if(view==null)return;}
   if(phase==0){rt=new RenderTexture(360,640,0,RenderTextureFormat.ARGB32);rt.Create();view.GetComponent<UIDocument>().panelSettings.targetTexture=rt;view.SetQaState(Fixture(false),0);At(1);}
   else if(phase==1&&Warm()){Shot("room");view.QaClick("menu");At(2);}
   else if(phase==2&&Warm()){Shot("menu");view.QaClick("close-settings");At(3);}
   else if(phase==3&&Warm()){frame=0;lastFrame=0;Frame("spin");Pointer(UI.Q<Button>("spin"),true);At(4);}
   else if(phase==4){Frame("spin");if(EditorApplication.timeSinceStartup-at>.16){Pointer(UI.Q<Button>("spin"),false);At(5);}}
   else if(phase==5){Frame("spin");if(EditorApplication.timeSinceStartup-at>3.3&&!view.QaSpinning){report.AppendLine("spin count="+view.Model.Snapshot.spins+" cash="+view.Model.Snapshot.cash+" parts="+view.Model.Snapshot.fragments[0]);if(view.Model.Snapshot.spins!=1)throw new Exception("Spin must commit once");Shot("reward");view.SetQaState(Fixture(true),0);At(6);}}
   else if(phase==6&&Warm()){Shot("ready");frame=0;lastFrame=0;Frame("repair");view.QaClick("buy-0");At(7);}
   else if(phase==7){Frame("repair");if(EditorApplication.timeSinceStartup-at>1.1){if(view.Model.PurchasedCount!=1||view.Model.Snapshot.cash!=40)throw new Exception("Purchase debit/persistence mismatch");Shot("installed");view.QaClick("menu");At(8);}}
   else if(phase==8&&Warm()){view.QaClick("tutorial-replay");At(9);}
   else if(phase==9&&Warm()){Shot("tutorial");report.AppendLine("PASS graphics-enabled Editor capture; real UITK pointer spin and explicit manual buy; no OS input; isolated fixture save");Finish();}
  }catch(Exception e){report.AppendLine("FAIL phase="+phase+" "+e);Debug.LogException(e);Finish();}}
  static void Finish(){SessionState.SetBool(Key,false);File.WriteAllText(Path.Combine(Root,"Evidence",Tag+".txt"),report.ToString());if(rt!=null){view.GetComponent<UIDocument>().panelSettings.targetTexture=null;rt.Release();UnityEngine.Object.Destroy(rt);rt=null;}EditorApplication.ExitPlaymode();EditorApplication.delayCall+=()=>EditorApplication.Exit(0);}
 }
}
