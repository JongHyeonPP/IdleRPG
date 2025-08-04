using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionTechUI : MonoBehaviour, IGeneralUI
{
    public VisualElement root { get; private set; }
    public VisualElement _bottom;
    public VisualElement _frame;
    private Label _nameLabel;
    private Label _diaLabel;
    private Label _cloverLabel;
    private Label _companionEffectLabel;
    private GameData _gameData;
    private readonly Color _acquireColor = new Color(1f, 1f, 1f);
    private readonly Color _unacquireColor = new Color(0.8f,0.8f,0.8f);
    private VisualElement[][] _renderTextureArr = new VisualElement[4][];
    private int _currentIndex_0;
    private int _currentIndex_1;
    private bool _isAcquired;
    private Button _confirmButton;
    private CompanionTechData _currentTechData;
    private int _currentCompanionIndex;
    private (int,int) _currentTech;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _nameLabel = root.Q<Label>("NameLabel");
        _diaLabel = root.Q<Label>("DiaLabel");
        _cloverLabel = root.Q<Label>("CloverLabel");
        _companionEffectLabel = root.Q<Label>("CompanionEffectLabel");
        _bottom = root.Q<VisualElement>("Bottom");
        _frame = root.Q<VisualElement>("Frame");
        _gameData = StartBroker.GetGameData();
        _renderTextureArr[0] = new VisualElement[1];
        _renderTextureArr[1] = new VisualElement[2];
        _renderTextureArr[2] = new VisualElement[2];
        _renderTextureArr[3] = new VisualElement[2];
        InitRenderTexture(0, 0);
        InitRenderTexture(1, 0);
        InitRenderTexture(1, 1);
        InitRenderTexture(2, 0);
        InitRenderTexture(2, 1);
        InitRenderTexture(3, 0);
        InitRenderTexture(3, 1);
        _currentIndex_0 = -1;
        _currentIndex_1 = -1;
        _confirmButton = root.Q<Button>("ConfirmButton");
        _confirmButton.RegisterCallback<ClickEvent>(evt => OnConfirmButtonClick());
    }

    private void OnConfirmButtonClick()
    {
        if (_isAcquired)
        {
            if (_gameData.currentCompanionPromoteTech[_currentCompanionIndex] != _currentTech)
            {
                PlayerBroker.OnCompanionAppearanceChange(_currentCompanionIndex, _currentTechData.appearanceData);
                _gameData.currentCompanionPromoteTech[_currentCompanionIndex] = _currentTech;
                NetworkBroker.SaveServerData();
            }
        }
        else
        {
            BattleBroker.SwitchToCompanionBattle(_currentCompanionIndex, _currentTech);
            UIBroker.ChangeMenu(0);
        }
        UIBroker.InactiveCurrentUI();
    }

    private void InitRenderTexture(int index_0, int index_1)
    {
        _renderTextureArr[index_0][index_1] = root.Q<VisualElement>($"CompanionRenderTexture_{index_0}_{index_1}");
        _renderTextureArr[index_0][index_1].style.display = DisplayStyle.None;
    }
    public void ActiveUI(int companionIndex, int techIndex_0, int techIndex_1)
    {
        _currentTech = (techIndex_0, techIndex_1);
        _currentCompanionIndex = companionIndex;
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        _currentTechData = CompanionManager.instance.GetCompanionTechData(companionIndex, techIndex_0, techIndex_1);
        //Skill
        SkillData techSkill = _currentTechData.techSkill;
        int companionLevel = CompanionManager.instance.GetCompanionLevelExp(companionIndex).Item1;
        Color techSkillValueColor;
        switch (techIndex_0)
        {
            default:
                techSkillValueColor = PriceManager.instance.rarityColor[6];
                break;
            case 1:
                techSkillValueColor = PriceManager.instance.rarityColor[0];
                break;
            case 2:
                techSkillValueColor = PriceManager.instance.rarityColor[2];
                break;
            case 3:
                techSkillValueColor = PriceManager.instance.rarityColor[4];
                break;
        }
        _companionEffectLabel.text = SkillManager.instance.GetParsedComplexExplain(techSkill, companionLevel, techSkillValueColor);
        //
        (int, int) currentReward = BattleBroker.GetCompanionReward(companionIndex, techIndex_0 - 1);
        _diaLabel.text = currentReward.Item1.ToString("N0");
        _cloverLabel.text = currentReward.Item2.ToString("N0");
        int techDataIndex = _gameData.companionPromoteTech[companionIndex][techIndex_1];
        _isAcquired = techDataIndex >= techIndex_0;
        if (techDataIndex >= techIndex_0)
        {
            _confirmButton.style.display = DisplayStyle.Flex;
            _nameLabel.text = $"{_currentTechData.techName} (»πµÊ«‘)";
            _nameLabel.style.color = _acquireColor;
            _confirmButton.Q<Label>().text = "¡˜æ˜ ∫Ø∞Ê";
            _bottom.style.display = DisplayStyle.None;
            _frame.style.height = Length.Percent(118f);
        }
        else if (techDataIndex+1==techIndex_0)
        {
            _confirmButton.style.display = DisplayStyle.Flex;
            _nameLabel.text = $"{_currentTechData.techName} (πÃ»πµÊ)";
            _nameLabel.style.color = _unacquireColor;
            _confirmButton.Q<Label>().text = "¿¸≈ı«œ±‚";
            _bottom.style.display = DisplayStyle.Flex;
            _frame.style.height = Length.Percent(112f);
        }
        else
        {
            _confirmButton.style.display = DisplayStyle.None;
            _nameLabel.style.color = _acquireColor;
            _nameLabel.text = $"{_currentTechData.techName} (πÃ»πµÊ)";
            _bottom.style.display = DisplayStyle.Flex;
            _frame.style.height = Length.Percent(112f);
        }
        if (_currentIndex_0 != -1)
        {
            _renderTextureArr[_currentIndex_0][_currentIndex_1].style.display = DisplayStyle.None;
        }
        _renderTextureArr[techIndex_0][techIndex_1].style.display = DisplayStyle.Flex;
        _currentIndex_0 = techIndex_0;
        _currentIndex_1 = techIndex_1;
    }
    public void OnBattle()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {
    }
}
