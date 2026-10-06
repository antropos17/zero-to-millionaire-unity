using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ZeroToMillionaire.Editor
{
 public static class PacingAudit{
  [Serializable]sealed class Distribution{public int seeds=1000,min,max,p50,p95;public double mean;public TaskRange[] tasks;public string assumptions="Spin counts are simulated; no guaranteed playtime. At4..6seconds/accepted spin plus reading/manual buys, typical total is an estimate only.";}
  [Serializable]sealed class TaskRange{public int task,min,max,p50,p95;}
  public static string Run(){
   int[] totals=new int[1000];int[][] intervals=new int[8][];for(int q=0;q<8;q++)intervals[q]=new int[1000];
   for(int seed=0;seed<1000;seed++){
    var m=new GameModel();var random=new SeededRandom(seed);int spins=0,prior=0,guard=0;
    while(!m.Complete&&guard++<2000){int q=m.ActiveQuest;if(m.CanBuy(q)){if(!m.TryBuy(q))throw new Exception("buy");intervals[q][seed]=spins-prior;prior=spins;if(m.TryBuy(q))throw new Exception("repeat");}else if(m.CanRecover){if(!m.GrantRecovery())throw new Exception("recovery");}else{SpinResult result;if(!m.TrySpin(random,out result))throw new Exception("softlock");spins++;if(m.Snapshot.cash<0)throw new Exception("overdraw");}}
    if(!m.Complete)throw new Exception("completion");totals[seed]=spins;
   }
   Array.Sort(totals);var report=new Distribution{min=totals[0],max=totals[999],p50=totals[499],p95=totals[949],mean=totals.Average(),tasks=new TaskRange[8]};
   for(int q=0;q<8;q++){Array.Sort(intervals[q]);report.tasks[q]=new TaskRange{task=q,min=intervals[q][0],max=intervals[q][999],p50=intervals[q][499],p95=intervals[q][949]};}
   string json=JsonUtility.ToJson(report,true);File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Evidence","pacing-v0.5-1000seeds.json"),json);return json;
  }
 }
}