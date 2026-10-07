using System;
using System.IO;
using UnityEngine;
namespace ZeroToMillionaire.Editor
{
    public static class V09DomainAudit
    {
        public static void Run()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string dir = Path.Combine(root, ".tooling", "v09-unity-core-checks-1");
            if (Directory.Exists(dir)) throw new Exception("Fresh private audit root required");
            string result = ModelChecks.Run(new UnityStateCodec(), dir);
            File.WriteAllText(Path.Combine(root, "Evidence", "v09-unity-core-checks-1.txt"), result);
            File.Copy(Path.Combine(dir, "stakes-v09-pacing.csv"), Path.Combine(root, "Evidence", "v09-unity-stakes-pacing.csv"), false);
            Debug.Log(result);
        }
    }
}
