using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(TransformActivator))]
public class TransformActivator : ITransformer
{
    public List<string> Transforms = [];
    
    public IReverter? Reverter => TransformReverter.Instance;
    public bool SavesInformation => false;
    public bool VisualizesInformation => false;

    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
        // Do nothing
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
        // Do nothing
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
        foreach (var activatedTransform in Transforms)
        {
            var t = partSwitch.gameObject.transform.FindChildRecursive(activatedTransform);
            if (ReferenceEquals(t,null) || t == null)
            {
                IVSwiftLogger.Instance.LogError($"Could not find child of {partSwitch.gameObject.name} with name {activatedTransform}");
                continue;
            }
            t.gameObject.SetActive(true);
        }
    }
}