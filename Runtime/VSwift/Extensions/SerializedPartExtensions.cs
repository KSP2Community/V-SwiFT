using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using KSP.Sim;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Data;

namespace VSwift.Extensions
{
    /// <summary>
    /// V-SwiFT extension methods on <see cref="SerializedPart" />.
    /// </summary>
    public static class SerializedPartExtensions
    {
        /// <summary>
        /// Stores per-variant transformer-saved data on the serialized part, indexed by variant-set index and transformer index.
        /// </summary>
        /// <param name="part">The part to store the override on.</param>
        /// <param name="data">The override data to store.</param>
        public static void SetPartSwitchOverride(this SerializedPart part, Dictionary<string, Dictionary<string, (string savedType, JToken savedData)>> data)
        {
            part.PartSwitchOverrides = data;
        }

        /// <summary>
        /// Returns the per-variant transformer-saved data previously stored on the serialized part.
        /// </summary>
        /// <param name="part">The part to read from.</param>
        /// <returns>The override data, or <c>null</c> when none has been stored.</returns>
        public static Dictionary<string, Dictionary<string, (string savedType, JToken savedData)>>? GetPartSwitchOverride(
            this SerializedPart part) => part.PartSwitchOverrides;


        /// <summary>
        /// Returns the active-variant identifier string for the serialized part's <see cref="Data_PartSwitch" /> module data, joining the variant IDs of all variant sets with <c>+</c>.
        /// </summary>
        /// <param name="part">The serialized part to read from.</param>
        /// <returns>The joined active-variant identifier, or <c>null</c> when the part has no <see cref="Data_PartSwitch" /> module data.</returns>
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
