using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Variants;

namespace VSwift.Modules.UI;

public interface IVSwiftUI
{
    public static IVSwiftUI Instance { get; set; }



    /// <summary>
    /// Used to show the popout UI for a variant set
    /// </summary>
    /// <param name="modulePartSwitch">The module that this is for</param>
    /// <param name="variantSet">The variant set that this is for</param>
    /// <param name="setVariant">The action to take when setting the variant</param>
    public void ShowUIFor(Module_PartSwitch modulePartSwitch, VariantSet variantSet);
    
    /// <summary>
    /// Meant to create a block for a single stat
    /// </summary>
    /// <param name="statBlockTitle">The stat name e.g. Engine ISP - Reverse</param>
    /// <param name="statBlockText">The stat itself e.g. Sea Level 300s/Vacuum 1000000s</param>
    /// <returns>A visual element for this stat block</returns>
    public VisualElement CreateStatBlock(string statBlockTitle, string statBlockText);
}