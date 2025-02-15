using System;
using JetBrains.Annotations;

namespace VSwift.Modules.Transformers
{
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(ITransformer))]
    [MeansImplicitUse]
    public class Transformer : Attribute
    {
        public Transformer(string transformerName)
        {
            TransformerName = transformerName;
        }
        public string TransformerName { get; }
    }
}