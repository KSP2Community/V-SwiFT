using I2.Loc;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(DefaultMassVisualizer))]
public class DefaultMassVisualizer : ITransformer
{
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

    private static readonly LocalizedString MassKey = "VSwift/Mass";
    private static readonly LocalizedString DescKey = "VSwift/Mass/Description";
    public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
    {
        var originalMass = modulePartSwitch.OABPart.AvailablePart.Mass;
        var digits = Math.Max(3 - (int)Math.Floor(Math.Log10(originalMass)), 0);
        return IVSwiftUI.Instance.CreateStatBlock(MassKey,
            string.Format(DescKey, originalMass.ToString($"N{digits}")));
    }
}