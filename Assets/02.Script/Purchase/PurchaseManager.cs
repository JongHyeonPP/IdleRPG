using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Purchasing;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudCode.GeneratedBindings.OfflineReward;
using Unity.Services.CloudCode;

public class PurchaseManager : MonoBehaviour, IStoreListener
{
    private IStoreController _controller;
    private IExtensionProvider _extensions;


    

    void Awake()
    {
        InitIAP();
        NetworkBroker.PurchaseItem += (productId) =>
        {
            _controller?.InitiatePurchase(productId);
        };
    }
    void InitIAP()
    {
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(ProductIds.DIA_0, ProductType.Consumable);
        UnityPurchasing.Initialize(this, builder);
    }

    // --------- IStoreListener 인터페이스 ---------

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _controller = controller;
        _extensions = extensions;
        Debug.Log("IAP 초기화 완료");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"IAP 초기화 실패: {error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseArgs)
    {
        HandlePurchaseAsync(purchaseArgs);
        return PurchaseProcessingResult.Pending; // 유니티에게 '처리 보류' 신호
    }

    private async void HandlePurchaseAsync(PurchaseEventArgs purchaseArgs)
    {
        Dictionary<string, object> args = new()
    {
        { "receipt", purchaseArgs.purchasedProduct.receipt },
        { "productId", purchaseArgs.purchasedProduct.definition.id },
        { "playerId", AuthenticationService.Instance.PlayerId },
    };

        PurchaseResult purchaseResult = await CloudCodeService.Instance.CallModuleEndpointAsync<PurchaseResult>(
            "PurchaseProcessor",
            "ProcessPurchase",
            args
        );

        // 서버 검증 성공 시
        //NetworkBroker.OnPurchaseSuccess?.Invoke(purchaseArgs.purchasedProduct.definition.id);
        //Debug.Log(purchaseArgs.purchasedProduct.receipt);
        // 구매 확정 처리
        _controller.ConfirmPendingPurchase(purchaseArgs.purchasedProduct);
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"구매 실패: {failureReason}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"초기화 실패: {message}");
    }
}
