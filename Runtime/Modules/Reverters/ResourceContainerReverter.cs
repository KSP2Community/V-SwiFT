using System.Collections.Generic;
using System.Linq;
using KSP.Game;
using KSP.Modules;
using KSP.OAB;
using KSP.Sim.Definitions;
using KSP.Sim.ResourceSystem;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Reverters
{
    /// <summary>
    /// Reverts a <see cref="Transformers.ResourceContainerAdder" /> or <see cref="Transformers.ResourceContainerRemover" /> by restoring the part's original resource containers.
    /// </summary>
    public class ResourceContainerReverter : IReverter
    {
        private static ResourceContainerReverter? _instance;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        /// <summary>
        /// Gets the singleton reverter instance.
        /// </summary>
        public static ResourceContainerReverter? Instance => _instance ??= new ResourceContainerReverter();

        /// <inheritdoc />
        public object? Store(Module_PartSwitch partSwitch)
        {
            var oabPart = (ObjectAssemblyPart)partSwitch.OABPart;
            // return oabPart.AvailablePart.PartData.resourceContainers.ToList();
            var partCore = GameManager.Instance.Game.Parts.Get(oabPart.AvailablePart.PartData.partName);
            // IVSwiftLogger.Instance.LogInfo($"Part data are same instance? {ReferenceEquals(partCore.data,oabPart.AvailablePart.PartData)}");
            return partCore.data.resourceContainers;
        }

        /// <inheritdoc />
        public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
        {
            // IVSwiftLogger.Instance.LogInfo("Reverting!!!");
            var oabPart = (ObjectAssemblyPart)partSwitch.OABPart;
            var resourceContainers = (List<ContainedResourceDefinition>)data;
            if (resourceContainers is { Count: > 0 })
            {
                var resourceContainer = new ResourceContainer(
                    GameManager.Instance.Game.ResourceDefinitionDatabase,
                    resourceContainers);
                resourceContainer.FreezeDefinitions();
                oabPart.Container = resourceContainer;
            }
            else
            {
                oabPart.Container = null;
            }

            if (!oabPart.TryGetModule(typeof(Module_ResourceCapacities), out var module)) return;
            var moduleResourceCapacities = (Module_ResourceCapacities)module;
            moduleResourceCapacities.Shutdown();
            moduleResourceCapacities.ValueChangeHandlers.Clear();
            moduleResourceCapacities.DataResourceCapacities.RebuildDataContext();
            moduleResourceCapacities.Initialize();
        }

        /// <inheritdoc />
        public bool RequiresInVariantSet => false;

        /// <inheritdoc />
        public bool AppliesInFlight => false;
    }
}
