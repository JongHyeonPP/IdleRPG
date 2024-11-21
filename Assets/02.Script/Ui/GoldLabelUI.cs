using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
public class GoldLabelUI : MonoBehaviour
{
    private VisualElement _root;
    private Label _goldLabel;
    void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _goldLabel = _root.Q<Label>("GoldLabel");
        BattleBroker.OnGoldGain += SetGold;
        SetGold();
    }
    private void SetGold()
    {
        _goldLabel.text = GameManager.instance.gameData.gold.ToString();
    }
}