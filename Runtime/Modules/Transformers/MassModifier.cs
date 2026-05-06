using System;
using I2.Loc;
using JetBrains.Annotations;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Adds a fixed value to the part's <see cref="Data.Data_PartSwitch.MassModifier" /> when active.
    /// </summary>
    [Transformer(nameof(MassModifier))]
    public class MassModifier : ITransformer
    {
        /// <summary>
        /// The mass delta added to the part when this transformer is active.
        /// </summary>
        [UsedImplicitly]
        public float Modifier = 0.0f;

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
            partSwitch.DataPartSwitch!.MassModifier += Modifier;
        }

        /// <inheritdoc />
        public void ApplyCommon(Module_PartSwitch partSwitch)
        {
        }

        private static readonly LocalizedString MassKey = "VSwift/MassModifier";
        private static readonly LocalizedString DescKey = "VSwift/Mass/Description";

        /// <inheritdoc />
        public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
        {
            var digits = Math.Max(3 - (int)Math.Floor(Math.Log10(Modifier)), 0);
            return IVSwiftUI.Instance.CreateStatBlock(MassKey,
                string.Format(DescKey, (Modifier >= 0.0 ? "+" : "") + Modifier.ToString($"N{digits}")));
        }
    }
}
