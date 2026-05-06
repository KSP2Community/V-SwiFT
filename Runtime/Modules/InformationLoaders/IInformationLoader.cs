using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;

namespace VSwift.Modules.InformationLoaders
{
    /// <summary>
    /// Interface for V-SwiFT information loaders, which re-apply per-variant transformer state to a freshly-loaded part.
    /// </summary>
    /// <remarks>
    /// When a transformer that <see cref="Transformers.ITransformer.SavesInformation" /> is active, V-SwiFT writes its <see cref="Transformers.ITransformer.SaveInformation" /> output onto the serialized part. On load, the saved loader type is instantiated and <see cref="LoadInformationInto" /> reapplies the saved JSON to the part data so the loaded part matches the active variant without re-running the transformer.
    /// </remarks>
    public interface IInformationLoader
    {
        /// <summary>
        /// Applies the saved per-variant state to the freshly-loaded part data.
        /// </summary>
        /// <param name="partData">The part data to mutate in place.</param>
        /// <param name="storedInformation">The saved JSON previously produced by the paired transformer's <see cref="Transformers.ITransformer.SaveInformation" />.</param>
        public void LoadInformationInto(PartData partData, JToken storedInformation);
    }
}
