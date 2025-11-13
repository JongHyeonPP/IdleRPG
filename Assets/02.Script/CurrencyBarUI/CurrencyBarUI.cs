using EnumCollection;
using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class CurrencyBarUI : MonoBehaviour, IGeneralUI
{
    private GameData _gameData;
    ProgressBar _expBar;
    Label _levelLabel;
    Label _nameLabel;
    Label _emeraldLabel;
    Label _diaLabel;
    Button _powerSaveButton;
    [SerializeField] TotalStatusUI _totalStatusUI;
    public VisualElement root { get; private set; }
    [SerializeField] SettingUI _settingUI;
    [SerializeField] PowerSavePanel _powerSavePanel;

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
        PlayerBroker.OnDiaSet += SetDia;
        PlayerBroker.OnCloverSet += SetEmerald;
        PlayerBroker.OnLevelExpSet += SetLevelExp;

        OnSetLevel(_gameData.level);
        SetName(_gameData.userName);
        SetLevelExp();
        SetDia();
        SetEmerald();

        VisualElement playerImage = root.Q<VisualElement>("PlayerImage");
        _totalStatusUI.root.style.display = DisplayStyle.None;

        playerImage.RegisterCallback<ClickEvent>(evt => OpenTotalStatusUI());

        root.Q<Button>("SettingButton").RegisterCallback<ClickEvent>(evt => ActiveSettingUI());
        _powerSaveButton = root.Q<Button>("PowerSaveButton");
        _powerSaveButton.RegisterCallback<ClickEvent>(evt => ActivePowerSavePanel());
    }

    private void OpenTotalStatusUI()
    {
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
        _totalStatusUI.ActiveUI();
    }

    private void ActivePowerSavePanel()
    {
        _powerSavePanel.ActivePowerSavePanel();
    }

    private void ActiveSettingUI()
    {
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
        _settingUI.ActiveUI();
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
            return 0f;

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
        string text = _gameData.clover.ToString();
        _emeraldLabel.text = text;
        AdjustFontSize(_emeraldLabel, text);
    }

    private void SetDia()
    {
        string text = _gameData.dia.ToString();
        _diaLabel.text = text;
        AdjustFontSize(_diaLabel, text);
    }

    private void AdjustFontSize(Label label, string text)
    {
        // 기본 폰트 크기
        float baseFontSize = 44f;
        float baseBorder = 2f; // 기본 테두리 두께
        float basePadding = 6f; // 기본 패딩값

        // 폰트 크기 계산
        float newFontSize;
        if (text.Length > 4)
        {
            // 글자 수가 많을수록 폰트 축소 (최소 14)
            newFontSize = Mathf.Clamp(baseFontSize - (text.Length - 4) * 7f, 14f, baseFontSize);
        }
        else
        {
            newFontSize = baseFontSize;
        }

        // 축소 비율 계산 (1이면 원래 크기)
        float scale = newFontSize / baseFontSize;

        // 스타일 적용
        label.style.fontSize = newFontSize;
        label.style.borderTopWidth = baseBorder * scale;
        label.style.borderBottomWidth = baseBorder * scale;
        label.style.borderLeftWidth = baseBorder * scale;
        label.style.borderRightWidth = baseBorder * scale;

        label.style.paddingTop = basePadding * scale;
        label.style.paddingBottom = basePadding * scale;
        label.style.paddingLeft = basePadding * scale;
        label.style.paddingRight = basePadding * scale;
    }


    public void OnBattle()
    {
        root.style.display = DisplayStyle.Flex;
        if (_powerSaveButton != null)
            _powerSaveButton.style.display = DisplayStyle.Flex;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {
        if (_powerSaveButton != null)
            _powerSaveButton.style.display = DisplayStyle.None;
    }
}
