using KSP.IO;

namespace VSwift.Modules.Extensions;

public static class GeneralExtensions
{ 
    public static T JsonClone<T>(this T obj) => IOProvider.FromJson<T>(IOProvider.ToJson(obj));
}