using System;
using JetBrains.Annotations;

namespace VSwift.Attributes
{
    /// <summary>
    /// Adapt a specific transformer type with this user data
    /// Any class this is used on requires a constructor with the parameters (JObject, VariantSelectable)
    /// </summary>
    /// <param name="transformerType">the type this is adapting</param>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
    [MeansImplicitUse]
    public class TransformerAdapter : Attribute
    {
        public TransformerAdapter(Type transformerType)
        {
            TransformerType = transformerType;
        }
        public Type TransformerType {get;}
    }
}