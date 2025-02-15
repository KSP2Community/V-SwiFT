using System;
using System.Collections.Generic;
using System.Linq;
using PatchManager.SassyPatching.Interfaces;
using VSwift.Attributes;

namespace VSwift.Utilities
{
    internal static class Adapters
    {
        private static Dictionary<Type, Type> _transformerAdapters;

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

        internal static bool TryGetAdapterFor(Type transformerType, out Type adapterType) =>
            _transformerAdapters.TryGetValue(transformerType, out adapterType);
    }
}