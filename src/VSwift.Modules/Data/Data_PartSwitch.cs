using Castle.Core.Internal;
using I2.Loc;
using KSP.Sim;
using KSP.Sim.Definitions;
using UnityEngine.Serialization;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Variants;

namespace VSwift.Modules.Data;

// ReSharper disable once InconsistentNaming
public class Data_PartSwitch : ModuleData
{
    public override Type ModuleType => typeof(Module_PartSwitch);

    
    [KSPDefinition]
    public List<VariantSet> VariantSets = [];

    [KSPState] public List<string> ActiveVariants = [];

    // [KSPState] public List<(string, bool)>? OriginalGameObjectStates = null;

    public override List<OABPartData.PartInfoModuleEntry> GetPartInfoEntries(Type partBehaviourModuleType, List<OABPartData.PartInfoModuleEntry> emptyPartInfoEntryList)
    {
        emptyPartInfoEntryList.Add(new OABPartData.PartInfoModuleEntry(LocalizationManager.GetTranslation("VSwift/Variants"),GetVariantSetEntries));
        return emptyPartInfoEntryList;
    }

    private List<OABPartData.PartInfoModuleSubEntry> GetVariantSetEntries(OABPartData.OABSituationStats stats) =>
        VariantSets.Select(
            variantSet => new OABPartData.PartInfoModuleSubEntry(
                LocalizationManager.GetTranslation(variantSet.VariantSetLocalizationKey.IsNullOrEmpty()
                    ? variantSet.VariantSetId
                    : variantSet.VariantSetLocalizationKey
                ),
                GetVariantEntries(stats, variantSet)
            )
        ).ToList();
    
    private static List<OABPartData.PartInfoModuleSubEntry> GetVariantEntries(OABPartData.OABSituationStats stats,
        VariantSet variantSet) =>
        variantSet.Variants.Select(
            variant => new OABPartData.PartInfoModuleSubEntry(
                LocalizationManager.GetTranslation(
                    variant.VariantLocalizationKey.IsNullOrEmpty() 
                        ? variant.VariantId 
                        : variant.VariantLocalizationKey)
                )
            ).ToList();
}