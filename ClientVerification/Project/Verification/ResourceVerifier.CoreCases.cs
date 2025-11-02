using ClientVerification.Etc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ClientVerification.Verification
{
    public partial class ResourceVerifier
    {
        private bool BattleCase(ResourceReport r, out string reason)
        {
            reason = "";

            if (r.Value < 0)
            {
                reason = "Battle negative value";
                return false;
            }

            switch (r.Resource)
            {
                case Resource.Gold:
                    return ValidateFormula(goldFormula, "GOLD", r, out reason);

                case Resource.Exp:
                    var ok = ValidateFormula(expFormula, "EXP", r, out reason);
                    if (ok) ProcessLevelUp();
                    return ok;

                case Resource.Fragment:
                    return ValidateFragment(r, out reason);

                case Resource.Weapon:
                    return ValidateWeapon(r, out reason);

                case Resource.Dia:
                case Resource.Clover:
                case Resource.Scroll:
                    return SimpleApply(r, out reason);

                default:
                    reason = $"Unsupported battle resource {r.Resource}";
                    return false;
            }
        }

        private bool AdventureCase(ResourceReport r, out string reason)
        {
            reason = "";

            if (r.Id == null)
            {
                reason = "Adventure report missing Id";
                return false;
            }

            var parts = r.Id.Split('_');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out int advIndex) ||
                !int.TryParse(parts[1], out int incIndex))
            {
                reason = $"Invalid Adventure Id format: {r.Id}";
                return false;
            }

            if (adventureReward == null)
            {
                reason = "Adventure reward config missing";
                return false;
            }

            int fee = Convert.ToInt32(adventureReward["EntranceFee"]);
            if (serverData.scroll < fee)
            {
                reason = $"Not enough scroll to enter adventure (need {fee}, have {serverData.scroll})";
                return false;
            }

            if (!adventureReward.TryGetValue($"Adventure_{advIndex}", out var advObj))
            {
                reason = $"Adventure_{advIndex} not found in ADVENTURE_REWARD";
                return false;
            }

            var advDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(advObj.ToString());
            int baseDia = advDict["Dia"];
            int baseClover = advDict["Clover"];
            int diaInc = Convert.ToInt32(adventureReward["DiaIncrease"]);
            int cloverInc = Convert.ToInt32(adventureReward["CloverIncrease"]);

            int totalDia = baseDia + diaInc * incIndex;
            int totalClover = baseClover + cloverInc * incIndex;

            serverData.scroll -= fee;
            serverData.dia += totalDia;
            serverData.clover += totalClover;

            logger.LogInformation($"Adventure_{advIndex} Clear x{incIndex} → Dia +{totalDia}, Clover +{totalClover}, Fee -{fee}");
            return true;
        }

        private bool DungeonCase(ResourceReport r, out string reason)
        {
            reason = "";

            if (dungeonReward == null)
            {
                reason = "Dungeon reward config missing";
                return false;
            }

            int fee = dungeonReward.TryGetValue("EntranceFee", out var feeObj)
                ? Convert.ToInt32(feeObj)
                : 0;

            if (serverData.scroll < fee)
            {
                reason = $"Not enough scroll to enter dungeon (need {fee}, have {serverData.scroll})";
                logger.LogInformation($"Dungeon fail → scroll -{fee}");
                return false;
            }

            if (r.Id == null)
            {
                serverData.scroll -= fee;
                serverData.lastScrollTime = DateTime.UtcNow.ToString("O");
                logger.LogInformation($"Dungeon enter (fail) → scroll -{fee}");
                return true;
            }

            var parts = r.Id.Split('_');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out int dungeonIndex) ||
                !int.TryParse(parts[1], out int rewardIndex))
            {
                reason = $"Invalid Dungeon Id format: {r.Id}";
                return false;
            }

            if (!dungeonReward.TryGetValue($"Dungeon_{dungeonIndex}", out var dungeonObj))
            {
                reason = $"Dungeon_{dungeonIndex} not found in DUNGEON_REWARD";
                return false;
            }

            var dungeonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(dungeonObj.ToString());
            string rewardType = dungeonDict["RewardType"].ToString();
            string key = rewardIndex.ToString();

            if (!dungeonDict.TryGetValue(key, out var rewardValue))
            {
                reason = $"Reward index {key} not found in Dungeon_{dungeonIndex}";
                return false;
            }

            if (serverData.scroll < fee)
            {
                reason = $"Not enough scroll to enter dungeon (need {fee}, have {serverData.scroll})";
                return false;
            }

            serverData.lastScrollTime = DateTime.UtcNow.ToString("O");

            switch (rewardType)
            {
                case "Gold":
                    serverData.gold += Convert.ToInt32(rewardValue);
                    break;
                case "Clover":
                    serverData.clover += Convert.ToInt32(rewardValue);
                    break;
                case "Fragment":
                    var data = rewardValue.ToString().Split(',');
                    if (data.Length != 2)
                    {
                        reason = $"Invalid fragment reward format in Dungeon_{dungeonIndex}, index {key}";
                        return false;
                    }

                    if (!Enum.TryParse<Rarity>(data[0].Trim(), true, out var rarity))
                    {
                        reason = $"Invalid fragment rarity '{data[0]}'";
                        return false;
                    }

                    if (!int.TryParse(data[1].Trim(), out int fragAmount))
                    {
                        reason = $"Invalid fragment amount '{data[1]}'";
                        return false;
                    }

                    serverData.skillFragment[rarity] = serverData.skillFragment.GetValueOrDefault(rarity) + fragAmount;
                    break;
                default:
                    reason = $"Unknown RewardType '{rewardType}' in Dungeon_{dungeonIndex}";
                    return false;
            }

            logger.LogInformation($"Dungeon_{dungeonIndex} Clear [{key}] → {rewardType} rewarded, Fee -{fee}");
            return true;
        }

        private bool CompanionCase(ResourceReport r, out string reason)
        {
            reason = "";

            if (r.Id == null)
            {
                reason = "Companion report missing Id";
                return false;
            }

            var parts = r.Id.Split('_');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out int compIndex) ||
                !int.TryParse(parts[1], out int incIndex))
            {
                reason = $"Invalid Companion Id format: {r.Id}";
                return false;
            }

            if (companionReward == null)
            {
                reason = "Companion reward config missing";
                return false;
            }

            if (!companionReward.TryGetValue($"Companion_{compIndex}", out var compObj))
            {
                reason = $"Companion_{compIndex} not found in COMPANION_REWARD";
                return false;
            }

            var compDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(compObj.ToString());

            int baseDia = compDict["Dia"];
            int baseClover = compDict["Clover"];
            int diaInc = compDict["DiaIncrease"];
            int cloverInc = compDict["CloverIncrease"];

            int totalDia = baseDia + diaInc * incIndex;
            int totalClover = baseClover + cloverInc * incIndex;

            serverData.dia += totalDia;
            serverData.clover += totalClover;

            logger.LogInformation($"Companion_{compIndex} Clear x{incIndex} → Dia +{totalDia}, Clover +{totalClover}");
            return true;
        }
    }
}
