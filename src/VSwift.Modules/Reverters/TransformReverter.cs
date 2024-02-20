using UnityEngine;
using VSwift.Modules.Behaviours;

namespace VSwift.Modules.Reverters;

public class TransformReverter : IReverter
{
    private static TransformReverter? _instance;
    public static TransformReverter Instance => _instance ??= new TransformReverter();
    
    public object Store(Module_PartSwitch partSwitch)
    {
        Dictionary<GameObject, bool> dict = new();
        RecursivelyStoreState(partSwitch.gameObject, dict);
        return dict;
    }

    public void Revert(Module_PartSwitch partSwitch, object data)
    {
        var dict = data as Dictionary<GameObject, bool>;
        foreach (var (obj, state) in dict!)
        {
            obj.SetActive(state);
        }
    }
    
    private static void RecursivelyStoreState(GameObject gameObject, Dictionary<GameObject,bool> state)
    {
        foreach (Transform child in gameObject.transform)
        {
            var o = child.gameObject;
            state[o] = o.activeSelf;
            RecursivelyStoreState(o, state);
        }
    }
}