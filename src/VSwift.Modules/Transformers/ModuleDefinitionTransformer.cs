using System.Reflection;
using JetBrains.Annotations;
using KSP.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Converters;
using VSwift.Modules.Extensions;
using VSwift.Modules.InformationLoaders;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(ModuleDefinitionTransformer))]
public class ModuleDefinitionTransformer : ITransformer
{
    [UsedImplicitly]
    [JsonConverter(typeof(BehaviourTypeConverter))]
    public Type BehaviourType = null!;

    [UsedImplicitly]
    [JsonConverter(typeof(DataTypeConverter))]
    public Type DataType = null!;

    [UsedImplicitly]
    public string Key = "";
    
    [UsedImplicitly]
    public JToken Value = "";

    public IReverter? Reverter => ModuleDefinitionReverter.GetInstanceFor(BehaviourType);
    public bool SavesInformation => true;
    public bool VisualizesInformation => false;
    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
        if (!partSwitch.OABPart.TryGetModule(BehaviourType, out var toBeLoaded)) return;
        toBeLoaded.OnShutdown();
        if (toBeLoaded.DataModules.TryGetValue(DataType, out var moduleData)) return;
        var field = moduleData.GetType()
            .GetField(Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null) return;
        field.SetValue(moduleData, IOProvider.FromJson(Value.ToString(Formatting.None),field.FieldType));
        moduleData.PrepareDataContext();
        toBeLoaded.OnInitialize();
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
    }

    public (Type savedType, JToken savedValue) SaveInformation()
    {
        return (typeof(ModuleDefinitionLoader), (BehaviourType, DataType, Key, Value).ToJToken());
    }
}