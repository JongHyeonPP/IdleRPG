using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStatusBarUI : MonoBehaviour
{
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

    }
    void Start()
    {
        _expBar = root.Q<ProgressBar>("ExpBar");
        _levelLabel = root.Q<Label>("LevelLabel");
        _nameLabel = root.Q<Label>("NameLabel");
        _emeraldLabel = root.Q<Label>("EmeraldLabel");
        _diaLabel = root.Q<Label>("DiaLabel");
        BattleBroker.OnDiaGain += SetDia;
        BattleBroker.OnEmeraldGain += SetEmerald;
        PlayerBroker.OnSetName += SetName;
        BattleBroker.OnExpGain += SetExp;
        PlayerBroker.OnSetLevel += SetLevel;
        SetLevel(GameManager.instance.gameData.level);
        SetName(GameManager.instance.userName);
        SetExp();
        SetDia();
        SetEmerald();
        VisualElement playerImage = root.Q<VisualElement>("PlayerImage");
        playerImage.RegisterCallback<ClickEvent>(evt =>
        {
            _totalStatusUI.ActiveTotalStatusUI();
        });
    }
    private void SetExp()
    {
        float value = GameManager.instance.GetExpPercent();
        _expBar.value = value;
        _expBar.title = (value*100f).ToString("F2");
    }
    private void SetLevel(int level)
    {
        _levelLabel.text = $"Lv. {level}";
    }
    private void SetName(string name)
    {
        _nameLabel.text = GameManager.instance.userName;
    }
    private void SetEmerald()
    {
        _emeraldLabel.text = GameManager.instance.emerald.ToString();
    }
    private void SetDia()
    {
        _diaLabel.text = GameManager.instance.dia.ToString();
    }

}