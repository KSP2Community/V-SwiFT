using System;
using JetBrains.Annotations;
using Mono.Cecil;
using Newtonsoft.Json.Linq;
using SpaceWarp.Preload.API;

namespace VSwift.Preload;

[UsedImplicitly]
public class SerializedPartPatcher : BasePatcher
{
    private const string SerializedPart = "SerializedPart";
    public override void ApplyPatch(ref AssemblyDefinition assembly)
    {
        var firstTargetType = assembly.MainModule.Types.First(t => t.Name == SerializedPart);
        var dictType = assembly.MainModule.ImportReference(typeof(Dictionary<string, Dictionary<string,(string savedType, JToken savedValue)>>));
        firstTargetType.Fields.Add(new FieldDefinition("partSwitchOverrides", FieldAttributes.Public, dictType));
    }

    public override IEnumerable<string> DLLsToPatch => ["Assembly-CSharp.dll"];
}