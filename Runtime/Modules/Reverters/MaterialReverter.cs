using System;
using System.Collections.Generic;
using UnityEngine;
using VSwift.Modules.Behaviours;

namespace VSwift.Modules.Reverters
{
    /// <summary>
    /// Reverts a <see cref="Transformers.MaterialSwapper" /> by restoring each affected material's original property block.
    /// </summary>
    public class MaterialReverter : IReverter
    {
        private readonly Func<GameObject, IEnumerable<Material>> _selector;

        /// <summary>
        /// Creates the reverter using the given material selector.
        /// </summary>
        /// <param name="selector">Returns the materials affected by the paired transformer for a given root <see cref="UnityEngine.GameObject" />.</param>
        public MaterialReverter(Func<GameObject, IEnumerable<Material>> selector)
        {
            _selector = selector;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool RequiresInVariantSet => false;
    }
}
