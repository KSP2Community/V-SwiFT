using KSP.IO;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VSwift.Modules.InformationLoaders;

public class ResourceContainerRemoveLoader : IInformationLoader
{
    public void LoadInformationInto(PartData partData, JToken storedInformation)
    {
        var data = IOProvider.FromJson<List<string>>(storedInformation.ToString(Formatting.None));
        partData.resourceContainers = partData.resourceContainers.Where(x => !data.Contains(x.name)).ToList();
    }
}