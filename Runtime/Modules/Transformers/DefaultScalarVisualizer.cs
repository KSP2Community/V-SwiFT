using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Renders the value at the configured key on the part data as a stat block in the variant-info popout without modifying the part.
    /// </summary>
    [Serializable]
    [Transformer(nameof(DefaultScalarVisualizer))]
    [TransformerCategory("Visualizers")]
    [TransformerDescription("Show a part-data value in the variant popout")]
    public class DefaultScalarVisualizer : ITransformer
    {
        /// <summary>
        /// The key to read from the part data.
        /// </summary>
        [Tooltip("Key path on the part data whose value is rendered as a stat block in the variant info popout.")]
        [UsedImplicitly]
        public string Key = "";

        /// <inheritdoc />
        public IReverter? Reverter => null;

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
        }

        /// <inheritdoc />
        public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
        {
            var data = modulePartSwitch.OABPart.AvailablePart.PartData;
            var field = data.GetType().GetField(Key);
            if (field == null) return new VisualElement();
            var value = field.GetValue(data);
            if (value != null && PartScalarTransformer.Visualizers.TryGetValue(Key, out var visualizer))
            {
                return IVSwiftUI.Instance.CreateStatBlock(visualizer.locKey,
                    string.Format(visualizer.formatKey, visualizer.stringConverter(value.ToJToken())));
            }
            return new VisualElement();
        }
    }
}
