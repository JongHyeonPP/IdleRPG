using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnumCollection;
using UnityEngine;

namespace Store.Money
{
    /// <summary>
    /// 상점 상품(Money) 목록 관리
    /// 아이콘 딕셔너리는 Initialize에서 전달받음
    /// </summary>
    public class MoneyStoreController : MonoBehaviour
    {
        // 자동 참조 (Initialize에서 설정)
        private StoreMoneyListController _listController;
        private Dictionary<string, Texture2D> _iconDic;

        public void Initialize(StoreMoneyListController listController, Dictionary<string, Texture2D> iconDic)
        {
            _listController = listController;
            _iconDic = iconDic ?? new Dictionary<string, Texture2D>();
        }

        public void RefreshProducts()
        {
            if (_listController == null || PurchaseManager.Instance == null) return;

            var pm = PurchaseManager.Instance;
            var items = new List<StoreMoneyItemData>();

            var products = pm.GetProducts(includeAdvertise: true)
                .OrderBy(p => CurrencyRank(p.grant.res))
                .ThenBy(p => AdRank(pm.IsAdvertise(p.productId)))
                .ThenBy(p => PriceKey(p.priceString))
                .ThenBy(p => p.grant.amt)
                .ToList();

            foreach (var p in products)
            {
                string priceString = string.IsNullOrEmpty(p.priceString) ? "-" : p.priceString;
                bool isAd = pm.IsAdvertise(p.productId) || p.source == "advertise";
                string moneyLabel = isAd ? "광고보기" : priceString;

                // 아이콘 조회 (재화 타입으로)
                Texture2D icon = null;
                var resKey = p.grant.res.ToString();
                if (_iconDic.TryGetValue(resKey, out var tex))
                    icon = tex;

                items.Add(new StoreMoneyItemData
                {
                    Gold = p.grant.amt.ToString(),
                    GoldEx = p.grant.res.ToString(),
                    Money = moneyLabel,
                    Icon = icon,
                    OnClick = () => TriggerProduct(p.productId)
                });
            }

            _listController.SetItems(items);
        }

        private static decimal PriceKey(string priceString)
        {
            if (string.IsNullOrWhiteSpace(priceString)) return decimal.MaxValue;
            var cleaned = new string(priceString.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
            cleaned = cleaned.Replace(",", ".");
            return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : decimal.MaxValue;
        }

        private static void TriggerProduct(string productId)
        {
            if (PurchaseManager.Instance == null)
            {
                Debug.LogError("[MoneyStoreController] PurchaseManager.Instance is null");
                return;
            }
            PlayerBroker.PurchaseCurrency.Invoke(productId);
        }

        private static int CurrencyRank(Resource r) => r switch
        {
            Resource.Dia => 0,
            Resource.Clover => 1,
            _ => 2
        };

        private static int AdRank(bool isAd) => isAd ? 0 : 1;
    }
}
