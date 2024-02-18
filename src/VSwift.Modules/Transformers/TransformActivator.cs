using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(TransformActivator))]
public class TransformActivator : ITransformer
{
    public List<string> Transforms = [];
    public object StoreOriginalState(Module_PartSwitch partSwitchModule)
    {
        var dict = new Dictionary<GameObject, bool>();
        RecursivelyStoreState(partSwitchModule.gameObject, dict);
        return dict;
    }

    private static void RecursivelyStoreState(GameObject gameObject, Dictionary<GameObject, bool> state)
    {
        foreach (Transform child in gameObject.transform)
        {
            var o = child.gameObject;
            state[o] = o.activeSelf;
            RecursivelyStoreState(o, state);
        }
    }

    public void ResetToOriginalState(Module_PartSwitch partSwitchModule, object originalState)
    {
        var dict = originalState as Dictionary<GameObject, bool>;
        foreach (var (obj, state) in dict!)
        {
            obj.SetActive(state);
        }
    }

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