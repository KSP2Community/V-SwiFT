using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;

namespace VSwift.Modules.Reverters;

public class ModuleDefinitionReverter : IReverter
{
    private Type _type;
    private ModuleDefinitionReverter(Type t)
    {
        _type = t;
    }

    private static readonly Dictionary<Type, ModuleDefinitionReverter> Instances = [];
    public static ModuleDefinitionReverter GetInstanceFor(Type t) =>
        Instances.TryGetValue(t,
            out var result)
            ? result
            : Instances[t] = new ModuleDefinitionReverter(t);

    public object? Store(Module_PartSwitch partSwitch) =>
        partSwitch.OABPart.TryGetModule(_type,
            out var toBeStored)
            ? (toBeStored.DataModules.KeysList.ToList(),toBeStored.DataModules.ValuesList.Select(data => data.JsonClone()).ToList())
            : null;

    public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
    {
        if (isStartingReset) return;
        if (data == null) return;
        var trueData = ((List<Type>,List<ModuleData>))data;
        if (!partSwitch.OABPart.TryGetModule(_type, out var toBeLoaded)) return;
        toBeLoaded.OnShutdown();
        foreach (var context in toBeLoaded.DataModules.list) context.ModuleDataContext.ClearAllData();
        toBeLoaded.DataModules.listKeys = trueData.Item1;
        toBeLoaded.DataModules.list = trueData.Item2.JsonClone();
        foreach (var context in toBeLoaded.DataModules.list) context.PrepareDataContext();
        toBeLoaded.AddDataModules();
        toBeLoaded.OnInitialize();
    }

    public bool RequiresInVariantSet => true;
}