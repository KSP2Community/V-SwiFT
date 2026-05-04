using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;
using PatchManager.Parts.Attributes;
using VSwift.Modules.Data;

namespace VSwift.UserData;

[ModuleDataAdapter(typeof(Data_PartSwitch))]
[MoonSharpUserData]
public class PartSwitchUserData : ExtensibleJsonUserData
{
    private DynValue _variantSets;
    private DynValue _predefinedDynamicNodes;
    
    public PartSwitchUserData(JToken token) : base(token)
    {
        _predefinedDynamicNodes = MoonSharp.Interpreter.UserData.Create(new NodesUserData((JArray)token["predefinedDynamicNodes"]));
        
    }

    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "VariantSets";
        yield return "PredefinedDynamicNodes";
    }

    public override DynValue TryToGet(string property)
    {
        if (property == "VariantSets") return _variantSets;
        if (property == "PredefinedDynamicNodes") return _predefinedDynamicNodes;
        return null;
    }

    public override bool TryToSet(string property, DynValue value)
    {
        if (property == "VariantSets" || property == "PredefinedDynamicNodes") throw new Exception("You are unable to set this property.");
        return false;
    }

    public override bool TryToRemove(string property)
    {
        throw new Exception("You are unable to remove this property.");
    }
}