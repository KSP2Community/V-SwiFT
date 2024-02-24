using I2.Loc;
using JetBrains.Annotations;
using KSP.Game;
using KSP.IO;
using KSP.Modules;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;
using VSwift.Modules.InformationLoaders;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(EngineModeSwapper))]
public class EngineModeSwapper : ITransformer
{
    [UsedImplicitly]
    public List<Data_Engine.EngineMode> Modes = [];
    [UsedImplicitly] public string Name = nameof(EngineModeSwapper);
    public IReverter? Reverter => EngineModesReverter.Instance;
    public bool SavesInformation => true;
    public bool VisualizesInformation => true;
    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
        if (!partSwitch.OABPart.TryGetModule(out Module_Engine moduleEngine)) return;
        moduleEngine.OnShutdown();
        var clonedData = moduleEngine.dataEngine.JsonClone();
        clonedData.engineModes = clonedData.engineModes.Select(x =>
        {
            if (x == null) return x;
            foreach (var engineMode in Modes.Where(engineMode => engineMode.engineID == x.engineID))
            {
                return engineMode;
            }
            return x;
        }).ToArray();
        moduleEngine.DataModules[typeof(Data_Engine)] = moduleEngine.dataEngine = clonedData;
        moduleEngine.dataEngine.RebuildDataContext();
        moduleEngine.OnInitialize();
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
    }

    public (Type savedType, JToken savedValue) SaveInformation()
    {
        return (typeof(EngineModeSwapLoader), JToken.Parse(IOProvider.ToJson(Modes)));
    }

    private const string SingleMode = "SingleMode";
    private const string MultiMode = "MultiMode";
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