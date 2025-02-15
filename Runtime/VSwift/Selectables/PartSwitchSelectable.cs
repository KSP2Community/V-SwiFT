using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.Attributes;
using PatchManager.Parts.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using VSwift.Modules.Data;
using VSwift.Modules.Logging;
using VSwift.Modules.Variants;

namespace VSwift.Selectables;

[ModuleDataAdapter(typeof(Data_PartSwitch))]
[UsedImplicitly]
public sealed class PartSwitchSelectable : BaseSelectable
{
    public readonly JObject SerializedData;

    public readonly PartSelectable Selectable;

    public Dictionary<string, VariantSetSelectable> MatchedClasses;

    public PartSwitchSelectable(JObject moduleData, ModuleSelectable moduleSelectable)
    {
        SerializedData = (JObject)moduleData["DataObject"];
        Selectable = moduleSelectable.Selectable;
        Name = "Data_PartSwitch";
        ElementType = moduleData["Name"].Value<string>();
        Classes = [];
        Children = [];
        MatchedClasses = [];
        foreach (var field in SerializedData)
        {
            // IVSwiftLogger.Instance.LogInfo($"Adding {field.Key} = {field.Value}");
            Classes.Add(field.Key);
            if (field.Key == "PredefinedDynamicNodes")
            {
                Children.Add(new PredefinedDynamicNodeSelectable((JArray)field.Value,Selectable));
            }
        }
        var sets = SerializedData["VariantSets"]!;
        foreach (var set in sets)
        {
            var setObject = (JObject)set;
            var selectable = new VariantSetSelectable(setObject, Selectable);
            Children.Add(selectable);
            Classes.Add(setObject["VariantSetId"]!.Value<string>());
            MatchedClasses[setObject["VariantSetId"]!.Value<string>()] = selectable;
        }
    }

    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        if (MatchedClasses.TryGetValue(@class, out var variantSet))
        {
            classValue = variantSet.GetValue();
            return true;
        }
        classValue = DataValue.Null;
        return false;
    }

    public override bool IsSameAs(ISelectable other) => other is PartSwitchSelectable partSwitchSelectable &&
                                                        partSwitchSelectable.SerializedData == SerializedData;

    public override IModifiable OpenModification() => new JTokenModifiable(SerializedData, Selectable.SetModified);

    public override ISelectable AddElement(string elementType)
    {
        var obj = new VariantSet
        {
            VariantSetId = elementType,
        };
        var jObj = JObject.FromObject(obj);
        var selectable = new VariantSetSelectable(jObj, Selectable);
        ((JArray)SerializedData["VariantSets"])!.Add(jObj);
        MatchedClasses[elementType] = selectable;
        Selectable.SetModified();
        return selectable;
    }

    public override string Serialize() => SerializedData.ToString();

    public override DataValue GetValue() => DataValue.FromJToken(SerializedData);

    public override List<ISelectable> Children { get; }
    public override string Name { get; }
    public override List<string> Classes { get; }
    public override string ElementType { get; }
}