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

    public string productID = "com.yourcompany.yourproduct"; // Google Play Console에 설정한 제품 ID

    [SerializeField] private Button purchaseButton; // 구매 버튼
    [SerializeField] private Text statusText; // 상태 표시용 텍스트

    private void Start()
    {
        // 구매 버튼 클릭 시 구매 시작
        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(InitiatePurchase);

        // IAP 초기화
        InitializeIAP();
    }

    // IAP 초기화
    private void InitializeIAP()
    {
        // IAP가 아직 초기화되지 않았다면 초기화 시작
        if (storeController == null)
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(productID, ProductType.Consumable);  // 소비형 아이템 설정
            UnityPurchasing.Initialize(this, builder); // IAP 초기화
        }
    }

    // IStoreListener 인터페이스 메서드 (IAP 초기화 성공 시 호출)
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller; // IAP 컨트롤러 초기화
        storeExtensionProvider = extensions; // 확장 기능 초기화

        statusText.text = "IAP Initialized!"; // 초기화 완료 메시지
    }

    // IAP 초기화 실패 시 호출
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        statusText.text = "IAP Initialization Failed: " + error.ToString(); // 초기화 실패 메시지
    }

    // 구매 완료 시 호출
    public void OnPurchaseCompleted(PurchaseEventArgs args)
    {
        // 구매한 제품이 지정한 productID와 일치하면
        if (args.purchasedProduct.definition.id == productID)
        {
            Debug.Log("Purchase Successful: " + args.purchasedProduct.definition.id); // 구매 성공 로그
            statusText.text = "Purchase Successful!"; // 구매 성공 메시지
            // 구매 완료 후 처리할 로직 (아이템 지급 등)
        }
    }

    // 구매 실패 시 호출
    public void OnPurchaseFailed(PurchaseFailureReason failureReason, PurchaseEventArgs args)
    {
        Debug.Log("Purchase Failed: " + failureReason.ToString()); // 구매 실패 로그
        statusText.text = "Purchase Failed: " + failureReason.ToString(); // 구매 실패 메시지
    }

    // 구매 버튼 클릭 시 호출되는 메서드
    private void InitiatePurchase()
    {
        // IAP 컨트롤러가 초기화되었고, 제품 정보가 있는 경우
        if (storeController != null && storeController.products != null)
        {
            // 구매하려는 제품을 찾음
            Product product = storeController.products.WithID(productID);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log("Purchasing product: " + product.definition.id); // 제품 구매 로그
                storeController.InitiatePurchase(product); // 구매 시작
            }
            else
            {
                Debug.Log("Product not available for purchase."); // 제품 구매 불가 메시지
                statusText.text = "Product not available!"; // 구매 불가 메시지
            }
        }
    }

    // 구매 실패 시 호출되는 메서드
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("Purchase failed: " + failureReason.ToString()); // 구매 실패 로그
    }*/
}
