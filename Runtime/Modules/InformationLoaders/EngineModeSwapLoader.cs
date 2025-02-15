using KSP.IO;
using KSP.Modules;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSwift.Modules.Logging;

namespace VSwift.Modules.InformationLoaders;

public class EngineModeSwapLoader : IInformationLoader
{
    public void LoadInformationInto(PartData partData, JToken storedInformation)
    {
        var modes = IOProvider.FromJson<List<Data_Engine.EngineMode>>(storedInformation.ToString(Formatting.None));
        // var module = partData.serializedPartModules.First(x => x.BehaviourType == typeof(Module_Engine));
        // partData.serializedPartModules = [..partData.serializedPartModules];
        partData.serializedPartModules = partData.serializedPartModules.Select(FilterModules).ToList();
        return;

        SerializedPartModule FilterModules(SerializedPartModule module)
        {
            IVSwiftLogger.Instance.LogInfo($"Filtering module {module}");
            if (module.BehaviourType == typeof(Module_Engine))
            {
                module.ModuleData = module.ModuleData.Select(FilterData).ToList();
            }
            return module;
        }

        SerializedModuleData FilterData(SerializedModuleData moduleData)
        {
            IVSwiftLogger.Instance.LogInfo($"Filtering moduleData {moduleData.DataObject}");
            if (moduleData.DataObject is not Data_Engine dataEngine) return moduleData;
            List<Data_Engine.EngineMode> engineModes = [];
            foreach (var engineMode in dataEngine.engineModes)
            {
                    
                foreach (var engineModeTwo in modes)
                {
                    if (engineMode.engineID != engineModeTwo.engineID) continue;
                    engineModes.Add(engineModeTwo);
                    goto found;
                }
                engineModes.Add(engineMode);
                found: ;
            }
            dataEngine.engineModes = engineModes.ToArray();
            moduleData.DataObject = dataEngine;
            return moduleData;
        }
    }
}