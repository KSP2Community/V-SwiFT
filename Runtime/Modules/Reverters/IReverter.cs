using VSwift.Modules.Behaviours;

namespace VSwift.Modules.Reverters;

public interface IReverter
{
    public object? Store(Module_PartSwitch partSwitch);
    public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset);

    public bool RequiresInVariantSet { get; }
}