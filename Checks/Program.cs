using System;
using System.Text.Json;
using ZeroToMillionaire;

class JsonCodec : IStateCodec
{
    static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };
    public string Encode(PlayerState state) { return JsonSerializer.Serialize(state, Options); }
    public PlayerState Decode(string text) { return JsonSerializer.Deserialize<PlayerState>(text, Options); }
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
