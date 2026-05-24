using System;
using System.Reflection;
using JetBrains.Annotations;
using KSP.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;
using VSwift.Modules.InformationLoaders;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Replaces a part module's data with a configured replacement when active, persisted across saves.
    /// </summary>
    [Serializable]
    [Transformer(nameof(ModuleDefinitionTransformer))]
    [TransformerCategory("Module Overrides")]
    [TransformerDescription("Swap a Data_<X> module")]
    public class ModuleDefinitionTransformer : ITransformer, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The short name of the part-behaviour-module type whose data this transformer replaces.
        /// </summary>
        [Tooltip("Short type name of the part behaviour module to override (e.g. Module_Engine).")]
        [UsedImplicitly]
        public string BehaviourType = "";

        [JsonIgnore] private Type? _behaviourType = null;

        /// <summary>
        /// Gets the resolved <see cref="System.Type" /> for <see cref="BehaviourType" />.
        /// </summary>
        /// <exception cref="Exception">Thrown when <see cref="BehaviourType" /> is not a registered component module.</exception>
        [JsonIgnore]
        [PublicAPI]
        public Type ActualBehaviourType
        {
            get
            {
                _behaviourType ??= ModulesUtilities.ComponentModules.TryGetValue(BehaviourType, out var tuple)
                    ? tuple.behaviour
                    : throw new Exception($"Unknown behaviour type: {BehaviourType}");
                return _behaviourType;
            }
        }

        /// <summary>
        /// The short name of the module-data type whose value at <see cref="Key" /> is replaced.
        /// </summary>
        [Tooltip("Short type name of the module data to swap in (e.g. Data_Engine).")]
        [UsedImplicitly]
        public string DataType = null!;
        [JsonIgnore] private Type? _dataType = null;

        /// <summary>
        /// Gets the resolved <see cref="System.Type" /> for <see cref="DataType" />.
        /// </summary>
        /// <exception cref="Exception">Thrown when <see cref="DataType" /> is not a registered module-data type.</exception>
        [JsonIgnore]
        [PublicAPI]
        public Type ActualDataType
        {
            get
            {
                _dataType ??= ModulesUtilities.DataModules.TryGetValue(DataType, out var dataType)
                    ? dataType
                    : throw new Exception($"Unknown data type: {DataType}");
                return _dataType;
            }
        }

        /// <summary>
        /// The field name on the module-data type to replace.
        /// </summary>
        [Tooltip("Name of the field on the module data type whose value is replaced.")]
        [UsedImplicitly]
        public string Key = "";

        /// <summary>
        /// The replacement value to deserialize into the field at <see cref="Key" />.
        /// </summary>
        [UsedImplicitly]
        public JToken Value = "";

        [SerializeField, JsonIgnore]
        private string _valueSerialized = "";

        /// <summary>
        /// Flushes <see cref="Value" /> to a string so Unity's serializer can persist it. JToken itself is invisible to Unity but the string round-trips through prefab YAML.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _valueSerialized = Value?.ToString(Formatting.None) ?? string.Empty;
        }

        /// <summary>
        /// Restores <see cref="Value" /> from the persisted string. Empty or malformed input yields a null JToken with a console warning.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(_valueSerialized))
            {
                Value = null;
                return;
            }
            try
            {
                Value = JToken.Parse(_valueSerialized);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning(
                    $"[ModuleDefinitionTransformer] Failed to parse stored value '{_valueSerialized}': {e.Message}");
                Value = null;
            }
        }

        /// <inheritdoc />
        [JsonIgnore] public IReverter? Reverter => ModuleDefinitionReverter.GetInstanceFor(ActualBehaviourType, ActualDataType, Key);

        /// <inheritdoc />
        public bool SavesInformation => true;

        /// <inheritdoc />
        public bool VisualizesInformation => false;

        /// <inheritdoc />
        public void ApplyInFlight(Module_PartSwitch partSwitch)
        {
        }

        /// <inheritdoc />
        public void ApplyInOab(Module_PartSwitch partSwitch)
        {
            if (!partSwitch.OABPart.TryGetModule(ActualBehaviourType, out var toBeLoaded)) return;
            toBeLoaded.Shutdown();
            if (!toBeLoaded.DataModules.TryGetValue(ActualDataType, out var moduleData)) return;
            var field = moduleData.GetType()
                .GetField(Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return;
            field.SetValue(moduleData, IOProvider.FromJson(Value.ToString(Formatting.None),field.FieldType));
            moduleData.RebuildDataContext();
            toBeLoaded.Initialize();
        }

        /// <inheritdoc />
        public void ApplyCommon(Module_PartSwitch partSwitch)
        {
        }

        /// <inheritdoc />
        public (Type savedType, JToken savedValue) SaveInformation()
        {
            return (typeof(ModuleDefinitionLoader), (ActualBehaviourType, ActualDataType, Key, Value).ToJToken());
        }
    }
}
