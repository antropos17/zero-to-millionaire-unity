using System;
using System.Text.Json;
using ZeroToMillionaire;
sealed class DecorCodecNet:ILevel1CodecV7
{
    static readonly JsonSerializerOptions Options=new JsonSerializerOptions{IncludeFields=true};
    public string Encode(Level1StateV7 s)=>JsonSerializer.Serialize(s,Options);
    public Level1StateV7 Decode(string s){if(!Schema7Shape.Valid(s))return null;var state=JsonSerializer.Deserialize<Level1StateV7>(s,Options);return Level1StateV7.Valid(state,out _)?state:null;}
}
sealed class LegacyCodecNet:IStateCodec
{
    static readonly JsonSerializerOptions Options=new JsonSerializerOptions{IncludeFields=true};
    public string Encode(PlayerState s)=>JsonSerializer.Serialize(s,Options);
    public PlayerState Decode(string s)
    {
        var state=JsonSerializer.Deserialize<PlayerState>(s,Options);if(state==null)return null;
        using(var doc=JsonDocument.Parse(s)){var r=doc.RootElement;state.version=r.TryGetProperty("version",out var v)?v.GetInt32():0;state.stakeMultiplier=r.TryGetProperty("stakeMultiplier",out v)?v.GetInt32():0;if(!r.TryGetProperty("fragments",out v))state.fragments=null;if(!r.TryGetProperty("purchased",out v))state.purchased=null;if(!r.TryGetProperty("lastGrid",out v))state.lastGrid=null;if(!r.TryGetProperty("quests",out v))state.quests=null;if(!r.TryGetProperty("tutorial",out v))state.tutorial=null;}return state;
    }
}
class DecorProgramV10
{
    static int Main(string[] args){try{Console.WriteLine(DecorChecksV10.Run(new DecorCodecNet(),new LegacyCodecNet(),args[0]));return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
