using System.Numerics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class CurrencyBarUI : MonoBehaviour
{
    private GameData _gameData;
    public VisualElement root { get; private set; }
    ProgressBar _expBar;
    Label _levelLabel;
    Label _nameLabel;
    Label _emeraldLabel;
    Label _diaLabel;
    [SerializeField] TotalStatusUI _totalStatusUI;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _gameData = StartBroker.GetGameData();
    }
    void Start()
    {
        _expBar = root.Q<ProgressBar>("ExpBar");
        _levelLabel = root.Q<Label>("LevelLabel");
        _nameLabel = root.Q<Label>("NameLabel");
        _emeraldLabel = root.Q<Label>("EmeraldLabel");
        _diaLabel = root.Q<Label>("DiaLabel");

        PlayerBroker.OnSetName += SetName;
        BattleBroker.OnDiaSet += SetDia;
        BattleBroker.OnCloverSet += SetEmerald;
        BattleBroker.OnLevelExpSet += SetLevelExp;
        OnSetLevel(_gameData.level);
        SetName(_gameData.userName);
        SetLevelExp();
        SetDia();
        SetEmerald();
        VisualElement playerImage = root.Q<VisualElement>("PlayerImage");
        playerImage.RegisterCallback<ClickEvent>(evt =>
        {
            _totalStatusUI.ActiveTotalStatusUI();
        });
    }
    private void SetLevelExp()
    {
        float value = GetExpPercent();
        _expBar.value = value;
        _expBar.title = $"{value * 100f:F2}%";
        _levelLabel.text = $"Lv. {StartBroker.GetGameData().level}";
    }
    public float GetExpPercent()
    {
        BigInteger needExp = BattleBroker.GetNeedExp();
        BigInteger exp = _gameData.exp;

        if (needExp == 0)
            return 0f; // 0으로 나누는 오류 방지

        return (float)((double)exp / (double)needExp);
    }
    private void OnSetLevel(int level)
    {
        _levelLabel.text = $"Lv. {level}";
    }
    private void SetName(string name)
    {
        _nameLabel.text = _gameData.userName;
    }
    private void SetEmerald()
    {
        _emeraldLabel.text = _gameData.clover.ToString();
    }
    private void SetDia()
    {
        _diaLabel.text = _gameData.dia.ToString();
    }
}