using System.Reflection;
using BepInEx;
using JetBrains.Annotations;
using PatchManager;
using SpaceWarp;
using SpaceWarp.API.Mods;
using VSwift.Logging;
using VSwift.Modules.Logging;

namespace VSwift;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
[BepInDependency(PatchManagerPlugin.ModGuid, PatchManagerPlugin.ModVer)]
public class VSwiftPlugin : BaseSpaceWarpPlugin
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    // Singleton instance of the plugin class
    [PublicAPI] public static VSwiftPlugin Instance { get; set; }

    /// <summary>
    /// Runs on loading of the plugin, loads the VSwift.Modules assembly
    /// VSwift.Modules is in a separate assembly such that it does not have to reference the main assembly
    /// </summary>
    public VSwiftPlugin()
    {
        var path = Assembly.GetExecutingAssembly().Location;
        var folder = new FileInfo(path).Directory;
        Assembly.LoadFile($"{folder}\\VSwift.Modules.dll");
        IVSwiftLogger.Instance = new VSwiftBepInExLogger(Logger);
    }
    
    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;
    }
}

