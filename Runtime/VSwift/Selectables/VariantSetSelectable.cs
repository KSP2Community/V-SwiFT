using Newtonsoft.Json.Linq;
using PatchManager.Parts.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;
using VSwift.Modules.Variants;

namespace VSwift.Selectables;

public sealed class VariantSetSelectable : BaseSelectable
{
    public readonly JObject SetObject;
    public readonly PartSelectable Selectable;
    public Dictionary<string, ISelectable> MatchedClasses;
    public VariantSetSelectable(JObject set, PartSelectable partSelectable)
    {
        SetObject = set;
        Selectable = partSelectable;
        
        Name = set["VariantSetId"].Value<string>();
        ElementType = Name;
        Classes = [];
        Children = [];
        MatchedClasses = [];
        foreach (var field in set)
        {
            Classes.Add(field.Key);
        }
        var variants = set["Variants"];
        foreach (var variant in variants)
        {
            var variantObject = (JObject)variant;
            var selectable = new VariantSelectable(variantObject, Selectable);
            Children.Add(selectable);
            Classes.Add(variantObject["VariantId"]!.Value<string>());
            MatchedClasses[variantObject["VariantId"]!.Value<string>()] = selectable;
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

    public override bool IsSameAs(ISelectable other) => other is VariantSetSelectable variantSetSelectable &&
                                                        variantSetSelectable.SetObject == SetObject;

    public override IModifiable OpenModification() => new JTokenModifiable(SetObject, Selectable.SetModified);

    public override ISelectable AddElement(string elementType)
    {
        var obj = new Variant
        {
            VariantId = elementType,
        };
        var jObj = JObject.FromObject(obj);
        var selectable = new VariantSelectable(jObj,Selectable);
        ((JArray)SetObject["Variants"])!.Add(jObj);
        MatchedClasses[elementType] = selectable;
        Selectable.SetModified();
        return selectable;
    }

    public override string Serialize() => SetObject.ToString();

    public override DataValue GetValue() => DataValue.FromJToken(SetObject);

    public override List<ISelectable> Children { get; }
    public override string Name { get; }
    public override List<string> Classes { get; }
    public override string ElementType { get; }
}