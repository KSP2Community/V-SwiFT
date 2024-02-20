﻿using Castle.Core.Internal;
using I2.Loc;
using KSP.Game;
using KSP.Sim.Definitions;
using KSP.UI.Binding;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VSwift.Modules.Components;
using VSwift.Modules.Data;
using VSwift.Modules.Logging;
using VSwift.Modules.Reverters;
using VSwift.Modules.Transformers;
using VSwift.Modules.Variants;

namespace VSwift.Modules.Behaviours;

// ReSharper disable once InconsistentNaming
public class Module_PartSwitch : PartBehaviourModule
{
    private class StoredState
    {
        // public readonly List<(GameObject gameObject, bool state)> OriginalTransforms = [];
        public readonly Dictionary<IReverter, object> OriginalTransformerData = [];
    }
    
    public override Type PartComponentModuleType => typeof(PartComponentModule_PartSwitch);
    private Data_PartSwitch? _dataPartSwitch;
    private StoredState? _storedState;
    
    
    public override void AddDataModules()
    {
        base.AddDataModules();
        _dataPartSwitch ??= new Data_PartSwitch();
        DataModules.TryAddUnique(_dataPartSwitch, out _dataPartSwitch);
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        IVSwiftLogger.Instance.LogInfo("Initializing!!!!");   
        if (PartBackingMode == PartBackingModes.Flight)
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
            return;
        }
        var j = 0;
        foreach (var variantSet in _dataPartSwitch!.VariantSets)
        {
            if (_dataPartSwitch!.ActiveVariants.Count <= j)
            {
                _dataPartSwitch.ActiveVariants.Add(variantSet.Variants.First().VariantId);
            }

            if (variantSet.Variants.All(v => _dataPartSwitch.ActiveVariants[j] != v.VariantId))
            {
                _dataPartSwitch.ActiveVariants[j] = variantSet.Variants.First().VariantId;
            }

            var variantSetDropdown = new ModuleProperty<string>(_dataPartSwitch.ActiveVariants[j])
            {
                ContextKey = variantSet.VariantSetId
            };
            _dataPartSwitch.AddProperty(
                LocalizationManager.GetTranslation(variantSet.VariantSetLocalizationKey.IsNullOrEmpty() ? variantSet.VariantSetId : variantSet.VariantSetLocalizationKey),
                    variantSetDropdown
                );
            var j1 = j;
            variantSetDropdown.SetValue(_dataPartSwitch.ActiveVariants[j]);
            variantSetDropdown.OnChangedValue += newVariant =>
            {
                IVSwiftLogger.Instance.LogInfo($"{OABPart.Name} switched variant to {newVariant}");
                try
                {
                    _dataPartSwitch.ActiveVariants[j1] = newVariant;
                    ApplyInOab();
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
            _dataPartSwitch.SetDropdownData(variantSetDropdown,list);
            j += 1;
            ApplyInOab();
        }
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
    
    private void ApplyVariantInFlight(Variant variant)
    {
        foreach (var transformer in variant.Transformers)
        {
            transformer.ApplyInFlight(this);
        }
    }


    private void ApplyInOab()
    {
        if (_storedState == null) StoreOriginalState();
        ResetToOriginalState();
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
    }

    private void ApplyVariantInOab(Variant variant)
    {
        foreach (var transformer in variant.Transformers)
        {
            transformer.ApplyInOab(this);
        }
    }
    private void ResetToOriginalState() {
        // IVSwiftLogger.Instance.LogInfo("ResetToOriginalState() called");
        foreach (var (instance, data) in _storedState!.OriginalTransformerData)
        {
            // IVSwiftLogger.Instance.LogInfo($"Reverting {instance} with data {data}");
            instance.Revert(this, data);
        }
    }

    private void StoreOriginalState()
    {
        _storedState = new StoredState();
        foreach (var transformer in from variantSet in _dataPartSwitch!.VariantSets
                 from variant in variantSet.Variants
                 from transformer in variant.Transformers
                 where !_storedState.OriginalTransformerData.ContainsKey(transformer.Reverter)
                 select transformer)
        {
            var reverter = transformer.Reverter;
            _storedState.OriginalTransformerData[reverter] = reverter.Store(this);
        }
    }

    private static bool AreAllTechsUnlocked(IEnumerable<string> techs)
    {
        if (!GameManager.Instance.GameModeManager.IsGameModeFeatureEnabled("SciencePoints")) return true;
        var scienceManager = GameManager.Instance.Game.ScienceManager;
        return techs.All(tech => scienceManager.IsNodeUnlocked(tech));
    }

    public override void OnShutdown()
    {
        base.OnShutdown();
        // IVSwiftLogger.Instance.LogInfo("Shutting Down");
        IsInitialized = false;
    }

    public Dictionary<string, Dictionary<string, (string savedType, JToken savedValue)>>? GetStoredVariantInformation()
    {
        return _dataPartSwitch?.GetStoredVariantInformation();
    }
}