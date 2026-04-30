using System.Collections.Generic;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Reverters
{
    public class TransformDeactivator : IReverter
    {
        private readonly List<string> _transforms;

        public TransformDeactivator(List<string> transforms)
        {
            _transforms = transforms;
        }

        public object? Store(Module_PartSwitch partSwitch) => null;

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

        public bool RequiresInVariantSet => true;
    }
}
