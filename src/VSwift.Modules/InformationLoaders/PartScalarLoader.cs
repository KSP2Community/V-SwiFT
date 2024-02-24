using KSP.IO;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Transformers;

namespace VSwift.Modules.InformationLoaders;

public class PartScalarLoader : IInformationLoader
{
    public void LoadInformationInto(PartData partData, JToken storedInformation)
    {
        var transformer = IOProvider.FromJson<PartScalarTransformer>(storedInformation.ToString(Formatting.None));
        var field = partData.GetType().GetField(transformer.Key);
        var value = IOProvider.FromJson(transformer.Value.ToString(Formatting.None), field.FieldType);
        field.SetValue(partData, value);
    }
}