using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using KSP.Game;
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
            
            var assets = GameManager.Instance.Assets;
            
            // Load the popout window controller uxml
            var popoutWindowControllerUxmlHandle = assets.LoadAssetAsync<VisualTreeAsset>("vs/part_popout");
            popoutWindowControllerUxmlHandle.WaitForCompletion();
            var popoutWindowControllerUxml = popoutWindowControllerUxmlHandle.Result;
            

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

            var popOutWindow = Window.Create(windowOptions, popoutWindowControllerUxml);
            var popoutWindowController = popOutWindow.gameObject.AddComponent<PartSwitchPopoutWindowController>();
            
            // TODO: Refactor vswift ui loading to use addressables
            
            var partStatisticHandle = assets.LoadAssetAsync<VisualTreeAsset>("vs/part_statistic");
            partStatisticHandle.WaitForCompletion();
            var partStatistic = partStatisticHandle.Result;
            VSwiftUI.StatBlockContainer = partStatistic;
            
            var variantNameHandle =assets.LoadAssetAsync<VisualTreeAsset>("vs/ps_variant_name");
            variantNameHandle.WaitForCompletion();
            var variantName = variantNameHandle.Result;
            PartSwitchPopoutWindowController.VariantNameContainer = variantName;
            
            var requiredTechHandle = assets.LoadAssetAsync<VisualTreeAsset>("vs/required_tech");
            requiredTechHandle.WaitForCompletion();
            var requiredTech = requiredTechHandle.Result;
            PartSwitchPopoutWindowController.RequirementContainer = requiredTech;
        }

        public override void OnPostInitialized()
        {
        }
    }
}
