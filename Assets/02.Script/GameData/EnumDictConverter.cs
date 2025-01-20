using EnumCollection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

//Json Convert 과정에서 특정 타입을 변환할 수 있도록 어트리뷰트 정의
public class EnumDictConverter<T> : JsonConverter<Dictionary<T, int>> where T : Enum
{
    public override void WriteJson(JsonWriter writer, Dictionary<T, int> value, JsonSerializer serializer)
    {
        var dictionary = new Dictionary<string, int>();
        foreach (var kvp in value)
        {
            dictionary[kvp.Key.ToString()] = kvp.Value;
        }
        serializer.Serialize(writer, dictionary);
    }

    public override Dictionary<T, int> ReadJson(JsonReader reader, Type objectType, Dictionary<T, int> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var dictionary = serializer.Deserialize<Dictionary<string, int>>(reader);
        var result = new Dictionary<T, int>();

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
