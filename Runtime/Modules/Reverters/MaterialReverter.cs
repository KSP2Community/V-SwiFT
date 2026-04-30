using System.Collections.Generic;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Transformers;

namespace VSwift.Modules.Reverters
{
    public class MaterialReverter : IReverter
    {
        private readonly MaterialSwapper _swapper;

        public MaterialReverter(MaterialSwapper swapper)
        {
            _swapper = swapper;
        }

        public object? Store(Module_PartSwitch partSwitch)
        {
            var snapshot = new Dictionary<Material, Material>();
            CaptureMatching(partSwitch.gameObject, _swapper.Swaps.Keys, snapshot);
            return snapshot;
        }

        public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
        {
            if (data is not Dictionary<Material, Material> snapshot) return;
            foreach (var (live, original) in snapshot)
            {
                if (live == null) continue;
                live.CopyPropertiesFromMaterial(original);
            }
            partSwitch.QueueUpdateColors();
        }

        public bool RequiresInVariantSet => false;

        private static void CaptureMatching(GameObject root, ICollection<string> targetNames, Dictionary<Material, Material> snapshot)
        {
            foreach (var renderer in root.GetComponents<Renderer>())
            {
                foreach (var material in renderer.materials)
                {
                    if (material == null) continue;
                    var name = material.name.Replace(" (Clone)", "").Replace(" (Instance)", "");
                    if (targetNames.Contains(name))
                    {
                        snapshot[material] = new Material(material);
                    }
                }
            }
            foreach (Transform child in root.transform)
            {
                CaptureMatching(child.gameObject, targetNames, snapshot);
            }
        }
    }
}
