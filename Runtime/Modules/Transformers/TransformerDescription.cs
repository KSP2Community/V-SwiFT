using System;
using JetBrains.Annotations;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Short author-facing description surfaced for an <see cref="ITransformer" /> in editor-side pickers and tooltips.
    /// </summary>
    /// <remarks>
    /// Editor surfaces (transformer picker, browse-by-type lookups) read this string to render a per-transformer subtitle line. Transformers without this attribute render with their type name only.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(ITransformer))]
    public class TransformerDescription : Attribute
    {
        /// <summary>
        /// Marks the decorated transformer with a short description.
        /// </summary>
        /// <param name="description">A single short sentence describing what the transformer does.</param>
        public TransformerDescription(string description)
        {
            Description = description;
        }

        /// <summary>
        /// Gets the description string.
        /// </summary>
        public string Description { get; }
    }
}
