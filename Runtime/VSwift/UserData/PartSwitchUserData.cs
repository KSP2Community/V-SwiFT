using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;
using PatchManager.Parts.Attributes;
using VSwift.Modules.Data;

namespace VSwift.UserData;

/// <summary>
/// Lua wrapper for the <see cref="Data_PartSwitch" /> module-data adapter, exposing <c>VariantSets</c> as a typed <see cref="VariantSetsUserData" /> and <c>PredefinedDynamicNodes</c> as a typed <see cref="NodesUserData" />.
/// </summary>
[ModuleDataAdapter(typeof(Data_PartSwitch))]
[MoonSharpUserData]
public class PartSwitchUserData : ExtensibleJsonUserData
{
    private DynValue _variantSets;
    private DynValue _predefinedDynamicNodes;

    /// <summary>
    /// Creates the wrapper for a <see cref="Data_PartSwitch" /> entry.
    /// </summary>
    /// <param name="token">The <see cref="Data_PartSwitch" /> JSON token.</param>
    public PartSwitchUserData(JToken token) : base(token)
    {
        _variantSets = MoonSharp.Interpreter.UserData.Create(new VariantSetsUserData((JArray)token["VariantSets"]));
        _predefinedDynamicNodes = MoonSharp.Interpreter.UserData.Create(new NodesUserData((JArray)token["PredefinedDynamicNodes"]));
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "VariantSets";
        yield return "PredefinedDynamicNodes";
    }

    /// <inheritdoc />
    public override DynValue TryToGet(string property)
    {
        if (property == "VariantSets") return _variantSets;
        if (property == "PredefinedDynamicNodes") return _predefinedDynamicNodes;
        return null;
    }

    /// <inheritdoc />
    public override bool TryToSet(string property, DynValue value)
    {
        if (property == "VariantSets" || property == "PredefinedDynamicNodes") throw new ScriptRuntimeException("You are unable to set this property.");
        return false;
    }

    /// <inheritdoc />
    public override bool TryToRemove(string property)
    {
        throw new ScriptRuntimeException("You are unable to remove this property.");
    }

    /// <summary>
    /// Adds a new variant set with the given <c>VariantSetId</c> and runs <paramref name="callback" /> against it for further configuration.
    /// </summary>
    /// <param name="name">The new variant set's <c>VariantSetId</c>.</param>
    /// <param name="callback">Callback that receives the new variant set for further configuration.</param>
    public void AddVariantSet(string name, Action<VariantSetUserData> callback)
    {
        ((VariantSetsUserData)_variantSets.UserData.Object).Add(name, callback);
    }

    /// <summary>
    /// Patches the named variant set if it exists, otherwise adds it.
    /// </summary>
    /// <param name="name">The variant set's <c>VariantSetId</c>.</param>
    /// <param name="callback">Callback that receives the variant set for further configuration.</param>
    public void EnsureVariantSet(string name, Action<VariantSetUserData> callback)
    {
        ((VariantSetsUserData)_variantSets.UserData.Object).Ensure(name, callback);
    }

    /// <summary>
    /// Runs <paramref name="callback" /> against the variant set with the given <c>VariantSetId</c>, doing nothing if absent.
    /// </summary>
    /// <param name="name">The variant set's <c>VariantSetId</c>.</param>
    /// <param name="callback">Callback that receives the existing variant set for further configuration.</param>
    public void PatchVariantSet(string name, Action<VariantSetUserData> callback)
    {
        var wrapper = (VariantSetsUserData)_variantSets.UserData.Object;
        var entry = wrapper[name];
        if (entry.Type != DataType.Nil)
        {
            callback((VariantSetUserData)entry.UserData.Object);
        }
    }

    /// <summary>
    /// Removes the named variant set from the part-switch.
    /// </summary>
    /// <param name="name">The variant set's <c>VariantSetId</c>.</param>
    public void RemoveVariantSet(string name)
    {
        ((VariantSetsUserData)_variantSets.UserData.Object).Remove(name);
    }
}
