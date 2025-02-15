using System.Collections.Generic;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Reverters
{
    public class DynamicAttachNodeReverter : IReverter
    {
        public List<string> DynamicNodeNames;
        public DynamicAttachNodeReverter(List<string> dynamicNodeNames)
        {
            DynamicNodeNames = dynamicNodeNames;
        }
        
        public object? Store(Module_PartSwitch partSwitch)
        {
            return null;
        }

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

        public bool RequiresInVariantSet => true;
    }
}