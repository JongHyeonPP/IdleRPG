using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.RemoteConfig;
using EnumCollection;

public class PurchaseManager : MonoBehaviour
{
    private StoreController storeController;
    private bool isInitialized;

    private List<ProductDefinition> productDefs;
    private Dictionary<string, string> productSources = new(); // productId -> source
    private Dictionary<string, (Resource resource, int amount)> productGrants = new(); // productId -> 보상 정보

    private void Awake()
    {
        _ = InitializeProductsFromRc();
        PlayerBroker.PurchaseCurrency += PurchaseAsync;
        PlayerBroker.RequestGacha += CallGacha;
    }

    private async Task InitializeProductsFromRc()
    {
        try
        {
            var catalogJson = RemoteConfigService.Instance.appConfig.GetJson("IAP_CATALOG");
            if (string.IsNullOrEmpty(catalogJson))
                throw new Exception("IAP_CATALOG is empty in RC");

            var rootDict = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, object>>(catalogJson);

            var productsDict = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, object>>(rootDict["products"].ToString());

            productDefs = new List<ProductDefinition>();

            foreach (var kv in productsDict)
            {
                string productId = kv.Key;

                var productEntry = Newtonsoft.Json.JsonConvert
                    .DeserializeObject<Dictionary<string, object>>(kv.Value.ToString());

                string typeStr = productEntry.ContainsKey("type") ? productEntry["type"].ToString() : "Consumable";
                string sourceStr = productEntry.ContainsKey("source") ? productEntry["source"].ToString() : "purchase";

                ProductType type = typeStr switch
                {
                    "Consumable" => ProductType.Consumable,
                    "NonConsumable" => ProductType.NonConsumable,
                    "Subscription" => ProductType.Subscription,
                    _ => ProductType.Consumable
                };

                // 광고 상품은 IAP 정의에 추가하지 않음
                if (sourceStr != "advertise")
                {
                    productDefs.Add(new ProductDefinition(productId, type));
                }

                productSources[productId] = sourceStr;

                // 보상 정보 캐싱
                if (productEntry.ContainsKey("grants"))
                {
                    var grants = Newtonsoft.Json.JsonConvert
                        .DeserializeObject<List<Dictionary<string, object>>>(productEntry["grants"].ToString());

                    var grant = grants[0];
                    string currencyStr = grant["currency"].ToString();
                    int amount = Convert.ToInt32(grant["amount"]);

                    if (!Enum.TryParse<Resource>(currencyStr, true, out var resEnum))
                        resEnum = Resource.None;

                    productGrants[productId] = (resEnum, amount);
                }
            }

            await InitializeIapAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP] RC에서 상품 정의 불러오기 실패: {e.Message}");
        }
    }

    private async Task InitializeIapAsync()
    {
        storeController = UnityIAPServices.StoreController();

        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;

        await storeController.Connect();
        storeController.FetchProducts(productDefs);
    }

    private void OnProductsFetched(List<Product> products)
    {
        isInitialized = true;

        // 로그를 한 번에 모아 찍기
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("=== IAP Products ===");

        // IAP에 등록된 상품
        foreach (var product in products)
        {
            string productId = product.definition.id;
            decimal price = product.metadata.localizedPrice;
            string priceString = product.metadata.localizedPriceString;
            string type = product.definition.type.ToString();

            string currencyStr = "Unknown";
            int amount = 0;

            if (productGrants.ContainsKey(productId))
            {
                var grant = productGrants[productId];
                currencyStr = grant.resource.ToString();
                amount = grant.amount;
            }

            sb.AppendLine($"ID={productId}, type={type}, price={priceString}, grant={currencyStr}+{amount}");
        }

        // 광고 상품
        sb.AppendLine("=== Advertise Products ===");
        foreach (var kv in productSources)
        {
            if (kv.Value == "advertise" && productGrants.TryGetValue(kv.Key, out var grant))
            {
                sb.AppendLine($"ID={kv.Key}, type=advertise, grant={grant.resource}+{grant.amount}");
            }
        }

        Debug.Log(sb.ToString());

        storeController.FetchPurchases();
    }


    private void OnProductsFetchFailed(ProductFetchFailed reason)
    {
        Debug.LogError($"[IAP] 상품 정보 조회 실패, 코드 {reason.FailureReason}");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        Debug.Log($"[IAP] 기존 주문 로드, 확정 {orders.ConfirmedOrders.Count}, 보류 {orders.PendingOrders.Count}");
    }

    private void PurchaseAsync(string productId)
    {
        if (!productSources.TryGetValue(productId, out var source))
        {
            Debug.LogError($"[IAP] 알 수 없는 productId: {productId}");
            return;
        }

        if (source == "advertise")
        {
            HandleAdvertiseProduct(productId);
            return;
        }

#if UNITY_EDITOR
        (Resource resource, int amount) mock = productGrants.ContainsKey(productId) ? productGrants[productId] : (Resource.None, 0);
        PlayerBroker.OnPurchaseCurrency?.Invoke(new PurchaseResult
        {
            Success = mock.resource != Resource.None && mock.amount > 0,
            ProductId = productId,
            Message = $"[EditorMock] {mock.resource}+{mock.amount}",
            Currency = new CurrencyResult { Resource = mock.resource, Value = mock.amount }
        });
        return;
#endif

        if (!isInitialized)
        {
            PlayerBroker.OnPurchaseCurrency?.Invoke(new PurchaseResult
            {
                Success = false,
                ProductId = productId,
                Message = "IAP not initialized"
            });
            return;
        }

        Product product = storeController.GetProductById(productId);
        if (product == null || !product.availableToPurchase)
        {
            PlayerBroker.OnPurchaseCurrency?.Invoke(new PurchaseResult
            {
                Success = false,
                ProductId = productId,
                Message = "Product unavailable"
            });
            return;
        }

        storeController.PurchaseProduct(productId);
    }

    private void HandleAdvertiseProduct(string productId)
    {
        if (!productGrants.TryGetValue(productId, out (Resource resource, int amount) grant))
        {
            Debug.LogError($"[Advertise] 보상 정보를 찾을 수 없습니다: {productId}");
            return;
        }
        Debug.Log($"[Advertise] 광고 보상 지급: {grant.resource}+{grant.amount}");
        NetworkBroker.LoadAd(grant);
    }

    private void OnPurchasePending(PendingOrder pending)
    {
        Debug.Log($"[IAP] 보류 수신, tx {pending.Info.TransactionID}");
        _ = HandlePendingOrderAsync(pending);
    }

    private async Task HandlePendingOrderAsync(PendingOrder pending)
    {
        string receipt = pending.Info.Receipt;
        string playerId = AuthenticationService.Instance.PlayerId;

        string productId = "unknown_product";
        try
        {
            var root = Newtonsoft.Json.Linq.JObject.Parse(receipt);
            var payloadStr = root["Payload"]?.ToString();
            if (!string.IsNullOrEmpty(payloadStr))
            {
                var payload = Newtonsoft.Json.Linq.JObject.Parse(payloadStr);
                var innerJson = payload["json"]?.ToString();
                if (!string.IsNullOrEmpty(innerJson))
                {
                    var inner = Newtonsoft.Json.Linq.JObject.Parse(innerJson);
                    productId = inner["productId"]?.ToString() ?? "unknown_product";
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP] receipt 파싱 실패: {e.Message}");
        }

        var args = new Dictionary<string, object>
        {
            { "receipt", receipt },
            { "productId", productId },
            { "playerId", playerId }
        };

        CurrencyResult result;

#if UNITY_EDITOR
        if (productGrants.TryGetValue(productId, out var grant))
        {
            result = new CurrencyResult { Resource = grant.resource, Value = grant.amount };
            Debug.Log($"[IAP][EditorMock] {productId} -> {grant.resource}+{grant.amount}");
        }
        else
        {
            result = new CurrencyResult { Resource = Resource.None, Value = 0 };
            Debug.LogError($"[IAP][EditorMock] 보상 정보 없음: {productId}");
        }
#else
        result = await CloudCodeService.Instance
            .CallModuleEndpointAsync<CurrencyResult>(
                "PurchaseProcessor",
                "ProcessCurrency",
                args
            );
#endif

        if (result == null || result.Value <= 0 || result.Resource == Resource.None)
        {
            PlayerBroker.OnPurchaseCurrency?.Invoke(new PurchaseResult
            {
                Success = false,
                ProductId = productId,
                Message = "Server verification failed",
                Currency = result
            });
            return;
        }

#if !UNITY_EDITOR
        storeController.ConfirmPurchase(pending);
#endif

        Debug.Log($"[IAP] 구매 확정 완료, productId {productId}, {result.Resource}+{result.Value}");

        PlayerBroker.OnPurchaseCurrency?.Invoke(new PurchaseResult
        {
            Success = true,
            ProductId = productId,
            Message = $"Purchase confirmed: {result.Resource}+{result.Value}",
            Currency = result
        });
    }

    private void OnPurchaseFailed(FailedOrder failed)
    {
        Debug.LogError($"[IAP] 구매 실패, 코드 {failed.FailureReason}");
    }

    private void OnPurchaseConfirmed(Order order)
    {
        Debug.Log($"[IAP] 구매 확정 이벤트, tx {order.Info.TransactionID}");
    }

    public async void CallGacha(GachaType type, int num)
    {
        Dictionary<string, object> args = new()
        {
            { "gachaType", type.ToString() },
            { "gachaNum",  num }
        };

        try
        {
            GachaResult result = await CloudCodeService.Instance
                .CallModuleEndpointAsync<GachaResult>(
                    "PurchaseProcessor",
                    "ProcessGacha",
                    args);

            PlayerBroker.OnRequestGacha?.Invoke(result);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Gacha] 예외 발생: {e.Message}");
            PlayerBroker.OnRequestGacha?.Invoke(new GachaResult
            {
                Success = false,
                Message = e.Message,
                Items = new List<string>()
            });
        }
    }
}
