using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Game;
using KSP.Modules;
using KSP.OAB;
using KSP.Sim.ResourceSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.InformationLoaders;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Removes resource containers by name from the part when active, persisted across saves.
    /// </summary>
    [Serializable]
    [Transformer(nameof(ResourceContainerRemover))]
    [TransformerCategory("Resources")]
    [TransformerDescription("Remove containers")]
    public class ResourceContainerRemover : ITransformer
    {
        /// <summary>
        /// The resource-container names to remove.
        /// </summary>
        [Tooltip("Names of resource containers to remove from the part when this variant is active.")]
        public List<string> Containers = new();

        /// <inheritdoc />
        public IReverter? Reverter => ResourceContainerReverter.Instance;

        /// <inheritdoc />
        public bool SavesInformation => true;

        /// <inheritdoc />
        public bool VisualizesInformation => false;

        /// <inheritdoc />
        public void ApplyInFlight(Module_PartSwitch partSwitch)
        {
        }

        /// <inheritdoc />
        public void ApplyInOab(Module_PartSwitch partSwitch)
        {
            var oabPart = (ObjectAssemblyPart)partSwitch.OABPart;
            var database = GameManager.Instance.Game.ResourceDefinitionDatabase;
            if (oabPart.Container is not ResourceContainer curContainer)
            {
                return;
            }

            List<ContainedResourceDefinition> newDefinitions = new()
            {
                Capacity = 0
            };
            foreach (ResourceDefinitionID resourceDef in curContainer.ResourceIDMap)
            {
                if (!Containers.Any(x => database.GetResourceIDFromName(x).Equals(resourceDef)))
                {
                    newDefinitions.Add(new ContainedResourceDefinition(
                        curContainer.GetResourceContainedData(resourceDef),
                        database));
                }
            }

            var newContainer = new ResourceContainer(database, newDefinitions);
            newContainer.FreezeDefinitions();
            oabPart.Container = newContainer;
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

        /// <inheritdoc />
        public (Type savedType, JToken savedValue) SaveInformation() =>
            (typeof(ResourceContainerRemoveLoader), JToken.FromObject(Containers));
    }
}
