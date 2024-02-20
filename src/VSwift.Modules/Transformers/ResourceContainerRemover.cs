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

namespace VSwift.Modules.Transformers;
[Transformer(nameof(ResourceContainerRemover))]
public class ResourceContainerRemover : ITransformer
{
    public List<string> Containers = [];
    public IReverter Reverter => ResourceContainerReverter.Instance;
    public bool SavesInformation => true;
    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
        var oabPart = (ObjectAssemblyPart)partSwitch.OABPart;
        // var allContainers =  oabPart.Containers.Cast<ResourceContainer>().Select(container =>
        //     container.Where(id =>
        //         Containers.Select(GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName)
        //             .All(id2 => id2 != id)).ToList()).ToList();
        List<ResourceContainer> newContainers = [];
        foreach (var container in oabPart.Containers)
        {
            var curContainer = container as ResourceContainer;
            List<ContainedResourceDefinition> newDefinitions = [];
            for (var internalIndex = 0; internalIndex < curContainer!._resourceIDMap.Count; internalIndex++)
            {
                var resourceDef = curContainer._resourceIDMap[internalIndex];
                if (!Containers.Any(x => GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName(x).Equals(resourceDef)))
                {
                    newDefinitions.Add(new ContainedResourceDefinition(new ContainedResourceData
                    {
                        IsPartOfRecipe = false,
                        ResourceID = resourceDef,
                        CapacityUnits = curContainer._capacityUnitsLookup[internalIndex],
                        StoredUnits = curContainer._storedUnitsLookup[internalIndex]
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
        moduleResourceCapacities.OnShutdown();
        moduleResourceCapacities.dataResourceCapacities.RebuildDataContext();
        moduleResourceCapacities.OnInitialize();
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
    }

    public (Type savedType, JToken savedValue) SaveInformation() =>
        (typeof(ResourceContainerRemoveLoader), JToken.FromObject(Containers));
}