using System.Reflection;
using KSP.Game;
using KSP.IO;
using KSP.OAB;
using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;

namespace VSwift.Modules.Reverters;

public class ModuleDefinitionReverter : IReverter
{
    private Type _moduleType;
    private Type _dataType;
    private FieldInfo _info;
    private ModuleDefinitionReverter(Type moduleType, Type dataType, string key)
    {
        _moduleType = moduleType;
        _dataType = dataType;
        _info = dataType.GetField(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                throw new Exception($"Invalid field of {dataType}: {key}");
    }

    private static readonly Dictionary<(Type, Type, string), ModuleDefinitionReverter> Instances = [];
    public static ModuleDefinitionReverter GetInstanceFor(Type moduleType, Type dataType, string key) =>
        Instances.TryGetValue((moduleType, dataType, key),
            out var result)
            ? result
            : Instances[(moduleType,dataType,key)] = new ModuleDefinitionReverter(moduleType, dataType, key);

    public object? Store(Module_PartSwitch partSwitch) =>
        partSwitch.OABPart.TryGetModule(_moduleType,
            out var toBeStored)
            ? toBeStored.DataModules.TryGetValue(_dataType, out var data)
                ? IOProvider.ToJson(_info.GetValue(data))
                : null
            : null;

    public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
    {
        if (isStartingReset) return;
        if (data == null) return;
        var trueData = (string)data;
        if (!partSwitch.OABPart.TryGetModule(_moduleType, out var toBeLoaded)) return;
        if (!toBeLoaded.DataModules.TryGetValue(_dataType, out var toBeModified)) return;
        toBeLoaded.OnShutdown();
        _info.SetValue(toBeModified, IOProvider.FromJson(trueData, _info.FieldType));
        toBeModified.RebuildDataContext();
        toBeLoaded.OnInitialize();
    }

    public bool RequiresInVariantSet => true;
}