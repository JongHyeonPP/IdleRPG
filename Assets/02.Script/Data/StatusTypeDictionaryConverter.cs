using EnumCollection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

//Json Convert 과정에서 특정 타입을 변환할 수 있도록 어트리뷰트 정의
public class StatusTypeDictionaryConverter : JsonConverter<Dictionary<StatusType, int>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<StatusType, int> value, JsonSerializer serializer)
    {
        var dictionary = new Dictionary<string, int>();
        foreach (var kvp in value)
        {
            dictionary[kvp.Key.ToString()] = kvp.Value;
        }
        serializer.Serialize(writer, dictionary);
    }

    public override Dictionary<StatusType, int> ReadJson(JsonReader reader, Type objectType, Dictionary<StatusType, int> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var dictionary = serializer.Deserialize<Dictionary<string, int>>(reader);
        var result = new Dictionary<StatusType, int>();

        foreach (var kvp in dictionary)
        {
            if (Enum.TryParse(kvp.Key, out StatusType statusType))
            {
                result[statusType] = kvp.Value;
            }
        }
        return result;
    }
}
