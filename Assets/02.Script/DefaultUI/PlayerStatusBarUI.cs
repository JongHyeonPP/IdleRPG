using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStatusBarUI : MonoBehaviour
{
    VisualElement _root;
    ProgressBar _expBar;
    Label _levelLabel;
    Label _nameLabel;
    Label _emeraldLabel;
    Label _diaLabel;
    void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _expBar = _root.Q<ProgressBar>("ExpBar");
        _levelLabel = _root.Q<Label>("LevelLabel");
        _nameLabel = _root.Q<Label>("NameLabel");
        _emeraldLabel = _root.Q<Label>("EmeraldLabel");
        _diaLabel = _root.Q<Label>("DiaLabel");
        BattleBroker.OnExpGain += SetExp;
        BattleBroker.OnLevelUp += SetLevel;
        BattleBroker.OnDiaGain += SetDia;
        BattleBroker.OnEmeraldGain += SetEmerald;
        BattleBroker.OnSetName += SetName;
        SetExp();
        SetLevel();
        SetName();
        SetDia();
        SetEmerald();
    }
    private void SetExp()
    {
        float value = GameManager.instance.GetExpPercent();
        _expBar.value = value;
        _expBar.title = (value*100f).ToString("F2");
    }
    private void SetLevel()
    {
        _levelLabel.text = $"Lv. {GameManager.instance.gameData.level}";
    }
    private void SetName()
    {
        _nameLabel.text = GameManager.userName;
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