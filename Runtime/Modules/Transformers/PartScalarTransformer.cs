using I2.Loc;
using JetBrains.Annotations;
using KSP.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;
using VSwift.Modules.InformationLoaders;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(PartScalarTransformer))]
public class PartScalarTransformer : ITransformer
{
    [UsedImplicitly]
    public string Key = "";

    [UsedImplicitly]
    public JToken Value = 0.0;
    
    public IReverter? Reverter => null;
    public bool SavesInformation => true;
    public bool VisualizesInformation => true;
    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
    }

    public (Type savedType, JToken savedValue) SaveInformation()
    {
        return (typeof(PartScalarLoader), JToken.Parse(IOProvider.ToJson(this)));
    }


    internal static readonly 
        Dictionary<string, (LocalizedString locKey, LocalizedString formatKey, Func<JToken, string> stringConverter)>
        Visualizers = new()
        {
            ["maxTemp"] = ("VSwift/MaxTemp/Title","VSwift/MaxTemp/Description",ConvertFloat)
        };

    private static string ConvertFloat(JToken flt)
    {
        var value = flt.FromJToken<float>();
        var digits = Math.Max(3 - (int)Math.Floor(Math.Log10(value)), 0);
        return value.ToString($"N{digits}");
    }
    
    
    public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
    {
        if (Visualizers.TryGetValue(Key, out var visualizer))
        {
            return IVSwiftUI.Instance.CreateStatBlock(visualizer.locKey,
                string.Format(visualizer.formatKey, visualizer.stringConverter(Value)));
        }
        return new VisualElement();
    }
}