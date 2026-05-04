using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;
using VSwift.Utilities;

namespace VSwift.UserData;

public class TransformersUserData : IndexedListUserData
{
    public TransformersUserData(JArray token) : base(token)
    {
    }

    public override string Name(JToken source)
    {
        string type = source["$type"].Value<string>();
        return TransformerName(type);
    }

    private static Dictionary<string, string> TransformerNames = new();

    private static string TransformerName(string type)
    {
        if (TransformerNames.TryGetValue(type, out var name))
            return name;

        return TransformerNames[type] =
            (Type.GetType(type)?.GetCustomAttributes(typeof(Modules.Transformers.Transformer), false).First() as
                Modules.Transformers.Transformer)?.TransformerName ?? type.Split(',')[0].Trim().Split('.').Last();
    }

    public override DynValue Convert(JToken source)
    {
        string type = source["$type"].Value<string>();
        var ty = Type.GetType(type);
        if (ty != null && Adapters.TryGetAdapterFor(ty, out var adapter))
        {
            return MoonSharp.Interpreter.UserData.Create(Activator.CreateInstance(adapter, source));
        }

        return GetFromJToken(source);
    }

    public void Add(string type, Action<DynValue> callback)
    {
        if (!Transformers.TryGetTransformerByName(type, out var transformerType))
            throw new ArgumentException($"{type} is not a valid transformer type", nameof(type));
        var obj = Activator.CreateInstance(transformerType);
        var jObj = new JObject
        {
            ["$type"] = transformerType.AssemblyQualifiedName
        };
        var props = JObject.FromObject(obj);
        foreach (var (k, v) in props)
        {
            jObj[k] = v;
        }

        Append(GetFromJToken(jObj));

        callback(Conversions[Count - 1]);
    }
}