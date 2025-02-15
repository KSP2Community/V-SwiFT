using System.Collections.Generic;
using KSP.IO;
using KSP.Modules;
using UniLinq;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Reverters
{
    public class EngineModesReverter : IReverter
    {
        private static EngineModesReverter? _instance;
        public static EngineModesReverter? Instance => _instance ??= new EngineModesReverter();
        public object? Store(Module_PartSwitch partSwitch)
        {
            return partSwitch.OABPart.TryGetModule(out Module_Engine moduleEngine)
                ? moduleEngine.DataEngine.engineModes.ToList()
                : new List<Data_Engine.EngineMode> { };
        }

        public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset)
        {
            if (!partSwitch.OABPart.TryGetModule(out Module_Engine moduleEngine)) return;
            moduleEngine.Shutdown();
            var clonedData = moduleEngine.DataEngine.JsonClone();
            clonedData.engineModes = ((List<Data_Engine.EngineMode>)data!).ToArray();
            moduleEngine.DataModules[typeof(Data_Engine)] = moduleEngine.DataEngine = clonedData;
            moduleEngine.DataEngine.RebuildDataContext();
            moduleEngine.Initialize();
        }

        public bool RequiresInVariantSet => false;
    }
}