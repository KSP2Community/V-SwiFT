using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Logging;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Activates child <see cref="UnityEngine.GameObject" /> transforms on the part by name when active.
    /// </summary>
    [Serializable]
    [Transformer(nameof(TransformActivator))]
    [TransformerCategory("Visual")]
    [TransformerDescription("Enable/disable GameObjects")]
    public class TransformActivator : ITransformer
    {
        /// <summary>
        /// The names of the child transforms to activate.
        /// </summary>
        [Tooltip("GameObject paths under the part to activate when this variant is selected. Resolved via FindChildRecursive.")]
        public List<string> Transforms = new() { };

        [JsonIgnore] private IReverter? _reverter;

        /// <inheritdoc />
        [JsonIgnore] public IReverter? Reverter => _reverter ??= new TransformDeactivator(Transforms);

        /// <inheritdoc />
        public bool SavesInformation => false;

        /// <inheritdoc />
        public bool VisualizesInformation => false;

        /// <inheritdoc />
        public void ApplyInFlight(Module_PartSwitch partSwitch)
        {
            // Do nothing
        }

        /// <inheritdoc />
        public void ApplyInOab(Module_PartSwitch partSwitch)
        {
            // Do nothing
        }

        /// <inheritdoc />
        public void ApplyCommon(Module_PartSwitch partSwitch)
        {
            foreach (var activatedTransform in Transforms)
            {
                var t = partSwitch.gameObject.transform.FindChildRecursive(activatedTransform);
                if (ReferenceEquals(t,null) || t == null)
                {
                    IVSwiftLogger.Instance.LogError($"Could not find child of {partSwitch.gameObject.name} with name {activatedTransform}");
                    continue;
                }
                t.gameObject.SetActive(true);
            }
        }
    }
}
