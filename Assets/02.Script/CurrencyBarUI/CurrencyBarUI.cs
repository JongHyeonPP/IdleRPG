using EnumCollection;
using System;
using System.Numerics;
using UnityEngine;
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

    VisualElement _nameChangePanel;
    VisualElement _background;
    TextField _nameInputField;
    Button _nameApplyButton;

    Label _placeHoldLabel;

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
        VisualElement namePanel = root.Q<VisualElement>("NamePanel");

        _totalStatusUI.root.style.display = DisplayStyle.None;

        playerImage.RegisterCallback<ClickEvent>(evt => OpenTotalStatusUI());

        _nameChangePanel = root.Q<VisualElement>("NameChangePanel");
        _background = root.Q<VisualElement>("Background");

        _nameChangePanel.style.display = DisplayStyle.None;
        _background.style.display = DisplayStyle.None;

        _nameInputField = root.Q<TextField>("NameInputField");
        _nameApplyButton = root.Q<Button>("ChangeButton");

        _placeHoldLabel = root.Q<Label>("PlaceHoldLabel");

        _nameInputField.value = string.Empty;
        _placeHoldLabel.text = "새로운 이름을 입력하세요.";

        // 입력 중 placeholder 제거. 빈 문자열일 때는 placeholder 유지.
        _nameInputField.RegisterValueChangedCallback(evt =>
        {
            string current = _nameInputField.value;

            // 빈 문자열이면 placeholder 유지
            if (string.IsNullOrEmpty(current))
                return;

            // 입력이 존재하면 placeholder 제거
            _placeHoldLabel.text = string.Empty;
        });

        namePanel.RegisterCallback<ClickEvent>(evt => OpenNameChangePanel());

        if (_nameApplyButton != null)
            _nameApplyButton.RegisterCallback<ClickEvent>(evt => ApplyNameChange());

        Button exitButton = root.Q<Button>("ExitButton");
        if (exitButton != null)
        {
            exitButton.RegisterCallback<ClickEvent>(evt =>
            {
                SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
                CloseNameChangePanel();
            });
        }

        _background.RegisterCallback<ClickEvent>(evt => CloseNameChangePanel());

        root.Q<Button>("SettingButton").RegisterCallback<ClickEvent>(evt => ActiveSettingUI());
        _powerSaveButton = root.Q<Button>("PowerSaveButton");
        _powerSaveButton.RegisterCallback<ClickEvent>(evt => ActivePowerSavePanel());
    }

    private void OpenTotalStatusUI()
    {
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
        _totalStatusUI.ActiveUI();
    }

    private void OpenNameChangePanel()
    {
        _nameInputField.value = string.Empty;
        _placeHoldLabel.text = "새로운 이름을 입력하세요.";

        _nameChangePanel.style.display = DisplayStyle.Flex;
        _background.style.display = DisplayStyle.Flex;
    }

    private void CloseNameChangePanel()
    {
        _nameChangePanel.style.display = DisplayStyle.None;
        _background.style.display = DisplayStyle.None;
    }

    private void ApplyNameChange()
    {
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

        string newName = _nameInputField.value;

        if (string.IsNullOrWhiteSpace(newName))
        {
            _placeHoldLabel.text = "이름은 비워둘 수 없습니다.";
            return;
        }

        if (newName.Length > 7)
        {
            _nameInputField.value = string.Empty;
            _placeHoldLabel.text = "이름은 7자를 넘을 수 없습니다.";
            return;
        }

        foreach (char c in newName)
        {
            bool isKorean = c >= 44032 && c <= 55203;
            bool isLower = c >= 'a' && c <= 'z';
            bool isUpper = c >= 'A' && c <= 'Z';
            bool isNumber = c >= '0' && c <= '9';

            if (isKorean == false && isLower == false && isUpper == false && isNumber == false)
            {
                _nameInputField.value = string.Empty;
                _placeHoldLabel.text = "잘못된 이름입니다.";
                return;
            }
        }

        _gameData.userName = newName;
        PlayerBroker.OnSetName(newName);
        CloseNameChangePanel();
        NetworkBroker.SaveServerData();
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
        _expBar.title = string.Format("{0:F2}% ", value * 100f);
        _levelLabel.text = string.Format("Lv. {0}", StartBroker.GetGameData().level);
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
        _levelLabel.text = string.Format("Lv. {0}", level);
    }

    private void SetName(string name)
    {
        _nameLabel.text = _gameData.userName;

        int length = name.Length;
        if (length > 7)
            length = 7;

        float newSize = 35f;

        if (length == 6)
            newSize = 28f;
        else if (length == 7)
            newSize = 24f;

        _nameLabel.style.fontSize = newSize;
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
        float baseFontSize = 44f;
        float baseBorder = 2f;
        float basePadding = 6f;

        float newFontSize;
        if (text.Length > 4)
            newFontSize = Mathf.Clamp(baseFontSize - (text.Length - 4) * 7f, 14f, baseFontSize);
        else
            newFontSize = baseFontSize;

        float scale = newFontSize / baseFontSize;

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
        root.style.display = DisplayStyle.Flex;
        if (_powerSaveButton != null)
            _powerSaveButton.style.display = DisplayStyle.None;
    }
}
