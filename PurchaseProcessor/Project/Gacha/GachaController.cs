using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Purchase;

namespace Purchase.Gacha
{
    public class GachaController
    {
        private readonly ILogger<GachaController> _logger;

        public GachaController(ILogger<GachaController> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction("ProcessGacha")]
        public async Task<GachaResult> ProcessCurrencyAsync(
            GachaType gachaType,
            int gachaNum,
            IExecutionContext context,
            IGameApiClient gameApiClient,
            IPurchaseSystem purchaseSystem
        )
        {
            try
            {
                ValidateCount(gachaNum);

                GachaResult result;

                switch (gachaType)
                {
                    case GachaType.Weapon:
                        result = await ProcessWeaponGacha(gachaNum, context, gameApiClient, purchaseSystem);
                        break;

                    case GachaType.Costume:
                        result = await ProcessCostumeGacha(gachaNum, context, gameApiClient, purchaseSystem);
                        break;

                    default:
                        return new GachaResult
                        {
                            Success = false,
                            Message = $"지원하지 않는 GachaType: {gachaType}"
                        };
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"ProcessGacha 실패: {e.Message}");
                return new GachaResult
                {
                    Success = false,
                    Message = $"예외 발생: {e.Message}"
                };
            }
        }

        // -------------------- Weapon --------------------
        private async Task<GachaResult> ProcessWeaponGacha(
            int gachaNum,
            IExecutionContext context,
            IGameApiClient gameApiClient,
            IPurchaseSystem purchaseSystem
        )
        {
            var gachaInfo = purchaseSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "GACHA_INFO");
            JObject gachaInfoObj = JObject.FromObject(gachaInfo);

            var res = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId, new() { "GameData" });

            GameData data = res.Data.Results.Count == 0 || res.Data.Results[0].Value == null
                ? new GameData { level = 1, maxStageNum = 1, currentStageNum = 1, gachaThreshold = 0 }
                : JsonConvert.DeserializeObject<GameData>(res.Data.Results[0].Value.ToString())
                  ?? new GameData { level = 1, maxStageNum = 1, currentStageNum = 1, gachaThreshold = 0 };

            ResolveCost(gachaInfoObj, "weapon", gachaNum, out string resource, out int amount);

            if (!Spend(data, resource, amount, out string failMsg))
            {
                return new GachaResult
                {
                    Success = false,
                    Message = failMsg,
                    RemainDia = data.dia
                };
            }

            int threshold = data.gachaThreshold;

