using ClientVerification.Etc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace ClientVerification.Verification
{
    [Serializable]
    public class ReinforceRule
    {
        public float baseInc;
        public int step;
        public float stepInc;
        public float startValue;
    }

    public class SpendVerifier : IDataVerifier
    {
        private readonly Dictionary<string, int> reportDict;
        private readonly GameData serverData;
        private readonly ILogger logger;
        private readonly Dictionary<StatusType, ReinforceRule> reinforcePriceRules;

        public SpendVerifier(
            Dictionary<string, int> reportDict,
            GameData serverData,
            ILogger logger,
            IVerificationSystem verificationSystem,
            IExecutionContext context,
            IGameApiClient gameApiClient)
        {
            this.reportDict = reportDict ?? new Dictionary<string, int>();
            this.serverData = serverData;
            this.logger = logger;

            reinforcePriceRules = verificationSystem.GetRemoteConfig<Dictionary<StatusType, ReinforceRule>>(
                context, gameApiClient, "REINFORCE_PRICE_GOLD"
            );
        }

        public bool Verify(out string failReason)
        {
            failReason = "";

            foreach (var kvp in reportDict)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                logger.LogDebug($"Key : {key}, Value : {value}");

                if (string.IsNullOrWhiteSpace(key))
                {
                    failReason = BuildFail("Spend.Key.Empty", "Spend key is empty", new { rawKey = key });
                    return false;
                }

                var parts = key.Split('_');
                if (parts.Length < 2)
                {
                    failReason = BuildFail("Spend.Key.Malformed", "Spend key must be Category_Target format", new { rawKey = key });
                    return false;
                }

                var category = parts[0];
                var target = parts[1];

                if (category != "Status")
                {
                    failReason = BuildFail("Spend.Category.Unsupported", "Unsupported spend category", new { category, rawKey = key });
                    return false;
                }

                if (!Enum.TryParse<StatusType>(target, true, out var statusType))
                {
                    failReason = BuildFail("Spend.Status.InvalidType", "Invalid status type in spend key", new { target, rawKey = key });
                    return false;
                }

                if (value <= 0)
                {
                    failReason = BuildFail("Spend.Value.NonPositive", "Spend value must be positive", new { rawKey = key, value });
                    return false;
                }

                if (!StatusCase(statusType, value, out failReason))
                    return false;
            }

            return true;
        }

        private bool StatusCase(StatusType statusType, int increaseCount, out string failReason)
        {
            failReason = "";

            if (reinforcePriceRules == null)
            {
                failReason = BuildFail("Spend.Config.Missing", "REINFORCE_PRICE_GOLD config missing", new { });
                return false;
            }

            if (!reinforcePriceRules.TryGetValue(statusType, out var rule))
            {
                failReason = BuildFail("Spend.Config.MissingRule", "Reinforce rule for status not found", new { status = statusType.ToString() });
                return false;
            }

            if (!serverData.statLevel_Gold.TryGetValue(statusType, out var currentLevel))
                currentLevel = 0;

            float totalCost = 0f;

            for (int i = 1; i <= increaseCount; i++)
            {
                int level = currentLevel + i;
                float inc = rule.baseInc + (level / (float)rule.step) * rule.stepInc;
                totalCost += inc;
            }

            totalCost += rule.startValue;

            int finalCost = (int)Math.Round(totalCost);
            if (finalCost < 0)
            {
                failReason = BuildFail("Spend.Cost.Negative", "Computed negative total cost", new { status = statusType.ToString(), totalCost });
                return false;
            }

            if (serverData.gold < finalCost)
            {
                failReason = BuildFail("Spend.Gold.Insufficient", "Not enough gold to reinforce", new
                {
                    status = statusType.ToString(),
                    required = finalCost,
                    current = serverData.gold,
                    increaseCount
                });
                return false;
            }

            serverData.gold -= finalCost;

            if (serverData.statLevel_Gold.ContainsKey(statusType))
                serverData.statLevel_Gold[statusType] += increaseCount;
            else
                serverData.statLevel_Gold.Add(statusType, increaseCount);

            logger?.LogDebug($"[SpendVerifier] Reinforced {statusType} +{increaseCount}, Cost {finalCost}, Gold Left {serverData.gold}");
            return true;
        }

        private static string BuildFail(string code, string message, object extra)
        {
            var payload = new { code, message, extra };
            return JsonConvert.SerializeObject(payload);
        }
    }
}
