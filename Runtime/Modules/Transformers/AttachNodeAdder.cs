using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Adds attach nodes to the part (or repositions existing nodes whose tag matches a configured node) when active.
    /// </summary>
    [Transformer(nameof(AttachNodeAdder))]
    public class AttachNodeAdder : ITransformer
    {
        /// <summary>
        /// The attach-node definitions to add or reposition.
        /// </summary>
        [UsedImplicitly] public List<AttachNodeDefinition> Nodes = new() { };

        [JsonIgnore] private IReverter? _reverter;

        /// <inheritdoc />
        [JsonIgnore] public IReverter? Reverter => _reverter ??= new DynamicAttachNodeReverter(Nodes.Select(x => x.nodeID).ToList());

        /// <inheritdoc />
        public bool SavesInformation => false;

        /// <inheritdoc />
        public bool VisualizesInformation => false;

        /// <inheritdoc />
        public void ApplyInFlight(Module_PartSwitch partSwitch)
        {
        }

        /// <inheritdoc />
        public void ApplyInOab(Module_PartSwitch partSwitch)
        {
            foreach (var definition in Nodes)
            {
                if (partSwitch.OABPart.FindNodeWithTag(definition.nodeID) is {} node)
                {
                    partSwitch.OABPart.SetNodeLocalPosition(node, definition.position);
                    partSwitch.OABPart.SetNodeLocalScale(node, definition.size);
                    if (node is ObjectAssemblyPartNode oabNode)
                    {
                        oabNode.SizeKey = PartSizeRegistry.GetAttachNodeSizeKey(definition);
                        oabNode.Diameter = PartSizeRegistry.GetAttachNodeDiameter(definition);
                    }
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
                            true,
                            definition.sizeKey));
                }
            }
        }

        /// <inheritdoc />
        public void ApplyCommon(Module_PartSwitch partSwitch)
        {
        }
    }
}
