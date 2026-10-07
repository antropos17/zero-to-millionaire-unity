using System;using System.IO;using UnityEngine;
namespace ZeroToMillionaire {
 public sealed class PresentationPreferences {
  [Serializable]sealed class Data{public bool reducedMotion;}
  readonly string path;bool reducedMotion;LocalPlayerStore localPlayer;public bool ReducedMotion=>localPlayer!=null?localPlayer.Snapshot.Calm:reducedMotion;
  public void Bind(LocalPlayerStore store){localPlayer=store;}
  public PresentationPreferences(string directory){path=Path.Combine(directory,"presentation-v1.json");try{if(File.Exists(path)){var d=JsonUtility.FromJson<Data>(File.ReadAllText(path));reducedMotion=d!=null&&d.reducedMotion;}}catch(Exception e)when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException){}}
  public bool Toggle(){if(localPlayer!=null){var p=localPlayer.Snapshot;p.animationMode=p.Calm?"full":"calm";return localPlayer.Save(p,out _);}bool value=!ReducedMotion;try{Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(new Data{reducedMotion=value}));if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);reducedMotion=value;return true;}catch(Exception e)when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException||e is NotSupportedException){return false;}}
 }
}
