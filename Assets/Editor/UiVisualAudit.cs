using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace ZeroToMillionaire.Editor
{
 [InitializeOnLoad] public static class UiVisualAudit
 {
  const string Active="ZTM_UI_VISUAL_ACTIVE",Tag="ZTM_UI_VISUAL_TAG";
  static GameView view;static RenderTexture target;static int viewport,phase,warm,firstFrame,lastFrame,captured;
  static double started,next,layoutStarted;static bool pressed,spun,bought;static float distance;static FloorPoint previous;
  static readonly List<string> times=new List<string>();static readonly StringBuilder checks=new StringBuilder();
  static string Root=>Directory.GetParent(Application.dataPath).FullName;
  static string Name=>"ui-v0.4b-"+SessionState.GetString(Tag,"");
  static UiVisualAudit(){EditorApplication.update+=Tick;}
  public static void Before(){Start("before");} public static void After(){Start("after");}
  static void Start(string tag){if(EditorApplication.isPlaying)throw new Exception("Own Playmode must be stopped");view=null;viewport=phase=warm=captured=0;started=next=0;distance=0;pressed=spun=bought=false;times.Clear();checks.Clear();Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",Path.Combine(Root,".tooling","ui-visual-qa-save"));UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");SessionState.SetString(Tag,tag);SessionState.SetBool(Active,true);EditorApplication.EnterPlaymode();}
  static void Check(bool ok,string message){if(!ok)throw new Exception(message);checks.AppendLine("PASS "+message);}
  static PlayerState Fixture(int count,bool grid){var s=new PlayerState{cash=1000,freeSpins=0,spins=grid?1:0};for(int i=0;i<count;i++)s.purchased[i]=true;s.fragments[count]=Economy.Upgrades[count].FragmentGoal-1;for(int i=0;i<15;i++)s.lastGrid[i]=grid?i%9:0;return s;}
  static void SetTarget(int width,int height){if(target!=null){view.GetComponent<UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);}target=new RenderTexture(width,height,0,RenderTextureFormat.ARGB32);target.Create();view.GetComponent<UIDocument>().panelSettings.targetTexture=target;}
  static void Tick(){if(!SessionState.GetBool(Active,false)||!EditorApplication.isPlaying)return;
   try{if(view==null)view=UnityEngine.Object.FindFirstObjectByType<GameView>();if(view==null)return;
    var ui=view.GetComponent<UIDocument>().rootVisualElement;
    if(phase==0){SetTarget(viewport==0?360:480,viewport==0?640:960);view.SetQaState(Fixture(5,true),2);warm=0;layoutStarted=EditorApplication.timeSinceStartup;phase=1;}
    else if(phase==1){if(++warm<25||EditorApplication.timeSinceStartup-layoutStarted<.25)return;
     var spin=ui.Q<Button>("spin");var menu=ui.Q<Button>("menu");
     Check(spin.worldBound.yMax<=ui.worldBound.yMax+1,"SPIN fits "+target.width+"x"+target.height);Check(menu.worldBound.height>=43.5f,"MENU target44");
     Check(ui.Query<VisualElement>().Where(v=>v.name!=null&&v.name.StartsWith("cell-")).ToList().Count==15,"5x3 remains stable");
     Check(ui.Q<VisualElement>("active-quest").worldBound.yMax<=ui.Q<VisualElement>("reels").worldBound.yMin,"task above reels");
     var room=ui.Q<VisualElement>("room-stage");Check(Math.Abs(room.worldBound.width-room.worldBound.height)<1,"room aspect preserved");
     File.WriteAllBytes(Path.Combine(Root,"Evidence",Name+"-"+target.width+"x"+target.height+".png"),Read());
     RecordBounds(ui);view.QaClick("spin");Check(view.QaSpinning,"actual SPIN callback started");view.QaBuy(5);Check(view.Model.PurchasedCount==5,"BUY rejected while spinning");phase=2;
    }else if(phase==2){if(view.QaSpinning)return;var fly=ui.Q<VisualElement>("reward-to-task");if(fly==null||fly.resolvedStyle.display!=DisplayStyle.Flex||fly.worldBound.height<5)return;
     Check(view.Model.CanBuy(5),"matching last-part spin reaches manual BUY readiness");
     if(SessionState.GetString(Tag,"")=="after"){var lane=ui.Q<VisualElement>("feedback-lane");Check(lane!=null&&fly.worldBound.yMin>=lane.worldBound.yMin-1&&fly.worldBound.yMax<=lane.worldBound.yMax+1,"part reward contained in reserved lane");Check(!fly.worldBound.Overlaps(ui.Q<VisualElement>("room").worldBound),"reward avoids heroine/room");Check(!fly.worldBound.Overlaps(ui.Q<VisualElement>("quest-item-well").worldBound),"reward avoids task artwork");Check(ui.Q<Label>("cash-count").resolvedStyle.fontSize>=12,"cash counter readable minimum12");}
     File.WriteAllBytes(Path.Combine(Root,"Evidence",Name+"-reward-"+target.width+"x"+target.height+".png"),Read());
     if(++viewport<2){phase=0;return;}
     if(SessionState.GetString(Tag,"")=="before"){Finish();return;}
     SetTarget(480,960);view.SetQaState(Fixture(7,false),3);warm=0;phase=3;
    }else if(phase==3){if(++warm<25)return;started=next=EditorApplication.timeSinceStartup;firstFrame=Time.frameCount;previous=view.QaFeet;Directory.CreateDirectory(Path.Combine(Root,".tooling",Name+"-frames"));phase=4;
    }else if(phase==4){double now=EditorApplication.timeSinceStartup,t=now-started;lastFrame=Time.frameCount;distance+=FloorPoint.Distance(previous,view.QaFeet);previous=view.QaFeet;if(!RoomGeometry.IsFree(view.QaFeet))throw new Exception("Feet intersect geometry at frame"+Time.frameCount);
     if(t>=1.6&&!pressed){view.QaPressedSpin();Check(Math.Abs(PixelUi.FaceTravel(ui.Q<Button>("spin"))-5)<.01f,"raised SPIN press5 over base10");pressed=true;}
     if(t>=1.85&&!spun){view.QaReleaseSpin();view.QaClick("spin");view.QaClick("spin");Check(view.Model.Snapshot.spins==1&&view.QaSpinning,"repeated SPIN commits once");spun=true;}
     if(t>=4&&!bought&&!view.QaSpinning){Check(view.Model.CanBuy(7),"last task ready from real spin");int cash=view.Model.Snapshot.cash;view.QaClick("buy-7");view.QaBuy(7);Check(view.Model.Complete&&view.Model.Snapshot.cash==cash-Economy.Upgrades[7].CashPrice,"manual BUY callback completes exactly once");string notice;var loaded=new GameModel(new SaveStore(Path.Combine(Environment.GetEnvironmentVariable("ZTM_QA_SAVE_DIR"),"level1-v1.json"),new UnityStateCodec()).Load(out notice));Check(loaded.Complete&&loaded.Snapshot.cash==view.Model.Snapshot.cash,"installation reload preserves cash and complete room");bought=true;}
     if(now>=next){File.WriteAllBytes(Path.Combine(Root,".tooling",Name+"-frames",captured.ToString("D4")+".png"),Read());times.Add(t.ToString("F4",System.Globalization.CultureInfo.InvariantCulture));captured++;next=now+1.0/12;}
     if(t<11)return;Check(lastFrame-firstFrame>=100&&captured>=80&&distance>1&&bought,"real advancing walk + press/reel/manual-install clip");
     view.QaShowReset();view.QaCancelReset();Check(view.Model.Complete,"reset cancel preserves completion");view.QaShowReset();view.QaConfirmReset();Check(view.Model.Snapshot.freeSpins==3&&view.Model.PurchasedCount==0,"confirmed reset works in isolated profile");
     string report="{\"source\":\"Editor runtime; UI callbacks, not physical touch\",\"width\":480,\"height\":960,\"seconds\":"+t.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"captures\":"+captured+",\"first_frame\":"+firstFrame+",\"last_frame\":"+lastFrame+",\"distance_m\":"+distance.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"pressed\":true,\"spun\":true,\"manual_buy_callback\":true,\"capture_times\":["+string.Join(",",times)+"]}";
     File.WriteAllText(Path.Combine(Root,"Evidence",Name+"-clip.json"),report);Finish();
    }
   }catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.Combine(Root,".tooling",Name+"-error.txt"),e.ToString());Finish();}}
  static void RecordBounds(VisualElement ui){checks.AppendLine("BOUNDS "+target.width+"x"+target.height);foreach(string name in new[]{"room","upgrades","reels","feedback-lane","cash-count","quest-title","quest-status","next-quest","spin"}){var v=ui.Q<VisualElement>(name);if(v!=null)checks.AppendLine(name+" "+v.worldBound+" font="+v.resolvedStyle.fontSize);}for(int i=0;i<9;i++)checks.AppendLine("symbol"+i+" "+ui.Q<VisualElement>("cell-"+i).Q<Image>("symbol").worldBound);}
  static byte[] Read(){var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();byte[] bytes=image.EncodeToPNG();UnityEngine.Object.Destroy(image);RenderTexture.active=old;return bytes;}
  static void Finish(){SessionState.SetBool(Active,false);File.WriteAllText(Path.Combine(Root,"Evidence",Name+"-checks.txt"),checks.ToString());if(target!=null){view.GetComponent<UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);target=null;}EditorApplication.ExitPlaymode();Debug.Log("UI_VISUAL_AUDIT_FINISHED "+Name);}
 }
}
