using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;
using VSwift.Modules.Variants;

namespace VSwift.UserData;

/// <summary>
/// Indexed-list wrapper for a <see cref="Data_PartSwitch" />.<c>VariantSets</c> array, keyed by each entry's <c>VariantSetId</c>.
/// </summary>
[MoonSharpUserData]
public class VariantSetsUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper around a variant-sets array.
    /// </summary>
    /// <param name="token">The <c>VariantSets</c> JSON array.</param>
    public VariantSetsUserData(JArray token) : base(token)
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return source["VariantSetId"].Value<string>();
    }

    /// <inheritdoc />
    public override DynValue Convert(JToken source)
    {
        return MoonSharp.Interpreter.UserData.Create(new VariantSetUserData(source));
    }

    /// <summary>
    /// Adds a new variant set with the given <c>VariantSetId</c> and runs <paramref name="callback" /> against it for further configuration.
    /// </summary>
    /// <param name="name">The new variant set's <c>VariantSetId</c>.</param>
    /// <param name="callback">Callback that receives the new variant set for further configuration.</param>
    public void Add(string name, Action<VariantSetUserData> callback)
    {
        var variantSet = new VariantSet
        {
            VariantSetId = name
        };
        var json = JObject.FromObject(variantSet);
        var ud = Convert(json);
        callback((VariantSetUserData)ud.UserData.Object);
        Append(ud);
    }

    /// <summary>
    /// Patches the named variant set if it exists, otherwise adds it.
    /// </summary>
    /// <param name="name">The variant set's <c>VariantSetId</c>.</param>
    /// <param name="callback">Callback that receives the variant set for further configuration.</param>
    public void Ensure(string name, Action<VariantSetUserData> callback)
    {
        var existing = this[name];
        if (existing.Type != DataType.Nil)
        {
            callback((VariantSetUserData)existing.UserData.Object);
        }
        else
        {
            Add(name, callback);
        }
    }
}
