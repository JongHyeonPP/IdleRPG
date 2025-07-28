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
        Button dia_0_Button = root.Q<Button>("Dia_0_Button");
        dia_0_Button.RegisterCallback<ClickEvent>(evt => NetworkBroker.PurchaseItem(ProductIds.DIA_0));
        Button dia_1_Button = root.Q<Button>("Dia_1_Button");
        dia_1_Button.RegisterCallback<ClickEvent>(evt => NetworkBroker.PurchaseItem(ProductIds.DIA_1));
        Button adButton = root.Q<Button>("AdButton");
        adButton.RegisterCallback<ClickEvent>(evt => NetworkBroker.LoadAd());
    }
}
