using UnityEngine;
using UnityEngine.UIElements;
public class GoldLabelUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private Label _goldLabel;
    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _goldLabel = root.Q<Label>("GoldLabel");
        BattleBroker.OnGoldGain += SetGold;
        SetGold();
        BattleBroker.OnStageEnter += OnStageEnter;
        BattleBroker.OnBossEnter += OnBossEnter;
    }

    private void OnBossEnter()
    {
        root.style.display = DisplayStyle.None;
    }

    private void OnStageEnter()
    {
        root.style.display = DisplayStyle.Flex;
    }

    private void SetGold()
    {
        _goldLabel.text = GameManager.instance.gameData.gold.ToString();
    }
}