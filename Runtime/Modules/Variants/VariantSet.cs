using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace VSwift.Modules.Variants
{
    /// <summary>
    /// A switchable axis on a part-switch module, listing the variants the player can pick from.
    /// </summary>
    [Serializable]
    [UsedImplicitly]
    public class VariantSet
    {
        /// <summary>
        /// Gets the variant set's identifier. Also used as the localization-key fallback.
        /// </summary>
        [Tooltip("Identifier used to reference this variant set. Must be unique within the part.")]
        public string VariantSetId = ""; // Just used for referencing the variant set

        /// <summary>
        /// Gets the variant set's localization key. Defaults to <c>VariantSetId</c> when empty.
        /// </summary>
        [Tooltip("Localization key for the set's display name. Falls back to Set ID when empty.")]
        public string VariantSetLocalizationKey = ""; // This defaults to the variant set id if its null or empty

        /// <summary>
        /// Gets whether this set surfaces as a popout-window button rather than an inline dropdown.
        /// </summary>
        [Tooltip("When unchecked, the set appears as a dropdown in the parts manager. When checked, it appears as a button that opens a modal picker.")]
        public bool IsPopout = false; // Whether or not this is a button to pull out a popout window

        /// <summary>
        /// Gets the variants the player can pick from in this set.
        /// </summary>
        public List<Variant> Variants = new() { };
    }
}
