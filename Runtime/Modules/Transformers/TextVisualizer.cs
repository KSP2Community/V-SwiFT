using System;
using I2.Loc;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Renders a localized title and description as a stat block in the variant-info popout without modifying the part.
    /// </summary>
    [Serializable]
    [Transformer(nameof(TextVisualizer))]
    [TransformerCategory("Visualizers")]
    [TransformerDescription("Show a localized title and description")]
    public class TextVisualizer : ITransformer
    {
        /// <summary>
        /// Localization key for the stat block's title.
        /// </summary>
        [Tooltip("Localization key for the title shown in the variant info popout.")]
        [UsedImplicitly]
        public string TitleKey = "";

        /// <summary>
        /// Localization key for the stat block's description.
        /// </summary>
        [Tooltip("Localization key for the description shown in the variant info popout.")]
        [UsedImplicitly]
        public string DescriptionKey = "";

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
            return IVSwiftUI.Instance.CreateStatBlock(new LocalizedString(TitleKey).ToString() ?? TitleKey, new LocalizedString(DescriptionKey).ToString() ?? DescriptionKey);
        }
    }
}
