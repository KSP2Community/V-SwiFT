using System;
using JetBrains.Annotations;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Groups an <see cref="ITransformer" /> implementation under a category for editor-side transformer-picker UX.
    /// </summary>
    /// <remarks>
    /// The decorated transformer's <see cref="Category" /> appears as the section header for it in the picker. Transformers without this attribute are surfaced under an "Uncategorized" bucket.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(ITransformer))]
    public class TransformerCategory : Attribute
    {
        /// <summary>
        /// Marks the decorated transformer as belonging to the given picker category.
        /// </summary>
        /// <param name="category">The category display string used to group this transformer in the picker.</param>
        public TransformerCategory(string category)
        {
            Category = category;
        }

        /// <summary>
        /// Gets the category display string.
        /// </summary>
        public string Category { get; }
    }
}
