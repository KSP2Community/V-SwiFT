using System.Reflection;
using Castle.Core.Internal;
using HarmonyLib;
using JetBrains.Annotations;
using KSP.Game;
using KSP.IO;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VSwift.Extensions;
using VSwift.Modules.Extensions;
using VSwift.Modules.InformationLoaders;
using VSwift.Modules.Logging;

namespace VSwift.Patches;

[HarmonyPatch]
public static class LoadVariantPartData
{
    [HarmonyPatch(typeof(SpaceSimulation))]
    [HarmonyPatch(nameof(SpaceSimulation.CreatePart))]
    [HarmonyILManipulator]
    private static void PatchCreatePartIL(ILContext ilContext, ILLabel endLabel)
    {
        ILCursor cursor = new(ilContext);
        cursor.GotoNext(MoveType.Before, instruction => instruction.MatchStloc(2));
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate(GetPartCore);
    }


    private static int Compare(string a, string b) =>
        int.TryParse(a, out var aInt) && int.TryParse(b, out var bInt)
            ? aInt.CompareTo(bInt)
            : string.Compare(a, b, StringComparison.OrdinalIgnoreCase);

    private static PartCore GetPartCore(PartCore originalPartCore, SerializedPart part)
    {
        if (part.GetPartSwitchOverride() is not { } data)
            return originalPartCore;
        // Try to get the original variant name
        var variantName = part.GetCurrentVariantNameString();
        if (!variantName.IsNullOrEmpty() &&
            GameManager.Instance.Game.Parts._partData.TryGetValue($"{part.partName}+{variantName}", out var cached))
        {
            return cached;
        }
        
        var newData = originalPartCore.JsonClone().data;
        List<(string, List<(string, IInformationLoader, JToken)>)> toBeDoublySorted = [];
        foreach (var (idx,variant) in data)
        {
            List<(string, IInformationLoader, JToken)> toBeSorted = [];
            foreach (var (key, (type, obj)) in variant)
            {
                var t = Type.GetType(type);
                if (t == null) continue;
                var instance = Activator.CreateInstance(t) as IInformationLoader;
                toBeSorted.Add((key, instance, obj));
            }
            toBeSorted.Sort((a, b) => Compare(a.Item1,b.Item1));
            toBeDoublySorted.Add((idx, toBeSorted));
        }
        toBeDoublySorted.Sort((a, b) => Compare(a.Item1,b.Item1));
        foreach (var (_, variant) in toBeDoublySorted)
        {
            foreach (var (_, loader, d) in variant)
            {
                loader.LoadInformationInto(newData, d);
            }
        }

        // GUIUtility.systemCopyBuffer = IOProvider.ToJson(newData);
        var result = new PartCore
        {
            version = PartCore.PART_SERIALIZATION_VERSION, // Todo replace this with reflection
            data = newData
        };
        if (!variantName.IsNullOrEmpty())
        {
            GameManager.Instance.Game.Parts._partData[$"{part.partName}+{variantName}"] = result;
            GameManager.Instance.Game.Parts._partJson[$"{part.partName}+{variantName}"] = IOProvider.ToJson(result);
        }
        return result;
    }
    
    [HarmonyPatch(typeof(PartComponent))]
    [HarmonyPatch(nameof(PartComponent.MergePartModuleData), typeof(List<SerializedPartModule>))]
    [HarmonyILManipulator]
    private static void PatchMergePartIL(ILContext ilContext, ILLabel endLabel)
    {
        ILCursor cursor = new(ilContext);
        cursor.GotoNext(MoveType.Before, inst => inst.MatchLdarg(1));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetVariantName);
    }

    private static string GetVariantName(string originalName, PartComponent part)
    {
        IVSwiftLogger.Instance.LogInfo($"Getting Variant Name of (original: {originalName}, partName: {part.PartName})");
#pragma warning disable Harmony003
        originalName ??= part.PartName;
#pragma warning restore Harmony003
        if (part.initialDefinitionData is not PartDefinition partDefinition) return originalName;
        var result = partDefinition.GetCurrentVariantNameString();
        return !result.IsNullOrEmpty() ? $"{originalName}+{result}" : originalName;
    }
}