using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;
using VSwift.Utilities;

namespace VSwift.UserData;

/// <summary>
/// Lua wrapper for a single <see cref="Variant" />, exposing each contained transformer as a virtual property keyed by its <see cref="Modules.Transformers.Transformer.TransformerName" /> short name.
/// </summary>
/// <remarks>
/// When a transformer's <c>$type</c> resolves to a class decorated with <see cref="Attributes.TransformerAdapter" />, the entry is returned as that adapter wrapper; otherwise it is returned as a raw <see cref="JsonUserData" />. Mutating transformers through the virtual properties is disallowed; use <see cref="AddTransformer" />, <see cref="PatchTransformer" />, or <see cref="RemoveTransformer" /> instead.
/// </remarks>
[MoonSharpUserData]
public class VariantUserData : ExtensibleJsonUserData
{
    private readonly JArray _transformersArray;
    private readonly Dictionary<string, int> _transformerIndices = new();
    private readonly Dictionary<string, DynValue> _transformers = new();

    private static readonly Dictionary<string, string> TransformerShortNames = new();

    private static string TransformerShortName(string type)
    {
        if (TransformerShortNames.TryGetValue(type, out var name)) return name;
        return TransformerShortNames[type] =
            (Type.GetType(type)?.GetCustomAttributes(typeof(Modules.Transformers.Transformer), false).FirstOrDefault() as
                Modules.Transformers.Transformer)?.TransformerName ?? type.Split(',')[0].Trim().Split('.').Last();
    }

    /// <summary>
    /// Creates the wrapper for a <see cref="Variant" /> entry.
    /// </summary>
    /// <param name="token">The <see cref="Variant" /> JSON token.</param>
    public VariantUserData(JToken token) : base(token)
    {
        _transformersArray = RequireArray(token["Transformers"], "Variant.Transformers");
        var index = 0;
        foreach (var transformer in _transformersArray)
        {
            var typeStr = RequireString(transformer["$type"], "Transformers[].$type");
            var name = TransformerShortName(typeStr);
            _transformerIndices[name] = index++;
            _transformers[name] = WrapTransformer(typeStr, transformer);
        }
    }

    private DynValue WrapTransformer(string typeStr, JToken transformer)
    {
        var ty = Type.GetType(typeStr);
        if (ty != null && Adapters.TryGetAdapterFor(ty, out var adapter))
        {
            return MoonSharp.Interpreter.UserData.Create(Activator.CreateInstance(adapter, transformer));
        }
        return GetFromJToken(transformer);
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "Transformers";
        foreach (var name in _transformerIndices.Keys)
        {
            yield return name;
        }
    }

    /// <inheritdoc />
    public override DynValue TryToGet(string property)
    {
        if (property == "Transformers") return GetFromJToken(_transformersArray);
        if (_transformers.TryGetValue(property, out var t)) return t;
        return null;
    }

    /// <inheritdoc />
    public override bool TryToSet(string property, DynValue value)
    {
        if (property == "Transformers")
        {
            throw new ScriptRuntimeException("Use the transformer patching methods to update transformers!");
        }
        if (_transformers.ContainsKey(property))
        {
            throw new ScriptRuntimeException("Use the transformer patching methods to update transformers!");
        }
        return false;
    }

    /// <inheritdoc />
    public override bool TryToRemove(string property)
    {
        if (_transformers.ContainsKey(property))
        {
            throw new ScriptRuntimeException("Use RemoveTransformer to remove transformers!");
        }
        return false;
    }

    /// <summary>
    /// Adds a new transformer of the given type to this variant and runs <paramref name="callback" /> against the new entry for further configuration.
    /// </summary>
    /// <param name="type">The transformer's short name (the <see cref="Modules.Transformers.Transformer" /> attribute argument).</param>
    /// <param name="callback">Callback that receives the new entry for further configuration.</param>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="type" /> is not a registered transformer.</exception>
    public void AddTransformer(string type, Action<DynValue> callback)
    {
        if (!Transformers.TryGetTransformerByName(type, out var transformerType))
        {
            throw new ScriptRuntimeException($"{type} is not a valid transformer type");
        }

        var instance = Activator.CreateInstance(transformerType);
        var jObj = new JObject
        {
            ["$type"] = transformerType.AssemblyQualifiedName
        };
        var props = JObject.FromObject(instance);
        foreach (var prop in props)
        {
            jObj[prop.Key] = prop.Value;
        }

        _transformersArray.Add(jObj);
        _transformerIndices[type] = _transformersArray.Count - 1;

        var ud = WrapTransformer(transformerType.AssemblyQualifiedName, jObj);
        _transformers[type] = ud;
        callback(ud);
    }

    /// <summary>
    /// Patches the named transformer if it exists, otherwise adds it.
    /// </summary>
    /// <param name="type">The transformer's short name.</param>
    /// <param name="callback">Callback that receives the entry for further configuration.</param>
    public void EnsureTransformer(string type, Action<DynValue> callback)
    {
        if (_transformers.TryGetValue(type, out var existing))
        {
            callback(existing);
        }
        else
        {
            AddTransformer(type, callback);
        }
    }

    /// <summary>
    /// Runs <paramref name="callback" /> against the transformer with the given short name, doing nothing if absent.
    /// </summary>
    /// <param name="type">The transformer's short name.</param>
    /// <param name="callback">Callback that receives the existing entry for further configuration.</param>
    public void PatchTransformer(string type, Action<DynValue> callback)
    {
        if (_transformers.TryGetValue(type, out var existing))
        {
            callback(existing);
        }
    }

    /// <summary>
    /// Removes the named transformer from this variant.
    /// </summary>
    /// <param name="type">The transformer's short name.</param>
    public void RemoveTransformer(string type)
    {
        if (!_transformerIndices.TryGetValue(type, out var index)) return;
        _transformersArray.RemoveAt(index);
        _transformers.Remove(type);
        _transformerIndices.Clear();
        var i = 0;
        foreach (var transformer in _transformersArray)
        {
            var name = TransformerShortName(RequireString(transformer["$type"], "Transformers[].$type"));
            _transformerIndices[name] = i++;
        }
    }

    /// <summary>
    /// Returns whether this variant has a transformer of the given type.
    /// </summary>
    /// <param name="type">The transformer's short name.</param>
    /// <returns>True if the transformer exists, false otherwise.</returns>
    public bool HasTransformer(string type) => _transformerIndices.ContainsKey(type);
}
