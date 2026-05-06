using System;
using System.Collections.Generic;
using Redux;
using KSP.Game;
using KSP.IO;
using KSP.Sim;
using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using VSwift.Extensions;
using VSwift.Modules.Extensions;
using VSwift.Modules.InformationLoaders;

namespace VSwift.Patches
{
    /// <summary>
    /// Reconstructs a part's <see cref="PartCore" /> with the active variant's transformer outputs applied, used when a saved part is loaded.
    /// </summary>
    public static class LoadVariantPartData
    {
        private static int Compare(string a, string b) =>
            int.TryParse(a, out var aInt) && int.TryParse(b, out var bInt)
                ? aInt.CompareTo(bInt)
                : string.Compare(a, b, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the part-core data for the saved part with its stored transformer overrides applied, or the original part-core when no overrides exist.
        /// </summary>
        /// <param name="originalPartCore">The base part-core to start from.</param>
        /// <param name="part">The serialized part carrying the variant overrides.</param>
        /// <returns>The transformed part-core, cached against <see cref="GameManager" />.<c>Game.Parts</c> by <c>partName+variantName</c>.</returns>
        internal static PartCore GetVariantPartData(PartCore originalPartCore, SerializedPart part)
        {
            if (part.GetPartSwitchOverride() is not { } data)
                return originalPartCore;
            // Try to get the original variant name
            var variantName = part.GetCurrentVariantNameString();
            if (!variantName.IsNullOrEmpty() &&
                GameManager.Instance.Game.Parts.TryGet($"{part.partName}+{variantName}", out var cached))
            {
                return cached;
            }

            var newData = originalPartCore.JsonClone().data;
            List<(string, List<(string, IInformationLoader, JToken)>)> toBeDoublySorted = new() { };
            foreach (var (idx,variant) in data)
            {
                List<(string, IInformationLoader, JToken)> toBeSorted = new() { };
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

            if (variantName.IsNullOrEmpty()) return result;
            GameManager.Instance.Game.Parts.PartData[$"{part.partName}+{variantName}"] = result;
            GameManager.Instance.Game.Parts.PartJson[$"{part.partName}+{variantName}"] = IOProvider.ToJson(result);
            return result;
        }
    }
}
