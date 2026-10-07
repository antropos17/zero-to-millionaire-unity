using System;using System.IO;using UnityEditor;
namespace ZeroToMillionaire.Editor {
 [InitializeOnLoad]public static class V08RegressionSuite {
  const string Key="ZTM_SUITE_V08_2";static string Root=>Directory.GetParent(UnityEngine.Application.dataPath).FullName;
  static V08RegressionSuite(){EditorApplication.update+=Tick;}
  public static void BatchStart(){if(EditorApplication.isPlaying)throw new Exception("Busy Editor");SessionState.SetBool(Key,true);SessionState.SetInt(Key+"stage",1);UsabilityRegressionV08.Start();}
  static bool Done(string path)=>File.Exists(Path.Combine(Root,"Evidence",path));
  static void Tick(){if(!SessionState.GetBool(Key,false)||EditorApplication.isPlayingOrWillChangePlaymode)return;try{int stage=SessionState.GetInt(Key+"stage",0);
   if(stage==0&&Done("critical-v08-1-editmode.txt")){SessionState.SetInt(Key+"stage",1);UsabilityRegressionV08.Start();}
   else if(stage==1&&Done("v08-usability-checks-2.txt")){SessionState.SetInt(Key+"stage",2);EnglishDialogueAuditV08.Start();}
   else if(stage==2&&Done("v08-dialogue-checks-2.txt")){SessionState.SetInt(Key+"stage",3);MotionInteractionAuditV08.Start();}
   else if(stage==3&&Done("motion-interaction-v08-checks.txt")){SessionState.SetInt(Key+"stage",4);FinalLevelAuditV08.Start();}
   else if(stage==4&&Done("final-v08-level-checks.txt")){SessionState.SetBool(Key,false);File.WriteAllText(Path.Combine(Root,"Evidence","v08-regression-suite-2.txt"),"Completed isolated runners. Each individual report is authoritative; exit code alone is not PASS. No native/OS input.\n");EditorApplication.Exit(0);}
  }catch(Exception e){SessionState.SetBool(Key,false);File.WriteAllText(Path.Combine(Root,"Evidence","v08-regression-suite-2.txt"),"FAIL suite "+e);EditorApplication.Exit(1);}}
 }
}
