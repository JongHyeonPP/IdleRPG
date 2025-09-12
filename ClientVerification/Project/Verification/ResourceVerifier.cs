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

        private readonly DataTable dataTable;

        private readonly string levelUpRequireExp;
        private readonly Dictionary<string, string> goldDropFormula;
        private readonly Dictionary<string, string> expDropFormula;

        private readonly FragmentDropConfig fragmentDropConfig;
        private readonly WeaponDropConfig weaponDropConfig;

        private readonly Dictionary<string, object> adventureReward;

        // 운영에서 Remote Config로 바꾸고 싶으면 주입하면 됨
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

            dataTable = new DataTable();

            goldDropFormula = verificationSystem.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "GOLD_DROP_FORMULA");
            expDropFormula = verificationSystem.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "EXP_DROP_FORMULA");

            var fragRaw = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "FRAGMENT_DROP_FORMULA");
            var weaponRaw = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "WEAPON_DROP_FORMULA");

            fragmentDropConfig = ParseFragmentConfigOrThrow(fragRaw);
            weaponDropConfig = ParseWeaponConfigOrThrow(weaponRaw);

            adventureReward = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "ADVENTURE_REWARD");
            levelUpRequireExp = verificationSystem.GetRemoteConfig<string>(context, gameApiClient, "LEVEL_UP_REQUIRE_EXP");
        }

        public bool Verify(out string failReason)
        {
            failReason = "";

            BigInteger goldDelta = 0;
            BigInteger expDelta = 0;
            int diaDelta = 0;
            int cloverDelta = 0;
            int scrollDelta = 0;

            var fragDelta = new Dictionary<Rarity, int>();
            var weaponDelta = new Dictionary<string, int>();

            foreach (var report in reports)
            {
                if (!VerifySingle(report, out var localFail))
                {
                    failReason = localFail;
                    return false;
                }

                switch (report.Resource)
                {
                    case Resource.Gold:
                        goldDelta += report.Value;
                        break;

                    case Resource.Exp:
                        expDelta += report.Value;
                        break;

                    case Resource.Dia:
                        diaDelta += report.Value;
                        break;

                    case Resource.Clover:
                        cloverDelta += report.Value;
                        break;

                    case Resource.Scroll:
                        scrollDelta += report.Value;
                        break;

                    case Resource.Fragment:
                        {
                            if (!TryGetFragmentRarity(report, out var rarity, out failReason))
                                return false;
                            AddOrIncrease(fragDelta, rarity, report.Value);
                            break;
                        }

                    case Resource.Weapon:
                        {
                            if (!TryGetReportId(report, out var weaponId, out failReason))
                                return false;
                            AddOrIncrease(weaponDelta, weaponId, report.Value == 0 ? 1 : report.Value);
                            break;
                        }

                    default:
                        failReason = BuildFail(
                            "Resource.Apply.UnknownResource",
                            "Unsupported resource when applying verified value",
                            new { resource = report.Resource.ToString(), reportedValue = report.Value }
                        );
                        return false;
                }
            }

            serverData.gold += goldDelta;
            serverData.exp += expDelta;
            serverData.dia += diaDelta;
            serverData.clover += cloverDelta;
            serverData.scroll += scrollDelta;

            foreach (var kv in fragDelta) AddOrIncrease(serverData.skillFragment, kv.Key, kv.Value);
            foreach (var kv in weaponDelta) AddOrIncrease(serverData.weaponCount, kv.Key, kv.Value);

            // 저장 직전 최종 정규화로 불변식 강제
            NormalizeLevelAndExp();

            return true;
        }

        private bool VerifySingle(ResourceReport report, out string failReason)
        {
            switch (report.Source)
            {
                case Source.Battle:
                    return BattleCase(report, out failReason);

                case Source.Adventure:
                    return AdventureCase(report, out failReason);

                case Source.Companion:
                    failReason = BuildFail(
                        "Verify.UnsupportedSource",
                        "Companion source is not supported yet",
                        new { resource = report.Resource.ToString(), reportedValue = report.Value }
                    );
                    return false;

                default:
                    failReason = BuildFail(
                        "Verify.UnknownSource",
                        "Unknown source",
                        new { source = report.Source.ToString(), resource = report.Resource.ToString(), reportedValue = report.Value }
                    );
                    return false;
            }
        }

        private bool BattleCase(ResourceReport report, out string failReason)
        {
            failReason = "";

            if (report.Value < 0)
            {
                failReason = BuildFail(
                    "Battle.NegativeValue",
                    "Reported value must be non-negative",
                    new { resource = report.Resource.ToString(), reportedValue = report.Value }
                );
                return false;
            }

            switch (report.Resource)
            {
                case Resource.Gold:
                    return ValidateGoldExp(goldDropFormula, "GOLD_DROP_FORMULA", report, out failReason);

                case Resource.Exp:
                    {
                        var ok = ValidateGoldExp(expDropFormula, "EXP_DROP_FORMULA", report, out failReason);
                        if (!ok) return false;
                        // 리포트 단위 레벨업은 선택 사항
                        // 최종 저장 직전 NormalizeLevelAndExp에서 다시 보정
                        ProcessLevelUp();
                        return true;
                    }

                case Resource.Fragment:
                    return ValidateFragmentReport(report, out failReason);

                case Resource.Weapon:
                    return ValidateWeaponReport(report, out failReason);

                default:
                    failReason = BuildFail(
                        "Battle.Resource.Unsupported",
                        "Unsupported resource in battle case",
                        new { resource = report.Resource.ToString() }
                    );
                    return false;
            }
        }

        private bool ValidateGoldExp(Dictionary<string, string> cfg, string cfgName, ResourceReport report, out string failReason)
        {
            failReason = "";

            if (cfg == null)
            {
                failReason = BuildFail(
                    "Battle.Config.Missing",
                    "Drop config is null",
                    new { resource = report.Resource.ToString(), configKey = cfgName }
                );
                return false;
            }

            if (!cfg.TryGetValue("Formula", out var formulaString) || string.IsNullOrWhiteSpace(formulaString))
            {
                failReason = BuildFail(
                    "Battle.Config.MissingFormula",
                    "Drop formula missing or empty",
                    new { resource = report.Resource.ToString(), configKey = cfgName }
                );
                return false;
            }

            if (!cfg.TryGetValue("Range", out var rangeString) || string.IsNullOrWhiteSpace(rangeString))
            {
                failReason = BuildFail(
                    "Battle.Config.MissingRange",
                    "Drop range missing or empty",
                    new { resource = report.Resource.ToString(), configKey = cfgName }
                );
                return false;
            }

            if (!float.TryParse(rangeString, NumberStyles.Float, CultureInfo.InvariantCulture, out var valueRange))
            {
                failReason = BuildFail(
                    "Battle.Config.InvalidRange",
                    "Drop range is not a valid float",
                    new { resource = report.Resource.ToString(), range = rangeString }
                );
                return false;
            }

            var stageNum = serverData.currentStageNum;
            var expr = formulaString.Replace("{stageNum}", stageNum.ToString(CultureInfo.InvariantCulture));

            if (!TryComputeInt(expr, out var standardValue, out var computeErr))
            {
                failReason = BuildFail(
                    "Battle.Formula.ComputeError",
                    "Failed to compute drop formula",
                    new { resource = report.Resource.ToString(), stageNum, formula = expr, error = computeErr }
                );
                return false;
            }

            var allowedMax = (int)Math.Ceiling(standardValue * (1 + valueRange));
            if (report.Value > allowedMax)
            {
                var anomaly = new
                {
                    type = $"{report.Resource}Gain",
                    expectedMax = allowedMax,
                    reported = report.Value,
                    stageNum,
                    standardValue,
                    range = valueRange
                };
                logger.LogError($"Unexpected {report.Resource}: {System.Text.Json.JsonSerializer.Serialize(anomaly)}");

                failReason = BuildFail("Battle.Value.OutOfRange", "Reported value exceeds allowed maximum", anomaly);
                return false;
            }

            return true;
        }

        private bool ValidateFragmentReport(ResourceReport report, out string failReason)
        {
            failReason = "";

            if (fragmentDropConfig == null)
            {
                failReason = BuildFail("Fragment.Config.Missing", "FRAGMENT_DROP_FORMULA is null", new { });
                return false;
            }

            if (fragmentDropConfig.dropInterval <= 0)
            {
                failReason = BuildFail(
                    "Fragment.Config.InvalidDropInterval",
                    "DropInterval must be positive",
                    new { dropInterval = fragmentDropConfig.dropInterval }
                );
                return false;
            }

            if (!TryGetFragmentRarity(report, out var reportedRarity, out failReason))
                return false;

            var stageNum = serverData.currentStageNum;

            if (fragmentDropConfig.forceAssign != null &&
                fragmentDropConfig.forceAssign.TryGetValue(stageNum, out var forcedRarityStr))
            {
                if (!string.Equals(forcedRarityStr, reportedRarity.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    failReason = BuildFail(
                        "Fragment.Rarity.ForceMismatch",
                        "Rarity must match ForceAssign for this stage",
                        new { stageNum, expected = forcedRarityStr, reported = reportedRarity.ToString() }
                    );
                    return false;
                }
            }
            else
            {
                if (stageNum % fragmentDropConfig.dropInterval != 0)
                {
                    if (report.Value == 0) return true;

                    failReason = BuildFail(
                        "Fragment.Interval.Violation",
                        "Fragment drop not allowed on this stage by DropInterval rule",
                        new { stageNum, dropInterval = fragmentDropConfig.dropInterval, reported = report.Value }
                    );
                    return false;
                }

                var allowed = GetAllowedRaritiesForStage(stageNum);
                if (allowed == null || allowed.Count == 0)
                {
                    failReason = BuildFail("Fragment.Range.Missing", "No allowed rarities defined for this stage", new { stageNum });
                    return false;
                }

                var ok = allowed.Exists(r => string.Equals(r, reportedRarity.ToString(), StringComparison.OrdinalIgnoreCase));
                if (!ok)
                {
                    failReason = BuildFail(
                        "Fragment.Rarity.NotAllowed",
                        "Reported rarity is not allowed for this stage",
                        new { stageNum, reported = reportedRarity.ToString(), allowed }
                    );
                    return false;
                }
            }

            var expr = fragmentDropConfig.formula.Replace("{stageNum}", stageNum.ToString(CultureInfo.InvariantCulture));
            if (!TryComputeDouble(expr, out var baseCount, out var compErr))
            {
                failReason = BuildFail(
                    "Fragment.Formula.ComputeError",
                    "Failed to compute fragment base count",
                    new { stageNum, formula = expr, error = compErr }
                );
                return false;
            }

            var rarityKey = reportedRarity.ToString();
            var adj = 1f;
            if (fragmentDropConfig.rarityAdjust != null &&
                fragmentDropConfig.rarityAdjust.TryGetValue(rarityKey, out var found))
                adj = found;

            var adjusted = baseCount * adj;
            var allowedMax = Math.Max(0, (int)Math.Ceiling(adjusted * (1 + fragmentDropConfig.range)));

            if (report.Value > allowedMax)
            {
                failReason = BuildFail(
                    "Fragment.Value.OutOfRange",
                    "Reported fragment count exceeds allowed maximum",
                    new
                    {
                        stageNum,
                        reported = report.Value,
                        baseCount,
                        rarityAdjust = adj,
                        range = fragmentDropConfig.range,
                        allowedMax
                    }
                );
                return false;
            }

            return true;
        }

        private bool ValidateWeaponReport(ResourceReport report, out string failReason)
        {
            failReason = "";

            if (weaponDropConfig == null || weaponDropConfig.weaponByStage == null)
            {
                failReason = BuildFail("Weapon.Config.Missing", "WEAPON_DROP_FORMULA is null or invalid", new { });
                return false;
            }

            var stageNum = serverData.currentStageNum;

            if (!weaponDropConfig.weaponByStage.TryGetValue(stageNum, out var expectedWeaponId))
            {
                failReason = BuildFail(
                    "Weapon.Stage.NotAllowed",
                    "Weapon drop is not allowed at this stage",
                    new { stageNum, reportedValue = report.Value }
                );
                return false;
            }

            if (!TryGetReportId(report, out var id, out failReason))
                return false;

            if (!string.Equals(expectedWeaponId, id, StringComparison.Ordinal))
            {
                failReason = BuildFail(
                    "Weapon.Id.Mismatch",
                    "Reported Id does not match stage mapping",
                    new { stageNum, expectedWeaponId, reportedId = id }
                );
                return false;
            }

            if (report.Value <= 0)
            {
                failReason = BuildFail(
                    "Weapon.Value.Invalid",
                    "Weapon report value must be positive",
                    new { stageNum, value = report.Value }
                );
                return false;
            }

            return true;
        }

        private bool AdventureCase(ResourceReport report, out string failReason)
        {
            failReason = "";
            return true;
        }

        // 기존 자리 유지. 내부에서는 정규화만 호출
        private void ProcessLevelUp()
        {
            NormalizeLevelAndExp();
        }

        // 요구 경험치 계산 안전 래퍼
        private bool TryGetRequiredExpForLevel(int level, out BigInteger required)
        {
            required = BigInteger.Zero;

            if (string.IsNullOrWhiteSpace(levelUpRequireExp))
                return false;

            if (level < 1)
                return false;

            var expr = levelUpRequireExp.Replace("{level}", level.ToString(CultureInfo.InvariantCulture));
            if (!TryComputeBigInt(expr, out var val, out _))
                return false;

            if (val <= BigInteger.Zero)
                return false;

            required = val;
            return true;
        }

        // 저장 직전 불변식 강제
        private void NormalizeLevelAndExp()
        {
            var level = serverData.level;
            var exp = serverData.exp;

            if (level < 1) level = 1;

            if (level >= maxLevel)
            {
                level = maxLevel;

                if (TryGetRequiredExpForLevel(level, out var cap))
                {
                    var capMinusOne = cap - BigInteger.One;
                    if (capMinusOne < BigInteger.Zero) capMinusOne = BigInteger.Zero;
                    if (exp > capMinusOne) exp = capMinusOne;
                }
                else
                {
                    exp = BigInteger.Zero;
                }

                serverData.level = level;
                serverData.exp = exp;
                return;
            }

            int guard = 0;
            while (guard++ <= maxLevel)
            {
                if (!TryGetRequiredExpForLevel(level, out var required))
                    break;

                if (exp < required)
                    break;

                exp -= required;
                level++;

                if (level >= maxLevel)
                {
                    if (TryGetRequiredExpForLevel(level, out var cap))
                    {
                        var capMinusOne = cap - BigInteger.One;
                        if (capMinusOne < BigInteger.Zero) capMinusOne = BigInteger.Zero;
                        if (exp > capMinusOne) exp = capMinusOne;
                    }
                    else
                    {
                        exp = BigInteger.Zero;
                    }
                    break;
                }
            }

            serverData.level = level;
            serverData.exp = exp;

            // 마지막 방어 한 번 더
            if (level < maxLevel && TryGetRequiredExpForLevel(level, out var req) && exp >= req)
            {
                guard = 0;
                while (guard++ <= maxLevel && TryGetRequiredExpForLevel(level, out req) && exp >= req)
                {
                    exp -= req;
                    level++;
                    if (level >= maxLevel) break;
                }
                serverData.level = level;
                serverData.exp = exp;
            }
        }

        private bool TryGetReportId(object report, out string id, out string failReason)
        {
            id = null;
            failReason = "";

            var t = report.GetType();

            var prop = t.GetProperty("Id") ?? t.GetProperty("id");
            if (prop != null)
            {
                var v = prop.GetValue(report)?.ToString();
                if (!string.IsNullOrEmpty(v)) { id = v; return true; }
            }

            var field = t.GetField("Id") ?? t.GetField("id");
            if (field != null)
            {
                var v = field.GetValue(report)?.ToString();
                if (!string.IsNullOrEmpty(v)) { id = v; return true; }
            }

            if (report is Dictionary<string, object> d)
            {
                if (d.TryGetValue("Id", out var v1) && v1 != null && !string.IsNullOrEmpty(v1.ToString())) { id = v1.ToString(); return true; }
                if (d.TryGetValue("id", out var v2) && v2 != null && !string.IsNullOrEmpty(v2.ToString())) { id = v2.ToString(); return true; }
            }

            if (report is string s && !string.IsNullOrWhiteSpace(s))
            {
                try
                {
                    var m = JsonConvert.DeserializeObject<Dictionary<string, object>>(s);
                    if (m != null)
                    {
                        if (m.TryGetValue("Id", out var v3) && v3 != null && !string.IsNullOrEmpty(v3.ToString())) { id = v3.ToString(); return true; }
                        if (m.TryGetValue("id", out var v4) && v4 != null && !string.IsNullOrEmpty(v4.ToString())) { id = v4.ToString(); return true; }
                    }
                }
                catch { }
            }

            failReason = BuildFail("Report.Id.Missing", "Required top-level Id missing", new { required = "Id" });
            return false;
        }

        private bool TryGetFragmentRarity(ResourceReport report, out Rarity rarity, out string failReason)
        {
            rarity = default;
            if (!TryGetMetaString(report, "rarity", out var rarityStr, out failReason))
                return false;

            if (!Enum.TryParse<Rarity>(rarityStr, true, out rarity))
            {
                failReason = BuildFail(
                    "Fragment.Meta.InvalidRarity",
                    "Invalid rarity in report meta",
                    new { rarity = rarityStr }
                );
                return false;
            }
            return true;
        }

        private bool TryGetMetaString(ResourceReport report, string key, out string value, out string failReason)
        {
            value = null;
            failReason = "";

            object metaObj = null;

            var metaProp = report.GetType().GetProperty("Meta") ??
                           report.GetType().GetProperty("Extra") ??
                           report.GetType().GetProperty("Payload");

            if (metaProp != null)
                metaObj = metaProp.GetValue(report);

            Dictionary<string, object> dict = null;

            if (metaObj is Dictionary<string, object> d)
                dict = d;
            else if (metaObj is string s && !string.IsNullOrWhiteSpace(s))
            {
                try { dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(s); }
                catch (Exception ex)
                {
                    failReason = BuildFail(
                        "Report.Meta.ParseError",
                        "Failed to parse meta JSON",
                        new { error = ex.Message, raw = s }
                    );
                    return false;
                }
            }

            if (dict == null || !dict.TryGetValue(key, out var v) || v == null)
            {
                failReason = BuildFail(
                    "Report.Meta.MissingKey",
                    "Required meta key missing",
                    new { key }
                );
                return false;
            }

            value = v.ToString();
            return true;
        }

        private List<string> GetAllowedRaritiesForStage(int stageNum)
        {
            foreach (var rng in fragmentDropConfig.valueDistribute)
            {
                if (stageNum >= rng.min && stageNum <= rng.max)
                    return rng.rarities;
            }
            return null;
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
            catch (Exception ex) { value = 0; error = ex.Message; return false; }
        }

        private bool TryComputeDouble(string expression, out double value, out string error)
        {
            try
            {
                var obj = dataTable.Compute(expression, null);
                value = Convert.ToDouble(obj, CultureInfo.InvariantCulture);
                error = "";
                return true;
            }
            catch (Exception ex) { value = 0; error = ex.Message; return false; }
        }

        private bool TryComputeBigInt(string expression, out BigInteger value, out string error)
        {
            try
            {
                var obj = dataTable.Compute(expression, null);
                if (obj == null) { value = BigInteger.Zero; error = "Compute returned null"; return false; }
                if (obj is string s)
                {
                    if (BigInteger.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) { error = ""; return true; }
                    value = BigInteger.Zero; error = "Cannot parse string to BigInteger"; return false;
                }
                var dec = Convert.ToDecimal(obj, CultureInfo.InvariantCulture);
                value = new BigInteger(dec);
                error = "";
                return true;
            }
            catch (Exception ex) { value = BigInteger.Zero; error = ex.Message; return false; }
        }

        private static void AddOrIncrease<TKey>(Dictionary<TKey, int> dict, TKey key, int delta)
        {
            if (delta == 0) return;
            if (dict.TryGetValue(key, out var cur)) dict[key] = cur + delta;
            else dict[key] = delta;
        }

        private static string BuildFail(string code, string message, object extra)
        {
            return JsonConvert.SerializeObject(new { code, message, extra });
        }

        private static FragmentDropConfig ParseFragmentConfigOrThrow(Dictionary<string, object> raw)
        {
            if (raw == null) throw new Exception("FRAGMENT_DROP_FORMULA is null");

            var cfg = new FragmentDropConfig
            {
                dropInterval = Convert.ToInt32(raw["DropInterval"], CultureInfo.InvariantCulture),
                formula = raw["Formula"].ToString(),
                range = Convert.ToSingle(raw["Range"], CultureInfo.InvariantCulture),
                rarityAdjust = JsonConvert.DeserializeObject<Dictionary<string, float>>(raw["RarityAdjust"].ToString()),
                valueDistribute = new List<StageRarityDistribute>(),
                forceAssign = new Dictionary<int, string>()
            };

            var rangesDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(raw["ValueDistribute"].ToString());
            foreach (var kv in rangesDict)
            {
                var sp = kv.Key.Split('-');
                var min = int.Parse(sp[0], CultureInfo.InvariantCulture);
                var max = int.Parse(sp[1], CultureInfo.InvariantCulture);
                cfg.valueDistribute.Add(new StageRarityDistribute { min = min, max = max, rarities = kv.Value });
            }

            if (raw.TryGetValue("ForceAssign", out var fa) && fa != null)
            {
                var faDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(fa.ToString());
                foreach (var kv in faDict)
                    if (int.TryParse(kv.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var st))
                        cfg.forceAssign[st] = kv.Value;
            }

            return cfg;
        }

        private static WeaponDropConfig ParseWeaponConfigOrThrow(Dictionary<string, object> raw)
        {
            if (raw == null) throw new Exception("WEAPON_DROP_FORMULA is null");
            if (!raw.TryGetValue("WeaponByStage", out var mapObj) || mapObj == null)
                throw new Exception("WEAPON_DROP_FORMULA.WeaponByStage missing");

            var strDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(mapObj.ToString());
            var dict = new Dictionary<int, string>();
            foreach (var kv in strDict)
                if (int.TryParse(kv.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var k))
                    dict[k] = kv.Value;

            return new WeaponDropConfig { weaponByStage = dict };
        }
    }

    public sealed class FragmentDropConfig
    {
        public int dropInterval;
        public string formula;
        public float range;
        public Dictionary<string, float> rarityAdjust;
        public List<StageRarityDistribute> valueDistribute;
        public Dictionary<int, string> forceAssign;
    }

    public sealed class StageRarityDistribute
    {
        public int min;
        public int max;
        public List<string> rarities;
    }

    public sealed class WeaponDropConfig
    {
        public Dictionary<int, string> weaponByStage;
    }
}