            Dictionary<string, object> itemBundle =
                purchaseSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "ITEM_NAME_BUNDLE");

            List<string> weaponList =
                JsonConvert.DeserializeObject<List<string>>(itemBundle["weapons"].ToString());

            TierConfig tiers =
                JsonConvert.DeserializeObject<TierConfig>(gachaInfoObj.ToString());

            Random random = new Random();
            List<string> result = new();

            for (int i = 0; i < gachaNum; i++)
            {
                Tier tier =
                    tiers.Tiers.Where(t => threshold >= t.Threshold)
                               .OrderByDescending(t => t.Threshold)
                               .FirstOrDefault()
                    ?? tiers.Tiers[0];

                int rarity = GetRarityByRates(random, tier.Rates);

                List<string> pool = weaponList.Where(w => GetRarityFromId(w) == rarity).ToList();
                if (pool.Count == 0)
                    return new GachaResult
                    {
                        Success = false,
                        Message = $"rarity {rarity}에 해당하는 무기가 없습니다.",
                        RemainDia = data.dia
                    };

                string picked = pool[random.Next(pool.Count)];
                result.Add(picked);

                threshold++;
            }

            data.gachaThreshold = threshold;

            ApplyWeaponResult(data, result);


            await gameApiClient.CloudSaveData.SetItemAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId,
                new("GameData", JsonConvert.SerializeObject(data)));

            _logger.LogInformation(
                $"Weapon 가챠 완료 | Num={gachaNum}, Cost={resource}:{amount}, NewThreshold={threshold}, Remain={data.dia}, Result={string.Join(", ", result)}");

            return new GachaResult
            {
                Success = true,
                Message = "Weapon 가챠 성공",
                Items = result,
                RemainDia = data.dia
            };
        }

        // -------------------- Costume --------------------
        private async Task<GachaResult> ProcessCostumeGacha(
            int gachaNum,
            IExecutionContext context,
            IGameApiClient gameApiClient,
            IPurchaseSystem purchaseSystem
        )
        {
            var gachaInfo = purchaseSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "GACHA_INFO");
            JObject gachaInfoObj = JObject.FromObject(gachaInfo);

            var res = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId, new() { "GameData" });

            GameData data = res.Data.Results.Count == 0 || res.Data.Results[0].Value == null
                ? new GameData()
                : JsonConvert.DeserializeObject<GameData>(res.Data.Results[0].Value.ToString())
                  ?? new GameData();

            ResolveCost(gachaInfoObj, "costume", gachaNum, out string resource, out int amount);

            if (!Spend(data, resource, amount, out string failMsg))
            {
                return new GachaResult
                {
                    Success = false,
                    Message = failMsg,
                    RemainDia = data.dia
                };
            }

            Dictionary<string, object> itemBundle =
                purchaseSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "ITEM_NAME_BUNDLE");

            if (!itemBundle.ContainsKey("costumes"))
            {
                return new GachaResult
                {
                    Success = false,
                    Message = "ITEM_NAME_BUNDLE에 'costumes' 키가 없습니다.",
                    RemainDia = data.dia
                };
            }

            Dictionary<string, List<string>> costumeBundle =
                JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(itemBundle["costumes"].ToString());

            List<string> pool = new();
            pool.AddRange(costumeBundle.GetValueOrDefault("Body", new()));
            pool.AddRange(costumeBundle.GetValueOrDefault("Hair", new()));
            pool.AddRange(costumeBundle.GetValueOrDefault("Helmet", new()));
            pool.AddRange(costumeBundle.GetValueOrDefault("Pant", new()));

            if (pool.Count == 0)
            {
                return new GachaResult
                {
                    Success = false,
                    Message = "코스튬 풀 데이터가 비어 있습니다.",
                    RemainDia = data.dia
                };
            }

            Random random = new Random();
            List<string> result = new();

            for (int i = 0; i < gachaNum; i++)
            {
                string picked = pool[random.Next(pool.Count)];
                result.Add(picked);
            }

            ApplyCostumeResult(data, result);

            await gameApiClient.CloudSaveData.SetItemAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId,
                new("GameData", JsonConvert.SerializeObject(data)));

            _logger.LogInformation(
                $"Costume 가챠 완료 | Num={gachaNum}, Cost={resource}:{amount}, Remain={data.dia}, Result={string.Join(", ", result)}");

            return new GachaResult
            {
                Success = true,
                Message = "Costume 가챠 성공",
                Items = result,
                RemainDia = data.dia
            };
        }

        // -------------------- Helpers --------------------
        private bool Spend(GameData data, string resource, int amount, out string message)
        {
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(resource))
            {
                message = "재화 키가 비었습니다.";
                return false;
            }

            if (amount <= 0)
            {
                message = $"차감 금액이 올바르지 않습니다. amount={amount}";
                return false;
            }

            switch (resource)
            {
                case "dia":
                    if (data.dia < amount)
                    {
                        message = $"다이아 부족 | 보유: {data.dia}, 필요: {amount}";
                        return false;
                    }
                    data.dia -= amount;
                    return true;

                default:
                    message = $"지원하지 않는 재화 타입: {resource}";
                    return false;
            }
        }

        private void ValidateCount(int gachaNum)
        {
            if (gachaNum != 1 && gachaNum != 10)
                throw new Exception($"가챠 수량은 1 또는 10만 허용됩니다. 입력값 {gachaNum}");
        }

        private int GetRarityByRates(Random random, List<double> rates)
        {
            double roll = random.NextDouble();
            double sum = 0;
            for (int i = 0; i < rates.Count; i++)
            {
                sum += rates[i];
                if (roll <= sum) return i;
            }
            return rates.Count - 1;
        }

        private int GetRarityFromId(string weaponId)
        {
            string[] parts = weaponId.Split('_');
            if (parts.Length < 2) return 0;
            return int.Parse(parts[1]) / 100;
        }

        private void ResolveCost(JObject gachaInfoObj, string type, int gachaNum, out string resource, out int amount)
        {
            var node = gachaNum == 1
                ? gachaInfoObj["cost"]?[type]?["single"]
                : gachaInfoObj["cost"]?[type]?["multi10"];

            if (node == null)
                throw new Exception($"GACHA_INFO cost 설정을 찾을 수 없습니다. type={type}, count={gachaNum}");

            resource = node["resource"]?.ToString();
            if (string.IsNullOrEmpty(resource))
                throw new Exception($"GACHA_INFO cost.resource가 비었습니다. type={type}, count={gachaNum}");

            if (!int.TryParse(node["amount"]?.ToString(), out amount) || amount <= 0)
                throw new Exception($"GACHA_INFO cost.amount가 올바르지 않습니다. type={type}, count={gachaNum}");
        }

        private void ApplyWeaponResult(GameData data, List<string> weapons)
        {
            foreach (var weaponId in weapons)
            {
                if (string.IsNullOrWhiteSpace(weaponId))
                    continue;

                if (data.weaponCount.ContainsKey(weaponId))
                    data.weaponCount[weaponId]++;
                else
                    data.weaponCount[weaponId] = 1;
            }
        }

        private void ApplyCostumeResult(GameData data, List<string> costumes)
        {
            foreach (var costumeId in costumes)
            {
                if (string.IsNullOrWhiteSpace(costumeId))
                    continue;

                if (!data.ownedCostumes.Contains(costumeId))
                    data.ownedCostumes.Add(costumeId);
            }
        }
    }

    public enum GachaType
    {
        Weapon,
        Costume
    }

    public class TierConfig
    {
        [JsonProperty("tiers")]
        public List<Tier> Tiers { get; set; }
    }

    public class Tier
    {
        [JsonProperty("threshold")]
        public int Threshold { get; set; }

        [JsonProperty("rates")]
        public List<double> Rates { get; set; }
    }

    public class GachaResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Items { get; set; } = new();
        public int RemainDia { get; set; }
    }
}
