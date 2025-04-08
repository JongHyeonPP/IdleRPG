using UnityEngine;
using UnityEngine.UIElements;
public class TotalGoldUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private Label _goldLabel;
    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _goldLabel = root.Q<Label>("GoldLabel");

        BattleBroker.OnGoldSet += SetGold;
        SetGold();
    }



    private void SetGold()
    {
        _goldLabel.text = StartBroker.GetGameData().gold.ToString("N0");
    }
}