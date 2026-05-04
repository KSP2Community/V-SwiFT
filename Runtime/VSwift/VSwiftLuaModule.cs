using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.Parts.UserData;

namespace VSwift;

[PatchManagerModule("VSwift")]
[MoonSharpUserData]
public class VSwiftLuaModule
{

    private PatchManagerCore _core;
    private Universe _universe;

    public VSwiftLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }

    public void AddPAMOverride(PartUserData part)
    {
        var asObject = (JObject)part.Token;
        ((JArray)asObject["PAMModuleVisualsOverride"]).Add(new JObject
        {
            ["PartComponentModuleName"] = "PartComponentModule_PartSwitch",
            ["ModuleDisplayName"] = "VSwift/PartSwitch",
            ["ShowHeader"] = true,
            ["ShowFooter"] = false
        });
    }

    public void AddPartSwitch(PartUserData part, Action<DynValue> callback)
    {
        part.AddModule("Module_PartSwitch", module =>
        {
            module.AddData("Data_PartSwitch", callback);
        });
        AddPAMOverride(part);
    }
}
