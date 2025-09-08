using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;

public class PurchaseManager : MonoBehaviour
{
    private StoreController storeController;
    private bool isInitialized;
    private readonly List<ProductDefinition> productDefs = new()
    {
        new ProductDefinition(ProductIds.DIA_0, ProductType.Consumable),
        new ProductDefinition(ProductIds.DIA_1, ProductType.Consumable)
    };

    private void Awake()
    {
        // 외부 요청으로 구매 시작
        NetworkBroker.PurchaseItem += OnPurchaseRequest;
        _ = InitializeIapAsync();
    }

    private async Task InitializeIapAsync()
    {
        // 컨트롤러 생성
        storeController = UnityIAPServices.StoreController();

        // 필수 이벤트 구독
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;

        // 스토어 연결
        await storeController.Connect();

        // 상품 정보 가져오기
        storeController.FetchProducts(productDefs);
    }

    private void OnProductsFetched(List<Product> products)
    {
        isInitialized = true;
        Debug.Log($"IAP 초기화 완료, 상품 개수 {products.Count}");
        // 보류 주문 복구 처리
        storeController.FetchPurchases();
    }

    private void OnProductsFetchFailed(ProductFetchFailed reason)
    {
        Debug.LogError($"상품 정보 조회 실패, 코드 {reason.FailureReason}");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        // 필요시 기존 확정 주문, 보류 주문, 지연 주문을 점검 가능
        Debug.Log($"기존 주문 로드, 확정 {orders.ConfirmedOrders.Count}, 보류 {orders.PendingOrders.Count}");
    }

    private void OnPurchaseRequest(string productId)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("IAP 미초기화 상태, 나중에 다시 시도");
            return;
        }

        Product product = storeController.GetProductById(productId);
        if (product == null || !product.availableToPurchase)
        {
            Debug.LogError($"구매 불가 상품, id {productId}");
            return;
        }

        storeController.PurchaseProduct(productId); // v5 방식
    }

    // 구매 보류 발생 시점, 여기서 서버 검증 후 확정
    private void OnPurchasePending(PendingOrder pending)
    {
        Debug.Log($"구매 보류 수신, tx {pending.Info.TransactionID}");
        _ = HandlePendingOrderAsync(pending);
    }

    private async Task HandlePendingOrderAsync(PendingOrder pending)
    {
        string receipt = pending.Info.Receipt;
        string productId = TryGetProductId(pending);
        string playerId = AuthenticationService.Instance.PlayerId;

        var args = new Dictionary<string, object>
    {
        { "receipt", receipt },
        { "productId", productId },
        { "playerId", playerId }
    };

        try
        {
            CurrencyResult result = await CloudCodeService.Instance.CallModuleEndpointAsync<CurrencyResult>(
                "PurchaseProcessor",
                "ProcessCurrency",
                args
            );

            //  반드시 체크
            if (result == null)
            {
                Debug.LogError("[IAP] 서버 검증 결과 null ? 보류 유지");
                return;
            }
            if (!result.success)
            {
                Debug.LogError($"[IAP] 서버 검증 실패: {result.message} ? 보류 유지");
                return;
            }



            // 여기서만 확정
            storeController.ConfirmPurchase(pending);
            Debug.Log($"구매 확정 완료, productId {productId}");
            NetworkBroker.OnPurchaseSuccess(productId);




        }
        catch (CloudCodeException cce)
        {
            Debug.LogError($"[IAP] CloudCode 예외 {cce.Reason}, {cce.Message} ? 보류 유지");
            return;
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP] 예외 {e.Message} ? 보류 유지");
            return;
        }
    }


    private static string TryGetProductId(PendingOrder pending)
    {
        // 1) v5 정식: Info.PurchasedProductInfo[0].productId 사용
        try
        {
            var infos = pending?.Info?.PurchasedProductInfo;
            if (infos != null && infos.Count > 0 && !string.IsNullOrEmpty(infos[0].productId))
                return infos[0].productId;
        }
        catch { /* 무시하고 폴백 */ }

        // 2) 최종 폴백
        return "unknown_product";   // v5에는 pending.Product가 없음
    }



    private void OnPurchaseFailed(FailedOrder failed)
    {
        Debug.LogError($"구매 실패, 코드 {failed.FailureReason}");
    }

    private void OnPurchaseConfirmed(Order order)
    {
        Debug.Log($"구매 확정 이벤트, tx {order.Info.TransactionID}");
        // 필요시 영수증이나 트랜잭션 기록을 저장
    }
}
