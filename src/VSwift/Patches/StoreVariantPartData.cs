﻿using System.Reflection;
using HarmonyLib;
using KSP.Game.Serialization;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.impl;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Components;
using VSwift.Modules.Data;

namespace VSwift.Patches;

[HarmonyPatch]
public static class StoreVariantPartData
{
    internal static readonly FieldInfo FieldInfo = typeof(SerializedPart).GetField("partSwitchOverrides");
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
    private static void PatchSerializePartsIL(ILContext ilContext, ILLabel endlable)
    {
        ILCursor cursor = new(ilContext);
        cursor.GotoNext(MoveType.After, instruction => instruction.MatchStloc(0));
        cursor.Emit(OpCodes.Ldloc_0);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(SetOverrideDataFlight);
    }

    private static void SetOverrideDataFlight(SerializedPart serializedPart, PartComponent partComponent)
    {
        FieldInfo.SetValue(serializedPart,GetOverrideData(partComponent));
    }
    
    private static Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>> GetOverrideData(
        PartComponent component) =>
        component.TryGetModuleData<PartComponentModule_PartSwitch,Data_PartSwitch>(out var dataPartSwitch) ?
            dataPartSwitch.GetStoredVariantInformation()
            : null;

    private static void SetOverrideData(SerializedPart serializedPart, IObjectAssemblyPart objectAssemblyPart)
    {
        FieldInfo.SetValue(serializedPart,GetOverrideData(objectAssemblyPart));
    }
    private static Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>> GetOverrideData(
        IObjectAssemblyPart objectAssemblyPart) =>
        objectAssemblyPart.TryGetModule(out Module_PartSwitch modulePartSwitch)
            ? modulePartSwitch.GetStoredVariantInformation()
            : null;
}