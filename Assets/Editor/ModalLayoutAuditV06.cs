using System;using System.IO;using System.Text;using UnityEditor;using UnityEngine;using UnityEngine.UIElements;
namespace ZeroToMillionaire.Editor {
 [InitializeOnLoad]public static class ModalLayoutAuditV06 {
  const string Key="ZTM_MODAL_AUDIT_V06";static GameView view;static RenderTexture target;static int size,phase,warm;static double started;static StringBuilder report=new StringBuilder();
  static string Root=>Directory.GetParent(Application.dataPath).FullName;static VisualElement UI=>view.GetComponent<UIDocument>().rootVisualElement;
  static ModalLayoutAuditV06(){EditorApplication.update+=Tick;}
  public static string Start(){if(EditorApplication.isPlaying)throw new Exception("Own Editor playing");view=null;phase=size=warm=0;started=0;report.Clear();Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",Path.Combine(Root,".tooling","modal-v06-save"));UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();return "queued";}
  static void Capture(string name){var old=RenderTexture.active;RenderTexture.active=target;var t=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);t.ReadPixels(new Rect(0,0,target.width,target.height),0,0);t.Apply();File.WriteAllBytes(Path.Combine(Root,"Evidence","modal-v06-"+name+"-"+target.width+"x"+target.height+".png"),t.EncodeToPNG());UnityEngine.Object.Destroy(t);RenderTexture.active=old;}
  static void Tick(){if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying)return;try{if(started==0)started=EditorApplication.timeSinceStartup;if(EditorApplication.timeSinceStartup-started>90)throw new Exception("timeout");if(view==null){view=UnityEngine.Object.FindFirstObjectByType<GameView>();if(view==null)return;}
   if(phase==0){target=new RenderTexture(size==0?320:size<3?360:480,size==0||size==1?640:size==2?800:960,0,RenderTextureFormat.ARGB32);target.Create();view.GetComponent<UIDocument>().panelSettings.targetTexture=target;var s=new PlayerState();s.tutorial.skipped=true;view.SetQaState(s,0);warm=0;phase=1;}
   else if(phase==1&&++warm>25){view.QaClick("menu");warm=0;phase=2;}
   else if(phase==2&&++warm>25){var modal=UI.Q<VisualElement>("settings");bool fit=modal.worldBound.yMin>=0&&modal.worldBound.yMax<=UI.worldBound.yMax;report.AppendLine((fit?"PASS":"FAIL")+" menu "+target.width+"x"+target.height+" modal="+modal.worldBound+" viewport="+UI.worldBound);Capture("menu");view.QaClick("reset");warm=0;phase=3;}
   else if(phase==3&&++warm>25){var modal=UI.Q<VisualElement>("reset-confirmation");bool fit=modal.worldBound.yMax<=UI.worldBound.yMax;report.AppendLine((fit?"PASS":"FAIL")+" confirmation "+target.width+"x"+target.height);Capture("confirm");view.QaClick("keep-room");view.GetComponent<UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);target=null;if(++size<4){phase=0;return;}Finish();}
  }catch(Exception e){report.AppendLine("FAIL "+e);Finish();}}
  static void Finish(){SessionState.SetBool(Key,false);File.WriteAllText(Path.Combine(Root,"Evidence","modal-v06-checks.txt"),report.ToString());if(target!=null){view.GetComponent<UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);target=null;}EditorApplication.ExitPlaymode();}
 }
}
