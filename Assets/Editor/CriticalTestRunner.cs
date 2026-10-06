using System;
using System.IO;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
namespace ZeroToMillionaire.Editor
{
 public static class CriticalTestRunner{
  static TestRunnerApi api;
  public static string Start(){
   if(UnityEditor.EditorApplication.isPlaying)throw new InvalidOperationException("Stop own Playmode first");
   api=ScriptableObject.CreateInstance<TestRunnerApi>();api.RegisterCallbacks(new Results());
   return api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,assemblyNames=new[]{"ZeroToMillionaire.CriticalTests"}}));
  }
  sealed class Results:ICallbacks{
   public void RunStarted(ITestAdaptor tests){}public void TestStarted(ITestAdaptor test){}public void TestFinished(ITestResultAdaptor result){}
   public void RunFinished(ITestResultAdaptor result){
    string dir=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Evidence");
    TestRunnerApi.SaveResultToFile(result,Path.Combine(dir,"critical-v0.5-editmode.xml"));
    File.WriteAllText(Path.Combine(dir,"critical-v0.5-editmode.txt"),"Unity Test Framework EditMode: "+result.ResultState+"; passed="+result.PassCount+" failed="+result.FailCount+" skipped="+result.SkipCount);
    Debug.Log("ZTM_CRITICAL_TESTS "+result.ResultState);UnityEngine.Object.DestroyImmediate(api);api=null;
   }
  }
 }
}