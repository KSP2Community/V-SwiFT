using System.Reflection;
using KSP.IO;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Extensions;
using VSwift.Modules.Logging;

namespace VSwift.Modules.InformationLoaders;

public class ModuleDefinitionLoader : IInformationLoader
{
    public void LoadInformationInto(PartData partData, JToken storedInformation)
    {
        var (behaviourType, dataType, key, value) =
            storedInformation.FromJToken<(Type behaviourType, Type dataType, string key, JToken value)>();
        try
        {
            var module = partData.serializedPartModules.FirstOrDefault(x => x.BehaviourType == behaviourType);
            var dataObject = module.ModuleData.FirstOrDefault(x => x.DataType == dataType).DataObject;
            var field = dataType.GetField(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return;
            field.SetValue(dataObject,IOProvider.FromJson(value.ToString(Formatting.None),field.FieldType));
        }
        catch (Exception e)
        {
            IVSwiftLogger.Instance.LogError(e);
        }
    }
}