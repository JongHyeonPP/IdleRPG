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

        PlayerBroker.OnSetName += SetName;
        BattleBroker.OnDiaSet += SetDia;
        BattleBroker.OnEmeraldSet += SetEmerald;
        BattleBroker.OnLevelExpSet += SetLevelExp;
        OnSetLevel(GameManager.instance.gameData.level);
        SetName(GameManager.instance.userName);
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
        float value = GameManager.instance.GetExpPercent();
        _expBar.value = value;
        _expBar.title = (value*100f).ToString("F2");
        _levelLabel.text = $"Lv. {GameManager.instance.gameData.level}";
    }
    private void OnSetLevel(int level)
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