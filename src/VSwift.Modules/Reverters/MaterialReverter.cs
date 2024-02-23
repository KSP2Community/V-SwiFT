using UnityEngine;
using VSwift.Modules.Behaviours;

namespace VSwift.Modules.Reverters;

public class MaterialReverter : IReverter
{
    
    private static MaterialReverter? _instance;
    public static MaterialReverter? Instance => _instance ??= new MaterialReverter();
    
    private static void RecursivelyStoreState(GameObject gameObject, Dictionary<Renderer, List<Material>> state)
    {
        var renderers = gameObject.GetComponents<Renderer>();
        foreach (var renderer in renderers)
        {
            state[renderer] = renderer.materials.Select(mat => new Material(mat)).ToList();
        }
        foreach (Transform child in gameObject.transform)
        {
            var o = child.gameObject;
            RecursivelyStoreState(o, state);
        }
    }

    public object Store(Module_PartSwitch partSwitch)
    {
        Dictionary<Renderer, List<Material>> dict = new();
        RecursivelyStoreState(partSwitch.gameObject, dict);
        return dict;
    }

    public void Revert(Module_PartSwitch partSwitch, object data)
    {
        var dict = data as Dictionary<Renderer, List<Material>>;
        foreach (var (renderer, mats) in dict!)
        {
            for (var i = 0; i < renderer.materials.Length; i++)
            {
                renderer.materials[i].CopyPropertiesFromMaterial(mats[i]);
            }
        }
    }
}