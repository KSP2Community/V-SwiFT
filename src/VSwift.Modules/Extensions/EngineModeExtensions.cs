using KSP.Modules;
using UnityEngine;

namespace VSwift.Modules.Extensions;

public static class EngineModeExtensions
{
    public static float GetMinFuelFlow(this Data_Engine.EngineMode currentMode)
    {
        return (float)(currentMode.minThrust / (currentMode.atmosphereCurve.Evaluate(0f) * 9.80665));
    }

    public static float GetMaxFuelFlow(this Data_Engine.EngineMode currentMode)
    {
        return (float)(currentMode.maxThrust / (currentMode.atmosphereCurve.Evaluate(0f) * 9.80665));
    }
    
    public static float GetThrust(this Data_Engine.EngineMode currentMode, float atmPressure, float thrustLevel)
    {
        return Mathf.Lerp(currentMode.GetMinFuelFlow(), currentMode.GetMaxFuelFlow(), thrustLevel) * (currentMode.atmosphereCurve.Evaluate(atmPressure) * 9.80665f);
    } 
}