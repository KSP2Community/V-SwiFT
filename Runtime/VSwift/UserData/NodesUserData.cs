using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace VSwift.UserData;

[MoonSharpUserData]
public class NodesUserData : IndexedListUserData
{
    public NodesUserData(JArray token) : base(token)
    {
    }

    public override string Name(JToken source)
    {
        return source["nodeID"].Value<string>();
    }
}