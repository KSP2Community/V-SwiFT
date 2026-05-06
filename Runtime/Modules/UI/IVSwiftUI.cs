using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Variants;

namespace VSwift.Modules.UI
{
    /// <summary>
    /// Interface for the V-SwiFT module-side UI surface, forwarding to the host's UI stack.
    /// </summary>
    /// <remarks>
    /// Implementations are wired up by the V-SwiFT plugin host (see <see cref="UI.VSwiftUI" />) so the modules assembly does not need to reference the host UI stack directly.
    /// </remarks>
    public interface IVSwiftUI
    {
        /// <summary>
        /// Gets or sets the active UI instance, assigned by the V-SwiFT plugin during initialization.
        /// </summary>
        public static IVSwiftUI Instance { get; set; }



        /// <summary>
        /// Opens the variant-set popout window for the given part-switch module.
        /// </summary>
        /// <param name="modulePartSwitch">The part-switch module the variant set belongs to.</param>
        /// <param name="variantSet">The variant set to display.</param>
        public void ShowUIFor(Module_PartSwitch modulePartSwitch, VariantSet variantSet);

        /// <summary>
        /// Creates a single-statistic visual element for variant-info display.
        /// </summary>
        /// <param name="statBlockTitle">The stat name (e.g. <c>Engine ISP - Reverse</c>).</param>
        /// <param name="statBlockText">The stat value (e.g. <c>Sea Level 300s / Vacuum 1000000s</c>).</param>
        /// <returns>The stat block visual element.</returns>
        public VisualElement CreateStatBlock(string statBlockTitle, string statBlockText);
    }
}
