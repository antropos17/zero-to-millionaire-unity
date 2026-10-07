using UnityEngine;
namespace ZeroToMillionaire
{
    public sealed class UnityLevel1CodecV7:ILevel1CodecV7
    {
        public string Encode(Level1StateV7 state)=>JsonUtility.ToJson(state,true);
        public Level1StateV7 Decode(string text)
        {
            if(!Schema7Shape.Valid(text))return null;
            if(text.Length>0&&text[0]=='\ufeff')text=text.Substring(1);
            var s=JsonUtility.FromJson<Level1StateV7>(text);string error;return Level1StateV7.Valid(s,out error)?s:null;
        }
    }
}
