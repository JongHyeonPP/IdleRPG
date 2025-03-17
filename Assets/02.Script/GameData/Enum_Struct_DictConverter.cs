using Newtonsoft.Json;
using System.Collections.Generic;
using System;

public class Enum_Struct_DictConverter<T, U> : JsonConverter<Dictionary<T, U>> where T : Enum where U : struct
{
    public override void WriteJson(JsonWriter writer, Dictionary<T, U> value, JsonSerializer serializer)
    {
        var dictionary = new Dictionary<string, U>();
        foreach (var kvp in value)
        {
            dictionary[kvp.Key.ToString()] = kvp.Value;
        }
        serializer.Serialize(writer, dictionary);
    }

    public override Dictionary<T, U> ReadJson(JsonReader reader, Type objectType, Dictionary<T, U> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var dictionary = serializer.Deserialize<Dictionary<string, U>>(reader);
        var result = new Dictionary<T, U>();

        foreach (var kvp in dictionary)
        {
            if (Enum.TryParse(typeof(T), kvp.Key, ignoreCase: true, out var parsedEnum))
            {
                result[(T)parsedEnum] = kvp.Value;
            }
        }
        return result;
    }
}
