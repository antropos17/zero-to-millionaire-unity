using System;
using System.Text.Json;
using ZeroToMillionaire;

class JsonCodec : IStateCodec
{
    static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };
    public string Encode(PlayerState state) { return JsonSerializer.Serialize(state, Options); }
    public PlayerState Decode(string text)
    {
        var state=JsonSerializer.Deserialize<PlayerState>(text,Options);if(state==null)return null;
        using(var doc=JsonDocument.Parse(text)){
            var root=doc.RootElement;JsonElement value;
            state.version=root.TryGetProperty("version",out value)?value.GetInt32():0;
            if(!root.TryGetProperty("fragments",out value))state.fragments=null;
            if(!root.TryGetProperty("purchased",out value))state.purchased=null;
            if(!root.TryGetProperty("lastGrid",out value))state.lastGrid=null;
            if(!root.TryGetProperty("quests",out value))state.quests=null;
        }
        return state;
    }
}
class Program
{
    static int Main(string[] args)
    {
        try
        {
            Console.WriteLine(ModelChecks.Run(new JsonCodec(), args.Length > 0 ? args[0] : "../Evidence/checks-tmp"));
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
}
