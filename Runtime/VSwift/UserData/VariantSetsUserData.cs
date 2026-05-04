using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace VSwift.UserData;

[MoonSharpUserData]
public class VariantSetsUserData : IndexedListUserData
{
    public VariantSetsUserData(JArray token) : base(token)
    {
    }

    public override string Name(JToken source)
    {
        return source["VariantSetId"].Value<string>();
    }

    public override DynValue Convert(JToken source)
    {
        return MoonSharp.Interpreter.UserData.Create(new VariantUserData(source));
    }
}