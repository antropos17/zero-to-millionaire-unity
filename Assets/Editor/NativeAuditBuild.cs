using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace ZeroToMillionaire.Editor
{
    public static class NativeAuditBuild
    {
        public static void BuildBaseline()
        {
            string root=Directory.GetParent(Application.dataPath).FullName;
            string label=Environment.GetEnvironmentVariable("ZTM_NATIVE_BUILD_TAG")??"baseline";
            if(label!="baseline"&&label!="optimized"&&label!="rendered")throw new InvalidOperationException("Unknown build tag");
            string dir=Path.Combine(root,"Builds","Windows-dev-v0.4d-"+label);
            if(File.Exists(Path.Combine(dir,"ZeroToMillionaireDev.exe")))throw new InvalidOperationException("Use a fresh build directory");
            Directory.CreateDirectory(dir);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Level1.unity"},
                locationPathName=Path.Combine(dir,"ZeroToMillionaireDev.exe"),target=BuildTarget.StandaloneWindows64,
                options=BuildOptions.Development});
            File.WriteAllText(Path.Combine(root,"Evidence","native-dev-"+label+"-build.txt"),
                report.summary.result+"\n"+report.summary.totalSize+" bytes\n"+report.summary.totalTime);
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Development build failed");
            Debug.Log("ZTM_NATIVE_BUILD_FINISHED "+dir);
        }
    }
}
