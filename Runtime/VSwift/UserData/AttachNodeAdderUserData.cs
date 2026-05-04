using System;
using KSP.Sim.Definitions;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;
using VSwift.Attributes;
using VSwift.Modules.Transformers;

namespace VSwift.UserData;

[TransformerAdapter(typeof(AttachNodeAdder))]
[MoonSharpUserData]
public class AttachNodeAdderUserData : IndexedListUserData
{
    public AttachNodeAdderUserData(JToken token) : base((JArray)token["Nodes"])
    {
        
    }

    public override string Name(JToken source)
    {
        return source["nodeID"].Value<string>();
    }

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