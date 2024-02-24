using System.Reflection;
using HarmonyLib;
using KSP.Game.Serialization;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.impl;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json.Linq;
using VSwift.Extensions;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Components;
using VSwift.Modules.Data;
using VSwift.Modules.Logging;

namespace VSwift.Patches;

[HarmonyPatch]
public static class StoreVariantPartData
{
    [HarmonyPatch(typeof(ObjectAssemblyBuilderFileIO))]
    [HarmonyILManipulator]
    [HarmonyPatch(nameof(ObjectAssemblyBuilderFileIO.CollectParts))]
    private static void PatchCollectPartsIL(ILContext ilContext, ILLabel endLabel)
    {
        ILCursor cursor = new(ilContext);
        cursor.GotoNext(MoveType.After, instruction => instruction.MatchStloc(0));
        cursor.Emit(OpCodes.Ldloc_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate(SetOverrideData);
    }

    [HarmonyPatch(typeof(SerializationUtility))]
    [HarmonyILManipulator]
    [HarmonyPatch(nameof(SerializationUtility.SerializePart))]
    private static void PatchSerializePartsIL(ILContext ilContext, ILLabel endlabel)
    {
        ILCursor cursor = new(ilContext);
        cursor.GotoNext(MoveType.After, instruction => instruction.MatchStloc(0));
        cursor.Emit(OpCodes.Ldloc_0);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(SetOverrideDataFlight);
    }

    private static void SetOverrideDataFlight(SerializedPart serializedPart, PartComponent partComponent)
    {
        serializedPart.SetPartSwitchOverride(GetOverrideData(partComponent));
    }
    
    private static Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>> GetOverrideData(
        PartComponent component) =>
        component.TryGetModuleData<PartComponentModule_PartSwitch,Data_PartSwitch>(out var dataPartSwitch) ?
            dataPartSwitch.GetStoredVariantInformation()
            : null;

    private static void SetOverrideData(SerializedPart serializedPart, IObjectAssemblyPart objectAssemblyPart)
    {
        serializedPart.SetPartSwitchOverride(GetOverrideData(objectAssemblyPart));
    }
    private static Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>> GetOverrideData(
        IObjectAssemblyPart objectAssemblyPart) =>
        objectAssemblyPart.TryGetModule(out Module_PartSwitch modulePartSwitch)
            ? modulePartSwitch.GetStoredVariantInformation()
            : null;
}