using UnityEngine;
//using UnityEngine.Purchasing;
using UnityEngine.UI;
using System.Collections.Generic;
using static NUnit.Framework.Internal.OSPlatform;
using Unity.VisualScripting;

public class IAPManager : MonoBehaviour//, IStoreListener
{
   /* private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    public string productID = "com.yourcompany.yourproduct"; // Google Play Console�� ������ ��ǰ ID

    [SerializeField] private Button purchaseButton; // ���� ��ư
    [SerializeField] private Text statusText; // ���� ǥ�ÿ� �ؽ�Ʈ

    private void Start()
    {
        // ���� ��ư Ŭ�� �� ���� ����
        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(InitiatePurchase);

        // IAP �ʱ�ȭ
        InitializeIAP();
    }

    // IAP �ʱ�ȭ
    private void InitializeIAP()
    {
        // IAP�� ���� �ʱ�ȭ���� �ʾҴٸ� �ʱ�ȭ ����
        if (storeController == null)
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(productID, ProductType.Consumable);  // �Һ��� ������ ����
            UnityPurchasing.Initialize(this, builder); // IAP �ʱ�ȭ
        }
    }

    // IStoreListener �������̽� �޼��� (IAP �ʱ�ȭ ���� �� ȣ��)
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller; // IAP ��Ʈ�ѷ� �ʱ�ȭ
        storeExtensionProvider = extensions; // Ȯ�� ��� �ʱ�ȭ

        statusText.text = "IAP Initialized!"; // �ʱ�ȭ �Ϸ� �޽���
    }

    // IAP �ʱ�ȭ ���� �� ȣ��
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        statusText.text = "IAP Initialization Failed: " + error.ToString(); // �ʱ�ȭ ���� �޽���
    }

    // ���� �Ϸ� �� ȣ��
    public void OnPurchaseCompleted(PurchaseEventArgs args)
    {
        // ������ ��ǰ�� ������ productID�� ��ġ�ϸ�
        if (args.purchasedProduct.definition.id == productID)
        {
            Debug.Log("Purchase Successful: " + args.purchasedProduct.definition.id); // ���� ���� �α�
            statusText.text = "Purchase Successful!"; // ���� ���� �޽���
            // ���� �Ϸ� �� ó���� ���� (������ ���� ��)
        }
    }

    // ���� ���� �� ȣ��
    public void OnPurchaseFailed(PurchaseFailureReason failureReason, PurchaseEventArgs args)
    {
        Debug.Log("Purchase Failed: " + failureReason.ToString()); // ���� ���� �α�
        statusText.text = "Purchase Failed: " + failureReason.ToString(); // ���� ���� �޽���
    }

    // ���� ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    private void InitiatePurchase()
    {
        // IAP ��Ʈ�ѷ��� �ʱ�ȭ�Ǿ���, ��ǰ ������ �ִ� ���
        if (storeController != null && storeController.products != null)
        {
            // �����Ϸ��� ��ǰ�� ã��
            Product product = storeController.products.WithID(productID);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log("Purchasing product: " + product.definition.id); // ��ǰ ���� �α�
                storeController.InitiatePurchase(product); // ���� ����
            }
            else
            {
                Debug.Log("Product not available for purchase."); // ��ǰ ���� �Ұ� �޽���
                statusText.text = "Product not available!"; // ���� �Ұ� �޽���
            }
        }
    }

    // ���� ���� �� ȣ��Ǵ� �޼���
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("Purchase failed: " + failureReason.ToString()); // ���� ���� �α�
    }*/
}
