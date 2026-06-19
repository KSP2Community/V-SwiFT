using System.Collections.Generic;
using KSP.IO;
using KSP.Modules;
using UniLinq;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Extensions;
using VSwift.Modules.Logging;

namespace VSwift.Modules.Reverters
{
    /// <summary>
    /// Reverts an <see cref="Transformers.EngineModeSwapper" /> by restoring the part's original engine modes.
    /// </summary>
    public class EngineModesReverter : IReverter
    {
        private static EngineModesReverter? _instance;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        /// <summary>
        /// Gets the singleton reverter instance.
        /// </summary>
        public static EngineModesReverter? Instance => _instance ??= new EngineModesReverter();

        /// <inheritdoc />
        public object? Store(Module_PartSwitch partSwitch)
        {
            return partSwitch.OABPart.TryGetModule(out Module_Engine moduleEngine)
                ? moduleEngine.DataEngine.engineModes.ToList()
                : new List<Data_Engine.EngineMode> { };
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool RequiresInVariantSet => false;

        /// <inheritdoc />
        public bool AppliesInFlight => false;
    }
}
