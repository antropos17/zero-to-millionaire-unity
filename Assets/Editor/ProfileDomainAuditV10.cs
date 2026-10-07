using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ZeroToMillionaire.Editor
{
    public static class ProfileDomainAuditV10
    {
        public static void Run()
        {
            string root=Directory.GetParent(Application.dataPath).FullName;
            try
            {
                string dir=Path.Combine(root,".tooling","v10-domain-checks-readyfix-3");Directory.CreateDirectory(dir);
                string result=ModelChecks.Run(new UnityStateCodec(),dir)+"\n"+ProfileChecksV10.Run(new UnityLocalPlayerCodec(),dir);
                File.WriteAllText(Path.Combine(root,"Evidence","v10-slice1-unity-domain-readyfix-3.txt"),result);EditorApplication.Exit(0);
            }
            catch(Exception e){File.WriteAllText(Path.Combine(root,"Evidence","v10-slice1-unity-domain-readyfix-3.txt"),"FAIL "+e);EditorApplication.Exit(1);}
        }
    }
}
