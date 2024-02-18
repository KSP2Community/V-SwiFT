using JetBrains.Annotations;
using VSwift.Modules.Transformers;

namespace VSwift.Modules.Variants;

[UsedImplicitly]
public class Variant
{
    [UsedImplicitly]
    public string VariantId = "";
    [UsedImplicitly]
    public string VariantLocalizationKey = ""; // If null or empty, defaults to the variant ID
    [UsedImplicitly]
    public List<string> VariantTechs = []; // This is a list of technologies to unlock this variant
    [UsedImplicitly]
    public List<ITransformer> Transformers = [];
}