using System;using System.IO;using System.Reflection;using System.Text;using UnityEditor;using UnityEngine;using UnityEngine.UIElements;
namespace ZeroToMillionaire.Editor {
 [InitializeOnLoad]public static class FocusIntentAuditV071 {
 const string Key="ZTM_FOCUS_INTENT_V071";static string tag=>SessionState.GetString(Key+"tag","");static GameView view;static RenderTexture target;static int phase,warm,count,failures;static StringBuilder report=new StringBuilder();static double at;static Button intended;static bool navigationIntent;
 static string Root=>Directory.GetParent(Application.dataPath).FullName;static VisualElement UI=>view.GetComponent<UIDocument>().rootVisualElement;
 static FocusIntentAuditV071(){EditorApplication.update+=Tick;}
 public static void BatchStart(){SessionState.SetBool(Key+"batch",true);Start("v071-focus-intent-before3");}
 public static string Start(string name){if(EditorApplication.isPlaying)throw new Exception("Owned Editor busy");if(!System.Text.RegularExpressions.Regex.IsMatch(name??"","^[a-z0-9-]+$"))throw new Exception("Scoped tag required");if(File.Exists(Path.Combine(Root,"Evidence",name+".txt")))throw new Exception("Immutable output exists");
 SessionState.SetString(Key+"tag",name);phase=warm=count=failures=0;view=null;report=new StringBuilder();Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",Path.Combine(Root,".tooling",name,"save"));UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();return "queued focus intent reproduction "+name;}
 static string Focus=>(UI.panel.focusController.focusedElement as VisualElement)?.name??"none";
 static void Expect(string wanted,string label){count++;if(Focus!=wanted)failures++;report.AppendLine((Focus==wanted?"PASS ":"FAIL ")+label+" expected="+wanted+" actual="+Focus);}
 static void Move(){var from=UI.panel.focusController.focusedElement as VisualElement??UI;using(var key=KeyDownEvent.GetPooled(new Event{type=EventType.KeyDown,keyCode=KeyCode.Tab})){key.target=from;from.SendEvent(key);}using(var nav=NavigationMoveEvent.GetPooled(NavigationMoveEvent.Direction.Next,EventModifiers.None)){nav.target=from;from.SendEvent(nav);}}
 static void Tap(Button b){using(var d=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=b.worldBound.center})){d.target=b;b.SendEvent(d);}using(var u=PointerUpEvent.GetPooled(new Event{type=EventType.MouseUp,button=0,mousePosition=b.worldBound.center})){u.target=b;b.SendEvent(u);}}
 static void At(int p){phase=p;warm=0;at=EditorApplication.timeSinceStartup;}
 static bool Warm()=>++warm>18;
 static void Capture(string suffix){var old=RenderTexture.active;RenderTexture.active=target;var t=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);t.ReadPixels(new Rect(0,0,target.width,target.height),0,0);t.Apply();File.WriteAllBytes(Path.Combine(Root,"Evidence",tag+"-"+suffix+".png"),t.EncodeToPNG());UnityEngine.Object.Destroy(t);RenderTexture.active=old;}
 static void Tick(){if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying)return;try{
 if(view==null){view=UnityEngine.Object.FindFirstObjectByType<GameView>();if(view==null)return;}
 if(phase==0){target=new RenderTexture(360,640,0,RenderTextureFormat.ARGB32);target.Create();view.GetComponent<UIDocument>().panelSettings.targetTexture=target;var s=new PlayerState{cash=1000};s.tutorial.completed=true;view.SetQaState(s,0);At(1);}
 else if(phase==1&&Warm()){view.QaClick("menu");Move();Move();intended=UI.Q<Button>("motion-toggle");navigationIntent=Focus=="motion-toggle";report.AppendLine("immediate paired-navigation focus="+Focus+" establishedIntent="+navigationIntent);At(2);}
 else if(phase==2&&Warm()){if(navigationIntent)Expect("motion-toggle","explicit paired Tab intent survives menu initial-focus scheduling");else report.AppendLine("INCONCLUSIVE rapid navigation preceded resolved modal layout; not classified as a defect");Capture("menu-after-navigation");view.QaClick("close-settings");At(3);}
 else if(phase==3&&Warm()){view.QaClick("menu");At(4);}
 else if(phase==4&&Warm()){view.QaClick("tutorial-replay");int before=view.Model.Snapshot.spins;Tap(UI.Q<Button>("spin"));report.AppendLine("replay then pointer SPIN immediate focus="+Focus+" committed_spins="+view.Model.Snapshot.spins);if(view.Model.Snapshot.spins!=before+1||Focus!="spin")throw new Exception("Actual pointer SPIN intent did not establish; inconclusive");At(5);}
 else if(phase==5&&++warm>=2){Expect("spin","explicit pointer SPIN intent survives replay deferred-focus scheduling");Capture("pointer-spin");At(6);}
 else if(phase==6&&!view.QaSpinning&&EditorApplication.timeSinceStartup-at>1.5){report.AppendLine("RESULT "+(failures==0?"PASS":"REPRODUCED")+" assertions="+count+" failures="+failures+"; real UITK paired events/pointer callbacks and actual Editor frames; no OS input");Finish();}
 }catch(Exception e){report.AppendLine("HARNESS FAIL phase="+phase+" "+e);Debug.LogException(e);Finish();}}
 static void Finish(){SessionState.SetBool(Key,false);File.WriteAllText(Path.Combine(Root,"Evidence",tag+".txt"),report.ToString());if(target!=null){if(view!=null)view.GetComponent<UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);target=null;}EditorApplication.ExitPlaymode();if(SessionState.GetBool(Key+"batch",false))EditorApplication.delayCall+=()=>EditorApplication.Exit(0);}
 }
}
