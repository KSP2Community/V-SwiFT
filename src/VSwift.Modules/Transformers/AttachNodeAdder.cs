using JetBrains.Annotations;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(AttachNodeAdder))]
public class AttachNodeAdder : ITransformer
{
    [UsedImplicitly] public List<AttachNodeDefinition> Nodes = [];

    [JsonIgnore] private IReverter? _reverter;
    public IReverter? Reverter => _reverter ??= new DynamicAttachNodeReverter(Nodes.Select(x => x.nodeID).ToList());
    public bool SavesInformation => false;
    public bool VisualizesInformation => false;

    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
        foreach (var definition in Nodes)
        {
            if (partSwitch.OABPart.FindNodeWithTag(definition.nodeID) is {} node)
            {
                partSwitch.OABPart.SetNodeLocalPosition(node, definition.position);
                partSwitch.OABPart.SetNodeLocalScale(node, definition.size);
            }
            else
            {
                partSwitch.OABPart.AddDynamicNode(partSwitch.OABPart,
                    new ObjectAssemblyAvailablePartNode(definition.size,
                        definition.position,
                        Quaternion.LookRotation(definition.orientation,Vector3.up),
                        definition.nodeID,
                        null,
                        definition.size,
                        AttachNodeType.Stack,
                        true));
            }
        }
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
    }
}