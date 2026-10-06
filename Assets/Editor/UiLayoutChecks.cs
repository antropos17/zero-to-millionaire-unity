using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire.Editor
{
 [InitializeOnLoad] public static class UiLayoutChecks
 {
  const string Active="ZTM_UI_LAYOUT_ACTIVE";static GameView view;static RenderTexture target;static int variant,quest,size,phase,warm,count;static double started;static readonly StringBuilder report=new StringBuilder();
  static string Root=>Directory.GetParent(Application.dataPath).FullName;
  static UiLayoutChecks(){EditorApplication.update+=Tick;}
  public static void Start(){if(EditorApplication.isPlaying)throw new Exception("Stop own Playmode first");phase=variant=quest=size=warm=count=0;view=null;report.Clear();Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",Path.Combine(Root,".tooling","ui-layout-qa-save"));UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");SessionState.SetBool(Active,true);EditorApplication.EnterPlaymode();}
  static void Check(bool value,string label){if(!value)throw new Exception(label);count++;}
  static void Tick(){if(!SessionState.GetBool(Active,false)||!EditorApplication.isPlaying)return;try{
   if(view==null)view=UnityEngine.Object.FindFirstObjectByType<GameView>();if(view==null)return;var doc=view.GetComponent<UIDocument>();var ui=doc.rootVisualElement;
   if(phase==0){if(target==null){target=new RenderTexture(size==0?360:480,size==0?640:960,0,RenderTextureFormat.ARGB32);target.Create();doc.panelSettings.targetTexture=target;}
    var d=Economy.Upgrades[quest];var state=new PlayerState{cash=variant==1||variant==3?d.CashPrice:d.CashPrice-1,freeSpins=0};for(int i=0;i<quest;i++)state.purchased[i]=true;state.fragments[quest]=variant>=2?d.FragmentGoal:d.FragmentGoal-1;view.SetQaState(state,quest/2);warm=0;started=EditorApplication.timeSinceStartup;phase=1;
   }else if(phase==1){if(++warm<20||EditorApplication.timeSinceStartup-started<.1)return;var d=Economy.Upgrades[quest];var buy=ui.Q<Button>("buy-"+quest);
    Check(buy!=null&&buy.enabledInHierarchy==(variant==3),"both-resource button gate q"+quest+" variant"+variant);Check(buy.worldBound.width>=44&&buy.worldBound.height>=44,"buy target44");
    Check(ui.Q<Label>("fragment-count-"+quest).text==Math.Min(view.Model.Snapshot.fragments[quest],d.FragmentGoal)+"/"+d.FragmentGoal,"parts count preserved");Check(ui.Q<Label>("cash-count").text==view.Model.Snapshot.cash+"/"+d.CashPrice,"cash available/price explicit");
    foreach(string name in new[]{"quest-title","quest-status","cash-count","fragment-count-"+quest}){var label=ui.Q<Label>(name);var measured=label.MeasureTextSize(label.text,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined);Check(measured.x<=label.contentRect.width+1,"unclipped "+name+" q"+quest+" size"+size+" needs"+measured.x+" has"+label.contentRect.width);}
    Check(ui.Q<VisualElement>("active-quest").worldBound.yMax<=ui.Q<VisualElement>("feedback-lane").worldBound.yMin,"task above feedback");Check(ui.Q<VisualElement>("feedback-lane").worldBound.yMax<=ui.Q<VisualElement>("reels").worldBound.yMin,"feedback above reels");Check(ui.Q<Button>("spin").worldBound.yMax<=ui.worldBound.yMax+1,"SPIN within viewport");
    int before=view.Model.PurchasedCount,cash=view.Model.Snapshot.cash;view.QaClick("buy-"+quest);view.QaBuy(quest);
    Check(view.Model.PurchasedCount==before+(variant==3?1:0),"purchase cannot bypass either resource");Check(view.Model.Snapshot.cash==cash-(variant==3?d.CashPrice:0),"no repeated overdraw");
    if(variant==3){string notice;var loaded=new GameModel(new SaveStore(Path.Combine(Environment.GetEnvironmentVariable("ZTM_QA_SAVE_DIR"),"level1-v1.json"),new UnityStateCodec()).Load(out notice));Check(loaded.PurchasedCount==view.Model.PurchasedCount&&loaded.ActiveQuest==view.Model.ActiveQuest&&loaded.Snapshot.cash==view.Model.Snapshot.cash,"purchase/next quest persists");}
    report.AppendLine("PASS "+target.width+"x"+target.height+" quest"+quest+" variant"+variant);
    if(++variant<4){phase=0;return;}variant=0;if(++quest<8){phase=0;return;}quest=0;
    doc.panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);target=null;if(++size<2){phase=0;return;}
    report.AppendLine("PASS "+count+" UI assertions;64 combinations;two actual layouts;all8task titles/counters/gates;explicit purchase/repeat/reload");Finish();
   }
  }catch(Exception e){report.AppendLine("FAIL "+e);Debug.LogException(e);Finish();}}
  static void Finish(){SessionState.SetBool(Active,false);if(target!=null){view.GetComponent<UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);target=null;}File.WriteAllText(Path.Combine(Root,"Evidence","ui-v0.4b-layout-checks.txt"),report.ToString());EditorApplication.ExitPlaymode();Debug.Log(report.ToString());}
 }
}
