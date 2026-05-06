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
    [Transformer(nameof(ResourceContainerAdder))]
    public class ResourceContainerAdder : ITransformer
    {
        /// <summary>
        /// The resource-container definitions to add.
        /// </summary>
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
                var list = oabPart.Containers.ToList();
                foreach (var resourceContainer in resourceContainers.Select(containedResourceDefinition =>
                             new ResourceContainer(GameManager.Instance.Game.ResourceDefinitionDatabase, containedResourceDefinition)))
                {
                    // IVSwiftLogger.Instance.LogInfo($"ApplyInOab adding {resourceContainer.First()}");
                    resourceContainer.FreezeDefinitions();
                    list.Add(resourceContainer);
                }
                oabPart.Containers = list.ToArray();
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
