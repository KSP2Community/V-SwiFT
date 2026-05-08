using System;
using JetBrains.Annotations;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Marks an <see cref="ITransformer" /> implementation with a short name registered in the <see cref="VSwift.Utilities.Transformers" /> registry, used as the JSON discriminator and the Lua-facing key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(ITransformer))]
    [MeansImplicitUse]
    public class Transformer : Attribute
    {
        /// <summary>
        /// Marks the decorated class as a transformer registered under the given short name.
        /// </summary>
        /// <param name="transformerName">The transformer's short name, used as the JSON discriminator and registry key.</param>
        public Transformer(string transformerName)
        {
            TransformerName = transformerName;
        }

        /// <summary>
        /// Gets the transformer's short name.
        /// </summary>
        public string TransformerName { get; }
    }
}
