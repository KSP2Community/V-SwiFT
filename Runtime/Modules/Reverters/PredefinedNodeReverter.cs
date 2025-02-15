using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;

namespace VSwift.Modules.Reverters;

public class PredefinedNodeReverter : IReverter
{
    private PredefinedNodeReverter()
    {
    }

    private static PredefinedNodeReverter? _instance;
    public static PredefinedNodeReverter Instance => _instance ??= new PredefinedNodeReverter();

    public object? Store(Module_PartSwitch partSwitch)
    {
        return null;
    }

    public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
    {
        if (isStartingReset) return;
        foreach (var node in partSwitch.DataPartSwitch!.PredefinedDynamicNodes)
        {
            if (partSwitch.OABPart.FindNodeWithTag(node.nodeID) is { } actualNode)
            {
                partSwitch.OABPart.FixedSetNodeLocalPosition(actualNode, node.position);
            }
        }
    }

    public bool RequiresInVariantSet => true;
}