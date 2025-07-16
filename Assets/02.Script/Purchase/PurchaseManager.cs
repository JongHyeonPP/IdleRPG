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

    // --------- IStoreListener �������̽� ---------

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _controller = controller;
        _extensions = extensions;
        Debug.Log("IAP �ʱ�ȭ �Ϸ�");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"IAP �ʱ�ȭ ����: {error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseArgs)
    {
        HandlePurchaseAsync(purchaseArgs);
        return PurchaseProcessingResult.Pending; // ����Ƽ���� 'ó�� ����' ��ȣ
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

        // ���� ���� ���� ��
        //NetworkBroker.OnPurchaseSuccess?.Invoke(purchaseArgs.purchasedProduct.definition.id);
        //Debug.Log(purchaseArgs.purchasedProduct.receipt);
        // ���� Ȯ�� ó��
        _controller.ConfirmPendingPurchase(purchaseArgs.purchasedProduct);
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"���� ����: {failureReason}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"�ʱ�ȭ ����: {message}");
    }
}
