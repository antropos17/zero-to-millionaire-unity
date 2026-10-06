using System;
using System.IO;
using UnityEngine;
namespace ZeroToMillionaire
{
 // Explicit private Windows profile for local verification; ordinary launches keep their user save.
 public static class RuntimePrivateProfile
 {
  public static string SelectedDirectory{get;private set;}
  public static string Resolve(string ordinaryDirectory){
   string[] args=Environment.GetCommandLineArgs();int option=Array.IndexOf(args,"--ztm-private-profile");
   if(option<0)return ordinaryDirectory;
   if(Application.platform!=RuntimePlatform.WindowsPlayer||option+1>=args.Length||!Path.IsPathRooted(args[option+1]))throw new InvalidOperationException("Private profile requires an absolute Windows test path");
   string root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..","..",".tooling"))+Path.DirectorySeparatorChar;
   string candidate=Path.GetFullPath(args[option+1]);
   if(!candidate.StartsWith(root,StringComparison.OrdinalIgnoreCase)||candidate.StartsWith(Path.GetFullPath(ordinaryDirectory),StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Private profile must be inside this project .tooling directory");
   SelectedDirectory=candidate;Debug.Log("ZTM_PRIVATE_PROFILE "+candidate);return candidate;
  }
 }
}