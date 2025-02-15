using JetBrains.Annotations;
using SpaceWarp.API.Assets;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.UI;
using VSwift.Modules.Variants;

namespace VSwift.UI;

public class VSwiftUI : IVSwiftUI
{
    public void ShowUIFor(Module_PartSwitch modulePartSwitch, VariantSet variantSet)
    {
        PartSwitchPopoutWindowController.ShowFor(modulePartSwitch, variantSet);
    }

    [CanBeNull] private static VisualTreeAsset _statBlockContainer;

    private static VisualTreeAsset StatBlockContainer =>
        _statBlockContainer ??= AssetManager.GetAsset<VisualTreeAsset>(
            $"{VSwiftPlugin.ModGuid}/" +
            "VSwift_ui/" +
            "ui/partswitchpopout/partstatistic.uxml");
    
    public VisualElement CreateStatBlock(string statBlockTitle, string statBlockText)
    {
        var clone = StatBlockContainer.CloneTree();
        var container = clone.Q<VisualElement>("variant-stat");
        var name = container.Q<Label>("variant-stat-title");
        name.text = statBlockTitle.ToUpper();
        var info = container.Q<Label>("variant-stat-info");
        info.text = statBlockText;
        return container;
    }
}