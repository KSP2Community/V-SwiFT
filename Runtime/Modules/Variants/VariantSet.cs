using System.Collections.Generic;
using JetBrains.Annotations;

namespace VSwift.Modules.Variants
{
    /// <summary>
    /// A switchable axis on a part-switch module, listing the variants the player can pick from.
    /// </summary>
    [UsedImplicitly]
    public class VariantSet
    {
        /// <summary>
        /// Gets the variant set's identifier; also used as the localization-key fallback.
        /// </summary>
        public string VariantSetId = ""; // Just used for referencing the variant set

        /// <summary>
        /// Gets the variant set's localization key; defaults to <c>VariantSetId</c> when empty.
        /// </summary>
        public string VariantSetLocalizationKey = ""; // This defaults to the variant set id if its null or empty

        /// <summary>
        /// Gets whether this set surfaces as a popout-window button rather than an inline dropdown.
        /// </summary>
        public bool IsPopout = false; // Whether or not this is a button to pull out a popout window

        /// <summary>
        /// Gets the variants the player can pick from in this set.
        /// </summary>
        public List<Variant> Variants = new() { };
    }
}
