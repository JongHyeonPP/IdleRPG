using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class TupleArr_Converter<T1, T2> : JsonConverter<(T1, T2)[]> 
    where T1 : struct
    where T2 : struct
{
    public override void WriteJson(JsonWriter writer, (T1, T2)[] value, JsonSerializer serializer)
    {
        var list = new List<string[]>();
        foreach (var tuple in value)
        {
            list.Add(new[] { tuple.Item1.ToString(), tuple.Item2.ToString() });
        }
        serializer.Serialize(writer, list);
    }

    public override (T1, T2)[] ReadJson(JsonReader reader, Type objectType, (T1, T2)[] existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var list = serializer.Deserialize<List<string[]>>(reader);
        var result = new (T1, T2)[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Length == 2 &&
                TryParseStruct(list[i][0], out T1 parsedT1) &&
                TryParseStruct(list[i][1], out T2 parsedT2))
            {
                result[i] = (parsedT1, parsedT2);
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
