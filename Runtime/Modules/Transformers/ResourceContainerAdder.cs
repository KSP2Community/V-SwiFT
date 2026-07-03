using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using KSP.Game;
using KSP.IO;
using KSP.Modules;
using KSP.OAB;
using KSP.Sim.ResourceSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.InformationLoaders;
using VSwift.Modules.Logging;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Adds resource containers to the part when active, persisted across saves and rendered in the variant-info popout.
    /// </summary>
    [Serializable]
    [Transformer(nameof(ResourceContainerAdder))]
    [TransformerCategory("Resources")]
    [TransformerDescription("Add containers")]
    public class ResourceContainerAdder : ITransformer
    {
        /// <summary>
        /// The resource-container definitions to add.
        /// </summary>
        [Tooltip("Resource containers to add to the part when this variant is active.")]
        [UsedImplicitly]
        public List<ContainedResourceDefinition> Containers = new() { };


        /// <inheritdoc />
        public IReverter? Reverter => ResourceContainerReverter.Instance;

        /// <inheritdoc />
        public bool SavesInformation => true;

        /// <inheritdoc />
        public bool VisualizesInformation => true;

        /// <inheritdoc />
        public void ApplyInFlight(Module_PartSwitch partSwitch)
        {
            // We really need to do some patching to make sure our module gets initialized *first* at all times
        }

        /// <inheritdoc />
        public (Type savedType, JToken savedValue) SaveInformation()
        {
            return (typeof(ResourceContainerLoader), JToken.Parse(IOProvider.ToJson(Containers)));
        }

        /// <inheritdoc />
        public void ApplyInOab(Module_PartSwitch partSwitch)
        {
            // IVSwiftLogger.Instance.LogInfo(Environment.StackTrace);
            var oabPart = (ObjectAssemblyPart)partSwitch.OABPart;
            var resourceContainers = Containers;
            if (resourceContainers is { Count: > 0 })
            {
                var database = GameManager.Instance.Game.ResourceDefinitionDatabase;
                var definitions = new List<ContainedResourceDefinition>();
                if (oabPart.Container is ResourceContainer existing)
                {
                    for (int i = 0; i < existing.ResourceIDMap.Count; i++)
                    {
                        definitions.Add(new ContainedResourceDefinition(
                            existing.GetResourceContainedData(existing.ResourceIDMap[i]),
                            database));
                    }
                }

                definitions.AddRange(resourceContainers);
                var resourceContainer = new ResourceContainer(database, definitions);
                resourceContainer.FreezeDefinitions();
                oabPart.Container = resourceContainer;
            }
            if (!oabPart.TryGetModule(typeof(Module_ResourceCapacities), out var module)) return;
            var moduleResourceCapacities = (Module_ResourceCapacities)module;
            moduleResourceCapacities.Shutdown();
            moduleResourceCapacities.ValueChangeHandlers.Clear();
            moduleResourceCapacities.DataResourceCapacities.RebuildDataContext();
            moduleResourceCapacities.Initialize();
        }

        /// <inheritdoc />
        public void ApplyCommon(Module_PartSwitch partSwitch)
        {
        }
    }
}
