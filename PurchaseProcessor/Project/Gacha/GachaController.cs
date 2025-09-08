using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            _logger.LogDebug($"gachaType : {gachaType}, gachaNum : {gachaNum}");
            try
            {
                List<string> result = new List<string>();

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
            // 1) GameData 불러오기(threshold 사용)
            var res =
                await gameApiClient.CloudSaveData.GetItemsAsync(
                    context, context.ServiceToken, context.ProjectId, context.PlayerId, new() { "GameData" });

            GameData data;
            if (res.Data.Results.Count == 0 || res.Data.Results[0].Value == null)
            {
                data = new GameData { level = 1, maxStageNum = 1, currentStageNum = 1 };
            }
            else
            {
                data = JsonConvert.DeserializeObject<GameData>(res.Data.Results[0].Value.ToString());
            }
            int threshold = data.gachaThreshold;
            // 2) RC 읽기
            Dictionary<string, object> itemBundle =
                purchaseSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "ITEM_NAME_BUNDLE");

            List<string> weaponList =
                JsonConvert.DeserializeObject<List<string>>(itemBundle["weapons"].ToString());

            TierConfig tiers =
                purchaseSystem.GetRemoteConfig<TierConfig>(context, gameApiClient, "GACHA_PROBABILITY");

            Random random = new Random();
            List<string> result = new List<string>();

            // 3) 뽑기
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

                threshold++; // 무기만 threshold 갱신
            }

            // 4) 저장
            data.gachaThreshold = threshold;
            _logger.LogDebug(JsonConvert.SerializeObject(data));
            await gameApiClient.CloudSaveData.SetItemAsync(
                context, context.ServiceToken, context.ProjectId, context.PlayerId,
                new("GameData", JsonConvert.SerializeObject(data)));

            _logger.LogInformation(
                $"Weapon 가챠 완료 | Num={gachaNum}, NewThreshold={threshold}, Result={string.Join(", ", result)}");

            return result;
        }

        // -------------------- Costume (균등 확률) --------------------
        private async Task<List<string>> ProcessCostumeGacha(
            int gachaNum,
            IExecutionContext context,
            IGameApiClient gameApiClient,
            IPurchaseSystem purchaseSystem
        )
        {
            // RC에서 코스튬 풀만 로드 (threshold/tiers 미사용)
            Dictionary<string, object> itemBundle =
                purchaseSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "ITEM_NAME_BUNDLE");

            if (!itemBundle.ContainsKey("costumes"))
                throw new Exception("ITEM_NAME_BUNDLE에 'costumes' 키가 없습니다.");

            Dictionary<string, List<string>> costumeBundle =
                JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(itemBundle["costumes"].ToString());

            List<string> bodyList = costumeBundle.ContainsKey("Body") ? costumeBundle["Body"] : new List<string>();
            List<string> hairList = costumeBundle.ContainsKey("Hair") ? costumeBundle["Hair"] : new List<string>();
            List<string> helmetList = costumeBundle.ContainsKey("Helmet") ? costumeBundle["Helmet"] : new List<string>();
            List<string> pantList = costumeBundle.ContainsKey("Pant") ? costumeBundle["Pant"] : new List<string>();

            // 하나의 풀로 합침 (균등 추첨)
            List<string> pool = new List<string>(bodyList.Count + hairList.Count + helmetList.Count + pantList.Count);
            pool.AddRange(bodyList);
            pool.AddRange(hairList);
            pool.AddRange(helmetList);
            pool.AddRange(pantList);

            if (pool.Count == 0)
                throw new Exception("코스튬 풀 데이터가 비어 있습니다.");

            Random random = new Random();
            List<string> result = new List<string>();

            for (int i = 0; i < gachaNum; i++)
            {
                string picked = pool[random.Next(pool.Count)];
                result.Add(picked);
            }

            _logger.LogInformation($"Costume 가챠 완료 | Num={gachaNum}, Result={string.Join(", ", result)}");
            return result;
        }

        // -------------------- Helpers --------------------
        private static int GetRarityByRates(Random random, List<double> rates)
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

        // 아이템 네이밍 규칙: Type_ABC  → ABC/100 이 rarity
        private static int GetRarityFromId(string weaponId)
        {
            string[] parts = weaponId.Split('_');
            if (parts.Length < 2) return 0;
            int code = int.Parse(parts[1]) / 100;
            return code; // 0:Common, 1:Uncommon, 2:Rare, 3:Unique, 4:Legendary, 5:Mythic
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
