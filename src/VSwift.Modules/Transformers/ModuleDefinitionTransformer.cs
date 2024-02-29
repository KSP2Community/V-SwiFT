using System.Reflection;
using JetBrains.Annotations;
using KSP.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;
using VSwift.Modules.InformationLoaders;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(ModuleDefinitionTransformer))]
public class ModuleDefinitionTransformer : ITransformer
{
    [UsedImplicitly]
    public string BehaviourType = "";

    [JsonIgnore] private Type? _behaviourType = null;

    [JsonIgnore]
    [PublicAPI]
    public Type ActualBehaviourType
    {
        get
        {
            _behaviourType ??= ModulesUtilities.ComponentModules.TryGetValue(BehaviourType, out var tuple)
                ? tuple.behaviour
                : throw new Exception($"Unknown behaviour type: {BehaviourType}");
            return _behaviourType;
        }
    }

    [UsedImplicitly]
    public string DataType = null!;
    [JsonIgnore] private Type? _dataType = null;

    [JsonIgnore]
    [PublicAPI]
    public Type ActualDataType
    {
        get
        {
            _dataType ??= ModulesUtilities.DataModules.TryGetValue(DataType, out var dataType)
                ? dataType
                : throw new Exception($"Unknown data type: {DataType}");
            return _dataType;
        }
    }    

    [UsedImplicitly]
    public string Key = "";
    
    [UsedImplicitly]
    public JToken Value = "";

    public IReverter? Reverter => ModuleDefinitionReverter.GetInstanceFor(ActualBehaviourType, ActualDataType, Key);
    public bool SavesInformation => true;
    public bool VisualizesInformation => false;
    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
        if (!partSwitch.OABPart.TryGetModule(ActualBehaviourType, out var toBeLoaded)) return;
        toBeLoaded.OnShutdown();
        if (toBeLoaded.DataModules.TryGetValue(ActualDataType, out var moduleData)) return;
        var field = moduleData.GetType()
            .GetField(Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null) return;
        field.SetValue(moduleData, IOProvider.FromJson(Value.ToString(Formatting.None),field.FieldType));
        moduleData.RebuildDataContext();
        toBeLoaded.OnInitialize();
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
    }

    public (Type savedType, JToken savedValue) SaveInformation()
    {
        return (typeof(ModuleDefinitionLoader), (ActualBehaviourType, ActualDataType, Key, Value).ToJToken());
    }
}