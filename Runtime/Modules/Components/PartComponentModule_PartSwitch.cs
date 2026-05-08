using System;
using KSP.Sim.impl;
using VSwift.Modules.Behaviours;

namespace VSwift.Modules.Components
{
    /// <summary>
    /// Flight-side component module paired with <see cref="Behaviours.Module_PartSwitch" />.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class PartComponentModule_PartSwitch : PartComponentModule
    {
        /// <inheritdoc />
        public override Type PartBehaviourModuleType => typeof(Module_PartSwitch);
    }
}
