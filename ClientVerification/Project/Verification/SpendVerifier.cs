using ClientVerification.Etc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace ClientVerification.Verification
{
    public class SpendVerifier : IDataVerifier
    {
        private readonly Dictionary<string, int> reportDict;
        private readonly GameData serverData;
        private readonly ILogger logger;
        private readonly DataTable dataTable;
        private readonly Dictionary<StatusType, string> reinforcePriceFormula;

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
            dataTable = new DataTable();
            reinforcePriceFormula = verificationSystem.GetRemoteConfig<Dictionary<StatusType, string>>(context, gameApiClient, "REINFORCE_PRICE_GOLD");
        }

        public bool Verify(out string failReason)
        {
            failReason = "";

            foreach (var kvp in reportDict)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                if (string.IsNullOrWhiteSpace(key))
                {
                    failReason = BuildFail(
                        code: "Spend.Key.Empty",
                        message: "Spend key is empty",
                        extra: new { rawKey = key }
                    );
                    return false;
                }

                var parts = key.Split('_');
                if (parts.Length < 2)
                {
                    failReason = BuildFail(
                        code: "Spend.Key.Malformed",
                        message: "Spend key must be Category_Target format",
                        extra: new { rawKey = key }
                    );
                    return false;
                }

                var category = parts[0];
                var target = parts[1];

                if (category != "Status")
                {
                    failReason = BuildFail(
                        code: "Spend.Category.Unsupported",
                        message: "Unsupported spend category",
                        extra: new { category, rawKey = key }
                    );
                    return false;
                }

                if (!Enum.TryParse<StatusType>(target, true, out var statusType))
                {
                    failReason = BuildFail(
                        code: "Spend.Status.InvalidType",
                        message: "Invalid status type in spend key",
                        extra: new { target, rawKey = key }
                    );
                    return false;
                }

                if (value <= 0)
                {
                    failReason = BuildFail(
                        code: "Spend.Value.NonPositive",
                        message: "Spend value must be positive",
                        extra: new { rawKey = key, value }
                    );
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

            if (reinforcePriceFormula == null)
            {
                failReason = BuildFail(
                    code: "Spend.Config.Missing",
                    message: "REINFORCE_PRICE_GOLD config missing",
                    extra: new { }
                );
                return false;
            }

            if (!reinforcePriceFormula.TryGetValue(statusType, out var priceExpr))
            {
                failReason = BuildFail(
                    code: "Spend.Config.MissingFormula",
                    message: "Price formula for status not found",
                    extra: new { status = statusType.ToString() }
                );
                return false;
            }

            if (string.IsNullOrWhiteSpace(priceExpr))
            {
                failReason = BuildFail(
                    code: "Spend.Config.EmptyFormula",
                    message: "Price formula is empty",
                    extra: new { status = statusType.ToString() }
                );
                return false;
            }

            if (!serverData.statLevel_Gold.TryGetValue(statusType, out var currentLevel))
                currentLevel = 0;

            var totalCost = 0;
            var safety = increaseCount;

            for (int i = 0; i < increaseCount; i++)
            {
                if (safety-- <= 0)
                {
                    failReason = BuildFail(
                        code: "Spend.Safety.Break",
                        message: "Safety break while computing total cost",
                        extra: new { status = statusType.ToString(), increaseCount }
                    );
                    return false;
                }

                var level = currentLevel + i;
                var expr = priceExpr.Replace("{level}", level.ToString(CultureInfo.InvariantCulture));

                if (!TryComputeInt(expr, out var stepCost, out var error))
                {
                    failReason = BuildFail(
                        code: "Spend.Formula.ComputeError",
                        message: "Failed to compute price formula",
                        extra: new { status = statusType.ToString(), level, formula = expr, error }
                    );
                    return false;
                }

                if (stepCost < 0)
                {
                    failReason = BuildFail(
                        code: "Spend.Formula.NegativeCost",
                        message: "Computed negative cost from formula",
                        extra: new { status = statusType.ToString(), level, stepCost }
                    );
                    return false;
                }

                try
                {
                    checked { totalCost += stepCost; }
                }
                catch (OverflowException)
                {
                    failReason = BuildFail(
                        code: "Spend.Cost.Overflow",
                        message: "Total cost overflow",
                        extra: new { status = statusType.ToString(), partialCost = totalCost, stepCost }
                    );
                    return false;
                }
            }

            if (serverData.gold < totalCost)
            {
                failReason = BuildFail(
                    code: "Spend.Gold.Insufficient",
                    message: "Not enough gold to reinforce",
                    extra: new { status = statusType.ToString(), required = totalCost, current = serverData.gold, increaseCount }
                );
                return false;
            }

            serverData.gold -= totalCost;

            if (serverData.statLevel_Gold.ContainsKey(statusType))
                serverData.statLevel_Gold[statusType] += increaseCount;
            else
                serverData.statLevel_Gold.Add(statusType, increaseCount);

            logger?.LogDebug($"Reinforced {statusType} by {increaseCount}. Cost {totalCost}. Gold left {serverData.gold}");

            return true;
        }

        private bool TryComputeInt(string expression, out int value, out string error)
        {
            try
            {
                var obj = dataTable.Compute(expression, null);
                var d = Convert.ToDouble(obj, CultureInfo.InvariantCulture);
                value = Convert.ToInt32(Math.Round(d));
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                value = 0;
                error = ex.Message;
                return false;
            }
        }

        private static string BuildFail(string code, string message, object extra)
        {
            var payload = new { code, message, extra };
            return JsonConvert.SerializeObject(payload);
        }
    }
}
