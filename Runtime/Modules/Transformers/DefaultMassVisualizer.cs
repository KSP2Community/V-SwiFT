using System;
using I2.Loc;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Renders the part's original mass as a stat block in the variant-info popout without modifying the part.
    /// </summary>
    [Transformer(nameof(DefaultMassVisualizer))]
    public class DefaultMassVisualizer : ITransformer
    {
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

        private static readonly LocalizedString MassKey = "VSwift/Mass";
        private static readonly LocalizedString DescKey = "VSwift/Mass/Description";

        /// <inheritdoc />
        public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
        {
            var originalMass = modulePartSwitch.OABPart.AvailablePart.Mass;
            var digits = Math.Max(3 - (int)Math.Floor(Math.Log10(originalMass)), 0);
            return IVSwiftUI.Instance.CreateStatBlock(MassKey,
                string.Format(DescKey, originalMass.ToString($"N{digits}")));
        }
    }
}
