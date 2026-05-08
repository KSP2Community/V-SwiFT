using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.Parts.UserData;

namespace VSwift;

/// <summary>
/// Lua module exposed under the global <c>PM.VSwift</c>, providing helpers for attaching V-SwiFT part-switching to parts.
/// </summary>
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

    /// <summary>
    /// Adds a PAM module-visuals override on the given part so <see cref="Module_PartSwitch" /> displays under a localized header in the parts manager.
    /// </summary>
    /// <param name="part">The part to add the PAM override to.</param>
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

    /// <summary>
    /// Adds <see cref="Module_PartSwitch" /> with a <see cref="Data_PartSwitch" /> entry to the given part, runs <paramref name="callback" /> against the new data for further configuration, and applies the PAM module-visuals override.
    /// </summary>
    /// <param name="part">The part to receive the part-switch module.</param>
    /// <param name="callback">Callback that receives the new <see cref="Data_PartSwitch" /> entry for further configuration.</param>
    public void AddPartSwitch(PartUserData part, Action<DynValue> callback)
    {
        part.AddModule("Module_PartSwitch", module =>
        {
            module.AddData("Data_PartSwitch", callback);
        });
        AddPAMOverride(part);
    }
}
