using System;
using System.Collections.Generic;
using System.Linq;
using Redux;
using I2.Loc;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using Redux.Ecs.Components;
using Unity.Entities;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Variants;

namespace VSwift.Modules.Data
{
    /// <summary>
    /// Module-data attached to a part-switch module, carrying the variant sets, the per-set active variant, and the predefined dynamic attach nodes.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    [Serializable]
    public class Data_PartSwitch : ModuleData
    {
        /// <inheritdoc />
        public override Type ModuleType => typeof(Module_PartSwitch);


        /// <summary>
        /// The variant sets this module data carries.
        /// </summary>
        [KSPDefinition] public List<VariantSet> VariantSets = new();

        /// <summary>
        /// The currently-active variant ID per variant set, indexed positionally.
        /// </summary>
        [KSPState] public List<string> ActiveVariants = new();

        /// <summary>
        /// The definition-side default variant ID per variant set, indexed positionally.
        /// </summary>
        [KSPDefinition] public List<string> DefaultActiveVariants = new();

        /// <summary>
        /// Attach nodes whose dynamic state can be toggled by transformers.
        /// </summary>
        [KSPDefinition] public List<AttachNodeDefinition> PredefinedDynamicNodes = new();

        // [KSPState] public List<(string, bool)>? OriginalGameObjectStates = null;

        /// <inheritdoc />
        public override void SyncAllSymmetricalData(ModuleData sourceModuleData)
        {
            base.SyncAllSymmetricalData(sourceModuleData);
            if (sourceModuleData is not Data_PartSwitch source)
            {
                return;
            }

            ActiveVariants = new List<string>(source.ActiveVariants);
            MassModifier = source.MassModifier;
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Returns the per-variant transformer-saved data, indexed by variant-set position and transformer position.
        /// </summary>
        /// <returns>The saved data, ready for persistence on the serialized part.</returns>
        public Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>>? GetStoredVariantInformation()
        {
            var i = 0;
            Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>> result = new();
            foreach (var variantSet in VariantSets)
            {
                var currentSet = result[$"{i}"] = new Dictionary<string, (string savedType, JToken savedValue)>();
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

        /// <summary>
        /// Gets or sets the additional mass contributed by the active variant configuration.
        /// </summary>
        [KSPState]
        public float MassModifier
        {
            get => Entity == Entity.Null
                ? _massModifier
                : (float)World.DefaultGameObjectInjectionWorld.EntityManager
                    .GetComponentData<MassModifierData>(Entity).Value;
            set
            {
                if (Entity == Entity.Null)
                {
                    _massModifier = value;
                    return;
                }

                World.DefaultGameObjectInjectionWorld.EntityManager
                    .SetComponentData(Entity, new MassModifierData { Value = value });
            }
        }

        private float _massModifier;
        
        public override void BindToEntity(EntityManager em, Entity entity)
        {
            base.BindToEntity(em, entity);
            em.SetComponentData(entity, new MassModifierData { Value = _massModifier });
        }

        /// <inheritdoc />
        public override void UnbindFromEntity()
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (Entity != Entity.Null && em.Exists(Entity))
                _massModifier = (float)em.GetComponentData<MassModifierData>(Entity).Value;

            base.UnbindFromEntity();
        }
    }
}
