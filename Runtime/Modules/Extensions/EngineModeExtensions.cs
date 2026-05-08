using KSP.Modules;
using UnityEngine;

namespace VSwift.Modules.Extensions
{
    /// <summary>
    /// V-SwiFT extension methods on <see cref="Data_Engine.EngineMode" />.
    /// </summary>
    public static class EngineModeExtensions
    {
        /// <summary>
        /// Returns the engine mode's minimum fuel flow rate at sea-level atmosphere.
        /// </summary>
        /// <param name="currentMode">The engine mode.</param>
        /// <returns>The minimum fuel flow.</returns>
        public static float GetMinFuelFlow(this Data_Engine.EngineMode currentMode)
        {
            return (float)(currentMode.minThrust / (currentMode.atmosphereCurve.Evaluate(0f) * PhysicsSettings.STANDARD_GRAVITY_EARTH));
        }

        /// <summary>
        /// Returns the engine mode's maximum fuel flow rate at sea-level atmosphere.
        /// </summary>
        /// <param name="currentMode">The engine mode.</param>
        /// <returns>The maximum fuel flow.</returns>
        public static float GetMaxFuelFlow(this Data_Engine.EngineMode currentMode)
        {
            return (float)(currentMode.maxThrust / (currentMode.atmosphereCurve.Evaluate(0f) * PhysicsSettings.STANDARD_GRAVITY_EARTH));
        }

        /// <summary>
        /// Returns the engine mode's thrust at the given atmospheric pressure and throttle level.
        /// </summary>
        /// <param name="currentMode">The engine mode.</param>
        /// <param name="atmPressure">The atmospheric pressure.</param>
        /// <param name="thrustLevel">The throttle level in the range [0, 1].</param>
        /// <returns>The thrust at the given pressure and throttle.</returns>
        public static float GetThrust(this Data_Engine.EngineMode currentMode, float atmPressure, float thrustLevel)
        {
            return Mathf.Lerp(currentMode.GetMinFuelFlow(), currentMode.GetMaxFuelFlow(), thrustLevel) * (currentMode.atmosphereCurve.Evaluate(atmPressure) * (float)PhysicsSettings.STANDARD_GRAVITY_EARTH);
        }
    }
}
