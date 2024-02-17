using Castle.Core.Internal;
using I2.Loc;
using KSP.Sim.Definitions;
using KSP.UI.Binding;
using UnityEngine;
using VSwift.Modules.Components;
using VSwift.Modules.Data;
using VSwift.Modules.Logging;
using VSwift.Modules.Variants;

namespace VSwift.Modules.Behaviours;

// ReSharper disable once InconsistentNaming
public class Module_PartSwitch : PartBehaviourModule
{
    private class StoredState
    {
        public List<(GameObject gameObject, bool state)> OriginalTransforms = [];
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
            ApplyVariantCommon(variantSet.Variants.First(variant =>
                _dataPartSwitch.ActiveVariants[i] == variant.VariantId));
            i++;
        }
    }
    
    private void ApplyVariantCommon(Variant variant)
    {
        foreach (var activatedTransform in variant.Transforms)
        {
            var t = gameObject.transform.FindChildRecursive(activatedTransform);
            if (ReferenceEquals(t,null) || t == null)
            {
                IVSwiftLogger.Instance.LogError($"Could not find child of {name} with name {activatedTransform}");
                continue;
            }
            t.gameObject.SetActive(true);
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
        foreach (var (go, state) in _storedState!.OriginalTransforms)
        {
            go.SetActive(state);
        }
    }

    private void StoreOriginalState()
    {
        _storedState = new StoredState();
        RecursivelyStoreChildren(gameObject);
    }

    private void RecursivelyStoreChildren(GameObject go)
    {
        foreach (Transform child in go.transform)
        {
            var o = child.gameObject;
            _storedState!.OriginalTransforms.Add((o, o.activeSelf));
            RecursivelyStoreChildren(child.gameObject);
        }
    }
}