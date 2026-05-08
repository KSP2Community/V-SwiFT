using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace VSwift.UserData;

/// <summary>
/// Indexed-list wrapper for an attach-node array (such as a part-switch's <c>PredefinedDynamicNodes</c>), keyed by each entry's <c>nodeID</c>.
/// </summary>
[MoonSharpUserData]
public class NodesUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper around an attach-node array.
    /// </summary>
    /// <param name="token">The attach-node JSON array.</param>
    public NodesUserData(JArray token) : base(token)
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return source["nodeID"].Value<string>();
    }
}
