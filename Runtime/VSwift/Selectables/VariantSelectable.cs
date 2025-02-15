using Newtonsoft.Json.Linq;
using PatchManager.Parts.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;
using VSwift.Utilities;

namespace VSwift.Selectables;

public sealed class VariantSelectable : BaseSelectable
{
    public readonly JObject VariantObject;
    public readonly PartSelectable PartSelectable;
    public readonly Dictionary<string, ISelectable> MatchedClasses;

    public VariantSelectable(JObject variantObject, PartSelectable partSelectable)
    {
        PartSelectable = partSelectable;
        VariantObject = variantObject;
        Name = variantObject["VariantId"].Value<string>();
        ElementType = Name;
        Classes = [];
        Children = [];
        MatchedClasses = [];
        foreach (var field in variantObject)
        {
            Classes.Add(field.Key);
        }

        var transformers = (JArray)variantObject["Transformers"];

        foreach (var transformer in transformers)
        {
            var transformerObject = (JObject)transformer;
            var type = Type.GetType(transformerObject["$type"]!.Value<string>());
            var name =
                (type!.GetCustomAttributes(typeof(Modules.Transformers.Transformer), false).First() as
                    Modules.Transformers.Transformer)!.TransformerName;
            ISelectable selectable;
            if (Adapters.TryGetAdapterFor(type, out var adapterType))
            {
                selectable = (ISelectable)Activator.CreateInstance(adapterType, transformerObject, this);
            }
            else
            {
                selectable = new JTokenSelectable(PartSelectable.SetModified, transformerObject, name);
            }
            MatchedClasses[name] = selectable;
        }
    }
    
    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        if (MatchedClasses.TryGetValue(@class, out var transformer))
        {
            classValue = transformer.GetValue();
            return true;
        }
        classValue = DataValue.Null;
        return false;
    }

    public override bool IsSameAs(ISelectable other) => other is VariantSelectable variantSelectable &&
                                                        variantSelectable.VariantObject == VariantObject;

    public override IModifiable OpenModification() => new JTokenModifiable(VariantObject, PartSelectable.SetModified);

    public override ISelectable AddElement(string elementType)
    {
        if (!Transformers.TryGetTransformerByName(elementType, out var transformerType))
            throw new ArgumentException($"{elementType} is not a valid transformer type", nameof(elementType));
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
        ISelectable selectable;
        if (Adapters.TryGetAdapterFor(obj.GetType(), out var adapterType))
        {
            selectable = (ISelectable)Activator.CreateInstance(adapterType, jObj, this);
        }
        else
        {
            selectable = new JTokenSelectable(PartSelectable.SetModified, jObj, elementType);
        }

        ((JArray)VariantObject["Transformers"])!.Add(jObj);
        MatchedClasses[elementType] = selectable;
        return selectable;

    }

    public override string Serialize() => VariantObject.ToString();

    public override DataValue GetValue() => DataValue.FromJToken(VariantObject);

    public override List<ISelectable> Children { get; }
    public override string Name { get; }
    public override List<string> Classes { get; }
    public override string ElementType { get; }
}