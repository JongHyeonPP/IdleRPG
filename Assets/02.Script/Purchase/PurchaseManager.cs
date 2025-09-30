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

    private readonly List<ProductDefinition> productDefs = new()
    {
        new ProductDefinition(ProductIds.DIA_0, ProductType.Consumable),
        new ProductDefinition(ProductIds.DIA_1, ProductType.Consumable)
    };

    private Action<PurchaseResult> currentCallback;

    private void Awake()
    {
        _ = InitializeIapAsync();
        PlayerBroker.PurchaseCurrency += PurchaseAsync;
        PlayerBroker.RequestGacha += CallGacha;
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
        Debug.Log($"[IAP] 초기화 완료, 상품 개수 {products.Count}");
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
#if UNITY_EDITOR
        CurrencyResult mockResult = LoadCurrencyFromRc(productId);
        PlayerBroker.OnPurchaseCurrency?.Invoke(new PurchaseResult
        {
            Success = mockResult.Resource != Resource.None && mockResult.Value > 0,
            ProductId = productId,
            Message = $"[EditorMock] {mockResult.Resource}+{mockResult.Value}",
            Currency = mockResult
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


    private void OnPurchasePending(PendingOrder pending)
    {
        Debug.Log($"[IAP] 보류 수신, tx {pending.Info.TransactionID}");
        _ = HandlePendingOrderAsync(pending);
    }

    private async Task HandlePendingOrderAsync(PendingOrder pending)
    {
        string receipt = pending.Info.Receipt;
        string playerId = AuthenticationService.Instance.PlayerId;

        // receipt에서 productId 직접 파싱
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
        try
        {
            var catalogJson = RemoteConfigService.Instance.appConfig.GetJson("IAP_CATALOG");
            if (string.IsNullOrEmpty(catalogJson))
                throw new Exception("IAP_CATALOG is empty in RC");

            var rootDict = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, object>>(catalogJson);

            var products = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, object>>(rootDict["products"].ToString());

            var productNode = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, object>>(products[productId].ToString());

            var grants = Newtonsoft.Json.JsonConvert
                .DeserializeObject<List<Dictionary<string, object>>>(productNode["grants"].ToString());

            var grant = grants[0];
            string currencyStr = grant["currency"].ToString();
            int amount = Convert.ToInt32(grant["amount"]);

            if (!Enum.TryParse<Resource>(currencyStr, true, out var resEnum))
                resEnum = Resource.None;

            result = new CurrencyResult
            {
                Resource = resEnum,
                Value = amount
            };

            Debug.Log($"[IAP][EditorMock] {productId} → {resEnum}+{amount}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP][EditorMock] RC load failed: {e.Message}");
            result = new CurrencyResult
            {
                Resource = Resource.None,
                Value = 0
            };
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
        currentCallback?.Invoke(new PurchaseResult
        {
            Success = false,
            ProductId = "unknown",
            Message = failed.FailureReason.ToString()
        });
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


#if UNITY_EDITOR
    private CurrencyResult LoadCurrencyFromRc(string productId)
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

            var productEntry = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, object>>(productsDict[productId].ToString());

            var grants = Newtonsoft.Json.JsonConvert
                .DeserializeObject<List<Dictionary<string, object>>>(productEntry["grants"].ToString());

            var grant = grants[0];
            string currencyStr = grant["currency"].ToString();
            int amount = Convert.ToInt32(grant["amount"]);

            if (!Enum.TryParse<Resource>(currencyStr, true, out var resEnum))
                resEnum = Resource.None;

            return new CurrencyResult
            {
                Resource = resEnum,
                Value = amount
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP][EditorMock] RC load failed: {e.Message}");
            return new CurrencyResult
            {
                Resource = Resource.None,
                Value = 0
            };
        }
    }
#endif
}
