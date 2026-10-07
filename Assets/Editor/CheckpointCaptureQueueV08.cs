using System;using System.IO;using UnityEditor;
namespace ZeroToMillionaire.Editor {
 [InitializeOnLoad]public static class CheckpointCaptureQueueV08 {
  const string Key="ZTM_CAPTURE_QUEUE_V08";static string Root=>Directory.GetParent(UnityEngine.Application.dataPath).FullName;
  static CheckpointCaptureQueueV08(){EditorApplication.update+=Tick;}
  public static void BatchStart(){SessionState.SetBool(Key,true);SessionState.SetInt(Key+"phase",0);UsabilityRegressionV08.Start();}
  static bool Done(string name)=>File.Exists(Path.Combine(Root,"Evidence",name));
  static void Tick(){if(!SessionState.GetBool(Key,false)||EditorApplication.isPlayingOrWillChangePlaymode)return;try{int phase=SessionState.GetInt(Key+"phase",0);
   if(phase==0&&Done("v08-usability-checks-3.txt")){SessionState.SetInt(Key+"phase",1);EnglishLayoutAuditV08.Start();}
   else if(phase==1&&Done("v08-english-layout-checks-2.txt")){SessionState.SetBool(Key,false);VisualCheckpointV08.BatchStart();}
  }catch(Exception e){SessionState.SetBool(Key,false);File.WriteAllText(Path.Combine(Root,"Evidence","v08-capture-queue-failed.txt"),e.ToString());EditorApplication.Exit(1);}}
 }
}
