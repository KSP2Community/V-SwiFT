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
    public static class StoreVariantPartData
    {
        internal static Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>> GetOverrideData(
            PartComponent component) =>
            component.TryGetModuleData<PartComponentModule_PartSwitch,Data_PartSwitch>(out var dataPartSwitch) ?
                dataPartSwitch.GetStoredVariantInformation()
                : null;

        internal static void SetOverrideData(SerializedPart serializedPart, IObjectAssemblyPart objectAssemblyPart)
        {
            serializedPart.SetPartSwitchOverride(GetOverrideData(objectAssemblyPart));
        }

        internal static Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>> GetOverrideData(
            IObjectAssemblyPart objectAssemblyPart) =>
            objectAssemblyPart.TryGetModule(out Module_PartSwitch modulePartSwitch)
                ? modulePartSwitch.GetStoredVariantInformation()
                : null;
    }
}