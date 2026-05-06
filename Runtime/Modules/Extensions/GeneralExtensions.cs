using KSP.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VSwift.Modules.Extensions
{
    /// <summary>
    /// General-purpose extension methods used across V-SwiFT modules.
    /// </summary>
    public static class GeneralExtensions
    {
        /// <summary>
        /// Returns a deep clone of the given object via JSON round-trip.
        /// </summary>
        /// <param name="obj">The object to clone.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>A deep clone of the object.</returns>
        public static T JsonClone<T>(this T obj) => IOProvider.FromJson<T>(IOProvider.ToJson(obj));

        /// <summary>
        /// Returns a <see cref="JToken" /> representation of the given object via JSON round-trip.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>A <see cref="JToken" /> representing the object.</returns>
        public static JToken ToJToken<T>(this T obj) => JToken.Parse(IOProvider.ToJson(obj));

        /// <summary>
        /// Deserializes the given <see cref="JToken" /> into a value of type <typeparamref name="T" />.
        /// </summary>
        /// <param name="obj">The token to deserialize.</param>
        /// <typeparam name="T">The target type.</typeparam>
        /// <returns>The deserialized value.</returns>
        public static T FromJToken<T>(this JToken obj) => IOProvider.FromJson<T>(obj.ToString(Formatting.None));
    }
}
