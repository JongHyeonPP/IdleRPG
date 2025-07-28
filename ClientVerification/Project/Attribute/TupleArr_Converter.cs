using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace ClientVerification.Attribute;
public class TupleArr_Converter<T1, T2> : JsonConverter<(T1, T2)[]>
    where T1 : struct
    where T2 : struct
{
    public override void WriteJson(JsonWriter writer, (T1, T2)[] value, JsonSerializer serializer)
    {
        var list = new List<object[]>();
        foreach (var tuple in value)
        {
            list.Add(new object[] { tuple.Item1, tuple.Item2 }); // string이 아니라 object
        }
        serializer.Serialize(writer, list);
    }


    public override (T1, T2)[] ReadJson(JsonReader reader, Type objectType, (T1, T2)[] existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var list = serializer.Deserialize<List<object[]>>(reader);
        var result = new (T1, T2)[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            T1 item1 = (T1)Convert.ChangeType(list[i][0], typeof(T1));
            T2 item2 = (T2)Convert.ChangeType(list[i][1], typeof(T2));
            result[i] = (item1, item2);
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
