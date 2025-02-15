using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using KSP.Sim;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Data;

namespace VSwift.Extensions
{
    public static class SerializedPartExtensions
    {
        public static void SetPartSwitchOverride(this SerializedPart part, Dictionary<string, Dictionary<string, (string savedType, JToken savedData)>> data)
        {
            part.PartSwitchOverrides = data;
        }
    
        public static Dictionary<string, Dictionary<string, (string savedType, JToken savedData)>>? GetPartSwitchOverride(
            this SerializedPart part) => part.PartSwitchOverrides;


        public static string? GetCurrentVariantNameString(this SerializedPart part)
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
}