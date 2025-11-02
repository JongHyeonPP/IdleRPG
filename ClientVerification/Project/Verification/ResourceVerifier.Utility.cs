using ClientVerification.Etc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Numerics;

namespace ClientVerification.Verification
{
    public partial class ResourceVerifier
    {
        // =========================================================
        // Formula 기반 검증 (골드, 경험치 등)
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
            allowedMax = Math.Ceiling(allowedMax);

            // Bonus 스테이지가 있을 경우
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
                logger.LogWarning($"[Fragment] exceeded: stage {stage}, reported {r.Value}, allowed {allowed}");
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
                logger.LogWarning($"[Weapon] Stage {stage} has no weapon mapping.");
                return false;
            }

            if (!TryGetReportId(r, out var wid, out reason))
                return false;

            if (wid != expected)
            {
                reason = $"Weapon id mismatch: expected {expected}, got {wid}";
                logger.LogWarning($"[Weapon] Mismatch → expected: {expected}, got: {wid}");
                return false;
            }

            serverData.weaponCount[wid] = serverData.weaponCount.GetValueOrDefault(wid) + (r.Value == 0 ? 1 : r.Value);
            return true;
        }

        // =========================================================
        // 단순 리소스 적용 (직접 누적)
        // =========================================================
        private bool SimpleApply(ResourceReport r, out string reason)
        {
            reason = "";

            switch (r.Resource)
            {
                case Resource.Gold:
                    serverData.gold += r.Value;
                    break;

                case Resource.Exp:
                    serverData.exp += r.Value;
                    ProcessLevelUp();
                    break;

                case Resource.Dia:
                    serverData.dia += r.Value;
                    break;

                case Resource.Clover:
                    serverData.clover += r.Value;
                    break;

                case Resource.Scroll:
                    serverData.scroll += r.Value;
                    break;

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
                    logger.LogWarning($"[SimpleApply] Unknown resource type {r.Resource}");
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
                logger.LogWarning("[Fragment] Invalid rarity info or missing Id.");
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
                logger.LogWarning("[Weapon] Missing Id field in ResourceReport.");
                return false;
            }
            return true;
        }
    }
}
