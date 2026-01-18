using System;
using System.Collections.Generic;
using EnumCollection;
using Newtonsoft.Json.Linq;
using Unity.Services.RemoteConfig;
using UnityEngine;

namespace Store.Gacha
{
    /// <summary>
    /// RemoteConfig에서 가챠 가격 정보 로드
    /// </summary>
    public static class GachaPriceService
    {
        // (가챠 종류, 횟수) → (재화, 수량)
        private static Dictionary<(GachaType, int), (Resource, int)> _prices;

        public static bool TryGetPrice(GachaType type, int num, out Resource resource, out int amount)
        {
            LoadIfNeeded();
            if (_prices.TryGetValue((type, num), out var p))
            {
                resource = p.Item1;
                amount = p.Item2;
                return true;
            }
            resource = default;
            amount = 0;
            return false;
        }

        public static int GetAmount(GachaType type, int num)
        {
            return TryGetPrice(type, num, out _, out var amt) ? amt : 0;
        }

        private static void LoadIfNeeded()
        {
            if (_prices != null) return;

            _prices = new Dictionary<(GachaType, int), (Resource, int)>();

            try
            {
                var json = RemoteConfigService.Instance.appConfig.GetJson("GACHA_INFO");
                if (string.IsNullOrEmpty(json)) throw new Exception("GACHA_INFO가 비어 있습니다.");

                var root = JObject.Parse(json);
                var cost = root["cost"] as JObject ?? throw new Exception("GACHA_INFO.cost 노드를 찾을 수 없습니다.");

                void SetPrice(GachaType gType, int n, JToken node)
                {
                    if (node == null) return;

                    string resourceStr = node["resource"]?.ToString();
                    if (string.IsNullOrEmpty(resourceStr)) return;

                    if (!Enum.TryParse<Resource>(resourceStr, true, out var resEnum)) return;

                    int amount = node["amount"]?.Value<int>() ?? 0;
                    if (amount <= 0) return;

                    _prices[(gType, n)] = (resEnum, amount);
                }

                var weapon = cost["weapon"];
                var costume = cost["costume"];

                SetPrice(GachaType.Weapon, 1, weapon?["single"]);
                SetPrice(GachaType.Weapon, 10, weapon?["multi10"]);
                SetPrice(GachaType.Costume, 1, costume?["single"]);
                SetPrice(GachaType.Costume, 10, costume?["multi10"]);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GachaPriceService] 오류: {e.Message}");
            }
        }
    }
}
