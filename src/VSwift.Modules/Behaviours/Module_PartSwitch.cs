using KSP.Sim.Definitions;
using VSwift.Modules.Components;
using VSwift.Modules.Data;

namespace VSwift.Modules.Behaviours;

// ReSharper disable once InconsistentNaming
public class Module_PartSwitch : PartBehaviourModule
{
    public override Type PartComponentModuleType => typeof(PartComponentModule_PartSwitch);
    private Data_PartSwitch? _dataPartSwitch;
    
    protected override void AddDataModules()
    {
        base.AddDataModules();
        _dataPartSwitch ??= new Data_PartSwitch();
        DataModules.TryAddUnique(_dataPartSwitch, out _dataPartSwitch);
    }
}