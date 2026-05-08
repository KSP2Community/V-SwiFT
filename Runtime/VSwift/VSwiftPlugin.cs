using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using KSP.Game;
using Redux.ExtraModTypes;
using SpaceWarp2;
using SpaceWarp2.API.Mods;
using VSwift.UI;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using VSwift.Logging;
using VSwift.Modules.Logging;
using VSwift.Modules.UI;

namespace VSwift
{
    /// <summary>
    /// V-SwiFT entry point as a SpaceWarp <see cref="KerbalMod" />, wiring up the logger and UI implementations and loading the part-switch popout window assets.
    /// </summary>
    public class VSwiftPlugin : KerbalMod
    {
        /// <summary>
        /// Gets the singleton plugin instance, set during <see cref="OnInitialized" />.
        /// </summary>
        [PublicAPI] public static VSwiftPlugin Instance { get; set; }

        // AppBar button IDs
        internal const string ToolbarFlightButtonID = "BTN-VSwiftFlight";
        internal const string ToolbarOabButtonID = "BTN-VSwiftOAB";
        internal const string ToolbarKscButtonID = "BTN-VSwiftKSC";

        /// <summary>
        /// Wires the V-SwiFT logger and UI implementations into the module-side interfaces so the assembly that defines the gameplay modules can use them without referencing the main assembly.
        /// </summary>
        public void Awake()
        {
            IVSwiftLogger.Instance = new VSwiftReduxLogger(SWLogger);
            IVSwiftUI.Instance = new VSwiftUI();
        }

        /// <inheritdoc />
        public override void OnPreInitialized()
        {
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void OnPostInitialized()
        {
        }
    }
}
