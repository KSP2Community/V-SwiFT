using System.Collections.Generic;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Reverters
{
    /// <summary>
    /// Reverts a <see cref="Transformers.TransformActivator" /> by deactivating the transforms it activated.
    /// </summary>
    public class TransformDeactivator : IReverter
    {
        private readonly List<string> _transforms;

        /// <summary>
        /// Creates the reverter for the given transform names.
        /// </summary>
        /// <param name="transforms">The transform names to deactivate on revert.</param>
        public TransformDeactivator(List<string> transforms)
        {
            _transforms = transforms;
        }

        /// <inheritdoc />
        public object? Store(Module_PartSwitch partSwitch) => null;

        /// <inheritdoc />
        public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
        {
            foreach (var transformName in _transforms)
            {
                var t = partSwitch.gameObject.transform.FindChildRecursive(transformName);
                if (ReferenceEquals(t, null) || t == null)
                {
                    IVSwiftLogger.Instance?.LogError($"Could not find child of {partSwitch.gameObject.name} with name {transformName}");
                    continue;
                }
                t.gameObject.SetActive(false);
            }
        }

        /// <inheritdoc />
        public bool RequiresInVariantSet => true;
    }
}
