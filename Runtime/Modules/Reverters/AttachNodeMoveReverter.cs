using System.Collections.Generic;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Transformers;

namespace VSwift.Modules.Reverters
{
    public class AttachNodeMoveReverter : IReverter
    {
        private readonly AttachNodeMover _mover;

        public AttachNodeMoveReverter(AttachNodeMover mover)
        {
            _mover = mover;
        }

        public object? Store(Module_PartSwitch partSwitch)
        {
            var positions = new Dictionary<string, Vector3>();
            foreach (var nodeId in _mover.MovedNodes.Keys)
            {
                if (partSwitch.OABPart.FindNodeWithTag(nodeId) is { } node)
                {
                    positions[nodeId] = node.NodeTransform.localPosition;
                }
            }
            return positions;
        }

        public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
        {
            if (isStartingReset) return;
            if (data is not Dictionary<string, Vector3> positions) return;
            foreach (var (nodeId, pos) in positions)
            {
                if (partSwitch.OABPart.FindNodeWithTag(nodeId) is { } node)
                {
                    partSwitch.OABPart.SetNodeLocalPosition(node, pos);
                }
            }
        }

        public bool RequiresInVariantSet => true;
    }
}
