using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Numerics;
using ClientVerification.Etc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;

namespace ClientVerification.Verification
{
    public class ResourceVerifier : IDataVerifier
    {
        private readonly List<ResourceReport> reports;
        private readonly GameData serverData;
        private readonly ILogger logger;
        private readonly DataTable table = new();

        private readonly Dictionary<string, object> goldFormula;
        private readonly Dictionary<string, object> expFormula;
        private readonly Dictionary<string, object> fragmentFormula;
        private readonly Dictionary<string, object> weaponFormula;
        private readonly Dictionary<string, object> adventureReward;
        private readonly Dictionary<string, object> dungeonReward;
        private readonly Dictionary<string, object> companionReward;
        private readonly string levelExpFormula;

        private readonly int maxLevel = 9999;

        public ResourceVerifier(
            List<ResourceReport> reports,
            GameData serverData,
            ILogger logger,
            IVerificationSystem verificationSystem,
            Unity.Services.CloudCode.Core.IExecutionContext context,
            IGameApiClient gameApiClient)
        {
            this.reports = reports;
            this.serverData = serverData;
            this.logger = logger;

            goldFormula = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "GOLD_DROP_FORMULA");
            expFormula = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "EXP_DROP_FORMULA");
            fragmentFormula = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "FRAGMENT_DROP_FORMULA");
            weaponFormula = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "WEAPON_DROP_FORMULA");
            adventureReward = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "ADVENTURE_REWARD");
            dungeonReward = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "DUNGEON_REWARD");
            companionReward = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "COMPANION_REWARD");
            levelExpFormula = verificationSystem.GetRemoteConfig<string>(context, gameApiClient, "LEVEL_UP_REQUIRE_EXP");
        }

        public bool Verify(out string reason)
        {
            reason = "";

            foreach (var r in reports)
            {
                if (!VerifySingle(r, out reason))
                    return false;
            }

            NormalizeLevelAndExp();
            return true;
        }

        private bool VerifySingle(ResourceReport r, out string reason)
        {
            switch (r.Source)
            {
                case Source.Battle: return BattleCase(r, out reason);
                case Source.Adventure: return AdventureCase(r, out reason);
                case Source.Dungeon: return DungeonCase(r, out reason);
                case Source.Companion: return CompanionCase(r, out reason);
                case Source.Advertise:
                    return SimpleApply(r, out reason);
                default:
                    reason = $"Unknown source: {r.Source}";
                    return false;
            }
        }

        // =========================================================
        // BattleCase (공식 기반 검증)
        // =========================================================
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
                    {
                        var ok = ValidateFormula(expFormula, "EXP", r, out reason);
                        if (ok) ProcessLevelUp();
                        return ok;
                    }

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

        // =========================================================
        // AdventureCase (RC 기반 보상 지급 + scroll 부족 시 즉시 fail)
        // =========================================================
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

        // =========================================================
        // DungeonCase (RC 기반 보상 지급 + 실패 시 scroll만 차감)
        // =========================================================
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
                serverData.lastScrollTime = DateTime.UtcNow.ToString("O"); // 최근 사용 시간 갱신
                logger.LogInformation($"Dungeon enter (fail) → scroll -{fee}, lastScrollTime updated");
                return true;
            }

            // ===== 성공 케이스 =====
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

            serverData.lastScrollTime = DateTime.UtcNow.ToString("O"); // 스크롤 사용 시점 기록

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

            logger.LogInformation($"Dungeon_{dungeonIndex} Clear [{key}] → {rewardType} rewarded, Fee -{fee}, lastScrollTime updated");
            return true;
        }


        // =========================================================
        // CompanionCase (RC 기반 보상 지급)
        // =========================================================
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

        // =========================================================
        // Formula 기반 검증
        // =========================================================
        private bool ValidateFormula(Dictionary<string, object> cfg, string key, ResourceReport r, out string reason)
        {
            reason = "";

            if (cfg == null || !cfg.ContainsKey("Formula"))
            {
                reason = $"{key} formula missing";
                return false;
            }

            int stage = serverData.currentStageNum;
            string formula = cfg["Formula"].ToString().Replace("{stageNum}", stage.ToString(CultureInfo.InvariantCulture));
            int baseVal = Convert.ToInt32(table.Compute(formula, null));

            float range = cfg.ContainsKey("Range")
                ? Convert.ToSingle(cfg["Range"], CultureInfo.InvariantCulture)
                : 0f;

            double allowedMax = baseVal * (1 + range);

            // Bonus 스테이지 적용
            if (cfg.TryGetValue("Bonus", out var bonusObj) && bonusObj is IList<object> bonusList)
            {
                foreach (var b in bonusList)
                {
                    if (int.TryParse(b.ToString(), out int bonusStage) && bonusStage == stage)
                    {
                        double bonusValue = cfg.ContainsKey("BonusValue")
                            ? Convert.ToDouble(cfg["BonusValue"], CultureInfo.InvariantCulture)
                            : 0;
                        allowedMax = baseVal * (1 + range + bonusValue);
                        logger.LogInformation($"[{key}] Bonus stage {stage} applied (+{bonusValue * 100}%)");
                        break;
                    }
                }
            }

            if (r.Value > allowedMax)
            {
                logger.LogWarning($"[{key}] exceeded limit: stage {stage}, reported {r.Value}, max {allowedMax}");
                reason = $"{key} value out of range";
                return false;
            }

            if (key == "GOLD") serverData.gold += r.Value;
            else if (key == "EXP") serverData.exp += r.Value;

            return true;
        }


        // =========================================================
        // Fragment Drop 검증
        // =========================================================
        private bool ValidateFragment(ResourceReport r, out string reason)
        {
            reason = "";

            if (!TryGetFragmentRarity(r, out var rarity, out reason))
                return false;

            int stage = serverData.currentStageNum;
            string formula = fragmentFormula["Formula"].ToString().Replace("{stageNum}", stage.ToString());
            double baseVal = Convert.ToDouble(table.Compute(formula, null));
            double range = Convert.ToDouble(fragmentFormula["Range"], CultureInfo.InvariantCulture);
            double allowed = baseVal * (1 + range);

            if (r.Value > allowed)
            {
                reason = "Fragment value out of range";
                return false;
            }

            serverData.skillFragment[rarity] = serverData.skillFragment.GetValueOrDefault(rarity) + r.Value;
            return true;
        }

        // =========================================================
        // Weapon Drop 검증
        // =========================================================
        private bool ValidateWeapon(ResourceReport r, out string reason)
        {
            reason = "";

            var mapJson = weaponFormula["WeaponByStage"].ToString();
            var weaponMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(mapJson);
            int stage = serverData.currentStageNum;

            if (!weaponMap.TryGetValue(stage.ToString(), out var expected))
            {
                reason = $"Weapon not allowed in stage {stage}";
                return false;
            }

            if (!TryGetReportId(r, out var wid, out reason))
                return false;

            if (wid != expected)
            {
                reason = $"Weapon id mismatch: expected {expected}, got {wid}";
                return false;
            }

            serverData.weaponCount[wid] = serverData.weaponCount.GetValueOrDefault(wid) + (r.Value == 0 ? 1 : r.Value);
            return true;
        }

        // =========================================================
        // SimpleApply (기본 자원 누적)
        // =========================================================
        private bool SimpleApply(ResourceReport r, out string reason)
        {
            reason = "";

            switch (r.Resource)
            {
                case Resource.Gold: serverData.gold += r.Value; break;
                case Resource.Exp:
                    serverData.exp += r.Value;
                    ProcessLevelUp();
                    break;
                case Resource.Dia: serverData.dia += r.Value; break;
                case Resource.Clover: serverData.clover += r.Value; break;
                case Resource.Scroll: serverData.scroll += r.Value; break;
                case Resource.Fragment:
                    if (!TryGetFragmentRarity(r, out var rarity, out reason))
                        return false;
                    serverData.skillFragment[rarity] = serverData.skillFragment.GetValueOrDefault(rarity) + r.Value;
                    break;
                case Resource.Weapon:
                    if (!TryGetReportId(r, out var wid, out reason))
                        return false;
                    serverData.weaponCount[wid] = serverData.weaponCount.GetValueOrDefault(wid) + (r.Value == 0 ? 1 : r.Value);
                    break;
                default:
                    reason = $"Unknown resource: {r.Resource}";
                    return false;
            }

            return true;
        }

        // =========================================================
        // Level & Exp 정규화
        // =========================================================
        private void ProcessLevelUp() => NormalizeLevelAndExp();

        private void NormalizeLevelAndExp()
        {
            int level = serverData.level;
            BigInteger exp = serverData.exp;

            for (int i = 0; i < 1000; i++)
            {
                if (!TryGetRequiredExp(level, out var req) || exp < req)
                    break;
                exp -= req;
                level++;
                if (level >= maxLevel)
                    break;
            }

            serverData.level = Math.Min(level, maxLevel);
            serverData.exp = exp;
        }

        private bool TryGetRequiredExp(int level, out BigInteger required)
        {
            required = BigInteger.Zero;
            if (string.IsNullOrEmpty(levelExpFormula)) return false;

            string expr = levelExpFormula.Replace("{level}", level.ToString(CultureInfo.InvariantCulture));
            required = new BigInteger(Convert.ToDouble(table.Compute(expr, null)));
            return required > 0;
        }

        // =========================================================
        // 공용 메서드
        // =========================================================
        private bool TryGetFragmentRarity(ResourceReport r, out Rarity rarity, out string reason)
        {
            rarity = default;
            reason = "";

            if (r.Id == null || !Enum.TryParse<Rarity>(r.Id, true, out rarity))
            {
                reason = "Invalid rarity info";
                return false;
            }
            return true;
        }

        private bool TryGetReportId(ResourceReport r, out string id, out string reason)
        {
            id = r.Id;
            reason = "";
            if (string.IsNullOrWhiteSpace(id))
            {
                reason = "Missing weapon Id";
                return false;
            }
            return true;
        }
    }
}
