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
        IVSwiftLogger.Instance.LogInfo($"Attempting to remove nodes: {isStartingReset}");
        if (isStartingReset) return;
        foreach (var nodeName in dynamicNodeNames)
        {
            IVSwiftLogger.Instance.LogInfo($"Removing {nodeName}");
            if (partSwitch.OABPart.FindNodeWithTag(nodeName) is {} node)
            {
                IVSwiftLogger.Instance.LogInfo($"Which is {node}");
                partSwitch.OABPart.RemoveDynamicNode(node);
            }
        }
    }

    public bool RequiresInVariantSet => true;
}