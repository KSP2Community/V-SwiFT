﻿using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers;

public interface ITransformer
{
    // public object StoreOriginalState(Module_PartSwitch partSwitchModule);
    //
    // public void ResetToOriginalState(Module_PartSwitch partSwitchModule, object originalState);

    public IReverter? Reverter { get; }

    public bool SavesInformation { get; }

    public bool VisualizesInformation { get; }

    public void ApplyInFlight(Module_PartSwitch partSwitch);
    public void ApplyInOab(Module_PartSwitch partSwitch);
    public void ApplyCommon(Module_PartSwitch partSwitch);

    public (Type savedType, JToken savedValue) SaveInformation()
    {
        throw new NotImplementedException();
    }

    public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
    {
        throw new NotImplementedException();
    }
}