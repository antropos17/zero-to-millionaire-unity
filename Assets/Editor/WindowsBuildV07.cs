using System;using System.IO;using System.Text.RegularExpressions;using UnityEditor;using UnityEditor.Build;using UnityEditor.Build.Reporting;using UnityEngine;
namespace ZeroToMillionaire.Editor {
 [InitializeOnLoad]public static class WindowsBuildV07 {
 const string Active="ZTM_WINDOWS_BUILD_V07";
 static string Root=>Directory.GetParent(Application.dataPath).FullName;
 static WindowsBuildV07(){EditorApplication.update+=Tick;}
 public static string Start(string source,string label="candidate-1"){
  if(EditorApplication.isPlaying||SessionState.GetBool(Active,false))throw new Exception("Owned Editor busy");
  if(!Regex.IsMatch(source??"","^[0-9a-f]{40}$")||!Regex.IsMatch(label??"","^[a-z0-9-]+$"))throw new Exception("Immutable identity required");
  if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneWindows64)throw new Exception("Windows64 target required");
  if(PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Contains("ZTM_RELEASE_AUDIT"))throw new Exception("Audit define prohibited");
  string folder="Windows-v0.7-"+label;if(Directory.Exists(Path.Combine(Root,"Builds",folder)))throw new Exception("Immutable output exists");
  SessionState.SetString(Active+"source",source);SessionState.SetString(Active+"folder",folder);SessionState.SetBool(Active,true);return "queued "+folder;
 }
 static void Tick(){if(!SessionState.GetBool(Active,false)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;SessionState.SetBool(Active,false);
  string source=SessionState.GetString(Active+"source","");string folder=SessionState.GetString(Active+"folder","");string dir=Path.Combine(Root,"Builds",folder);string oldVersion=PlayerSettings.bundleVersion;
  try{PlayerSettings.bundleVersion="0.7.0";Directory.CreateDirectory(dir);
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Level1.unity"},locationPathName=Path.Combine(dir,"ZeroToMillionaire.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None,extraScriptingDefines=Array.Empty<string>()});
   File.WriteAllText(Path.Combine(Root,"Evidence",folder+"-build.txt"),report.summary.result+"\nsource="+source+"\noptions=None\nauditDefine=False\nbundleVersion=0.7.0\ncompany="+PlayerSettings.companyName+"\nproduct="+PlayerSettings.productName+"\nguid="+report.summary.guid+"\nbytes="+report.summary.totalSize+"\nduration="+report.summary.totalTime);
   if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Windows Release build failed");Debug.Log("ZTM_WINDOWS_V07_SUCCESS "+folder);
  }catch(Exception e){File.WriteAllText(Path.Combine(Root,"Evidence",folder+"-error.txt"),e.ToString());Debug.LogException(e);}finally{PlayerSettings.bundleVersion=oldVersion;AssetDatabase.SaveAssets();}
 }
 }
}