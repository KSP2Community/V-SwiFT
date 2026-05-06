using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;
using VSwift.Modules.Variants;

namespace VSwift.UserData;

/// <summary>
/// Lua wrapper for a single <see cref="VariantSet" />, exposing each contained variant as a virtual property keyed by its <c>VariantId</c>.
/// </summary>
/// <remarks>
/// Mutating variants through the virtual properties is disallowed; use <see cref="AddVariant" />, <see cref="PatchVariant" />, or <see cref="RemoveVariant" /> instead.
/// </remarks>
[MoonSharpUserData]
public class VariantSetUserData : ExtensibleJsonUserData
{
    private readonly JArray _variantsArray;
    private readonly Dictionary<string, int> _variantIndices = new();
    private readonly Dictionary<string, DynValue> _variants = new();

    /// <summary>
    /// Creates the wrapper for a <see cref="VariantSet" /> entry.
    /// </summary>
    /// <param name="token">The <see cref="VariantSet" /> JSON token.</param>
    public VariantSetUserData(JToken token) : base(token)
    {
        _variantsArray = RequireArray(token["Variants"], "VariantSet.Variants");
        var index = 0;
        foreach (var variant in _variantsArray)
        {
            var variantId = RequireString(variant["VariantId"], "Variants[].VariantId");
            _variantIndices[variantId] = index++;
            _variants[variantId] = MoonSharp.Interpreter.UserData.Create(new VariantUserData(variant));
        }
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "Variants";
        foreach (var variantId in _variantIndices.Keys)
        {
            yield return variantId;
        }
    }

    /// <inheritdoc />
    public override DynValue TryToGet(string property)
    {
        if (property == "Variants") return GetFromJToken(_variantsArray);
        if (_variants.TryGetValue(property, out var variant)) return variant;
        return null;
    }

    /// <inheritdoc />
    public override bool TryToSet(string property, DynValue value)
    {
        if (property == "Variants")
        {
            throw new ScriptRuntimeException("Use the variant patching methods to update variants!");
        }
        if (_variants.ContainsKey(property))
        {
            throw new ScriptRuntimeException("Use the variant patching methods to update variants!");
        }
        return false;
    }

    /// <inheritdoc />
    public override bool TryToRemove(string property)
    {
        if (_variants.ContainsKey(property))
        {
            throw new ScriptRuntimeException("Use RemoveVariant to remove variants!");
        }
        return false;
    }

    /// <summary>
    /// Adds a new variant with the given <c>VariantId</c> to this set and runs <paramref name="callback" /> against it for further configuration.
    /// </summary>
    /// <param name="variantId">The new variant's <c>VariantId</c>.</param>
    /// <param name="callback">Callback that receives the new variant for further configuration.</param>
    public void AddVariant(string variantId, Action<VariantUserData> callback)
    {
        var variant = new Variant
        {
            VariantId = variantId
        };
        var json = JObject.FromObject(variant);
        _variantsArray.Add(json);
        _variantIndices[variantId] = _variantsArray.Count - 1;

        var typed = new VariantUserData(json);
        var ud = MoonSharp.Interpreter.UserData.Create(typed);
        _variants[variantId] = ud;
        callback(typed);
    }

    /// <summary>
    /// Patches the named variant if it exists, otherwise adds it.
    /// </summary>
    /// <param name="variantId">The variant's <c>VariantId</c>.</param>
    /// <param name="callback">Callback that receives the variant for further configuration.</param>
    public void EnsureVariant(string variantId, Action<VariantUserData> callback)
    {
        if (_variants.TryGetValue(variantId, out var existing))
        {
            callback((VariantUserData)existing.UserData.Object);
        }
        else
        {
            AddVariant(variantId, callback);
        }
    }

    /// <summary>
    /// Runs <paramref name="callback" /> against the variant with the given <c>VariantId</c>, doing nothing if absent.
    /// </summary>
    /// <param name="variantId">The variant's <c>VariantId</c>.</param>
    /// <param name="callback">Callback that receives the existing variant for further configuration.</param>
    public void PatchVariant(string variantId, Action<VariantUserData> callback)
    {
        if (_variants.TryGetValue(variantId, out var existing))
        {
            callback((VariantUserData)existing.UserData.Object);
        }
    }

    /// <summary>
    /// Removes the named variant from this set.
    /// </summary>
    /// <param name="variantId">The variant's <c>VariantId</c>.</param>
    public void RemoveVariant(string variantId)
    {
        if (!_variantIndices.TryGetValue(variantId, out var index)) return;
        _variantsArray.RemoveAt(index);
        _variants.Remove(variantId);
        _variantIndices.Clear();
        var i = 0;
        foreach (var variant in _variantsArray)
        {
            _variantIndices[RequireString(variant["VariantId"], "Variants[].VariantId")] = i++;
        }
    }

    /// <summary>
    /// Returns whether this set contains a variant with the given <c>VariantId</c>.
    /// </summary>
    /// <param name="variantId">The variant's <c>VariantId</c>.</param>
    /// <returns>True if the variant exists, false otherwise.</returns>
    public bool HasVariant(string variantId) => _variantIndices.ContainsKey(variantId);
}
