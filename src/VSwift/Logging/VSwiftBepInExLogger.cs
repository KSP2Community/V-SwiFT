using BepInEx.Logging;
using VSwift.Modules.Logging;

namespace VSwift.Logging;

public class VSwiftBepInExLogger(ManualLogSource logSource) : IVSwiftLogger
{
    public void LogDebug(object debug)
    {
        logSource.LogDebug(debug);
    }

    public void LogInfo(object info)
    {
        logSource.LogInfo(info);
    }

    public void LogWarning(object warning)
    {
        logSource.LogWarning(warning);
    }

    public void LogError(object error)
    {
        logSource.LogError(error);
    }
}