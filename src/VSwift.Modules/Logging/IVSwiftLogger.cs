namespace VSwift.Modules.Logging;

public interface IVSwiftLogger
{


    public static IVSwiftLogger Instance { get; set; }

    public void LogDebug(object debug);

    public void LogInfo(object info);

    public void LogWarning(object warning);

    public void LogError(object error);

}