using System;
using System.Collections.Generic;
using UnityEngine;
using VSwift.Modules.Behaviours;

namespace VSwift.Modules.Reverters
{
    public class MaterialReverter : IReverter
    {
        private readonly Func<GameObject, IEnumerable<Material>> _selector;

        public MaterialReverter(Func<GameObject, IEnumerable<Material>> selector)
        {
            _selector = selector;
        }

        public object? Store(Module_PartSwitch partSwitch)
        {
            var snapshot = new Dictionary<Material, Material>();
            foreach (var material in _selector(partSwitch.gameObject))
            {
                if (material == null) continue;
                if (snapshot.ContainsKey(material)) continue;
                snapshot[material] = new Material(material);
            }
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
    }
}
