using JetBrains.Annotations;
using KSP.Sim.Definitions;
using VSwift.Modules.Data;

namespace VSwift.Extensions
{
    /// <summary>
    /// V-SwiFT extension methods on <see cref="PartDefinition" />.
    /// </summary>
    public static class PartDefinitionExtensions
    {
        /// <summary>
        /// Returns the active-variant identifier string for the part's <see cref="Data_PartSwitch" /> module data, joining the variant IDs of all variant sets with <c>+</c>.
        /// </summary>
        /// <param name="part">The part to read from.</param>
        /// <returns>The joined active-variant identifier, or <c>null</c> when the part has no <see cref="Data_PartSwitch" /> module data.</returns>
        [CanBeNull]
        public static string GetCurrentVariantNameString(this PartDefinition part)
        {

            foreach (var module in part.Modules)
            {
                foreach (var datum in module.ModuleData)
                {
                    if (datum.DataObject is Data_PartSwitch dataPartSwitch)
                    {
                        return string.Join('+', dataPartSwitch.ActiveVariants);
                    }
                }
            }
            return null;
        }
    }
}
