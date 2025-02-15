using JetBrains.Annotations;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(AttachNodeMover))]
public class AttachNodeMover : ITransformer
{
    [UsedImplicitly]
    public Dictionary<string, Vector3d> MovedNodes = [];
    public IReverter? Reverter => PredefinedNodeReverter.Instance;
    public bool SavesInformation => false;
    public bool VisualizesInformation => false;
    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
        foreach (var (node, pos) in MovedNodes)
        {
            if (partSwitch.OABPart.FindNodeWithTag(node) is { } actualNode)
            {
                partSwitch.OABPart.FixedSetNodeLocalPosition(actualNode, pos);
            } 
        }
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
    }
}