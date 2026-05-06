using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace VSwift.UserData;


[MoonSharpUserData]
public class VariantUserData : ExtensibleJsonUserData
{
    private DynValue _transformers;
    public VariantUserData(JToken token) : base(token)
    {
        _transformers = MoonSharp.Interpreter.UserData.Create(new TransformersUserData((JArray)token["Transformers"]));
    }

    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "Transformers";
    }

    public override DynValue TryToGet(string property)
    {
        if (property == "Transformers")
        {
            return _transformers;
        }

        return null;
    }

    public override bool TryToSet(string property, DynValue value)
    {
        if (property == "Transformers") throw new Exception("You cannot set this property.");
        return false;
    }

    public override bool TryToRemove(string property)
    {
        throw new Exception("You cannot remove this property.");
    }
}