using System.Collections.Generic;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Transformers;

namespace VSwift.Modules.Reverters
{
    /// <summary>
    /// Reverts an <see cref="AttachNodeMover" /> by restoring each moved node's original local position.
    /// </summary>
    public class AttachNodeMoveReverter : IReverter
    {
        private readonly AttachNodeMover _mover;

        /// <summary>
        /// Creates the reverter for the given <see cref="AttachNodeMover" />.
        /// </summary>
        /// <param name="mover">The mover whose nodes are tracked for reversion.</param>
        public AttachNodeMoveReverter(AttachNodeMover mover)
        {
            _mover = mover;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool RequiresInVariantSet => true;
    }
}
