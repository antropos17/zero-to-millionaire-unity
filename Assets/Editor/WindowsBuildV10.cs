using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ZeroToMillionaire.Editor
{
    [InitializeOnLoad] public static class WindowsBuildV10
    {
        const string Key="ZTM_WINDOWS_BUILD_V10";
        static string Root=>Directory.GetParent(Application.dataPath).FullName;
        static WindowsBuildV10(){EditorApplication.update+=Tick;}
        public static void BatchStart()
        {
            string source=Environment.GetEnvironmentVariable("ZTM_RELEASE_SOURCE"),label=Environment.GetEnvironmentVariable("ZTM_RELEASE_LABEL");
            if(EditorApplication.isPlaying||SessionState.GetBool(Key,false))throw new InvalidOperationException("Owned Editor busy");
            if(!Regex.IsMatch(source??"","^[0-9a-f]{40}$")||label!="slice1-candidate-1")throw new InvalidOperationException("Immutable slice identity required");
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneWindows64)throw new InvalidOperationException("Windows64 target required");
            if(PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Contains("ZTM_RELEASE_AUDIT"))throw new InvalidOperationException("Audit define prohibited");
            string folder="Windows-v0.10-"+label;if(Directory.Exists(Path.Combine(Root,"Builds",folder)))throw new InvalidOperationException("Immutable output already exists");
            SessionState.SetString(Key+"source",source);SessionState.SetString(Key+"folder",folder);SessionState.SetBool(Key,true);
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Key,false)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            SessionState.SetBool(Key,false);string oldVersion=PlayerSettings.bundleVersion;
            string source=SessionState.GetString(Key+"source",""),folder=SessionState.GetString(Key+"folder",""),dir=Path.Combine(Root,"Builds",folder);bool success=false;
            try
            {
                PlayerSettings.bundleVersion="0.10.0";Directory.CreateDirectory(dir);
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Level1.unity"},locationPathName=Path.Combine(dir,"ZeroToMillionaire.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None,extraScriptingDefines=Array.Empty<string>()});
                success=report.summary.result==BuildResult.Succeeded;
                File.WriteAllText(Path.Combine(Root,"Evidence",folder+"-build.txt"),report.summary.result+"\nsource="+source+"\noptions=None\nauditDefine=False\nbundleVersion=0.10.0\nscope=profile/settings/sections/session-notifications; game schema6; decor pending\ncompany="+PlayerSettings.companyName+"\nproduct="+PlayerSettings.productName+"\nguid="+report.summary.guid+"\nbytes="+report.summary.totalSize+"\nduration="+report.summary.totalTime);
                if(!success)throw new InvalidOperationException("Windows slice build failed");
            }
            catch(Exception e){File.WriteAllText(Path.Combine(Root,"Evidence",folder+"-error.txt"),e.ToString());}
            finally{PlayerSettings.bundleVersion=oldVersion;AssetDatabase.SaveAssets();EditorApplication.delayCall+=()=>EditorApplication.Exit(success?0:1);}
        }
    }
}
