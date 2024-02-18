using VSwift.Modules.Behaviours;

namespace VSwift.Modules.Transformers;

public interface ITransformer
{
    public object StoreOriginalState(Module_PartSwitch partSwitchModule);

    public void ResetToOriginalState(Module_PartSwitch partSwitchModule, object originalState);

    public void ApplyInFlight(Module_PartSwitch partSwitch);
    public void ApplyInOab(Module_PartSwitch partSwitch);
    public void ApplyCommon(Module_PartSwitch partSwitch);
}