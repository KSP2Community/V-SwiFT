using KSP.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VSwift.Modules.Extensions;

public static class GeneralExtensions
{ 
    public static T JsonClone<T>(this T obj) => IOProvider.FromJson<T>(IOProvider.ToJson(obj));

    public static JToken ToJToken<T>(this T obj) => JToken.Parse(IOProvider.ToJson(obj));

    public static T FromJToken<T>(this JToken obj) => IOProvider.FromJson<T>(obj.ToString(Formatting.None));
}