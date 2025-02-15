using KSP.OAB;
using UnityEngine;

namespace VSwift.Modules.Extensions
{
    public static class ObjectAssemblyPartExtensions
    {
        public static void FixedSetNodeLocalPosition(this IObjectAssemblyPart objectAssemblyPart, IObjectAssemblyPartNode node, Vector3 newLocalPosition)
        {
            var nodeTransform = node.NodeTransform;
            var vector = ((Vector3.Dot(newLocalPosition, Vector3.one) > 0f) ? (newLocalPosition - nodeTransform.localPosition) : (nodeTransform.localPosition - newLocalPosition));
            nodeTransform.localPosition = newLocalPosition;
            if (node.ConnectedPart == null) return;
        
            var num = Mathf.Sign(Vector3.Dot(node.ConnectedPart.WorldPosition - objectAssemblyPart.WorldPosition, objectAssemblyPart.WorldPosition));
            var partTransform = node.ConnectedPart.PartTransform;
            var transformVector = partTransform.rotation * partTransform.TransformVector(vector);
            if (node.ConnectionIsParent)
            {
                objectAssemblyPart.WorldPosition += transformVector.SwapYAndZ() * num;
            }
            else
            {
                node.ConnectedPart.WorldPosition -= transformVector.SwapYAndZ() * num;
            }
        }

        private static Vector3 SwapYAndZ(this Vector3 toSwap)
        {
            return new Vector3(toSwap.x, toSwap.z, toSwap.y);
        }
    }
}