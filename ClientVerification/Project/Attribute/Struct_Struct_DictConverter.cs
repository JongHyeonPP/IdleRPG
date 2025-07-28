using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace ClientVerification.Attribute;
public class Struct_Struct_DictConverter<T, U> : JsonConverter<Dictionary<T, U>> where T : struct where U : struct
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
            if (TryParseStruct(kvp.Key, out T parsedKey))
            {
                result[parsedKey] = kvp.Value;
            }
        }
        return result;
    }

    private bool TryParseStruct<TStruct>(string input, out TStruct result) where TStruct : struct
    {
        Type type = typeof(TStruct);

        if (type.IsEnum)
        {
            if (Enum.TryParse(type, input, ignoreCase: true, out object parsedEnum))
            {
                result = (TStruct)parsedEnum;
                return true;
            }
        }
        else if (type == typeof(int))
        {
            if (int.TryParse(input, out int parsedInt))
            {
                result = (TStruct)(object)parsedInt;
                return true;
            }
        }
        else if (type == typeof(float))
        {
            if (float.TryParse(input, out float parsedFloat))
            {
                result = (TStruct)(object)parsedFloat;
                return true;
            }
        }
        else if (type == typeof(bool))
        {
            if (bool.TryParse(input, out bool parsedBool))
            {
                result = (TStruct)(object)parsedBool;
                return true;
            }
        }
        else if (type == typeof(double))
        {
            if (double.TryParse(input, out double parsedDouble))
            {
                result = (TStruct)(object)parsedDouble;
                return true;
            }
        }
        else if (type == typeof(decimal))
        {
            if (decimal.TryParse(input, out decimal parsedDecimal))
            {
                result = (TStruct)(object)parsedDecimal;
                return true;
            }
        }

        result = default;
        return false;
    }
}
