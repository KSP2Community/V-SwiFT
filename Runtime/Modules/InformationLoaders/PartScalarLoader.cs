using KSP.IO;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Transformers;

namespace VSwift.Modules.InformationLoaders
{
    /// <summary>
    /// Loads <see cref="Transformers.PartScalarTransformer" /> output by setting a scalar field on the part data with the saved value.
    /// </summary>
    public class PartScalarLoader : IInformationLoader
    {
        /// <inheritdoc />
        public void LoadInformationInto(PartData partData, JToken storedInformation)
        {
            var transformer = IOProvider.FromJson<PartScalarTransformer>(storedInformation.ToString(Formatting.None));
            var field = partData.GetType().GetField(transformer.Key);
            var value = IOProvider.FromJson(transformer.Value.ToString(Formatting.None), field.FieldType);
            field.SetValue(partData, value);
        }
    }
}
