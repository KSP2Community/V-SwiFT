﻿using Castle.Core.Internal;
using I2.Loc;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using UnityEngine.Serialization;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Variants;

namespace VSwift.Modules.Data;

// ReSharper disable once InconsistentNaming
public class Data_PartSwitch : ModuleData, IMassModifier
{
    public override Type ModuleType => typeof(Module_PartSwitch);


    [KSPDefinition] public List<VariantSet> VariantSets = [];

    [KSPState] public List<string> ActiveVariants = [];

    [KSPDefinition] public List<AttachNodeDefinition> PredefinedDynamicNodes = [];
    
    // [KSPState] public List<(string, bool)>? OriginalGameObjectStates = null;

    public override List<OABPartData.PartInfoModuleEntry> GetPartInfoEntries(Type partBehaviourModuleType,
        List<OABPartData.PartInfoModuleEntry> emptyPartInfoEntryList)
    {
        emptyPartInfoEntryList.Add(
            new OABPartData.PartInfoModuleEntry(LocalizationManager.GetTranslation("VSwift/Variants"),
                GetVariantSetEntries));
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
        VariantSet variantSet) => variantSet.Variants.Select(GetVariantEntry).ToList();

    private static OABPartData.PartInfoModuleSubEntry GetVariantEntry(Variant variant) =>
        variant.VariantTechs.Count == 0
            ? new OABPartData.PartInfoModuleSubEntry(
                LocalizationManager.GetTranslation(
                    variant.VariantLocalizationKey.IsNullOrEmpty()
                        ? variant.VariantId
                        : variant.VariantLocalizationKey)
            )
            : new OABPartData.PartInfoModuleSubEntry(
                LocalizationManager.GetTranslation(
                    variant.VariantLocalizationKey.IsNullOrEmpty()
                        ? variant.VariantId
                        : variant.VariantLocalizationKey),
                variant.VariantTechs.Select(tech =>
                    new OABPartData.PartInfoModuleSubEntry(LocalizationManager.GetTranslation(GameManager.Instance.Game
                        .ScienceManager.TechNodeDataStore.AvailableData[tech]?.NameLocKey ?? ""))).ToList()
            );
    public Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>>? GetStoredVariantInformation()
    {
        var i = 0;
        Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>> result = [];
        foreach (var variantSet in VariantSets)
        {
            var currentSet = result[$"{i}"] = [];
            if (ActiveVariants.Count <= i)
            {
                ActiveVariants.Add(variantSet.Variants.First().VariantId);
            }

            if (variantSet.Variants.All(variant => ActiveVariants[i] != variant.VariantId))
            {
                ActiveVariants[i] = variantSet.Variants.First().VariantId;
            }
            var variant = variantSet.Variants.First(variant =>
                ActiveVariants[i] == variant.VariantId);
            var j = 0;
            foreach (var transformer in variant.Transformers.Where(transformer => transformer.SavesInformation))
            {
                var savedInformation = transformer.SaveInformation();
                currentSet[$"{j++}"] =
                    (savedInformation.savedType.AssemblyQualifiedName, savedInformation.savedValue);
            }
            i += 1;
        }

        return result; // Just make it easier when deserializing
    }

    [KSPState] public float MassModifier { get; set; } = 0;
}