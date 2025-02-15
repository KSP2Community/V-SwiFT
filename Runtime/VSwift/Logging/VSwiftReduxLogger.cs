using ReduxLib.Logging;
using VSwift.Modules.Logging;

namespace VSwift.Logging
{
    public class VSwiftReduxLogger : IVSwiftLogger
    {
        public VSwiftReduxLogger(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }

        public void LogDebug(object debug)
        {
            Logger.LogDebug(debug);
        }

        public void LogInfo(object info)
        {
            Logger.LogInfo(info);
        }

        public void LogWarning(object warning)
        {
            Logger.LogWarning(warning);
        }

        public void LogError(object error)
        {
            Logger.LogError(error);
        }
    }
}