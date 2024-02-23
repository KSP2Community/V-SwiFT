using System.Reflection;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI.Appbar;
using VSwift.UI;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using VSwift.Logging;
using VSwift.Modules.Logging;
using VSwift.Modules.UI;

namespace VSwift;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class VSwiftPlugin : BaseSpaceWarpPlugin
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// Singleton instance of the plugin class
    [PublicAPI] public static VSwiftPlugin Instance { get; set; }

    // AppBar button IDs
    internal const string ToolbarFlightButtonID = "BTN-VSwiftFlight";
    internal const string ToolbarOabButtonID = "BTN-VSwiftOAB";
    internal const string ToolbarKscButtonID = "BTN-VSwiftKSC";

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
        IVSwiftUI.Instance = new VSwiftUI();
    }
    
    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        Instance = this;

        // Load all the other assemblies used by this mod
        LoadAssemblies();

        var partSwitchPopoutWindowControllerUxml = AssetManager.GetAsset<VisualTreeAsset>(
            $"{ModGuid}/" +
            "VSwift_ui/" +
            "ui/partswitchpopout/partswitchpopout.uxml"
        );

        var windowOptions = new WindowOptions
        {
            WindowId = "VSwift_PartSwitchPopout",
            Parent = null,
            IsHidingEnabled = true,
            DisableGameInputForTextFields = false, // There will be no text fields
            MoveOptions = new MoveOptions
            {
                IsMovingEnabled = true,
                CheckScreenBounds = true,
            }
        };

        var popOutWindow = Window.Create(windowOptions, partSwitchPopoutWindowControllerUxml);
        var popoutWindowController = popOutWindow.gameObject.AddComponent<PartSwitchPopoutWindowController>();
    }

    /// <summary>
    /// Loads all the assemblies for the mod.
    /// </summary>
    private static void LoadAssemblies()
    {
        // Load the Unity project assembly
        var currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;
        var unityAssembly = Assembly.LoadFrom(Path.Combine(currentFolder, "VSwift.Unity.dll"));
        // Register any custom UI controls from the loaded assembly
        CustomControls.RegisterFromAssembly(unityAssembly);
    }
}
