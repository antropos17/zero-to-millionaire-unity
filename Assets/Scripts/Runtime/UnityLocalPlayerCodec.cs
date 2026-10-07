using System;
using UnityEngine;
namespace ZeroToMillionaire
{
    public sealed class UnityLocalPlayerCodec : ILocalPlayerCodec
    {
        [Serializable] sealed class Raw
        { public int version; public long revision; public string displayName, avatarId, animationMode, notificationMode; }
        public string Encode(LocalPlayer p) => JsonUtility.ToJson(p);
        public LocalPlayer Decode(string text)
        {
            var raw=JsonUtility.FromJson<Raw>(text);
            if(raw==null)return null;
            return new LocalPlayer {version=raw.version,revision=raw.revision,displayName=raw.displayName,avatarId=raw.avatarId,animationMode=raw.animationMode,notificationMode=raw.notificationMode};
        }
    }
}
