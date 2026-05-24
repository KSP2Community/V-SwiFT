using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using JetBrains.Annotations;
using KSP.Game;
using KSP.IO;
using KSP.Modules;
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
    /// Replaces matching engine modes on the part's <c>Module_Engine</c> with the configured modes when active, persists them across saves, and renders their stats in the variant-info popout.
    /// </summary>
    [Serializable]
    [Transformer(nameof(EngineModeSwapper))]
    [TransformerCategory("Module Overrides")]
    [TransformerDescription("Replace engine modes")]
    public class EngineModeSwapper : ITransformer
    {
        /// <summary>
        /// The engine modes to swap in. Each entry replaces the existing mode whose <c>engineID</c> matches.
        /// </summary>
        [Tooltip("Engine modes that replace the engine's existing modes when this variant is active.")]
        [UsedImplicitly]
        public List<Data_Engine.EngineMode> Modes = new() { };

        /// <summary>
        /// The transformer instance name. Defaults to the <c>EngineModeSwapper</c> short name.
        /// </summary>
        [Tooltip("Identifier for this engine-mode swap. Defaults to the transformer's short name.")]
        [UsedImplicitly] public string Name = nameof(EngineModeSwapper);

        /// <inheritdoc />
        public IReverter? Reverter => EngineModesReverter.Instance;

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
            if (!partSwitch.OABPart.TryGetModule(out Module_Engine moduleEngine)) return;
            moduleEngine.Shutdown();
            var clonedData = moduleEngine.DataEngine.JsonClone();
            clonedData.engineModes = clonedData.engineModes.Select(x =>
            {
                if (x == null) return x;
                foreach (var engineMode in Modes.Where(engineMode => engineMode.engineID == x.engineID))
                {
                    return engineMode;
                }
                return x;
            }).ToArray();
            moduleEngine.DataModules[typeof(Data_Engine)] = moduleEngine.DataEngine = clonedData;
            moduleEngine.DataEngine.RebuildDataContext();
            moduleEngine.Initialize();
        }

        /// <inheritdoc />
        public void ApplyCommon(Module_PartSwitch partSwitch)
        {
        }

        /// <inheritdoc />
        public (Type savedType, JToken savedValue) SaveInformation()
        {
            return (typeof(EngineModeSwapLoader), JToken.Parse(IOProvider.ToJson(Modes)));
        }

        private const string SingleMode = "SingleMode";
        private const string MultiMode = "MultiMode";

        /// <inheritdoc />
        public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
        {
            var database = GameManager.Instance.Game.ResourceDefinitionDatabase;
            var element = new VisualElement();
            var engineModeString = Modes.Count != 1 ? MultiMode : SingleMode;
            foreach (var engineMode in Modes)
            {
                if (engineMode == null) continue;
                LocalizedString displayName = engineMode.EngineDisplayName;

                // First show the propellant name
                var propellant = engineMode.propellant;

                LocalizedString propName = database
                    .GetDefinitionData(database.GetResourceIDFromName(propellant.mixtureName)).displayNameKey;
                element.Add(IVSwiftUI.Instance.CreateStatBlock(GetLocalizedStatBlockName("Propellant"), propName));

                // Next show the thrust
                var vacuumThrust = engineMode.GetThrust(0, 1);
                var seaLevelThrust = engineMode.GetThrust(1, 1);
                var digitsVacuumThrust = Math.Max(3 - (int)Math.Floor(Math.Log10(vacuumThrust)), 0);
                var digitsSeaLevelThrust = Math.Max(3 - (int)Math.Floor(Math.Log10(seaLevelThrust)), 0);
                element.Add(
                    IVSwiftUI.Instance.CreateStatBlock(
                        GetLocalizedStatBlockName("Thrust"),
                        string.Format(new LocalizedString("VSwift/Thrust/SeaLevel"),seaLevelThrust.ToString($"N{digitsSeaLevelThrust}")) + "\n" +
                        string.Format(new LocalizedString("VSwift/Thrust/Vacuum"),vacuumThrust.ToString($"N{digitsVacuumThrust}"))
                    )
                );

                // And finally show the ISP
                var vacuumIsp = engineMode.atmosphereCurve.Evaluate(0);
                var seaLevelIsp = engineMode.atmosphereCurve.Evaluate(1);
                var digitsVacuumIsp = Math.Max(3 - (int)Math.Floor(Math.Log10(vacuumIsp)), 0);
                var digitsSeaLevelIsp = Math.Max(3 - (int)Math.Floor(Math.Log10(seaLevelIsp)), 0);
                element.Add(
                    IVSwiftUI.Instance.CreateStatBlock(
                        GetLocalizedStatBlockName("ISP"),
                        string.Format(new LocalizedString("VSwift/ISP/SeaLevel"),seaLevelIsp.ToString($"N{digitsSeaLevelIsp}")) + "\n" +
                        string.Format(new LocalizedString("VSwift/ISP/Vacuum"),vacuumIsp.ToString($"N{digitsVacuumIsp}"))
                    )
                );

                continue;
                string GetLocalizedStatBlockName(string key)
                {
                    LocalizedString format = $"VSwift/{key}/{engineModeString}";
                    return string.Format(format, displayName.ToString());
                }
            }
            return element;
        }
    }
}
