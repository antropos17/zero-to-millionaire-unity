using System;
using System.IO;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
namespace ZeroToMillionaire.Editor
{
 public static class CriticalTestRunner{
  static TestRunnerApi api;static string reportTag="v0.5";
  public static string Start(string tag="v0.5"){reportTag=tag;UnityEditor.SessionState.SetString("ZTM_CRITICAL_REPORT_TAG",tag);
   if(UnityEditor.EditorApplication.isPlaying)throw new InvalidOperationException("Stop own Playmode first");
   api=ScriptableObject.CreateInstance<TestRunnerApi>();api.RegisterCallbacks(new Results());
   return api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,assemblyNames=new[]{"ZeroToMillionaire.CriticalTests"}}));
  }
  sealed class Results:ICallbacks{
   public void RunStarted(ITestAdaptor tests){}public void TestStarted(ITestAdaptor test){}public void TestFinished(ITestResultAdaptor result){}
   public void RunFinished(ITestResultAdaptor result){
    reportTag=UnityEditor.SessionState.GetString("ZTM_CRITICAL_REPORT_TAG","v0.5");string dir=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Evidence");
    TestRunnerApi.SaveResultToFile(result,Path.Combine(dir,"critical-"+reportTag+"-editmode.xml"));
    File.WriteAllText(Path.Combine(dir,"critical-"+reportTag+"-editmode.txt"),"Unity Test Framework EditMode: "+result.ResultState+"; passed="+result.PassCount+" failed="+result.FailCount+" skipped="+result.SkipCount);
    Debug.Log("ZTM_CRITICAL_TESTS "+result.ResultState);UnityEngine.Object.DestroyImmediate(api);api=null;
   }
  }
 }
}