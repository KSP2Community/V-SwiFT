using UnityEngine;
using UnityEngine.AddressableAssets;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(MaterialSwapper))]
public class MaterialSwapper : ITransformer
{
    public Dictionary<string, string> Swaps = [];
    private Dictionary<string,Material> _material = [];
    
    public object StoreOriginalState(Module_PartSwitch partSwitchModule)
    {
        Dictionary<Renderer, List<Material>> dict = new();
        RecursivelyStoreState(partSwitchModule.gameObject, dict);
        return dict;
    }
    
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

    public void ResetToOriginalState(Module_PartSwitch partSwitchModule, object originalState)
    {
        var dict = originalState as Dictionary<Renderer, List<Material>>;
        foreach (var (renderer, mats) in dict!)
        {
            for (var i = 0; i < renderer.materials.Length; i++)
            {
                renderer.materials[i].CopyPropertiesFromMaterial(mats[i]);
            }
        }
    }

    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
        // _material ??= LoadMaterial();
        // if (_material == null) return;
        // RecursivelySwitch(partSwitch.gameObject, _material);
        foreach (var (from, to) in Swaps)
        {
            if (!_material.TryGetValue(to, out var mat))
            {
                mat = LoadMaterial(to);
                _material[to] = mat;
            }

            RecursivelySwitch(partSwitch.gameObject, from, mat);
        }
    }

    private Material LoadMaterial(string name)
    {
        if (name.StartsWith("addressables://"))
        {
            var addressableKey = name.Replace("addressables://", "");
            var handle = Addressables.LoadAssetAsync<Material>(addressableKey);
            var result =  handle.WaitForCompletion();
            var clone = new Material(result);
            // Addressables.Release(result);
            return clone;
        }
        throw new Exception($"Unknown material {name}");
    }
    
    private void RecursivelySwitch(GameObject gameObject, string name, Material targetMat)
    {
        var renderers = gameObject.GetComponents<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                // IVSwiftLogger.Instance.LogInfo($"Attempting to see if I should switch {material.name} to {targetMat.name}, the name I am looking for is {name}");
                if (material.name.Replace(" (Clone)", "").Replace(" (Instance)", "") != name) continue;
                // IVSwiftLogger.Instance.LogInfo("Switched!");
                material.CopyPropertiesFromMaterial(targetMat);
            }
        }
        foreach (Transform child in gameObject.transform)
        {
            var o = child.gameObject;
            RecursivelySwitch(o, name, targetMat);
        }
    }
}