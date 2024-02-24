using I2.Loc;
using JetBrains.Annotations;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(MassModifier))]
public class MassModifier : ITransformer
{
    [UsedImplicitly]
    public float Modifier = 0.0f;
    public IReverter? Reverter => null;
    public bool SavesInformation => false;
    public bool VisualizesInformation => true;
    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
        partSwitch.DataPartSwitch!.MassModifier += Modifier;
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
    }

    private static readonly LocalizedString MassKey = "VSwift/MassModifier";
    private static readonly LocalizedString DescKey = "VSwift/Mass/Description";
    public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
    {
        var digits = Math.Max(3 - (int)Math.Floor(Math.Log10(Modifier)), 0);
        return IVSwiftUI.Instance.CreateStatBlock(MassKey,
            string.Format(DescKey, (Modifier >= 0.0 ? "+" : "") + Modifier.ToString($"N{digits}")));
    }
}