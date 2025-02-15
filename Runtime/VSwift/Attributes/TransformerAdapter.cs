using JetBrains.Annotations;
using PatchManager.SassyPatching.Interfaces;

namespace VSwift.Attributes;

/// <summary>
/// Adapt a specific transformer type with this selectable
/// Any class this is used on requires a constructor with the parameters (JObject, VariantSelectable)
/// </summary>
/// <param name="transformerType">the type this is adapting</param>
[AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
[BaseTypeRequired(typeof(ISelectable))]
[MeansImplicitUse]
public class TransformerAdapter(Type transformerType) : Attribute
{
    public Type TransformerType => transformerType;
}