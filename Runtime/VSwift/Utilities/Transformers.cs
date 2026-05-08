using System;
using System.Collections.Generic;
using System.Linq;
using VSwift.Modules.Logging;
using VSwift.Modules.Transformers;

namespace VSwift.Utilities
{
    /// <summary>
    /// Registry mapping <see cref="Modules.Transformers.Transformer" /> attribute names to their concrete transformer types, populated by reflection at static-construction time.
    /// </summary>
    public static class Transformers
    {
        private static readonly Dictionary<string, Type> TransformerTypes;

        static Transformers()
        {
            TransformerTypes = new Dictionary<string, Type> { };
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()))
            {
                var attr = type.GetCustomAttributes(typeof(Transformer), false).FirstOrDefault();
                if (attr is Transformer transformer)
                {
                    TransformerTypes[transformer.TransformerName] = type;
                }
            }
        }

        /// <summary>
        /// Looks up the transformer type registered under the given short name.
        /// </summary>
        /// <param name="name">The transformer's short name (the <see cref="Modules.Transformers.Transformer" /> attribute argument).</param>
        /// <param name="adapterType">The transformer type, or <c>null</c> when no transformer with that name is registered.</param>
        /// <returns>True if a transformer with that name is registered, false otherwise.</returns>
        internal static bool TryGetTransformerByName(string name, out Type adapterType) =>
            TransformerTypes.TryGetValue(name, out adapterType);
    }
}
