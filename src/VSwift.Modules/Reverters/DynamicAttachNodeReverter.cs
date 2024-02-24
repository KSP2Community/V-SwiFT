using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Reverters;

public class DynamicAttachNodeReverter(List<string> dynamicNodeNames) : IReverter
{
    public object? Store(Module_PartSwitch partSwitch)
    {
        return null;
    }

    public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
    {
        if (isStartingReset) return;
        foreach (var nodeName in dynamicNodeNames)
        {
            if (partSwitch.OABPart.FindNodeWithTag(nodeName) is {} node)
            {
                partSwitch.OABPart.RemoveDynamicNode(node);
            }
        }
    }

    public bool RequiresInVariantSet => true;
}