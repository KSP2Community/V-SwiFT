using I2.Loc;
using JetBrains.Annotations;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(TextVisualizer))]
public class TextVisualizer : ITransformer
{
    [UsedImplicitly]
    public string TitleKey = "";
    [UsedImplicitly]
    public string DescriptionKey = "";
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
        return IVSwiftUI.Instance.CreateStatBlock(new LocalizedString(TitleKey), new LocalizedString(DescriptionKey));
    }
}