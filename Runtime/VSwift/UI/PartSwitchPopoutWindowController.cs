using System;
using System.Linq;
using Redux;
using I2.Loc;
using KSP.Game;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Variants;

namespace VSwift.UI
{
    /// <summary>
    /// MonoBehaviour controller for the part-switch popout window, rendering the variant list for a single <see cref="VariantSet" /> and applying the player's selection to the underlying <see cref="Module_PartSwitch" />.
    /// </summary>
    public class PartSwitchPopoutWindowController : MonoBehaviour
    {
        private static PartSwitchPopoutWindowController Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
            VariantNameContainer = null;
            RequirementContainer = null;
        }

        private PanelRenderer _window;
        private VisualElement _rootElement;
        private Module_PartSwitch _currentPartSwitchModule;
        // private VariantSet _currentVariantSet;
        private Label _titleLabel;
        private Button _closeButton;
        private ScrollView _variantSelect;
        private ScrollView _variantInformation;
        private Button _selectVariant;

        private void Awake()
        {
            Instance = this;
            Redux.Messages.StateChanges.VehicleAssemblyBuilderLeft += _ => IsWindowOpen = false;
        }

        private bool _isOpen;

        /// <summary>
        /// Gets or sets whether the popout window is visible.
        /// </summary>
        public bool IsWindowOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                _rootElement.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private string PartName
        {
            get => _titleLabel.text;
            set => _titleLabel.text = value.ToUpper();
        }


        internal static VisualTreeAsset VariantNameContainer;
//            throw new NotImplementedException("Need to refactor to use addressables");
            // _variantNameContainer ??= AssetManager.GetAsset<VisualTreeAsset>(
            //     $"{VSwiftPlugin.ModGuid}/" +
            //     "VSwift_ui/" +
            //     "ui/partswitchpopout/partswitchvariantname.uxml");

        internal static VisualTreeAsset RequirementContainer;
//            throw new NotImplementedException("Need to refactor to use addressables");
            // _requirementContainer ??= AssetManager.GetAsset<VisualTreeAsset>(
            //     $"{VSwiftPlugin.ModGuid}/" +
            //     "VSwift_ui/" +
            //     "ui/partswitchpopout/requiredtechnology.uxml");

        /// <summary>
        /// Runs when the window is first created, and every time the window is re-enabled.
        /// </summary>
        private void OnEnable()
        {
            // Get the PanelRenderer component from the game object
            _window = GetComponent<PanelRenderer>();

            // Get the root element of the window.
            // Since we're cloning the UXML tree from a VisualTreeAsset, the actual root element is a TemplateContainer,
            // so we need to get the first child of the TemplateContainer to get our actual root VisualElement.
            _window.OnPanelRoot(panelRoot =>
            {
                _rootElement = panelRoot.Q<VisualElement>("window");
                _closeButton = _rootElement.Q<Button>("close-button");
                _closeButton.clicked += () => IsWindowOpen = false;
                _titleLabel = _rootElement.Q<Label>("part-name");
                _variantSelect = _rootElement.Q<ScrollView>("variant-select");
                _variantInformation = _rootElement.Q<ScrollView>("variant-information");
                _selectVariant = _rootElement.Q<Button>("select-variant");
                ResetWindow();
                IsWindowOpen = false;
            });
        }

        private void ResetWindow()
        {
            _variantSelect.Clear();
            _variantInformation.Clear();
        }

        /// <summary>
        /// Opens the popout window for the given part-switch module and variant set.
        /// </summary>
        /// <param name="partSwitch">The part-switch module the variant set belongs to.</param>
        /// <param name="variantSet">The variant set to display.</param>
        public static void ShowFor(Module_PartSwitch partSwitch, VariantSet variantSet) => Instance.ShowForInternal(partSwitch,variantSet);

        private int _variantIndex;

        private string CurrentlySelected
        {
            get => _currentPartSwitchModule!.DataPartSwitch!.ActiveVariants[_variantIndex];
            set
            {
                _currentPartSwitchModule!.SelectVariantInOab(_variantIndex, value);
            }
        }

        private VisualElement? _currentlySelectedVariantButton;
        private Action? _lastClickAction;
        private static readonly LocalizedString UnlockableLoc = "VSwift/Unlockable";
        private static readonly LocalizedString UnmetRequirementsLoc = "VSwift/UnmetRequirements";
        private static readonly LocalizedString CurrentlySelectedLoc = "VSwift/CurrentlySelected";
        private static readonly LocalizedString SelectLoc = "VSwift/Select";
        private const string CheckMark = "✓";
        private const string XMark = "×";

