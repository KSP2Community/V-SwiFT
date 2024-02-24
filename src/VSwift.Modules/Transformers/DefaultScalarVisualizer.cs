using JetBrains.Annotations;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(DefaultScalarVisualizer))]
public class DefaultScalarVisualizer : ITransformer
{
    [UsedImplicitly]
    public string Key = "";

    public IReverter? Reverter => null;
    public bool SavesInformation => false;
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

    public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
    {
        var data = modulePartSwitch.OABPart.AvailablePart.PartData;
        var field = data.GetType().GetField(Key);
        if (field == null) return new VisualElement();
        var value = field.GetValue(data);
        if (value != null && PartScalarTransformer.Visualizers.TryGetValue(Key, out var visualizer))
        {
            return IVSwiftUI.Instance.CreateStatBlock(visualizer.locKey,
                string.Format(visualizer.formatKey, visualizer.stringConverter(value.ToJToken())));
        }
        return new VisualElement();
    }
}