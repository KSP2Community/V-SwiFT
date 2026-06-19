using System;
using JetBrains.Annotations;
using KSP.Assets;
using KSP.Game;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.UI;
using VSwift.Modules.Variants;

namespace VSwift.UI
{
    /// <summary>
    /// <see cref="Modules.UI.IVSwiftUI" /> implementation backed by UI Toolkit, displaying the part-switch popout window and constructing variant-info stat blocks.
    /// </summary>
    public class VSwiftUI : IVSwiftUI
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            StatBlockContainer = null;
        }

        /// <inheritdoc />
        public void ShowUIFor(Module_PartSwitch modulePartSwitch, VariantSet variantSet)
        {
            PartSwitchPopoutWindowController.ShowFor(modulePartSwitch, variantSet);
        }

        internal static VisualTreeAsset StatBlockContainer;

        // _statBlockContainer ??= AssetManager.GetAsset<VisualTreeAsset>(
        //     $"{VSwiftPlugin.ModGuid}/" +
        //     "VSwift_ui/" +
        //     "ui/partswitchpopout/partstatistic.uxml");
        //
        /// <inheritdoc />
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
}
