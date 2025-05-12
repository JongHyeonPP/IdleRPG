using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public static class UtilityManager
{
    private static float grayScaleAmount = 0.8f;

    public static bool CalculateProbability(float probability)
    {
        return Random.Range(0f, 1f) <= Mathf.Clamp(probability, 0f, 1f);
    }

    public static int AllocateProbability(params float[] probabilities)
    {
        float total = 0f;
        foreach (float prob in probabilities)
        {
            total += prob;
        }
        float randomValue = Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (randomValue < cumulative)
            {
                return i;
            }
        }
        return 0;
    }
    public static Dictionary<T, string> GetParsedFormularDict<T>(string jsonStr)
    {
        var rawFormulas = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);

        if (typeof(T) == typeof(string))
        {
            return rawFormulas.ToDictionary(
                pair => (T)(object)pair.Key,
                pair => pair.Value
            );
        }

        if (!typeof(T).IsEnum)
            throw new InvalidOperationException($"T must be an enum or string. Provided: {typeof(T).Name}");

        Dictionary<T, string> formulas = new();
        foreach (var pair in rawFormulas)
        {
            if (Enum.TryParse(typeof(T), pair.Key, out object enumValue))
            {
                formulas[(T)enumValue] = pair.Value;
            }
        }
        return formulas;
    }



}
