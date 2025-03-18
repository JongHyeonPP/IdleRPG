using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class Struct_StructTuple_DictArrConverter<K, T1, T2> : JsonConverter
    where K : struct
    where T1 : struct
    where T2 : struct
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Dictionary<K, (T1, T2)>[]);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var array = (Dictionary<K, (T1, T2)>[])value;
        var serializedArray = new List<Dictionary<string, string[]>>();

        foreach (var dict in array)
        {
            var serializedDict = new Dictionary<string, string[]>();
            foreach (var kvp in dict)
            {
                serializedDict[kvp.Key.ToString()] = new[] { kvp.Value.Item1.ToString(), kvp.Value.Item2.ToString() };
            }
            serializedArray.Add(serializedDict);
        }

        serializer.Serialize(writer, serializedArray);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var arrayData = serializer.Deserialize<List<Dictionary<string, string[]>>>(reader);
        var result = new Dictionary<K, (T1, T2)>[arrayData.Count];

        for (int i = 0; i < arrayData.Count; i++)
        {
            var dict = new Dictionary<K, (T1, T2)>();
            foreach (var kvp in arrayData[i])
            {
                if (TryParseStruct(kvp.Key, out K parsedK) &&
                    TryParseStruct(kvp.Value[0], out T1 parsedT1) &&
                    TryParseStruct(kvp.Value[1], out T2 parsedT2))
                {
                    dict[parsedK] = (parsedT1, parsedT2);
                }
            }
            result[i] = dict;
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