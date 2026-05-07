using System;
using System.Collections.Generic;
using System.Reflection;
using KSP.Game;
using KSP.IO;
using KSP.OAB;
using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;

namespace VSwift.Modules.Reverters
{
    /// <summary>
    /// Reverts a <see cref="Transformers.ModuleDefinitionTransformer" /> by restoring the original value of a single field on a part-module's data.
    /// </summary>
    public class ModuleDefinitionReverter : IReverter
    {
        private Type _moduleType;
        private Type _dataType;
        private FieldInfo _info;
        private ModuleDefinitionReverter(Type moduleType, Type dataType, string key)
        {
            _moduleType = moduleType;
            _dataType = dataType;
            _info = dataType.GetField(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                    throw new Exception($"Invalid field of {dataType}: {key}");
        }

        private static readonly Dictionary<(Type, Type, string), ModuleDefinitionReverter> Instances = new() { };

        /// <summary>
        /// Returns the cached reverter instance for the given module / data / field tuple, creating one when none exists.
        /// </summary>
        /// <param name="moduleType">The part-behaviour-module type.</param>
        /// <param name="dataType">The module-data type.</param>
        /// <param name="key">The field name on the module-data type to revert.</param>
        /// <returns>The cached reverter for that tuple.</returns>
        /// <exception cref="Exception">Thrown when <paramref name="key" /> is not a field on <paramref name="dataType" />.</exception>
        public static ModuleDefinitionReverter GetInstanceFor(Type moduleType, Type dataType, string key) =>
            Instances.TryGetValue((moduleType, dataType, key),
                out var result)
                ? result
                : Instances[(moduleType,dataType,key)] = new ModuleDefinitionReverter(moduleType, dataType, key);

        /// <inheritdoc />
        public object? Store(Module_PartSwitch partSwitch) =>
            partSwitch.OABPart.TryGetModule(_moduleType,
                out var toBeStored)
                ? toBeStored.DataModules.TryGetValue(_dataType, out var data)
                    ? IOProvider.ToJson(_info.GetValue(data))
                    : null
                : null;

        /// <inheritdoc />
        public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
        {
            if (isStartingReset) return;
            if (data == null) return;
            var trueData = (string)data;
            if (!partSwitch.OABPart.TryGetModule(_moduleType, out var toBeLoaded)) return;
            if (!toBeLoaded.DataModules.TryGetValue(_dataType, out var toBeModified)) return;
            toBeLoaded.Shutdown();
            _info.SetValue(toBeModified, IOProvider.FromJson(trueData, _info.FieldType));
            toBeModified.RebuildDataContext();
            toBeLoaded.Initialize();
        }

        /// <inheritdoc />
        public bool RequiresInVariantSet => true;

        /// <inheritdoc />
        public bool AppliesInFlight => false;
    }
}
