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
        public async Task<List<string>> ProcessCurrencyAsync(
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

                List<string> result;

                switch (gachaType)
                {
                    case GachaType.Weapon:
                        result = await ProcessWeaponGacha(gachaNum, context, gameApiClient, purchaseSystem);
                        break;

                    case GachaType.Costume:
                        result = await ProcessCostumeGacha(gachaNum, context, gameApiClient, purchaseSystem);
                        break;

                    default:
                        throw new Exception($"지원하지 않는 GachaType: {gachaType}");
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"ProcessGacha 실패: {e.Message}");
                throw;
            }
        }

        // -------------------- Weapon --------------------
        private async Task<List<string>> ProcessWeaponGacha(
            int gachaNum,
            IExecutionContext context,
            IGameApiClient gameApiClient,
            IPurchaseSystem purchaseSystem
        )
        {
            // 1. Remote Config 읽기
            var gachaInfo = purchaseSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "GACHA_INFO");
            JObject gachaInfoObj = JObject.FromObject(gachaInfo);

            // 2. GameData 불러오기
            var res = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId, new() { "GameData" });

            GameData data;
            if (res.Data.Results.Count == 0 || res.Data.Results[0].Value == null)
            {
                data = new GameData { level = 1, maxStageNum = 1, currentStageNum = 1, gachaThreshold = 0 };
            }
            else
            {
                data = JsonConvert.DeserializeObject<GameData>(res.Data.Results[0].Value.ToString())
                       ?? new GameData { level = 1, maxStageNum = 1, currentStageNum = 1, gachaThreshold = 0 };
            }

            // 3. 가격 계산 및 재화 차감
            ResolveCost(gachaInfoObj, "weapon", gachaNum, out string resource, out int amount);
            Spend(data, resource, amount);

            int threshold = data.gachaThreshold;

            // 4. 무기 풀 로드
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
                    throw new Exception($"rarity {rarity}에 해당하는 무기가 없습니다.");

                string picked = pool[random.Next(pool.Count)];
                result.Add(picked);

                threshold++;
            }

            data.gachaThreshold = threshold;

            // 5. 저장
            await gameApiClient.CloudSaveData.SetItemAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId,
                new("GameData", JsonConvert.SerializeObject(data)));

            _logger.LogInformation(
                $"Weapon 가챠 완료 | Num={gachaNum}, Cost={resource}:{amount}, NewThreshold={threshold}, Remain={data.dia}, Result={string.Join(", ", result)}");


            return result;
        }

        // -------------------- Costume --------------------
        private async Task<List<string>> ProcessCostumeGacha(
            int gachaNum,
            IExecutionContext context,
            IGameApiClient gameApiClient,
            IPurchaseSystem purchaseSystem
        )
        {
            var gachaInfo = purchaseSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "GACHA_INFO");
            JObject gachaInfoObj = JObject.FromObject(gachaInfo);

            // GameData 로드
            var res = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId, new() { "GameData" });

            GameData data;
            if (res.Data.Results.Count == 0 || res.Data.Results[0].Value == null)
            {
                data = new GameData();
            }
            else
            {
                data = JsonConvert.DeserializeObject<GameData>(res.Data.Results[0].Value.ToString())
                       ?? new GameData();
            }

            // 가격 계산 및 차감
            ResolveCost(gachaInfoObj, "costume", gachaNum, out string resource, out int amount);
            Spend(data, resource, amount);

            Dictionary<string, object> itemBundle =
                purchaseSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "ITEM_NAME_BUNDLE");

            if (!itemBundle.ContainsKey("costumes"))
                throw new Exception("ITEM_NAME_BUNDLE에 'costumes' 키가 없습니다.");

            Dictionary<string, List<string>> costumeBundle =
                JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(itemBundle["costumes"].ToString());

            List<string> bodyList = costumeBundle.GetValueOrDefault("Body", new());
            List<string> hairList = costumeBundle.GetValueOrDefault("Hair", new());
            List<string> helmetList = costumeBundle.GetValueOrDefault("Helmet", new());
            List<string> pantList = costumeBundle.GetValueOrDefault("Pant", new());

            List<string> pool = new();
            pool.AddRange(bodyList);
            pool.AddRange(hairList);
            pool.AddRange(helmetList);
            pool.AddRange(pantList);

            if (pool.Count == 0)
                throw new Exception("코스튬 풀 데이터가 비어 있습니다.");

            Random random = new Random();
            List<string> result = new();

            for (int i = 0; i < gachaNum; i++)
            {
                string picked = pool[random.Next(pool.Count)];
                result.Add(picked);
            }

            // 저장
            await gameApiClient.CloudSaveData.SetItemAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId,
                new("GameData", JsonConvert.SerializeObject(data)));
            _logger.LogInformation(
                $"Costume 가챠 완료 | Num={gachaNum}, Cost={resource}:{amount}, Remain={data.dia}, Result={string.Join(", ", result)}");


            return result;
        }

        // -------------------- Helpers --------------------
        private void Spend(GameData data, string resource, int amount)
        {
            if (string.IsNullOrWhiteSpace(resource))
                throw new Exception("재화 키가 비었습니다.");
            if (amount <= 0)
                throw new Exception($"차감 금액이 올바르지 않습니다. amount={amount}");

            switch (resource)
            {
                case "dia":
                    if (data.dia < amount)
                        throw new Exception($"다이아 부족 | 보유: {data.dia}, 필요: {amount}");
                    data.dia -= amount;
                    break;
                default:
                    throw new Exception($"지원하지 않는 재화 타입: {resource}");
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
}
