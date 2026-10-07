using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ZeroToMillionaire.Editor
{
    public static class DecorDomainAuditV10
    {
        public static void Run()
        {
            string root=Directory.GetParent(Application.dataPath).FullName;
            try{string result=DecorChecksV10.Run(new UnityLevel1CodecV7(),new UnityStateCodec(),Path.Combine(root,".tooling","v10-decor-unity-domain-2"));File.WriteAllText(Path.Combine(root,"Evidence","v10-decor-unity-domain-2.txt"),result);EditorApplication.Exit(0);}
            catch(Exception e){File.WriteAllText(Path.Combine(root,"Evidence","v10-decor-unity-domain-2.txt"),"FAIL "+e);EditorApplication.Exit(1);}
        }
    }
}
