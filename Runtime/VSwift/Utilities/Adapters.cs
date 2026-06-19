using System;
using System.Collections.Generic;
using System.Linq;
using VSwift.Attributes;

namespace VSwift.Utilities
{
    /// <summary>
    /// Registry mapping transformer types to their <see cref="Attributes.TransformerAdapter" />-decorated Lua wrappers, populated by reflection at static-construction time.
    /// </summary>
    internal static class Adapters
    {
        private static Dictionary<Type, Type> _transformerAdapters;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _transformerAdapters = new Dictionary<Type, Type> { };
        }

        static Adapters()
        {
            _transformerAdapters = new Dictionary<Type, Type> { };
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()))
            {
                var attr = type.GetCustomAttributes(typeof(TransformerAdapter), false).FirstOrDefault();
                if (attr is TransformerAdapter transformerAdapter)
                {
                    _transformerAdapters[transformerAdapter.TransformerType] = type;
                }
            }
        }

        /// <summary>
        /// Looks up the adapter wrapper registered for the given transformer type.
        /// </summary>
        /// <param name="transformerType">The transformer type to look up.</param>
        /// <param name="adapterType">The adapter wrapper type, or <c>null</c> when no adapter is registered.</param>
        /// <returns>True if an adapter is registered, false otherwise.</returns>
        internal static bool TryGetAdapterFor(Type transformerType, out Type adapterType) =>
            _transformerAdapters.TryGetValue(transformerType, out adapterType);
    }
}
