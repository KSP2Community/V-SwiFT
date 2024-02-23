using I2.Loc;
using KSP.Game;
using KSP.Modules;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Reverters;
using VSwift.Modules.UI;

namespace VSwift.Modules.Transformers;

[Transformer(nameof(DefaultEngineModesVisualizer))]
public class DefaultEngineModesVisualizer : ITransformer
{
    public IReverter? Reverter => null;
    public bool SavesInformation => false;
    public bool VisualizesInformation => true;

    public void ApplyInFlight(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyInOab(Module_PartSwitch partSwitch)
    {
    }

    public void ApplyCommon(Module_PartSwitch partSwitch)
    {
    }

    private const string SingleMode = "SingleMode";
    private const string MultiMode = "MultiMode";
    
    public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
    {
        var database = GameManager.Instance.Game.ResourceDefinitionDatabase;
        if (modulePartSwitch.OABPart.TryGetModule(out Module_Engine moduleEngine))
        {
            var partCore = GameManager.Instance.Game.Parts.Get(modulePartSwitch.OABPart.Name);
            var data = partCore.data.serializedPartModules.First(x => x.BehaviourType == typeof(Module_Engine))
                .ModuleData.First(x => x.DataObject is Data_Engine).DataObject as Data_Engine;
            var element = new VisualElement();
            var engineModeString = data!.engineModes.Length != 1 ? MultiMode : SingleMode;
            foreach (var engineMode in data.engineModes)
            {
                if (engineMode == null) continue; 
                LocalizedString displayName = engineMode.EngineDisplayName;
                
                // First show the propellant name
                var propellant = engineMode.propellant;

                LocalizedString propName = database
                    .GetDefinitionData(database.GetResourceIDFromName(propellant.mixtureName)).displayNameKey;
                element.Add(IVSwiftUI.Instance.CreateStatBlock(GetLocalizedStatBlockName("Propellant"), propName));
                
                // Next show the thrust
                var vacuumThrust = data.OABGetThrust(engineMode, 0, 1);
                var seaLevelThrust = data.OABGetThrust(engineMode, 1, 1);
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
        return null;
    }
}