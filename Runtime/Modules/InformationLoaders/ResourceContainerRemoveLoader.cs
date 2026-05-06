using System.Collections.Generic;
using System.Linq;
using KSP.IO;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VSwift.Modules.InformationLoaders
{
    /// <summary>
    /// Loads <see cref="Transformers.ResourceContainerRemover" /> output by filtering out the saved resource-container names from the part data.
    /// </summary>
    public class ResourceContainerRemoveLoader : IInformationLoader
    {
        /// <inheritdoc />
        public void LoadInformationInto(PartData partData, JToken storedInformation)
        {
            var data = IOProvider.FromJson<List<string>>(storedInformation.ToString(Formatting.None));
            partData.resourceContainers = partData.resourceContainers.Where(x => !data.Contains(x.name)).ToList();
        }
    }
}
