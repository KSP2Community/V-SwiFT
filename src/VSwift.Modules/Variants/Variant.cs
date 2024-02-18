﻿using JetBrains.Annotations;
using VSwift.Modules.Transformers;

namespace VSwift.Modules.Variants;

[UsedImplicitly]
public class Variant
{
    public string VariantId = "";
    public string VariantLocalizationKey = ""; // If null or empty, defaults to the variant ID
    // public List<string> Transforms = [];
    public List<ITransformer> Transformers = [];
}