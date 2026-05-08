using System.Collections.Generic;
using System.Reflection;
using KSP.Game.Serialization;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.impl;
using Newtonsoft.Json.Linq;
using VSwift.Extensions;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Components;
using VSwift.Modules.Data;
using VSwift.Modules.Logging;

namespace VSwift.Patches
{
    /// <summary>
    /// Captures the active-variant transformer state from a part-switch module so it can be persisted on the serialized part.
    /// </summary>
    public static class StoreVariantPartData
    {
        /// <summary>
        /// Returns the stored variant information from the flight-side part-switch component, or <c>null</c> when the component has no <see cref="Data_PartSwitch" /> module data.
        /// </summary>
        /// <param name="component">The flight-side part component.</param>
        /// <returns>The per-variant transformer-saved data, or <c>null</c>.</returns>
        internal static Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>> GetOverrideData(
            PartComponent component) =>
            component.TryGetModuleData<PartComponentModule_PartSwitch,Data_PartSwitch>(out var dataPartSwitch) ?
                dataPartSwitch.GetStoredVariantInformation()
                : null;

        /// <summary>
        /// Captures the override data from the OAB part and stores it on the serialized part.
        /// </summary>
        /// <param name="serializedPart">The serialized part to store the override on.</param>
        /// <param name="objectAssemblyPart">The OAB part to read from.</param>
        internal static void SetOverrideData(SerializedPart serializedPart, IObjectAssemblyPart objectAssemblyPart)
        {
            serializedPart.SetPartSwitchOverride(GetOverrideData(objectAssemblyPart));
        }

        /// <summary>
        /// Returns the stored variant information from the OAB part-switch module, or <c>null</c> when the part has no <see cref="Module_PartSwitch" />.
        /// </summary>
        /// <param name="objectAssemblyPart">The OAB part.</param>
        /// <returns>The per-variant transformer-saved data, or <c>null</c>.</returns>
        internal static Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>> GetOverrideData(
            IObjectAssemblyPart objectAssemblyPart) =>
            objectAssemblyPart.TryGetModule(out Module_PartSwitch modulePartSwitch)
                ? modulePartSwitch.GetStoredVariantInformation()
                : null;
    }
}
