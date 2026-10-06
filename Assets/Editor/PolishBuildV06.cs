using System;using System.IO;using UnityEditor;using UnityEditor.Build.Reporting;using UnityEngine;
namespace ZeroToMillionaire.Editor {
 [InitializeOnLoad] public static class PolishBuildV06 {
  const string Key="ZTM_POLISH_BUILD_V06";static double readyAfter;
  static string Root=>Directory.GetParent(Application.dataPath).FullName;
  static PolishBuildV06(){EditorApplication.update+=Tick;}
  public static string Start(bool web,string source,string label="safety"){
   if(EditorApplication.isPlaying||SessionState.GetBool(Key,false))throw new Exception("Editor busy");if(string.IsNullOrEmpty(source)||source.Length!=40)throw new Exception("Record immutable source commit");
   if(label!="safety"&&label!="motion-checkpoint")throw new Exception("Unknown immutable build label");string folder=web?"WebGL-QA-v0.6-safety":"Windows-v0.6-"+label;if(Directory.Exists(Path.Combine(Root,"Builds",folder)))throw new Exception("Immutable output exists");
   SessionState.SetString(Key+"folder",folder);SessionState.SetString(Key+"source",source);SessionState.SetBool(Key+"web",web);
   SessionState.SetInt(Key+"target",(int)EditorUserBuildSettings.activeBuildTarget);SessionState.SetInt(Key+"width",PlayerSettings.defaultScreenWidth);SessionState.SetInt(Key+"height",PlayerSettings.defaultScreenHeight);
   SessionState.SetInt(Key+"compression",(int)PlayerSettings.WebGL.compressionFormat);SessionState.SetBool(Key+"fallback",PlayerSettings.WebGL.decompressionFallback);SessionState.SetInt(Key+"phase",0);SessionState.SetBool(Key,true);
   var target=web?BuildTarget.WebGL:BuildTarget.StandaloneWindows64;
   if(EditorUserBuildSettings.activeBuildTarget!=target&&!EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildPipeline.GetBuildTargetGroup(target),target)){SessionState.SetBool(Key,false);throw new Exception("Target switch rejected");}
   readyAfter=EditorApplication.timeSinceStartup+2;return "queued "+folder;
  }
  static void Tick(){if(!SessionState.GetBool(Key,false)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
   bool web=SessionState.GetBool(Key+"web",false);var target=web?BuildTarget.WebGL:BuildTarget.StandaloneWindows64;int phase=SessionState.GetInt(Key+"phase",0);
   if(phase==1){var old=(BuildTarget)SessionState.GetInt(Key+"target",(int)BuildTarget.StandaloneWindows64);if(EditorUserBuildSettings.activeBuildTarget==old){SessionState.SetBool(Key,false);Debug.Log("ZTM_POLISH_BUILD_RESTORED "+old);}return;}
   if(EditorUserBuildSettings.activeBuildTarget!=target||EditorApplication.timeSinceStartup<readyAfter)return;
   // Claim once before invoking a blocking build; domain reload will not duplicate it.
   SessionState.SetInt(Key+"phase",1);string folder=SessionState.GetString(Key+"folder","");string source=SessionState.GetString(Key+"source","");
   try {
    if(web){PlayerSettings.defaultScreenWidth=480;PlayerSettings.defaultScreenHeight=960;PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;PlayerSettings.WebGL.decompressionFallback=true;}
    string dir=Path.Combine(Root,"Builds",folder);Directory.CreateDirectory(dir);
    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Level1.unity"},locationPathName=web?dir:Path.Combine(dir,"ZeroToMillionaire.exe"),target=target,options=BuildOptions.None});
    File.WriteAllText(Path.Combine(Root,"Evidence",folder+"-build.txt"),report.summary.result+"\nsource="+source+"\noptions=None\ntarget="+target+"\nbytes="+report.summary.totalSize+"\nduration="+report.summary.totalTime);
    if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed");
    if(web){string html=Path.Combine(dir,"index.html");string text=File.ReadAllText(html);text=text.Replace("</head>","<style>html,body{margin:0;background:#142c28;}#unity-container.unity-desktop{width:min(480px,100vw,calc((100vh - 44px)*.5))!important;height:calc(min(480px,100vw,(100vh - 44px)*.5)*2)!important;position:absolute;left:50%;top:50%;transform:translate(-50%,-50%);}#unity-canvas{width:100%!important;height:100%!important;}#unity-footer{display:none;}</style></head>");File.WriteAllText(html,text);File.WriteAllText(Path.Combine(dir,"QA_README.txt"),"Local independent QA only. Serve this folder with python3 -m http.server 8765, then open http://localhost:8765 in a fresh browser profile/origin. Windows private-profile flag does not apply to WebGL. No publication. Source: "+source+". Windows remains primary. Test saves are browser-origin-local.\n");}
    Debug.Log("ZTM_POLISH_BUILD_SUCCESS "+folder);
   }catch(Exception e){File.WriteAllText(Path.Combine(Root,"Evidence",folder+"-error.txt"),e.ToString());Debug.LogException(e);}
   finally {
    PlayerSettings.defaultScreenWidth=SessionState.GetInt(Key+"width",480);PlayerSettings.defaultScreenHeight=SessionState.GetInt(Key+"height",960);PlayerSettings.WebGL.compressionFormat=(WebGLCompressionFormat)SessionState.GetInt(Key+"compression",0);PlayerSettings.WebGL.decompressionFallback=SessionState.GetBool(Key+"fallback",false);
    var old=(BuildTarget)SessionState.GetInt(Key+"target",(int)BuildTarget.StandaloneWindows64);if(EditorUserBuildSettings.activeBuildTarget!=old)EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildPipeline.GetBuildTargetGroup(old),old);else SessionState.SetBool(Key,false);
   }
  }
 }
}
