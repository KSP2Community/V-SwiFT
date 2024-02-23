using HarmonyLib;
using KSP.IO;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Extensions;
using VSwift.Modules.InformationLoaders;

namespace VSwift.Patches;

[HarmonyPatch(typeof(SpaceSimulation))]
public static class LoadVariantPartData
{
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
        if (StoreVariantPartData.FieldInfo.GetValue(part) is not Dictionary<string, Dictionary<string, (string savedType, JToken savedData)>> data)
            return originalPartCore;

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
        return new PartCore
        {
            version = PartCore.PART_SERIALIZATION_VERSION, // Todo replace this with reflection
            data = newData
        };
    }
}