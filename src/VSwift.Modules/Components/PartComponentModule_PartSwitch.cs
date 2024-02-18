using KSP.Sim.impl;
using VSwift.Modules.Behaviours;

namespace VSwift.Modules.Components;

// ReSharper disable once InconsistentNaming
public class PartComponentModule_PartSwitch : PartComponentModule
{
    public override Type PartBehaviourModuleType => typeof(Module_PartSwitch);
}