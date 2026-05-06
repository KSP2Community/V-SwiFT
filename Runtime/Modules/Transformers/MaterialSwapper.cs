using System;
using System.Collections.Generic;
using KSP.Game;
using KSP.Modules;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Swaps materials on the part by mapping each source material name to a replacement loaded from addressables.
    /// </summary>
    [Transformer(nameof(MaterialSwapper))]
    public class MaterialSwapper : ITransformer
    {
        /// <summary>
        /// Map of source material name to the addressables address of the replacement material.
        /// </summary>
        public Dictionary<string, string> Swaps = new() { };
        [JsonIgnore] private Dictionary<string,Material> _material = new() { };

        [JsonIgnore] private IReverter? _reverter;

        /// <inheritdoc />
        [JsonIgnore] public IReverter? Reverter => _reverter ??= new MaterialReverter(CollectAffectedMaterials);

        /// <inheritdoc />
        public bool SavesInformation => false;

        /// <inheritdoc />
        public bool VisualizesInformation => true;

        /// <inheritdoc />
        public void ApplyInFlight(Module_PartSwitch partSwitch)
        {
        }

        /// <inheritdoc />
        public void ApplyInOab(Module_PartSwitch partSwitch)
        {
        }

        /// <inheritdoc />
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
            partSwitch.QueueUpdateColors();
        }

        private void LoadMaterial(string name, Action<Material> callback)
        {
            // TODO: Look at solely supporting addressables
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

        private IEnumerable<Material> CollectAffectedMaterials(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var material in renderer.materials)
                {
                    if (material == null) continue;
                    var name = material.name.Replace(" (Clone)", "").Replace(" (Instance)", "");
                    if (Swaps.ContainsKey(name)) yield return material;
                }
            }
        }
    }
}
