using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace ZeroToMillionaire.Editor
{
 public static class ReleaseCandidateBuild{
  public static void Build(){
   string root=Directory.GetParent(Application.dataPath).FullName;
   bool audit=Environment.GetEnvironmentVariable("ZTM_RELEASE_KIND")=="audit";
   string dir=Path.Combine(root,"Builds",audit?"Windows-release-audit-v0.4f":"Windows-rc-v0.4f");
   if(Directory.Exists(dir))throw new InvalidOperationException("Immutable release directory already exists");
   string source=Environment.GetEnvironmentVariable("ZTM_RELEASE_SOURCE");if(string.IsNullOrEmpty(source))throw new InvalidOperationException("Record source commit before building");
   var p=UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.2d.animation");
   if(p==null||p.version!="13.0.6")throw new InvalidOperationException("Expected imported 2D Animation13.0.6");
   Directory.CreateDirectory(dir);
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Level1.unity"},locationPathName=Path.Combine(dir,"ZeroToMillionaire.exe"),target=BuildTarget.StandaloneWindows64,
    options=BuildOptions.None,extraScriptingDefines=audit?new[]{"ZTM_RELEASE_AUDIT"}:Array.Empty<string>()});
   string text=report.summary.result+"\nsource="+source+"\noptions=None\nauditDefine="+audit+"\n2DAnimation="+p.version+"\ncompany="+PlayerSettings.companyName+"\nproduct="+PlayerSettings.productName+"\nbundleVersion="+PlayerSettings.bundleVersion+"\nguid="+report.summary.guid+"\nbytes="+report.summary.totalSize+"\nduration="+report.summary.totalTime;
   File.WriteAllText(Path.Combine(root,"Evidence",audit?"release-v0.4f-audit-build.txt":"release-v0.4f-candidate-build.txt"),text);
   if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Release build failed");
   Debug.Log("ZTM_RELEASE_BUILD "+dir);
  }
 }
}