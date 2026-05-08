using VSwift.Modules.Behaviours;

namespace VSwift.Modules.Reverters
{
    /// <summary>
    /// Interface for V-SwiFT reverters, the per-transformer hooks that undo a transformer's effect when the active variant changes.
    /// </summary>
    /// <remarks>
    /// A reverter is paired with one (or more) <see cref="Transformers.ITransformer" /> implementation via <see cref="Transformers.ITransformer.Reverter" />. Before a variant is applied, V-SwiFT calls <see cref="Store" /> on every reverter to snapshot the original state; on swap, <see cref="Revert" /> restores from that snapshot. Stateless or additive transformers return <c>null</c> instead of an instance.
    /// </remarks>
    public interface IReverter
    {
        /// <summary>
        /// Captures the original state to restore later, called once before any variant is applied.
        /// </summary>
        /// <param name="partSwitch">The part-switch module to read state from.</param>
        /// <returns>An opaque snapshot, or <c>null</c> when no per-instance state is needed.</returns>
        public object? Store(Module_PartSwitch partSwitch);

        /// <summary>
        /// Restores the original state from <paramref name="data" />, called when the active variant is being swapped.
        /// </summary>
        /// <param name="partSwitch">The part-switch module to apply the revert against.</param>
        /// <param name="data">The snapshot previously returned by <see cref="Store" />, or <c>null</c> when no snapshot was taken.</param>
        /// <param name="isStartingReset">True when this revert is part of the initial application before any variant has been chosen, false when it is a true swap; reverters that should only fire on actual swaps gate on this.</param>
        public void Revert(Module_PartSwitch partSwitch, object? data, bool isStartingReset);

        /// <summary>
        /// Gets whether this reverter only runs when at least one transformer in the swapping variant set references it (true), or runs unconditionally on every swap (false).
        /// </summary>
        public bool RequiresInVariantSet { get; }

        /// <summary>
        /// Gets whether this reverter is safe and necessary to run during in-flight initialization.
        /// </summary>
        /// <remarks>
        /// In flight, the part prefab loads fresh, so transformers whose effects are persisted via <see cref="Transformers.ITransformer.SaveInformation" /> do not need their reverters re-run. Reverters that operate on the live <see cref="UnityEngine.GameObject" /> (transform activation, material state) must run in flight to undo prefab-default state that conflicts with the active variant. True if the reverter operates on prefab-loaded GameObject state and must fire during flight init, false otherwise.
        /// </remarks>
        public bool AppliesInFlight { get; }
    }
}
