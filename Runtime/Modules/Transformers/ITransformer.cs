using System;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;
using VSwift.Modules.Behaviours;
using VSwift.Modules.Reverters;

namespace VSwift.Modules.Transformers
{
    /// <summary>
    /// Interface for V-SwiFT transformers, the per-variant operations that mutate a part when a variant is active.
    /// </summary>
    /// <remarks>
    /// Concrete implementations decorate themselves with <see cref="Transformer" /> to register a short name in the <see cref="VSwift.Utilities.Transformers" /> registry. <see cref="Reverter" /> describes how to undo this transformer's effect when the variant changes; transformers that don't need explicit reversal (e.g. pure visualizers, additive scalar mass) return <c>null</c>.
    /// </remarks>
    public interface ITransformer
    {
        // public object StoreOriginalState(Module_PartSwitch partSwitchModule);
        //
        // public void ResetToOriginalState(Module_PartSwitch partSwitchModule, object originalState);

        /// <summary>
        /// Gets the reverter that undoes this transformer's effect when the active variant changes, or <c>null</c> when the transformer's effect is additive or stateless.
        /// </summary>
        public IReverter? Reverter { get; }

        /// <summary>
        /// Gets whether this transformer persists per-variant state into the saved part data via <see cref="SaveInformation" />.
        /// </summary>
        public bool SavesInformation { get; }

        /// <summary>
        /// Gets whether this transformer contributes a visual element to the variant-info popout via <see cref="VisualizeInformation" />.
        /// </summary>
        public bool VisualizesInformation { get; }

        /// <summary>
        /// Applies this transformer's effect to the given part-switch module in flight.
        /// </summary>
        /// <param name="partSwitch">The part-switch module to apply against.</param>
        public void ApplyInFlight(Module_PartSwitch partSwitch);

        /// <summary>
        /// Applies this transformer's effect to the given part-switch module in the OAB.
        /// </summary>
        /// <param name="partSwitch">The part-switch module to apply against.</param>
        public void ApplyInOab(Module_PartSwitch partSwitch);

        /// <summary>
        /// Applies this transformer's effect to the given part-switch module in both flight and OAB.
        /// </summary>
        /// <param name="partSwitch">The part-switch module to apply against.</param>
        public void ApplyCommon(Module_PartSwitch partSwitch);

        /// <summary>
        /// Returns the serialized state to persist for this transformer when the variant is active. Only called when <see cref="SavesInformation" /> is true.
        /// </summary>
        /// <returns>The information loader type that re-applies the saved value, paired with the saved value itself.</returns>
        public (Type savedType, JToken savedValue) SaveInformation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the visual element rendered for this transformer in the variant-info popout. Only called when <see cref="VisualizesInformation" /> is true.
        /// </summary>
        /// <param name="modulePartSwitch">The part-switch module being visualized.</param>
        /// <returns>The visual element, or <c>null</c> when the transformer has nothing to render for the current state.</returns>
        public VisualElement? VisualizeInformation(Module_PartSwitch modulePartSwitch)
        {
            throw new NotImplementedException();
        }
    }
}
