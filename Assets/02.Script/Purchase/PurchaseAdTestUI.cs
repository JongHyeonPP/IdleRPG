using UnityEngine;
using UnityEngine.UIElements;

public class PurchaseAdTestUI : MonoBehaviour
{
    private void Awake()
    {
        InitUI();
    }
    void InitUI()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button purchaseButton = root.Q<Button>("PurchaseButton");
        purchaseButton.RegisterCallback<ClickEvent>(evt => NetworkBroker.PurchaseItem(ProductIds.DIA_0));
        Button adButton = root.Q<Button>("AdButton");
        adButton.RegisterCallback<ClickEvent>(evt => NetworkBroker.LoadAd());
    }
}
