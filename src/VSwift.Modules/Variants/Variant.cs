using JetBrains.Annotations;

namespace VSwift.Modules.Variants;

[UsedImplicitly]
public class Variant
{
    public string VariantId = "";
    public string VariantLocalizationKey = ""; // If null or empty, defaults to the variant ID
    public List<string> Transforms = [];
}