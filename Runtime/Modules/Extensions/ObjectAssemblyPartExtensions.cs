using HoudiniEngineUnity;
using KSP.OAB;
using UnityEngine;

namespace VSwift.Modules.Extensions;

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
        var vector2 = partTransform.rotation * partTransform.TransformVector(vector);
        if (node.ConnectionIsParent)
        {
            objectAssemblyPart.WorldPosition += vector2.SwapYAndZ() * num;
        }
        else
        {
            node.ConnectedPart.WorldPosition -= vector2.SwapYAndZ() * num;
        }
    }
}