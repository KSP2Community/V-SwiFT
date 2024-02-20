using HarmonyLib;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json.Linq;
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
    
    
    private static PartCore GetPartCore(PartCore originalPartCore, SerializedPart part)
    {
        if (StoreVariantPartData.FieldInfo.GetValue(part) is not Dictionary<string, Dictionary<string, (string savedType, JToken savedData)>> data)
            return originalPartCore;

        var newData = PartData.FromPartProperties(PartProperties.InferFromPartData(originalPartCore.data));
        foreach (var (_,variant) in data)
        {
            foreach (var (_, (type, obj)) in variant)
            {
                var t = Type.GetType(type);
                if (t == null) continue;
                var instance = Activator.CreateInstance(t) as IInformationLoader;
                instance?.LoadInformationInto(newData, obj);
            }
        }

        return new PartCore
        {
            version = PartCore.PART_SERIALIZATION_VERSION, // Todo replace this with reflection
            data = newData
        };
    }
}