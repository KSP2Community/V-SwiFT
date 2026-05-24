using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Reverters;
using VSwift.Utilities;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Repositions existing attach nodes on the part to the configured local positions when active.
    /// </summary>
    [Serializable]
    [Transformer(nameof(AttachNodeMover))]
    [TransformerCategory("Attach nodes")]
    [TransformerDescription("Move existing nodes")]
    public class AttachNodeMover : ITransformer
    {
        /// <summary>
        /// Map of node ID to the new local position to move it to.
        /// </summary>
        [Tooltip("Map from existing node ID to the new local position to move it to.")]
        [UsedImplicitly]
        public SerializedDictionary<string, Vector3d> MovedNodes = new() { };

        [JsonIgnore] private IReverter? _reverter;

        /// <inheritdoc />
        [JsonIgnore] public IReverter? Reverter => _reverter ??= new AttachNodeMoveReverter(this);

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
            foreach (var (node, pos) in MovedNodes)
            {
                if (partSwitch.OABPart.FindNodeWithTag(node) is { } actualNode)
                {
                    partSwitch.OABPart.SetNodeLocalPosition(actualNode, pos);
                }
            }
        }

        /// <inheritdoc />
        public void ApplyCommon(Module_PartSwitch partSwitch)
        {
        }
    }
}
