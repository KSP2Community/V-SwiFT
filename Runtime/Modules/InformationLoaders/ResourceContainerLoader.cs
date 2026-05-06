using System.Collections.Generic;
using KSP.IO;
using KSP.Sim.Definitions;
using KSP.Sim.ResourceSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VSwift.Modules.InformationLoaders
{
    /// <summary>
    /// Loads <see cref="Transformers.ResourceContainerAdder" /> output by appending the saved resource-container definitions to the part data.
    /// </summary>
    public class ResourceContainerLoader : IInformationLoader
    {
        /// <inheritdoc />
        public void LoadInformationInto(PartData partData, JToken storedInformation)
        {
            partData.resourceContainers.AddRange(IOProvider.FromJson<List<ContainedResourceDefinition>>(storedInformation.ToString(Formatting.None)));
        }
    }
}
