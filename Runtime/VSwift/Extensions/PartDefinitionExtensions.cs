using JetBrains.Annotations;
using KSP.Sim.Definitions;
using VSwift.Modules.Data;

namespace VSwift.Extensions;

public static class PartDefinitionExtensions
{
    [CanBeNull]
    public static string GetCurrentVariantNameString(this PartDefinition part)
    {
        
        foreach (var module in part.Modules)
        {
            foreach (var datum in module.ModuleData)
            {
                if (datum.DataObject is Data_PartSwitch dataPartSwitch)
                {
                    return string.Join('+', dataPartSwitch.ActiveVariants);
                }
            }
        }
        return null;
    }
}