using KSP.Modules;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;
using VSwift.Attributes;
using VSwift.Modules.Transformers;

namespace VSwift.Selectables;

[TransformerAdapter(typeof(EngineModeSwapper))]
public sealed class EngineModeTransformerSelectable : BaseSelectable
{
    /// <summary>
    /// The serialized data for this selectable
    /// </summary>
    public readonly JObject SerializedData;

    /// <summary>
    /// The part selectable that owns this selectable
    /// </summary>
    public readonly PartSelectable Selectable;

    /// <summary>
    /// Initialize the selectable
    /// </summary>
    /// <param name="transformerData">Transformer data</param>
    /// <param name="variantSelectable">Variant selectable</param>
    public EngineModeTransformerSelectable(JObject transformerData, VariantSelectable variantSelectable)
    {
        SerializedData = transformerData;
        Name = transformerData["Name"].Value<string>();
        Selectable = variantSelectable.PartSelectable;
        ElementType = transformerData["Name"].Value<string>();
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

        var index = 0;
        List<int> removals = [];
        foreach (var jToken in (JArray)SerializedData["Modes"]!)
        {
            var currentIdx = index++;
            if (jToken.Type is JTokenType.Null or JTokenType.None)
            {
                removals.Add(currentIdx);
                continue;
            }
            var mode = (JObject)jToken;
            Classes.Add(mode["engineID"]!.Value<string>());
            Children.Add(new JTokenSelectable(Selectable.SetModified,mode,m => m["engineID"].Value<string>(),"engine_mode"));
        }

        removals.Reverse();
        foreach (var idx in removals)
            ((JArray)SerializedData["Modes"]!)[idx].Remove();
    }

    /// <inheritdoc />
    public override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        if (SerializedData.TryGetValue(@class, out var value))
        {
            classValue = DataValue.FromJToken(value);
            return true;
        }

        foreach (var jToken in (JArray)SerializedData["engineModes"])
        {
            var mode = (JObject)jToken;
            if (mode["engineID"].Value<string>() != @class)
            {
                continue;
            }

            classValue = DataValue.FromJToken(mode);
            return true;
        }

        classValue = DataValue.Null;
        return false;
    }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) =>
        (other is EngineModeTransformerSelectable dataEngineSelectable) &&
        SerializedData == dataEngineSelectable.SerializedData;

    /// <inheritdoc />
    public override IModifiable OpenModification() => new JTokenModifiable(SerializedData, Selectable.SetModified);

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        var engineModeData = new Data_Engine.EngineMode()
        {
            engineID = elementType
        };
        var json = JObject.FromObject(engineModeData);
        ((JArray)SerializedData["Modes"]!).Add(json);
        return new JTokenSelectable(Selectable.SetModified, json, mode => mode["engineID"].Value<string>(),
            "engine_mode");
    }

    /// <inheritdoc />
    public override string Serialize() => SerializedData.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(SerializedData);

    /// <inheritdoc />
    public override string ElementType { get; }
}