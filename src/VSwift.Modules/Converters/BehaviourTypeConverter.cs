using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VSwift.Modules.Converters;

public class BehaviourTypeConverter : JsonConverter<Type>
{
    public override void WriteJson(JsonWriter writer, Type? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value?.Name);
    }

    public override Type? ReadJson(JsonReader reader, Type objectType, Type? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var asString = reader.ReadAsString();
        return ModulesUtilities.ComponentModules.TryGetValue(asString!, out var value) ? value.behaviour : existingValue;
    }
}