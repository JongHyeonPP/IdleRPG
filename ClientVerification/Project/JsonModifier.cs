using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Numerics;
using System.Data;
using Verification;
using Microsoft.Extensions.Logging;

public static class JsonModifier
{
    public static string AddToFieldValues(string json, Dictionary<string, object> updates, ILogger<VerificationController> logger)
    {
        JObject root = JObject.Parse(json);

        // 먼저 모든 updates를 적용
        foreach (var pair in updates)
        {
            string[] keys = pair.Key.Split('.');
            JToken current = root;

            for (int i = 0; i < keys.Length - 1; i++)
            {
                current = current[keys[i]];
                if (current == null)
                    throw new KeyNotFoundException($"Key path '{string.Join(".", keys, 0, i + 1)}' not found.");
            }

            string finalKey = keys[^1];

            if (current is JObject obj)
            {
                JToken existingValueToken = obj[finalKey];

                BigInteger existingValue = existingValueToken != null
                    ? BigInteger.Parse(existingValueToken.ToString())
                    : BigInteger.Zero;

                BigInteger addValue = ToBigInteger(pair.Value);
                BigInteger resultValue = existingValue + addValue;

                obj[finalKey] = JToken.FromObject(resultValue);
            }
            else
            {
                throw new InvalidOperationException($"Parent of '{finalKey}' is not a JObject.");
            }
        }

        // exp 키가 있었으면, 레벨업 계산은 여기서 별도로 수행
        if (updates.ContainsKey("Exp"))
        {
            JObject obj = root;
            int currentLevel = int.Parse(obj["level"].ToString());
            BigInteger currentExp = BigInteger.Parse(obj["exp"].ToString());
            


            DataTable dt = new();
            //logger.LogDebug($"Before - Level : {currentLevel}, Exp : {currentExp}");
            while (true)
            {
                string formula = VerificationController.levelUpRequireExp.Replace("{level}", currentLevel.ToString());
                int requiredExp = Convert.ToInt32(dt.Compute(formula, null));

                if (currentExp >= requiredExp)
                {
                    currentExp -= requiredExp;
                    currentLevel += 1;
                }
                else
                {
                    break;
                }
            }
            //logger.LogDebug($"After - Level : {currentLevel}, Exp : {currentExp}");
            obj["level"] = JToken.FromObject(currentLevel);
            obj["exp"] = JToken.FromObject(currentExp);
        }

        return root.ToString();
    }


    private static BigInteger ToBigInteger(object value)
    {
        if (value == null)
            return BigInteger.Zero;

        if (value is BigInteger bi)
            return bi;

        return BigInteger.Parse(value.ToString());
    }
}
