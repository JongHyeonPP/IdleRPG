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
    /// </summary>
    public class MoneyStoreController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StoreMoneyListController _listController;
        [SerializeField] private List<ProductIconEntry> _iconEntries = new();

        private Dictionary<string, Texture2D> _iconDic;

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
                var icon = GetIconTex(p.productId);

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

        private Texture2D GetIconTex(string productId)
        {
            if (_iconDic == null)
            {
                _iconDic = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
                foreach (var e in _iconEntries)
                    if (!string.IsNullOrWhiteSpace(e.key) && e.iconTex != null)
                        _iconDic[e.key] = e.iconTex;
            }

            return !string.IsNullOrEmpty(productId) && _iconDic.TryGetValue(productId, out var tex) ? tex : null;
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
