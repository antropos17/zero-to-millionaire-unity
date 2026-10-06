using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire.Editor
{
 [InitializeOnLoad]public static class PointerEventAudit
 {
  const string Active="ZTM_POINTER_SYNTHETIC_ACTIVE",Tag="ZTM_POINTER_SYNTHETIC_TAG";
  static GameView view;static RenderTexture texture;static int phase,warm,checks,failures;static Button spin;static readonly StringBuilder report=new StringBuilder();
  static string Root=>Directory.GetParent(Application.dataPath).FullName;
  static PointerEventAudit(){EditorApplication.update+=Tick;}
  public static void Start(string tag){
   if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop own Playmode first");
   if(tag!="before"&&tag!="fixed")throw new ArgumentException("Audit tag");
   report.Clear();phase=warm=checks=failures=0;view=null;texture=null;spin=null;
   Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",Path.Combine(Root,".tooling","pointer-synthetic-v0.4f-"+tag,"save"));
   SessionState.SetString(Tag,tag);SessionState.SetBool(Active,true);UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");EditorApplication.EnterPlaymode();
  }
  static void Check(bool condition,string label){checks++;if(!condition)failures++;report.AppendLine((condition?"PASS ":"FAIL ")+label);}
  static Event Raw(EventType type,Vector2 position){return new Event{type=type,button=0,mousePosition=position};}
  static void Move(Button b,Vector2 point){using(var e=PointerMoveEvent.GetPooled(Raw(EventType.MouseMove,point))){e.target=b;b.SendEvent(e);}}
  static void Down(Button b){Move(b,b.worldBound.center);using(var e=PointerDownEvent.GetPooled(Raw(EventType.MouseDown,b.worldBound.center))){e.target=b;b.SendEvent(e);}}
  static void Up(Button b){using(var e=PointerUpEvent.GetPooled(Raw(EventType.MouseUp,b.worldBound.center))){e.target=b;b.SendEvent(e);}}
  static void Cancel(Button b){using(var e=PointerCancelEvent.GetPooled(Raw(EventType.MouseUp,b.worldBound.center))){e.target=b;b.SendEvent(e);}}
  static void Leave(Button b){using(var e=PointerLeaveEvent.GetPooled(Raw(EventType.MouseMove,new Vector2(-100,-100)))){e.target=b;b.SendEvent(e);}}
  static void Click(Button b){Down(b);Up(b);}
  static void Tick(){
   if(!SessionState.GetBool(Active,false)||!EditorApplication.isPlaying)return;
   try{
    if(view==null)view=UnityEngine.Object.FindFirstObjectByType<GameView>();if(view==null)return;
    var doc=view.GetComponent<UIDocument>();var ui=doc.rootVisualElement;
    if(phase==0){
     texture=new RenderTexture(360,640,0,RenderTextureFormat.ARGB32);texture.Create();doc.panelSettings.targetTexture=texture;view.SetQaState(new PlayerState(),0);view.QaSave();phase=1;warm=0;
    }else if(phase==1){
     if(++warm<30)return;spin=ui.Q<Button>("spin");Down(spin);
     Check(view.Model.Snapshot.spins==0,"SPIN down does not commit");Check(Math.Abs(PixelUi.FaceTravel(spin)-5)<.01,"SPIN pointer down moves face5 over fixed base10");phase=2;
    }else if(phase==2){
     Capture("down");Cancel(spin);Check(PixelUi.FaceTravel(spin)==0,"cancel releases raised face");Check(view.Model.Snapshot.spins==0,"cancel commits no spin");
     Down(spin);Leave(spin);Check(PixelUi.FaceTravel(spin)==0,"leave releases face");Cancel(spin);Check(view.Model.Snapshot.spins==0,"drag-off/cancel commits no spin");
     Down(spin);using(var capture=PointerCaptureOutEvent.GetPooled(spin,null,PointerId.mousePointerId)){capture.target=spin;spin.SendEvent(capture);}Check(PixelUi.FaceTravel(spin)==0,"capture loss releases face");Cancel(spin);
     Down(spin);using(var blur=BlurEvent.GetPooled()){blur.target=spin;spin.SendEvent(blur);}Check(PixelUi.FaceTravel(spin)==0,"focus blur releases face");Cancel(spin);
     Click(spin);Check(view.Model.Snapshot.spins==1,"pointer down/up fires SPIN exactly once");Check(PixelUi.FaceTravel(spin)==0,"pointer up releases face");
     Up(spin);Click(spin);Check(view.Model.Snapshot.spins==1,"extra pointer up/rapid click cannot double spin");phase=3;
    }else if(phase==3){
     if(view.QaSpinning)return;var state=new PlayerState{cash=1000,freeSpins=0};state.fragments[0]=8;view.SetQaState(state,0);warm=0;phase=4;
    }else if(phase==4){
     if(++warm<30)return;var buy=ui.Q<Button>("buy-0");Down(buy);Check(PixelUi.FaceTravel(buy)==2,"BUY pointer down moves face2");Check(view.Model.PurchasedCount==0,"BUY down does not install");
     Cancel(buy);Check(PixelUi.FaceTravel(buy)==0,"BUY cancel releases face");Check(view.Model.PurchasedCount==0,"BUY cancel preserves resources");
     report.AppendLine("BUY fixture ready="+view.Model.CanBuy(0)+" enabled="+buy.enabledInHierarchy+" cash="+view.Model.Snapshot.cash+" parts="+view.Model.Snapshot.fragments[0]);phase=6;warm=0;
    }else if(phase==6){
     if(++warm<4)return;var buy=ui.Q<Button>("buy-0");buy.clicked+=()=>report.AppendLine("OBSERVED actual BUY clicked callback");Down(buy);phase=7;
    }else if(phase==7){
     var buy=ui.Q<Button>("buy-0");Up(buy);Up(buy);report.AppendLine("BUY result owned="+view.Model.PurchasedCount+" cash="+view.Model.Snapshot.cash+" spinning="+view.QaSpinning);
     Check(view.Model.PurchasedCount==1&&view.Model.Snapshot.cash==982,"BUY pointer up installs and debits exactly once");phase=5;warm=0;
    }else if(phase==5){
     if(++warm<20)return;Click(ui.Q<Button>("menu"));Check(ui.Q<VisualElement>("settings").resolvedStyle.display!=DisplayStyle.None,"pointer MENU opens");
     Click(ui.Q<Button>("reset"));Check(ui.Q<VisualElement>("reset-confirmation").resolvedStyle.display!=DisplayStyle.None,"pointer reset requires confirmation");
     Click(ui.Q<Button>("keep-room"));Check(view.Model.PurchasedCount==1&&view.Model.Snapshot.cash==982,"pointer reset cancel preserves installed room and cash");
     report.AppendLine("SOURCE synthetic UI Toolkit Event.GetPooled/SendEvent in private Editor; no Windows SendInput/human pointer.");
     Finish();
    }
   }catch(Exception e){report.AppendLine("ERROR "+e);failures++;Debug.LogException(e);Finish();}
  }
  static void Capture(string name){var old=RenderTexture.active;RenderTexture.active=texture;var image=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Root,"Evidence","ui-pointer-v0.4f-"+SessionState.GetString(Tag,"")+"-"+name+".png"),image.EncodeToPNG());UnityEngine.Object.Destroy(image);RenderTexture.active=old;}
  static void Finish(){
   SessionState.SetBool(Active,false);report.AppendLine("CHECKS "+checks+" FAILURES "+failures);
   File.WriteAllText(Path.Combine(Root,"Evidence","ui-pointer-v0.4f-"+SessionState.GetString(Tag,"")+"-checks.txt"),report.ToString());
   if(texture!=null){if(view!=null)view.GetComponent<UIDocument>().panelSettings.targetTexture=null;texture.Release();UnityEngine.Object.Destroy(texture);texture=null;}
   EditorApplication.ExitPlaymode();Debug.Log("ZTM_POINTER_SYNTHETIC_FINISHED "+failures);
  }
 }
}