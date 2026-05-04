using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.UserData;
using VSwift.Attributes;
using VSwift.Modules.Transformers;

namespace VSwift.UserData;

[TransformerAdapter(typeof(EngineModeSwapper))]
[MoonSharpUserData]
public class EngineModeUserData : ModesUserData
{
    public EngineModeUserData(JToken token) : base((JArray)token["Modes"])
    {
    }
}