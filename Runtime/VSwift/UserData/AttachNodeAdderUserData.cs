using System;
using KSP.Sim.Definitions;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;
using VSwift.Attributes;
using VSwift.Modules.Transformers;

namespace VSwift.UserData;

/// <summary>
/// Transformer-adapter wrapper for <see cref="AttachNodeAdder" />, exposing the transformer's <c>Nodes</c> array as a typed indexed-list keyed by each entry's <c>nodeID</c>.
/// </summary>
[TransformerAdapter(typeof(AttachNodeAdder))]
[MoonSharpUserData]
public class AttachNodeAdderUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper around an <see cref="AttachNodeAdder" /> transformer's <c>Nodes</c> array.
    /// </summary>
    /// <param name="token">The transformer JSON token.</param>
    public AttachNodeAdderUserData(JToken token) : base((JArray)token["Nodes"])
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return source["nodeID"].Value<string>();
    }

    /// <summary>
    /// Adds a new attach-node definition with the given <c>nodeID</c> and runs <paramref name="callback" /> against the wrapped JSON for further configuration.
    /// </summary>
    /// <param name="id">The new node's <c>nodeID</c>.</param>
    /// <param name="callback">Callback that receives the new attach-node JSON for further configuration.</param>
    public void Add(string id, Action<JsonUserData> callback)
    {
        var attachNodeDefinition = new AttachNodeDefinition
        {
            nodeID = id
        };
        var json = JObject.FromObject(attachNodeDefinition);
        var ud = GetFromJToken(json);
        callback((JsonUserData)ud.UserData.Object);
        Append(ud);
    }
}
