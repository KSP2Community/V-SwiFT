using System;
using System.Collections.Generic;
using I2.Loc;
using JetBrains.Annotations;
using KSP.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;
using VSwift.Modules.InformationLoaders;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Sets a scalar value at the configured key path on the part data when active, persisted across saves.
    /// </summary>
    [Serializable]
    [Transformer(nameof(PartScalarTransformer))]
    [TransformerCategory("Scalars")]
    [TransformerDescription("Mutate any scalar via keypath")]
    public class PartScalarTransformer : ITransformer, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The key path on the part data to set.
        /// </summary>
        [Tooltip("Key path into the part data identifying the scalar field to set.")]
        [UsedImplicitly]
        public string Key = "";

        /// <summary>
        /// The value to set at <see cref="Key" />.
        /// </summary>
        [Tooltip("JSON value written at the key path when this variant is active.")]
        [UsedImplicitly]
        public JToken Value = 0.0;

        [SerializeField, Newtonsoft.Json.JsonIgnore]
        private string _valueSerialized = "0";

        /// <summary>
        /// Flushes <see cref="Value" /> to a string so Unity's serializer can persist it. The JToken itself is invisible to Unity but the string round-trips through prefab YAML.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _valueSerialized = Value?.ToString(Newtonsoft.Json.Formatting.None) ?? string.Empty;
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
                    $"[PartScalarTransformer] Failed to parse stored value '{_valueSerialized}': {e.Message}");
                Value = null;
            }
        }

        /// <inheritdoc />
        public IReverter? Reverter => null;

        /// <inheritdoc />
        public bool SavesInformation => true;

        /// <inheritdoc />
        public bool VisualizesInformation => true;

        /// <inheritdoc />
        public void ApplyInFlight(Module_PartSwitch partSwitch)
        {
        }

        /// <inheritdoc />
        public void ApplyInOab(Module_PartSwitch partSwitch)
        {
        }

        /// <inheritdoc />
        public void ApplyCommon(Module_PartSwitch partSwitch)
        {
        }

        /// <inheritdoc />
        public (Type savedType, JToken savedValue) SaveInformation()
        {
            return (typeof(PartScalarLoader), JToken.Parse(IOProvider.ToJson(this)));
        }


        internal static readonly
            Dictionary<string, (LocalizedString locKey, LocalizedString formatKey, Func<JToken, string> stringConverter)>
            Visualizers = new()
            {
                ["maxTemp"] = ("VSwift/MaxTemp/Title","VSwift/MaxTemp/Description",ConvertFloat)
            };

        private static string ConvertFloat(JToken flt)
        {
            var value = flt.FromJToken<float>();
            var digits = Math.Max(3 - (int)Math.Floor(Math.Log10(value)), 0);
            return value.ToString($"N{digits}");
        }


        /// <inheritdoc />
        public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
        {
            if (Visualizers.TryGetValue(Key, out var visualizer))
            {
                return IVSwiftUI.Instance.CreateStatBlock(visualizer.locKey,
                    string.Format(visualizer.formatKey, visualizer.stringConverter(Value)));
            }
            return new VisualElement();
        }
    }
}
