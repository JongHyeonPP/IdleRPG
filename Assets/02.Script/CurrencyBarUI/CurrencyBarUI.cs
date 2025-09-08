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
    [SerializeField] TotalStatusUI _totalStatusUI;
    public VisualElement root { get;private set; }

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _gameData = StartBroker.GetGameData();
        PlayerBroker.OnGacha += OnGacha;
    }

    private void OnGacha(GachaType type, int arg2)
    {
        switch (type)
        {
            case GachaType.Weapon:
            case GachaType.Costume:
                SetDia();
                break;
        }
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

    public void OnBattle()
    {
        root.style.display = DisplayStyle.Flex;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {
    }
}