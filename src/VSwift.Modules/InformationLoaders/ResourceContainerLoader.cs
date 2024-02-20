using KSP.IO;
using KSP.Sim.Definitions;
using KSP.Sim.ResourceSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VSwift.Modules.InformationLoaders;

public class ResourceContainerLoader : IInformationLoader
{
    public void LoadInformationInto(PartData partData, JToken storedInformation)
    {
        partData.resourceContainers =
        [
            ..partData.resourceContainers, 
            ..IOProvider.FromJson<List<ContainedResourceDefinition>>(storedInformation.ToString(Formatting.None))
        ];
    }
}