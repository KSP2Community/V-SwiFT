using System;
using JetBrains.Annotations;

namespace VSwift.Attributes
{
    /// <summary>
    /// Marks a class as the Lua-facing wrapper for a specific transformer type, dispatched from <see cref="Utilities.Adapters" />.
    /// </summary>
    /// <remarks>
    /// Decorated classes must declare a constructor taking a single <see cref="Newtonsoft.Json.Linq.JToken" /> parameter; the dispatcher activates the wrapper with the transformer's JSON token.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
    [MeansImplicitUse]
    public class TransformerAdapter : Attribute
    {
        /// <summary>
        /// Marks a class as the adapter wrapper for the given transformer type.
        /// </summary>
        /// <param name="transformerType">The transformer type this class adapts.</param>
        public TransformerAdapter(Type transformerType)
        {
            TransformerType = transformerType;
        }

        /// <summary>
        /// Gets the transformer type this class adapts.
        /// </summary>
        public Type TransformerType {get;}
    }
}
