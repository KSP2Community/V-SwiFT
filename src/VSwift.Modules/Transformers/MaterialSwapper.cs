using KSP.Game;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(MaterialSwapper))]
public class MaterialSwapper : ITransformer
{
    public Dictionary<string, string> Swaps = [];
    private Dictionary<string,Material> _material = [];


    public IReverter Reverter => MaterialReverter.Instance;
    public bool SavesInformation => false;

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
                LoadMaterial(to, m =>
                {
                    _material[to] = m;
                    RecursivelySwitch(partSwitch.gameObject, from, m);
                });
            }
            else
            {
                RecursivelySwitch(partSwitch.gameObject, from, mat);
            }
        }
    }

    private void LoadMaterial(string name, Action<Material> callback)
    {
        if (name.StartsWith("addressables://"))
        {
            var addressableKey = name.Replace("addressables://", "");
            GameManager.Instance.Assets.Load(addressableKey, callback);
        }
        else
        {
            throw new Exception($"Unknown material {name}");
        }
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