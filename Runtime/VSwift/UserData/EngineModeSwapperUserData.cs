using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.UserData;
using VSwift.Attributes;
using VSwift.Modules.Transformers;

namespace VSwift.UserData;

/// <summary>
/// Transformer-adapter wrapper for <see cref="EngineModeSwapper" />, exposing the transformer's <c>Modes</c> array as a typed <see cref="ModesUserData" />.
/// </summary>
[TransformerAdapter(typeof(EngineModeSwapper))]
[MoonSharpUserData]
public class EngineModeSwapperUserData : ModesUserData
{
    /// <summary>
    /// Creates the wrapper around an <see cref="EngineModeSwapper" /> transformer's <c>Modes</c> array.
    /// </summary>
    /// <param name="token">The transformer JSON token.</param>
    public EngineModeSwapperUserData(JToken token) : base((JArray)token["Modes"])
    {
    }
}
