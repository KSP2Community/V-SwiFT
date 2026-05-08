using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Game;
using KSP.IO;
using KSP.Modules;
using KSP.OAB;
using KSP.Sim.ResourceSystem;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Behaviours;
using VSwift.Modules.InformationLoaders;
using VSwift.Modules.Logging;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Removes resource containers by name from the part when active, persisted across saves.
    /// </summary>
    [Transformer(nameof(ResourceContainerRemover))]
    public class ResourceContainerRemover : ITransformer
    {
        /// <summary>
        /// The resource-container names to remove.
        /// </summary>
        public List<string> Containers = new() { };

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
            // var allContainers =  oabPart.Containers.Cast<ResourceContainer>().Select(container =>
            //     container.Where(id =>
            //         Containers.Select(GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName)
            //             .All(id2 => id2 != id)).ToList()).ToList();
            List<ResourceContainer> newContainers = new() { };
            foreach (var container in oabPart.Containers)
            {
                var curContainer = container as ResourceContainer;
                List<ContainedResourceDefinition> newDefinitions = new() { };
                for (var internalIndex = 0; internalIndex < curContainer!.ResourceIDMap.Count; internalIndex++)
                {
                    var resourceDef = curContainer.ResourceIDMap[internalIndex];
                    if (!Containers.Any(x => GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName(x).Equals(resourceDef)))
                    {
                        newDefinitions.Add(new ContainedResourceDefinition(new ContainedResourceData
                        {
                            IsPartOfRecipe = false,
                            ResourceID = resourceDef,
                            CapacityUnits = curContainer.CapacityUnitsLookup[internalIndex],
                            StoredUnits = curContainer.StoredUnitsLookup[internalIndex]
                        }, GameManager.Instance.Game.ResourceDefinitionDatabase));
                    }
                }
                if (newDefinitions.Count < 0) continue;
                var newContainer = new ResourceContainer(GameManager.Instance.Game.ResourceDefinitionDatabase,newDefinitions);
                newContainer.FreezeDefinitions();
                newContainers.Add(newContainer);
            }

            oabPart.Containers = newContainers.ToArray();
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
