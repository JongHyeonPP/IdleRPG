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
        // �ܺ� ��û���� ���� ����
        NetworkBroker.PurchaseItem += OnPurchaseRequest;
        _ = InitializeIapAsync();
    }

    private async Task InitializeIapAsync()
    {
        // ��Ʈ�ѷ� ����
        storeController = UnityIAPServices.StoreController();

        // �ʼ� �̺�Ʈ ����
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;

        // ����� ����
        await storeController.Connect();

        // ��ǰ ���� ��������
        storeController.FetchProducts(productDefs);
    }

    private void OnProductsFetched(List<Product> products)
    {
        isInitialized = true;
        Debug.Log($"IAP �ʱ�ȭ �Ϸ�, ��ǰ ���� {products.Count}");
        // ���� �ֹ� ���� ó��
        storeController.FetchPurchases();
    }

    private void OnProductsFetchFailed(ProductFetchFailed reason)
    {
        Debug.LogError($"��ǰ ���� ��ȸ ����, �ڵ� {reason.FailureReason}");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        // �ʿ�� ���� Ȯ�� �ֹ�, ���� �ֹ�, ���� �ֹ��� ���� ����
        Debug.Log($"���� �ֹ� �ε�, Ȯ�� {orders.ConfirmedOrders.Count}, ���� {orders.PendingOrders.Count}");
    }

    private void OnPurchaseRequest(string productId)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("IAP ���ʱ�ȭ ����, ���߿� �ٽ� �õ�");
            return;
        }

        Product product = storeController.GetProductById(productId);
        if (product == null || !product.availableToPurchase)
        {
            Debug.LogError($"���� �Ұ� ��ǰ, id {productId}");
            return;
        }

        storeController.PurchaseProduct(productId); // v5 ���
    }

    // ���� ���� �߻� ����, ���⼭ ���� ���� �� Ȯ��
    private void OnPurchasePending(PendingOrder pending)
    {
        Debug.Log($"���� ���� ����, tx {pending.Info.TransactionID}");
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

            //  �ݵ�� üũ
            if (result == null)
            {
                Debug.LogError("[IAP] ���� ���� ��� null ? ���� ����");
                return;
            }
            if (!result.success)
            {
                Debug.LogError($"[IAP] ���� ���� ����: {result.message} ? ���� ����");
                return;
            }



            // ���⼭�� Ȯ��
            storeController.ConfirmPurchase(pending);
            Debug.Log($"���� Ȯ�� �Ϸ�, productId {productId}");
            NetworkBroker.OnPurchaseSuccess(productId);




        }
        catch (CloudCodeException cce)
        {
            Debug.LogError($"[IAP] CloudCode ���� {cce.Reason}, {cce.Message} ? ���� ����");
            return;
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP] ���� {e.Message} ? ���� ����");
            return;
        }
    }


    private static string TryGetProductId(PendingOrder pending)
    {
        // 1) v5 ����: Info.PurchasedProductInfo[0].productId ���
        try
        {
            var infos = pending?.Info?.PurchasedProductInfo;
            if (infos != null && infos.Count > 0 && !string.IsNullOrEmpty(infos[0].productId))
                return infos[0].productId;
        }
        catch { /* �����ϰ� ���� */ }

        // 2) ���� ����
        return "unknown_product";   // v5���� pending.Product�� ����
    }



    private void OnPurchaseFailed(FailedOrder failed)
    {
        Debug.LogError($"���� ����, �ڵ� {failed.FailureReason}");
    }

    private void OnPurchaseConfirmed(Order order)
    {
        Debug.Log($"���� Ȯ�� �̺�Ʈ, tx {order.Info.TransactionID}");
        // �ʿ�� �������̳� Ʈ����� ����� ����
    }
}
