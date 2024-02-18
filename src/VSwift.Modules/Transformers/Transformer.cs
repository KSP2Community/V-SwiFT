using JetBrains.Annotations;

namespace VSwift.Modules.Transformers;

[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(ITransformer))]
[MeansImplicitUse]
public class Transformer(string transformerName) : Attribute
{
    public string TransformerName => transformerName;
}