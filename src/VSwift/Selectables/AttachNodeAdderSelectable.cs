using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;
using VSwift.Attributes;
using VSwift.Modules.Transformers;

namespace VSwift.Selectables;

[TransformerAdapter(typeof(AttachNodeAdder))]
public sealed class AttachNodeAdderSelectable : BaseSelectable
{/// <summary>
    /// The serialized data for this selectable
    /// </summary>
    public readonly JObject SerializedData;

    /// <summary>
    /// The part selectable that owns this selectable
    /// </summary>
    public readonly PartSelectable Selectable;
    public AttachNodeAdderSelectable(JObject transformerData, VariantSelectable variantSelectable)
    {
        
        SerializedData = transformerData;
        Name = "AttachNodeAdder";
        Selectable = variantSelectable.PartSelectable;
        ElementType = "AttachNodeAdder";
        Classes = [];
        Children = [];
        foreach (var field in SerializedData)
        {
            Classes.Add(field.Key);
            if (field.Value!.Type == JTokenType.Object)
            {
                Children.Add(new JTokenSelectable(Selectable.SetModified, field.Value, field.Key, field.Key));
            }
        }
        foreach (var jToken in (JArray)SerializedData["Nodes"]!)
        {
            var mode = (JObject)jToken;
            Classes.Add(mode["nodeID"]!.Value<string>());
            Children.Add(new JTokenSelectable(Selectable.SetModified,mode,m => m["nodeID"].Value<string>(),"attach_node"));
        }
    }

    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        if (SerializedData.TryGetValue(@class, out var value))
        {
            classValue = DataValue.FromJToken(value);
            return true;
        }

        foreach (var jToken in (JArray)SerializedData["Nodes"])
        {
            var mode = (JObject)jToken;
            if (mode["nodeID"].Value<string>() != @class)
            {
                continue;
            }

            classValue = DataValue.FromJToken(mode);
            return true;
        }

        classValue = DataValue.Null;
        return false;
    }

    public override bool IsSameAs(ISelectable other)=>
        (other is AttachNodeAdderSelectable dataEngineSelectable) &&
        SerializedData == dataEngineSelectable.SerializedData;

    public override IModifiable OpenModification() => new JTokenModifiable(SerializedData, Selectable.SetModified);

    public override ISelectable AddElement(string elementType)
    {
        var engineModeData = new AttachNodeDefinition()
        {
            nodeID = elementType
        };
        var json = JObject.FromObject(engineModeData);
        ((JArray)SerializedData["Nodes"]!).Add(json);
        return new JTokenSelectable(Selectable.SetModified, json, mode => mode["nodeID"].Value<string>(),
            "attach_node");
    }

    public override string Serialize() => SerializedData.ToString();

    public override DataValue GetValue() => DataValue.FromJToken(SerializedData);

    public override List<ISelectable> Children { get; }
    public override string Name { get; }
    public override List<string> Classes { get; }
    public override string ElementType { get; }
}