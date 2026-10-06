using System;using System.IO;using UnityEditor;using UnityEngine;using UnityEngine.UIElements;
namespace ZeroToMillionaire.Editor {
 [InitializeOnLoad]public static class RuntimeNavigationAuditV07 {
 const string Active="ZTM_NAVIGATION_V07";static int phase,warm,failures;static GameView view;static string report="";
 static RuntimeNavigationAuditV07(){EditorApplication.update+=Tick;}
 public static string Start(){if(EditorApplication.isPlaying)throw new Exception("Owned Editor busy");Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",Path.Combine(Directory.GetParent(Application.dataPath).FullName,".tooling/navigation-v07/save"));phase=warm=failures=0;view=null;report="";SessionState.SetBool(Active,true);EditorApplication.EnterPlaymode();return "private paired/native-navigation reproduction queued";}
 static string Focus(VisualElement ui)=>(ui.panel.focusController.focusedElement as VisualElement)?.name??"none";
 static void Move(VisualElement target,NavigationMoveEvent.Direction dir){using(var e=NavigationMoveEvent.GetPooled(dir,EventModifiers.None)){e.target=target;target.SendEvent(e);}}
 static void Expect(VisualElement ui,string wanted,string name){string actual=Focus(ui);if(actual!=wanted)failures++;report+=name+" expected="+wanted+" actual="+actual+"\n";}
 static void Tick(){if(!SessionState.GetBool(Active,false)||!EditorApplication.isPlaying)return;try{if(view==null){view=UnityEngine.Object.FindFirstObjectByType<GameView>();if(view==null)return;}var ui=view.GetComponent<UIDocument>().rootVisualElement;
 if(phase==0){var s=new PlayerState{cash=59,freeSpins=0};s.fragments[0]=19;s.tutorial.completed=true;view.SetQaState(s,0);view.enabled=false;phase=1;warm=0;}
 else if(phase==1&&++warm>24){view.QaClick("menu");phase=2;warm=0;}
 else if(phase==2&&++warm>24){var old=ui.panel.focusController.focusedElement as VisualElement;using(var key=KeyDownEvent.GetPooled(new Event{type=EventType.KeyDown,keyCode=KeyCode.Tab})){key.target=old;old.SendEvent(key);}Move(old,NavigationMoveEvent.Direction.Next);Expect(ui,"motion-toggle","paired KeyDown+NavigationMove Next original target");Move(ui.panel.focusController.focusedElement as VisualElement,NavigationMoveEvent.Direction.Previous);Expect(ui,"tutorial-replay","navigation-only Previous");foreach(var wanted in new[]{"motion-toggle","reset","close-settings","tutorial-replay"}){Move(ui.panel.focusController.focusedElement as VisualElement,NavigationMoveEvent.Direction.Next);Expect(ui,wanted,"navigation-only Next modal wrap");}report+="RESULT "+(failures==0?"PASS":"FAIL")+" failures="+failures+"; synthetic events, no OS keyboard claim";File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Evidence/v07-navigation-reviewed.txt"),report);view.enabled=true;SessionState.SetBool(Active,false);EditorApplication.ExitPlaymode();}
 }catch(Exception e){SessionState.SetBool(Active,false);Debug.LogException(e);EditorApplication.ExitPlaymode();}}
 }
}