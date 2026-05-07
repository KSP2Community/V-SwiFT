using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redux;
using I2.Loc;
using KSP.Game;
using KSP.Messages;
using KSP.Modules;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.ResourceSystem;
using KSP.UI.Binding;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VSwift.Modules.Components;
using VSwift.Modules.Data;
using VSwift.Modules.Extensions;
using VSwift.Modules.Logging;
using VSwift.Modules.Reverters;
using VSwift.Modules.Transformers;
using VSwift.Modules.UI;
using VSwift.Modules.Variants;

namespace VSwift.Modules.Behaviours
{
    /// <summary>
    /// Part-behaviour module that applies the active <see cref="Data_PartSwitch" /> variants to a part, handling OAB UI generation, in-flight application, and per-variant state revert.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class Module_PartSwitch : PartBehaviourModule
    {
        private class StoredState
        {
            // public readonly List<(GameObject gameObject, bool state)> OriginalTransforms = [];
            public readonly Dictionary<IReverter, object?> OriginalTransformerData = new() { };
        }

        /// <inheritdoc />
        public override Type PartComponentModuleType => typeof(PartComponentModule_PartSwitch);
        private Data_PartSwitch? _dataPartSwitch;
        private StoredState? _storedState;

        /// <summary>
        /// Gets the <see cref="Data_PartSwitch" /> module data this behaviour drives.
        /// </summary>
        public Data_PartSwitch? DataPartSwitch => _dataPartSwitch;

        /// <inheritdoc />
        protected override void AddDataModules()
        {
            base.AddDataModules();
            _dataPartSwitch ??= new Data_PartSwitch();
            DataModules.TryAddUnique(_dataPartSwitch, out _dataPartSwitch);
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (PartBackingMode == PartBackingModes.Flight)
                HandleInFlightInitialization();
            else
                HandleInOabInitialization();
        }

        private void HandleInOabInitialization()
        {
            _dataPartSwitch!.VariantSets.Aggregate(0, HandleVariantSetInOab);
            foreach (var predefinedNode in _dataPartSwitch.PredefinedDynamicNodes.Where(predefinedNode => OABPart.FindNodeWithTag(predefinedNode.nodeID) == null))
            {
                OABPart.AddDynamicNode(OABPart, new ObjectAssemblyAvailablePartNode(
                    predefinedNode.size,
                    predefinedNode.position,
                    Quaternion.LookRotation(predefinedNode.orientation, Vector3.up),
                    predefinedNode.nodeID,
                    null,
                    predefinedNode.size,
                    AttachNodeType.Stack,
                    true
                ));
            }
            ApplyInOab(true);
        }

        private int HandleVariantSetInOab(int j, VariantSet variantSet)
        {
            if (_dataPartSwitch!.ActiveVariants.Count <= j)
            {
                _dataPartSwitch.ActiveVariants.Add(variantSet.Variants.First().VariantId);
            }

            if (variantSet.Variants.All(v => _dataPartSwitch.ActiveVariants[j] != v.VariantId))
            {
                _dataPartSwitch.ActiveVariants[j] = variantSet.Variants.First().VariantId;
            }

            if (variantSet.IsPopout)
            {
                GenerateVariantSetButton(variantSet);
            }
            else
            {
                GenerateVariantSetDropdown(j, variantSet);
            }

            j += 1;
            return j;
        }

        private void GenerateVariantSetButton(VariantSet variantSet)
        {
            var moduleAction = new ModuleAction((Action)ShowUI);
            DataPartSwitch!.AddAction(LocalizationManager.GetTranslation(
                    variantSet.VariantSetLocalizationKey.IsNullOrEmpty()
                        ? variantSet.VariantSetId
                        : variantSet.VariantSetLocalizationKey),
                moduleAction);
            return;

            void ShowUI()
            {
                IVSwiftUI.Instance.ShowUIFor(this, variantSet);
            }
        }

        private void GenerateVariantSetDropdown(int j, VariantSet variantSet)
        {
            var variantSetDropdown = new ModuleProperty<string>(_dataPartSwitch!.ActiveVariants[j])
            {
                ContextKey = variantSet.VariantSetId
            };
            _dataPartSwitch.AddProperty(
                LocalizationManager.GetTranslation(variantSet.VariantSetLocalizationKey.IsNullOrEmpty()
                    ? variantSet.VariantSetId
                    : variantSet.VariantSetLocalizationKey),
                variantSetDropdown
            );
            variantSetDropdown.SetValue(_dataPartSwitch.ActiveVariants[j]);
            variantSetDropdown.OnChangedValue += newVariant =>
            {
                IVSwiftLogger.Instance.LogInfo($"{OABPart.Name} switched variant to {newVariant}");
                try
                {
                    _dataPartSwitch.ActiveVariants[j] = newVariant;
                    ApplyInOab(false,variantSet);
                    QueuePamUpdate();
                }
                catch (Exception e)
                {
                    IVSwiftLogger.Instance.LogError(e);
                }
            };
            var list = new DropdownItemList();
            foreach (var variant in variantSet.Variants)
            {
                if (!AreAllTechsUnlocked(variant.VariantTechs)) continue;
                list.Add(variant.VariantId, new DropdownItem
                {
                    key = variant.VariantId,
                    text = LocalizationManager.GetTranslation(variant.VariantLocalizationKey.IsNullOrEmpty()
                        ? variant.VariantId
                        : variant.VariantLocalizationKey)
                });
            }

            _dataPartSwitch.SetDropdownData(variantSetDropdown, list);
        }

        private void HandleInFlightInitialization()
        {
            var i = 0;
            foreach (var variant in _dataPartSwitch!.VariantSets)
            {
                if (_dataPartSwitch!.ActiveVariants.Count <= i)
                {
                    _dataPartSwitch.ActiveVariants.Add(variant.Variants.First().VariantId);
                }

                if (variant.Variants.All(v => _dataPartSwitch.ActiveVariants[i] != v.VariantId))
                {
                    _dataPartSwitch.ActiveVariants[i] = variant.Variants.First().VariantId;
                }

                i += 1;
            }

            ApplyInFlight();
        }

        private void ApplyCommon()
        {
            var i = 0;
            foreach (var variantSet in _dataPartSwitch!.VariantSets)
            {
                if (_dataPartSwitch.ActiveVariants.Count <= i)
                {
                    _dataPartSwitch.ActiveVariants.Add(variantSet.Variants.First().VariantId);
                }

                if (variantSet.Variants.All(variant => _dataPartSwitch.ActiveVariants[i] != variant.VariantId))
                {
                    _dataPartSwitch.ActiveVariants[i] = variantSet.Variants.First().VariantId;
                }

                ApplyVariantCommon(variantSet.Variants.First(variant =>
                    _dataPartSwitch.ActiveVariants[i] == variant.VariantId));
                i++;
            }
        }

        private void ApplyVariantCommon(Variant variant)
        {
            foreach (var transformer in variant.Transformers)
            {
                transformer.ApplyCommon(this);
            }
        }

        private void ApplyInFlight()
        {
            ResetFlightVisualState();
            ApplyCommon();
            var i = 0;
            foreach (var variantSet in _dataPartSwitch!.VariantSets)
            {
                if (_dataPartSwitch.ActiveVariants.Count <= i)
                {
                    _dataPartSwitch.ActiveVariants.Add(variantSet.Variants.First().VariantId);
                }

                if (variantSet.Variants.All(variant => _dataPartSwitch.ActiveVariants[i] != variant.VariantId))
                {
                    _dataPartSwitch.ActiveVariants[i] = variantSet.Variants.First().VariantId;
                }

                ApplyVariantInFlight(variantSet.Variants.First(variant =>
                    _dataPartSwitch.ActiveVariants[i] == variant.VariantId));
                i++;
            }
        }

        private void ResetFlightVisualState()
        {
            if (_storedState == null)
            {
                _storedState = new StoredState();
                var transformers = _dataPartSwitch!.VariantSets
                    .SelectMany(variantSet => variantSet.Variants)
                    .SelectMany(variant => variant.Transformers)
                    .Where(transformer => transformer.Reverter is { AppliesInFlight: true }
                        && !_storedState.OriginalTransformerData.ContainsKey(transformer.Reverter));
                foreach (var transformer in transformers)
                {
                    var reverter = transformer.Reverter!;
                    _storedState.OriginalTransformerData[reverter] = reverter.Store(this);
                }
            }
            foreach (var (instance, data) in _storedState.OriginalTransformerData)
            {
                instance.Revert(this, data, true);
            }
        }

        /// <summary>
        /// Queues a coroutine that opens the parts manager and scrolls to this part at end-of-frame, used after a variant switch changes the visible PAM rows.
        /// </summary>
        public void QueuePamUpdate()
        {
            if (gameObject == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                return;
            }

            StartCoroutine(UpdatePam());
        }

        private IEnumerator UpdatePam()
        {
            yield return new WaitForEndOfFrame();
            if (gameObject == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                yield break;
            }

            var objectAssemblyPart = (ObjectAssemblyPart)OABPart;
            Game.PartsManager.IsVisible = true;
            Game.PartsManager.PartsList.ScrollToPart(objectAssemblyPart.GlobalId);
            Game.Messages.Publish<PartManagerOpenedMessage>();
        }

        /// <summary>
        /// Queues a coroutine that refreshes the part's color application after a half-second delay; falls back to immediate refresh when the part is not active.
        /// </summary>
        public void QueueUpdateColors()
        {
            if (gameObject == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                RefreshColors();
                return;
            }

            StartCoroutine(UpdateColors());
        }

        private IEnumerator UpdateColors()
        {
            yield return new WaitForSeconds(0.5f);
            RefreshColors();
        }

        private void RefreshColors()
        {
            if (PartBackingMode == PartBackingModes.OAB)
            {
                if (OABPart != null && OABPart.TryGetModule(out Module_Color moduleColor))
                {
                    moduleColor.RefreshColors();
                }
            }
            else
            {
                if (part != null && part.GetModule<Module_Color>() is { } moduleColor)
                {
                    moduleColor.RefreshColors();
                }
            }
        }

        private void ApplyVariantInFlight(Variant variant)
        {
            foreach (var transformer in variant.Transformers)
            {
                transformer.ApplyInFlight(this);
            }
        }


        /// <summary>
        /// Reverts to the original state, then applies every active variant's transformers, used when initializing the OAB part or after a variant swap.
        /// </summary>
        /// <param name="isStarting">Whether this is the initial OAB application (true) or a swap (false).</param>
        /// <param name="swapped">The variant set whose variants were swapped, or <c>null</c> when applying every variant set.</param>
        public void ApplyInOab(bool isStarting,VariantSet? swapped=null)
        {
            if (_storedState == null) StoreOriginalState();
            var savedStoredUnits = SnapshotContainerStoredUnits();
            ResetToOriginalState(isStarting,swapped);
            ApplyCommon();
            var i = 0;
            foreach (var variantSet in _dataPartSwitch!.VariantSets)
            {
                if (_dataPartSwitch.ActiveVariants.Count <= i)
                {
                    _dataPartSwitch.ActiveVariants.Add(variantSet.Variants.First().VariantId);
                }

                ApplyVariantInOab(variantSet.Variants.First(variant =>
                    _dataPartSwitch.ActiveVariants[i] == variant.VariantId));
                i++;
            }
            RestoreContainerStoredUnits(savedStoredUnits);
            (OABPart as ObjectAssemblyPart)?.UpdateMassValues();
        }

        private Dictionary<string, double> SnapshotContainerStoredUnits()
        {
            var snapshot = new Dictionary<string, double>();
            if (OABPart is not ObjectAssemblyPart { Containers: { } containers }) return snapshot;
            var database = GameManager.Instance.Game.ResourceDefinitionDatabase;
            foreach (var container in containers)
            {
                foreach (var resourceID in container)
                {
                    snapshot[database.GetDefinitionData(resourceID).name] = container.GetResourceStoredUnits(resourceID);
                }
            }
            return snapshot;
        }

        private void RestoreContainerStoredUnits(Dictionary<string, double> snapshot)
        {
            if (snapshot.Count == 0) return;
            if (OABPart is not ObjectAssemblyPart { Containers: { } containers }) return;
            var database = GameManager.Instance.Game.ResourceDefinitionDatabase;
            foreach (var container in containers)
            {
                foreach (var resourceID in container)
                {
                    if (snapshot.TryGetValue(database.GetDefinitionData(resourceID).name, out var stored))
                    {
                        container.SetResourceStoredUnits(resourceID, stored);
                    }
                }
            }
        }

        private void ApplyVariantInOab(Variant variant)
        {
            foreach (var transformer in variant.Transformers)
            {
                transformer.ApplyInOab(this);
            }
        }

        private void ResetToOriginalState(bool isStarting=false,VariantSet? swapped=null)
        {
            _dataPartSwitch!.MassModifier = 0.0f;
            // IVSwiftLogger.Instance.LogInfo("ResetToOriginalState() called");
            foreach (var (instance, data) in _storedState!.OriginalTransformerData)
            {
                // IVSwiftLogger.Instance.LogInfo($"Reverting {instance} with data {data}");
                if (!instance.RequiresInVariantSet || isStarting ||
                    (swapped != null && swapped.Variants.Any(x => x.Transformers.Any(y => y.Reverter == instance))))
                    instance.Revert(this, data, isStarting);
            }
        }

        private void StoreOriginalState()
        {
            _storedState = new StoredState();
            var transformers = _dataPartSwitch!.VariantSets
                .SelectMany(variantSet => variantSet.Variants)
                .SelectMany(variant => variant.Transformers)
                .Where(transformer => transformer.Reverter != null
                    && !_storedState.OriginalTransformerData.ContainsKey(transformer.Reverter));
            foreach (var transformer in transformers)
            {
                var reverter = transformer.Reverter;
                _storedState.OriginalTransformerData[reverter!] = reverter!.Store(this);
            }
        }

        private static bool AreAllTechsUnlocked(IEnumerable<string> techs)
        {
            if (!GameManager.Instance.GameModeManager.IsGameModeFeatureEnabled("SciencePoints")) return true;
            var scienceManager = GameManager.Instance.Game.ScienceManager;
            return techs.All(tech => scienceManager.IsNodeUnlocked(tech));
        }

        /// <inheritdoc />
        protected override void OnShutdown()
        {
            base.OnShutdown();
            // IVSwiftLogger.Instance.LogInfo("Shutting Down");
            IsInitialized = false;
        }

        /// <summary>
        /// Returns the per-variant transformer-saved data from <see cref="DataPartSwitch" />, or <c>null</c> when no module data is attached.
        /// </summary>
        /// <returns>The saved data, or <c>null</c>.</returns>
        public Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>>? GetStoredVariantInformation()
        {
            return _dataPartSwitch?.GetStoredVariantInformation();
        }
    }
}
