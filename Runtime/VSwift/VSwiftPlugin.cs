using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Redux.ExtraModTypes;
using SpaceWarp;
using SpaceWarp.API.Mods;
using VSwift.UI;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using VSwift.Logging;
using VSwift.Modules.Logging;
using VSwift.Modules.UI;

namespace VSwift
{
    public class VSwiftPlugin : KerbalMod
    {

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
            IVSwiftLogger.Instance = new VSwiftReduxLogger(SWLogger);
            IVSwiftUI.Instance = new VSwiftUI();
        }

        public override void OnPreInitialized()
        {
        }

        /// <summary>
        /// Runs when the mod is first initialized.
        /// </summary>
        public override void OnInitialized()
        {
            Instance = this;

            // Load all the other assemblies used by this mod
            LoadAssemblies();

            // var partSwitchPopoutWindowControllerUxml = AssetManager.GetAsset<VisualTreeAsset>(
            //     $"{ModGuid}/" +
            //     "VSwift_ui/" +
            //     "ui/partswitchpopout/partswitchpopout.uxml"
            // );
            
            

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

            //var popOutWindow = Window.Create(windowOptions, partSwitchPopoutWindowControllerUxml);
            //var popoutWindowController = popOutWindow.gameObject.AddComponent<PartSwitchPopoutWindowController>();
            
            // TODO: Refactor vswift ui loading to use addressables
        }

        public override void OnPostInitialized()
        {
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
}
