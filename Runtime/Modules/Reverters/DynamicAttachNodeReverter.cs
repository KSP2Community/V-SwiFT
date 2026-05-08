using System.Collections.Generic;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Reverters
{
    /// <summary>
    /// Reverts an <see cref="Transformers.AttachNodeAdder" /> by removing each dynamic attach node it added.
    /// </summary>
    public class DynamicAttachNodeReverter : IReverter
    {
        /// <summary>
        /// The dynamic-node tags this reverter removes on revert.
        /// </summary>
        public List<string> DynamicNodeNames;

        /// <summary>
        /// Creates the reverter for the given dynamic-node tags.
        /// </summary>
        /// <param name="dynamicNodeNames">The dynamic-node tags to remove on revert.</param>
        public DynamicAttachNodeReverter(List<string> dynamicNodeNames)
        {
            DynamicNodeNames = dynamicNodeNames;
        }

        /// <inheritdoc />
        public object? Store(Module_PartSwitch partSwitch)
        {
            return null;
        }

        /// <inheritdoc />
        public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
        {
            if (isStartingReset) return;
            foreach (var nodeName in DynamicNodeNames)
            {
                if (partSwitch.OABPart.FindNodeWithTag(nodeName) is {} node)
                {
                    partSwitch.OABPart.RemoveDynamicNode(node);
                }
            }
        }

        /// <inheritdoc />
        public bool RequiresInVariantSet => true;

        /// <inheritdoc />
        public bool AppliesInFlight => false;
    }
}
