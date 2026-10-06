using System;using System.IO;using UnityEditor;using UnityEngine;using UnityEngine.UIElements;
namespace ZeroToMillionaire.Editor {
 [InitializeOnLoad]public static class DialogueFocusAuditV07 {
 const string Active="ZTM_DIALOGUE_FOCUS_V07";static int phase,warm;static GameView view;static string report="";
 static DialogueFocusAuditV07(){EditorApplication.update+=Tick;}
 public static string Start(){if(EditorApplication.isPlaying)throw new Exception("Owned Editor busy");Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",Path.Combine(Directory.GetParent(Application.dataPath).FullName,".tooling/dialogue-focus-v07/save"));phase=warm=0;view=null;report="";SessionState.SetBool(Active,true);EditorApplication.EnterPlaymode();return "private dialogue focus reproduction queued";}
 static string Focus(VisualElement ui)=>(ui.panel.focusController.focusedElement as VisualElement)?.name??"none";
 static void Tick(){if(!SessionState.GetBool(Active,false)||!EditorApplication.isPlaying)return;try{if(view==null){view=UnityEngine.Object.FindFirstObjectByType<GameView>();if(view==null)return;}var ui=view.GetComponent<UIDocument>().rootVisualElement;
 if(phase==0){var s=new PlayerState{cash=59,freeSpins=0};s.fragments[0]=19;s.tutorial.completed=true;view.SetQaState(s,0);view.enabled=false;phase=1;warm=0;}
 else if(phase==1&&++warm>24){view.QaClick("menu");phase=2;warm=0;}
 else if(phase==2&&++warm>24){view.QaClick("tutorial-replay");phase=3;warm=0;}
 else if(phase==3&&++warm>24){report="dialogueVisible="+ui.Q<VisualElement>("heroine-dialogue").resolvedStyle.display+" focused="+Focus(ui)+" next="+ui.Q<Button>("tutorial-next").enabledInHierarchy+"\n";var focused=ui.panel.focusController.focusedElement as VisualElement;using(var key=KeyDownEvent.GetPooled(new Event{type=EventType.KeyDown,keyCode=KeyCode.Return})){key.target=focused;focused.SendEvent(key);}using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=focused;focused.SendEvent(submit);}using(var key=KeyUpEvent.GetPooled(new Event{type=EventType.KeyUp,keyCode=KeyCode.Return})){key.target=focused;focused.SendEvent(key);}phase=4;warm=0;}
 else if(phase==4&&++warm>24){report+="afterEnter settings="+ui.Q<VisualElement>("settings").resolvedStyle.display+" focus="+Focus(ui)+" dialogueText="+ui.Q<Label>("dialogue-text").text+"\nSynthetic UITK events; no OS input";File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Evidence/v07-dialogue-focus-reproduction.txt"),report);view.enabled=true;SessionState.SetBool(Active,false);EditorApplication.ExitPlaymode();}
 }catch(Exception e){SessionState.SetBool(Active,false);Debug.LogException(e);EditorApplication.ExitPlaymode();}}
 }
}