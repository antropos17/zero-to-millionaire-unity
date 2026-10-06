using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ZeroToMillionaire.Editor
{
    public static class BuildPrototype
    {
        static string Root { get { return Directory.GetParent(Application.dataPath).FullName; } }
        public static void PrepareAndCheck()
        {
            Directory.CreateDirectory(Path.Combine(Root, "Evidence"));
            string report = ModelChecks.Run(new UnityStateCodec(), Path.Combine(Root, "Evidence", "checks-tmp"));
            Debug.Log(report); File.WriteAllText(Path.Combine(Root, "Evidence", "unity-model-checks.txt"), report);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(249, 238, 213, 255);
            camera.orthographic = true;
            new GameObject("Level1 Game").AddComponent<GameView>();
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Scenes"));
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level1.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Level1.unity", true) };
            PlayerSettings.companyName = "PrototypeWorkspace";
            PlayerSettings.productName = "ZERO TO MILLIONAIRE";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.prototypeworkspace.zerotomillionaire");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth = 480; PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
            PlayerSettings.Android.forceInternetPermission = false;
            PlayerSettings.Android.forceSDCardPermission = false;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
            PlayerSettings.bundleVersion = "0.3.0";
            PlayerSettings.Android.bundleVersionCode = 3;
            AssetDatabase.SaveAssets();
        }
        public static void Android()
        {
            PrepareAndCheck();
            Directory.CreateDirectory(Path.Combine(Root, "Builds", "Android"));
            string path = Path.Combine(Root, "Builds", "Android", "zero-to-millionaire-v0.3.apk");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/Level1.unity" }, locationPathName = path,
                target = BuildTarget.Android, options = BuildOptions.None
            });
            File.WriteAllText(Path.Combine(Root, "Evidence", "android-v0.3-build.txt"), result.summary.result + "\n" + result.summary.totalSize + " bytes\n" + result.summary.totalTime);
            if (result.summary.result != BuildResult.Succeeded) throw new Exception("Android build failed");
        }
        public static void Windows()
        {
            PrepareAndCheck();
            Directory.CreateDirectory(Path.Combine(Root, "Builds", "Windows-v0.3"));
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/Level1.unity" },
                locationPathName = Path.Combine(Root, "Builds", "Windows-v0.3", "ZeroToMillionaire.exe"),
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
            });
            if (result.summary.result != BuildResult.Succeeded) throw new Exception("Windows build failed");
        }
    }
}
