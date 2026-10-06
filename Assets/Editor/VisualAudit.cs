using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZeroToMillionaire.Editor
{
    [InitializeOnLoad]
    public static class VisualAudit
    {
        const string Active="ZTM_VISUAL_ACTIVE",Tag="ZTM_VISUAL_TAG";
        static GameView view;static RenderTexture target;
        static int warm,firstFrame,lastFrame,captured;static double started,next;
        static FloorPoint previous;static float distance;
        static readonly HashSet<int> phases=new HashSet<int>();
        static readonly List<string> samples=new List<string>();
        static string Root=>Directory.GetParent(Application.dataPath).FullName;
        static VisualAudit(){EditorApplication.update+=Tick;}
        public static void Before(){Start("before");}
        public static void After(){Start("after");}
        static void Start(string tag)
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop own test Playmode first");
            Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",Path.Combine(Root,".tooling","visual-qa-save"));
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity",UnityEditor.SceneManagement.OpenSceneMode.Single);
            SessionState.SetString(Tag,tag);SessionState.SetBool(Active,true);EditorApplication.EnterPlaymode();
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Active,false)||!EditorApplication.isPlaying)return;
            try{
                if(view==null)view=UnityEngine.Object.FindFirstObjectByType<GameView>();
                if(view==null)return;
                if(target==null){
                    target=new RenderTexture(480,960,0,RenderTextureFormat.ARGB32);target.Create();
                    view.GetComponent<UnityEngine.UIElements.UIDocument>().panelSettings.targetTexture=target;
                    var state=new PlayerState{cash=100};for(int i=0;i<8;i++)state.purchased[i]=true;
                    view.SetQaState(state,3);previous=view.QaFeet;firstFrame=Time.frameCount;
                }
                if(++warm<30)return;
                if(started==0){started=EditorApplication.timeSinceStartup;next=started;}
                float moved=FloorPoint.Distance(previous,view.QaFeet);distance+=moved;previous=view.QaFeet;
                if(!RoomGeometry.IsFree(view.QaFeet))throw new Exception("Feet intersect reserved geometry");
                phases.Add(view.QaAnimationPhase);lastFrame=Time.frameCount;
                double now=EditorApplication.timeSinceStartup;
                if(now>=next){
                    string tag=SessionState.GetString(Tag,"unknown");string folder=Path.Combine(Root,".tooling","visual-demo-"+tag);
                    Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,captured.ToString("D4")+".png"),Read());
                    samples.Add((now-started).ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
                    captured++;next=now+1.0/12;
                }
                if(now-started<10)return;
                if(lastFrame-firstFrame<100||captured<65||distance<1||!phases.Contains(-1))throw new Exception("No real advancing walk/idle capture");
                string report="{\"tag\":\""+SessionState.GetString(Tag,"unknown")+"\",\"width\":480,\"height\":960,\"seconds\":"+(now-started).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"captured\":"+captured+",\"first_frame\":"+firstFrame+",\"last_frame\":"+lastFrame+",\"distance_m\":"+distance.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"idle_seen\":true,\"feet_clear\":true,\"capture_times\":["+string.Join(",",samples)+"]}";
                File.WriteAllText(Path.Combine(Root,"Evidence","visual-v0.4-"+SessionState.GetString(Tag,"unknown")+".json"),report);
                Finish();Debug.Log("VISUAL_AUDIT_PASS "+report);
            }catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.Combine(Root,".tooling","visual-audit-error.txt"),e.ToString());Finish();}
        }
        static void Finish(){SessionState.SetBool(Active,false);if(target!=null){view.GetComponent<UnityEngine.UIElements.UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);target=null;}EditorApplication.ExitPlaymode();}
        static byte[] Read()
        {
            var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();byte[] bytes=image.EncodeToPNG();UnityEngine.Object.Destroy(image);RenderTexture.active=old;return bytes;
        }
    }
}
