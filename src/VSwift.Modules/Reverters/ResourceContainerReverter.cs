using KSP.Game;
using KSP.Modules;
using KSP.OAB;
using KSP.Sim.Definitions;
using KSP.Sim.ResourceSystem;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Reverters;

public class ResourceContainerReverter : IReverter
{
    private static ResourceContainerReverter? _instance;
    public static ResourceContainerReverter Instance => _instance ??= new ResourceContainerReverter();
    public object Store(Module_PartSwitch partSwitch)
    {
        var oabPart = (ObjectAssemblyPart)partSwitch.OABPart;
        // return oabPart.AvailablePart.PartData.resourceContainers.ToList();
        var partCore = GameManager.Instance.Game.Parts.Get(oabPart.AvailablePart.PartData.partName);
        // IVSwiftLogger.Instance.LogInfo($"Part data are same instance? {ReferenceEquals(partCore.data,oabPart.AvailablePart.PartData)}");
        return partCore.data.resourceContainers;
    }

    public void Revert(Module_PartSwitch partSwitch, object data)
    {
        // IVSwiftLogger.Instance.LogInfo("Reverting!!!");
        var oabPart = (ObjectAssemblyPart)partSwitch.OABPart;
        var resourceContainers = (List<ContainedResourceDefinition>)data;
        if (resourceContainers is { Count: > 0 })
        {
            var list = new List<IResourceContainer>();
            foreach (var resourceContainer in resourceContainers.Select(containedResourceDefinition =>
                         new ResourceContainer(GameManager.Instance.Game.ResourceDefinitionDatabase,
                         [
                             containedResourceDefinition
                         ])))
            {
                // IVSwiftLogger.Instance.LogInfo($"ResetToOriginalState adding {resourceContainer.First()}");
                resourceContainer.FreezeDefinitions();
                list.Add(resourceContainer);
            }
            oabPart.Containers = list.ToArray();
        }

        if (!oabPart.TryGetModule(typeof(Module_ResourceCapacities), out var module)) return;
        var moduleResourceCapacities = (Module_ResourceCapacities)module;
        moduleResourceCapacities.OnShutdown();
        moduleResourceCapacities.dataResourceCapacities.RebuildDataContext();
        moduleResourceCapacities.OnInitialize();
    }
}