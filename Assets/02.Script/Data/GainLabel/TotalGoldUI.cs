using UnityEngine;
using UnityEngine.UIElements;
public class TotalGoldUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private Label _goldLabel;
    private GameData _gameData;
    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _goldLabel = root.Q<Label>("GoldLabel");
        _gameData = StartBroker.GetGameData();
        BattleBroker.OnGoldSet += SetGold;
        SetGold();
    }



    private void SetGold()
    {
        _goldLabel.text = _gameData.gold.ToString("N0");
    }
}