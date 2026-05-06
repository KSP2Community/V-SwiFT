using System.Collections.Generic;
using JetBrains.Annotations;
using VSwift.Modules.Transformers;

namespace VSwift.Modules.Variants
{
    /// <summary>
    /// A single variant within a <see cref="VariantSet" />, applying its <see cref="Transformers" /> when active.
    /// </summary>
    [UsedImplicitly]
    public class Variant
    {
        /// <summary>
        /// The variant's identifier; also used as the localization-key fallback.
        /// </summary>
        [UsedImplicitly]
        public string VariantId = "";

        /// <summary>
        /// The variant's localization key; defaults to <c>VariantId</c> when empty.
        /// </summary>
        [UsedImplicitly]
        public string VariantLocalizationKey = ""; // If null or empty, defaults to the variant ID

        /// <summary>
        /// Technology IDs that must be unlocked for this variant to be selectable.
        /// </summary>
        [UsedImplicitly]
        public List<string> VariantTechs = new() { }; // This is a list of technologies to unlock this variant

        /// <summary>
        /// The transformers applied to the part when this variant is active.
        /// </summary>
        [UsedImplicitly]
        public List<ITransformer> Transformers = new() { };

        // TODO: Allow for custom unlock conditions
    }
}
