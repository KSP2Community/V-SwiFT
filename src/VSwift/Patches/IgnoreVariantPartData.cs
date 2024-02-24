using HarmonyLib;
using KSP.Game;
using KSP.Sim.Definitions;

namespace VSwift.Patches;

[HarmonyPatch]
public static class IgnoreVariantPartData
{
    [HarmonyPatch(typeof(PartProvider))]
    [HarmonyPatch(nameof(PartProvider.AllParts))]
    [HarmonyPrefix]
    public static bool Ignore(PartProvider __instance,  ref IReadOnlyCollection<PartCore> __result)
    {
        __result = __instance._partData.Where(kv => !kv.Key.Contains('+')).Select(kv => kv.Value).ToList();
        return false;
    }
}