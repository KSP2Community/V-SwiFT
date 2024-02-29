using HarmonyLib;
using KSP.Modules;
using KSP.Sim.Definitions;

namespace VSwift.Patches;

[HarmonyPatch(typeof(Module_ResourceCapacities))]
public static class FixModuleResourceCapacities
{
    [HarmonyPatch(nameof(Module_ResourceCapacities.OnInitialize))]
    [HarmonyPrefix]
    public static bool SkipEmptyContainers(Module_ResourceCapacities __instance) =>
        __instance.PartBackingMode != PartBehaviourModule.PartBackingModes.OAB ||
        __instance.OABPart.Containers != null;
}