        private VisualElement GetButtonForVariant(Variant variant)
        {
            var localizedString = new LocalizedString(variant.VariantLocalizationKey.IsNullOrEmpty()
                ? variant.VariantId
                : variant.VariantLocalizationKey);
            var hasUnlockRequirements = variant.VariantTechs.Count > 0;
            var unlockedTechs = variant.VariantTechs.Select(x => (GetTechDisplayName(x), IsTechUnlocked(x))).ToList();
            var instance = VariantNameContainer.CloneTree().Q<VisualElement>("variant-container");
            var variantName = instance.Q<Label>("variant-name");
            var allUnlocked = unlockedTechs.All(x => x.Item2);
            variantName.text = localizedString;
            if (!hasUnlockRequirements || allUnlocked)
            {
                instance.Q<Label>("unlockability").RemoveFromHierarchy();
                instance.Q<VisualElement>("requirements").RemoveFromHierarchy();
            }
            else
            {
                var unlockability = instance.Q<Label>("unlockability");
                unlockability.text = UnmetRequirementsLoc;
                unlockability.AddToClassList("locked");
                var reqs = instance.Q<VisualElement>("requirements");
                foreach (var (techName, isUnlocked) in unlockedTechs)
                {
                    var requirementInstance = RequirementContainer.CloneTree().Q<VisualElement>("requirement");
                    var checkMark = requirementInstance.Q<Label>("is-unlocked");
                    var technology = requirementInstance.Q<Label>("technology");
                    checkMark.text = isUnlocked ? CheckMark : XMark;
                    technology.text = techName;
                    checkMark.AddToClassList(isUnlocked ? "unlocked-technology" : "locked-technology");
                    technology.AddToClassList(isUnlocked ? "unlocked-technology" : "locked-technology");
                    reqs.Add(requirementInstance);
                }
            }

            var clickable = new Clickable(() =>
            {
                if (_currentlySelectedVariantButton == instance) return;
                _currentlySelectedVariantButton?.RemoveFromClassList("selected");
                _currentlySelectedVariantButton = instance;
                instance.AddToClassList("selected");
                GenerateInformationFor(variant);
                if (variant.VariantId == CurrentlySelected)
                {
                    _selectVariant.text = CurrentlySelectedLoc;
                    if (_lastClickAction != null) _selectVariant.clicked -= _lastClickAction;
                    _lastClickAction = null;
                } else if (!allUnlocked)
                {
                    _selectVariant.text = UnmetRequirementsLoc;
                    if (_lastClickAction != null) _selectVariant.clicked -= _lastClickAction;
                    _lastClickAction = null;
                }
                else
                {
                    _selectVariant.text = SelectLoc;
                    if (_lastClickAction != null) _selectVariant.clicked -= _lastClickAction;
                    _lastClickAction = () =>
                    {
                        CurrentlySelected = variant.VariantId;
                        _selectVariant.text = CurrentlySelectedLoc;
                        if (_lastClickAction != null) _selectVariant.clicked -= _lastClickAction;
                        _lastClickAction = null;
                    };
                    _selectVariant.clicked += _lastClickAction;
                }
            });
            instance.AddManipulator(clickable);
            if (variant.VariantId != CurrentlySelected) return instance;
            _currentlySelectedVariantButton = instance;
            _currentlySelectedVariantButton!.AddToClassList("selected");
            return instance;
        }

        private static bool IsTechUnlocked(string tech)
        {
            if (!GameManager.Instance.GameModeManager.IsGameModeFeatureEnabled("SciencePoints")) return true;
            var scienceManager = GameManager.Instance.Game.ScienceManager;
            return scienceManager.IsNodeUnlocked(tech);
        }

        private static LocalizedString GetTechDisplayName(string tech) =>
            GameManager.Instance.Game
                .ScienceManager.TechNodeDataStore.AvailableData[tech]?.NameLocKey ?? "";

        private void ShowForInternal(Module_PartSwitch partSwitch, VariantSet variantSet)
        {
            ResetWindow();
            _currentPartSwitchModule = partSwitch;
            PartName = new LocalizedString($"Parts/Title/{partSwitch.OABPart.Name}");
            for (var i = 0; i < partSwitch.DataPartSwitch!.VariantSets.Count; i++)
            {
                if (ReferenceEquals(partSwitch.DataPartSwitch.VariantSets[i], variantSet))
                {
                    _variantIndex = i;
                }

                if (partSwitch.DataPartSwitch.ActiveVariants.Count <= i)
                {
                    partSwitch.DataPartSwitch.ActiveVariants.Add(partSwitch.DataPartSwitch.VariantSets[i].Variants[0].VariantId);
                }
            }

            foreach (var variant in variantSet.Variants)
            {
                _variantSelect.Add(GetButtonForVariant(variant));
            }

            GenerateInformationFor(variantSet.Variants.First(x => x.VariantId == CurrentlySelected));

            IsWindowOpen = true;
        }

        private void GenerateInformationFor(Variant selected)
        {
            _variantInformation.Clear();
            foreach (var transformer in selected.Transformers.Where(x => x.VisualizesInformation))
            {
                _variantInformation.Add(transformer.VisualizeInformation(_currentPartSwitchModule));
            }
        }
    }
}
