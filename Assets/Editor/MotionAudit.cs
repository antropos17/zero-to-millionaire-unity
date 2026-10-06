using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZeroToMillionaire.Editor
{
    [InitializeOnLoad]
    public static class MotionAudit
    {
        const string Active="ZTM_MOTION_ACTIVE",Tag="ZTM_MOTION_TAG";
        static GameView view;static RenderTexture target;
        static int warm,firstFrame,lastFrame,captured;static double started,next;
        static FloorPoint previous;static float distance,maxJointGap;static bool turningSeen;static readonly HashSet<int> armAngles=new HashSet<int>();
        static readonly HashSet<int> phases=new HashSet<int>();
        static readonly List<string> samples=new List<string>();
        static string Root=>Directory.GetParent(Application.dataPath).FullName;
        static MotionAudit(){EditorApplication.update+=Tick;}
        static string Prefix=>SessionState.GetString(Tag,"")=="v0.4d"?"motion-v0.4d":"motion-v0.4c-"+SessionState.GetString(Tag,"unknown");
        public static void Current(){Start("v0.4d");}
        public static void Before(){Start("before");}
        public static void After(){Start("after");}
        static void Start(string tag)
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop own test Playmode first");view=null;warm=firstFrame=lastFrame=captured=0;started=next=0;distance=maxJointGap=0;turningSeen=false;phases.Clear();samples.Clear();armAngles.Clear();
            Environment.SetEnvironmentVariable("ZTM_QA_SAVE_DIR",Path.Combine(Root,".tooling","motion-qa-save"));
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
                if(SessionState.GetString(Tag,"")!="before"){var rig=view.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("articulated-upper-body");if(rig==null||rig.Query<Image>().ToList().Count!=6)throw new Exception("Upper rig missing");foreach(var picture in rig.Query<Image>().ToList())if(picture.image==null||picture.worldBound.height<1)throw new Exception("Actual upper image not rendered");}
                if(started==0){started=EditorApplication.timeSinceStartup;next=started;}
                float moved=FloorPoint.Distance(previous,view.QaFeet);distance+=moved;previous=view.QaFeet;
                if(!RoomGeometry.IsFree(view.QaFeet))throw new Exception("Feet intersect reserved geometry");
                phases.Add(view.QaAnimationPhase);lastFrame=Time.frameCount;
                float leftArm=(float)(view.GetType().GetProperty("QaLeftArmAngle")?.GetValue(view)??0f),rightArm=(float)(view.GetType().GetProperty("QaRightArmAngle")?.GetValue(view)??0f);
                bool turning=(bool)(view.GetType().GetProperty("QaTurning")?.GetValue(view)??false);turningSeen|=turning;
                float jointGap=(float)(view.GetType().GetProperty("QaArmJointGap")?.GetValue(view)??0f);maxJointGap=Math.Max(maxJointGap,jointGap);
                if(Math.Abs(leftArm)>6||Math.Abs(rightArm)>6||leftArm*rightArm>.01f)throw new Exception("Unrestrained or non-opposite arm phase");
                armAngles.Add((int)Math.Round(leftArm*10));
                double now=EditorApplication.timeSinceStartup;
                if(now>=next){
                    string tag=SessionState.GetString(Tag,"unknown");string folder=Path.Combine(Root,".tooling",Prefix);
                    Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,captured.ToString("D4")+".png"),Read());
                    samples.Add((now-started).ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
                    captured++;next=now+1.0/12;
                }
                if(now-started<12)return;
                if(SessionState.GetString(Tag,"")!="before"&&(armAngles.Count<20||!turningSeen||maxJointGap>1))throw new Exception("Arm/turn/joint check failed: angles"+armAngles.Count+" turning"+turningSeen+" gap"+maxJointGap);
                if(lastFrame-firstFrame<100||captured<65||distance<1||!phases.Contains(-1))throw new Exception("No real advancing walk/idle capture");
                string report="{\"tag\":\""+SessionState.GetString(Tag,"unknown")+"\",\"width\":480,\"height\":960,\"seconds\":"+(now-started).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"captured\":"+captured+",\"first_frame\":"+firstFrame+",\"last_frame\":"+lastFrame+",\"distance_m\":"+distance.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"idle_seen\":true,\"feet_clear\":true,\"turning_seen\":"+turningSeen.ToString().ToLowerInvariant()+",\"arm_angle_bins\":"+armAngles.Count+",\"max_joint_gap_ui_units\":"+maxJointGap.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+",\"capture_times\":["+string.Join(",",samples)+"]}";
                File.WriteAllText(Path.Combine(Root,"Evidence",Prefix+".json"),report);
                Finish();Debug.Log("MOTION_AUDIT_PASS "+report);
            }catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.Combine(Root,".tooling","motion-audit-error.txt"),e.ToString());Finish();}
        }
        static void Finish(){SessionState.SetBool(Active,false);if(target!=null){view.GetComponent<UnityEngine.UIElements.UIDocument>().panelSettings.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);target=null;}EditorApplication.ExitPlaymode();}
        static byte[] Read()
        {
            var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();byte[] bytes=image.EncodeToPNG();UnityEngine.Object.Destroy(image);RenderTexture.active=old;return bytes;
        }
    }
}
