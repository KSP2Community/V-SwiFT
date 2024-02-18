using Castle.Core.Internal;
using I2.Loc;
using KSP.Game;
using KSP.Sim.Definitions;
using KSP.UI.Binding;
using UnityEngine;
using VSwift.Modules.Components;
using VSwift.Modules.Data;
using VSwift.Modules.Logging;
using VSwift.Modules.Transformers;
using VSwift.Modules.Variants;

namespace VSwift.Modules.Behaviours;

// ReSharper disable once InconsistentNaming
public class Module_PartSwitch : PartBehaviourModule
{
    private class StoredState
    {
        // public readonly List<(GameObject gameObject, bool state)> OriginalTransforms = [];
        public readonly Dictionary<Type, (ITransformer instance, object data)> OriginalTransformerData = [];
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
                IVSwiftLogger.Instance.LogInfo($"Switched variant to {newVariant}");
                try
                {
                    _dataPartSwitch.ActiveVariants[j1] = newVariant;
                    ResetToOriginalState();
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
    }


    private void ApplyInOab()
    {
        if (_storedState == null) StoreOriginalState();
        ApplyCommon();
    }

    private void ResetToOriginalState() {
        foreach (var (key, (instance, data)) in _storedState!.OriginalTransformerData)
        {
            instance.ResetToOriginalState(this, data);
        }
    }

    private void StoreOriginalState()
    {
        _storedState = new StoredState();
        foreach (var transformer in from variantSet in _dataPartSwitch!.VariantSets
                 from variant in variantSet.Variants
                 from transformer in variant.Transformers
                 where !_storedState.OriginalTransformerData.ContainsKey(transformer.GetType())
                 select transformer)
        {
            _storedState.OriginalTransformerData[transformer.GetType()] =
                (transformer, transformer.StoreOriginalState(this));
        }
    }

    private static bool AreAllTechsUnlocked(List<string> techs)
    {
        if (!GameManager.Instance.GameModeManager.IsGameModeFeatureEnabled("SciencePoints")) return true;
        var scienceManager = GameManager.Instance.Game.ScienceManager;
        return techs.All(tech => scienceManager.IsNodeUnlocked(tech));
    }
}