using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Struct_StructTuple_DictConverter<K, T1, T2> : JsonConverter
    where K : struct
    where T1 : struct
    where T2 : struct
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Dictionary<K, (T1, T2)>);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dict = (Dictionary<K, (T1, T2)>)value;
        var serializedDict = new Dictionary<string, string[]>();

        foreach (var kvp in dict)
        {
            serializedDict[kvp.Key.ToString()] = new[] { kvp.Value.Item1.ToString(), kvp.Value.Item2.ToString() };
        }

        serializer.Serialize(writer, serializedDict);  // 리스트로 감싸지 않고 딕셔너리만 직렬화
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var serializedDict = serializer.Deserialize<Dictionary<string, string[]>>(reader);
        var result = new Dictionary<K, (T1, T2)>();

        foreach (var kvp in serializedDict)
        {
            if (TryParseStruct(kvp.Key, out K parsedK) &&
                TryParseStruct(kvp.Value[0], out T1 parsedT1) &&
                TryParseStruct(kvp.Value[1], out T2 parsedT2))
            {
                result[parsedK] = (parsedT1, parsedT2);
            }
        }

        return result;
    }

    private bool TryParseStruct<T>(string input, out T result) where T : struct
    {
        Type type = typeof(T);

        if (type.IsEnum)
        {
            if (Enum.TryParse(type, input, out object parsedEnum))
            {
                result = (T)parsedEnum;
                return true;
            }
        }
        else if (type == typeof(int))
        {
            if (int.TryParse(input, out int parsedInt))
            {
                result = (T)(object)parsedInt;
                return true;
            }
        }
        else if (type == typeof(float))
        {
            if (float.TryParse(input, out float parsedFloat))
            {
                result = (T)(object)parsedFloat;
                return true;
            }
        }
        else if (type == typeof(bool))
        {
            if (bool.TryParse(input, out bool parsedBool))
            {
                result = (T)(object)parsedBool;
                return true;
            }
        }
        else if (type == typeof(double))
        {
            if (double.TryParse(input, out double parsedDouble))
            {
                result = (T)(object)parsedDouble;
                return true;
            }
        }
        else if (type == typeof(decimal))
        {
            if (decimal.TryParse(input, out decimal parsedDecimal))
            {
                result = (T)(object)parsedDecimal;
                return true;
            }
        }

        result = default;
        return false;
    }
}
