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
    /// I will do a custom adapter for this on release to bring it inline with the games resource container stuff
    /// </summary>
    [Transformer(nameof(ResourceContainerAdder))]
    public class ResourceContainerAdder : ITransformer
    {
        [UsedImplicitly]
        public List<ContainedResourceDefinition> Containers = new() { };


        public IReverter? Reverter => ResourceContainerReverter.Instance;
        public bool SavesInformation => true;
        public bool VisualizesInformation => true;

        public void ApplyInFlight(Module_PartSwitch partSwitch)
        {
            // We really need to do some patching to make sure our module gets initialized *first* at all times
        }

        public (Type savedType, JToken savedValue) SaveInformation()
        {
            return (typeof(ResourceContainerLoader), JToken.Parse(IOProvider.ToJson(Containers)));
        }

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

        public void ApplyCommon(Module_PartSwitch partSwitch)
        {
        }
    }
}