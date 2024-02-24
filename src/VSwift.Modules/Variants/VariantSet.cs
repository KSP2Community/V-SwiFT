using JetBrains.Annotations;

namespace VSwift.Modules.Variants;


[UsedImplicitly]
public class VariantSet
{
    public string VariantSetId = ""; // Just used for referencing the variant set
    public string VariantSetLocalizationKey = ""; // This defaults to the variant set id if its null or empty
    public bool IsPopout = false; // Whether or not this is a button to pull out a popout window
    public List<Variant> Variants = [];
}