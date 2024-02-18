using VSwift.Modules.Logging;
using VSwift.Modules.Transformers;

namespace VSwift.Utilities;

public static class Transformers
{
    private static readonly Dictionary<string, Type> TransformerTypes;

    static Transformers()
    {
        TransformerTypes = [];
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()))
        {
            var attr = type.GetCustomAttributes(typeof(Transformer), false).FirstOrDefault();
            if (attr is Transformer transformer)
            {
                TransformerTypes[transformer.TransformerName] = type;
            }
        }
    }
    internal static bool TryGetTransformerByName(string name, out Type adapterType) =>
        TransformerTypes.TryGetValue(name, out adapterType);
}