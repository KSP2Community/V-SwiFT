using System.Reflection;
using JetBrains.Annotations;
using KSP.Sim;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Data;

namespace VSwift.Extensions;

public static class SerializedPartExtensions
{
    private static readonly FieldInfo FieldInfo = typeof(SerializedPart).GetField("partSwitchOverrides");
    public static void SetPartSwitchOverride(this SerializedPart part, Dictionary<string, Dictionary<string, (string savedType, JToken savedData)>> data)
    {
        FieldInfo.SetValue(part, data);
    }

    [CanBeNull]
    public static Dictionary<string, Dictionary<string, (string savedType, JToken savedData)>> GetPartSwitchOverride(
        this SerializedPart part)
    {
        return FieldInfo.GetValue(part) as Dictionary<string, Dictionary<string, (string savedType, JToken savedData)>>;
    }


    [CanBeNull]
    public static string GetCurrentVariantNameString(this SerializedPart part)
    {
        foreach (var module in part.PartModulesState)
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