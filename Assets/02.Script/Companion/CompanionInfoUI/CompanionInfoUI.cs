using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class CompanionInfoUI : MonoBehaviour, IGeneralUI
{
    private CompanionManager _companionManager;
    private GameData _gameData;
    public VisualElement root { get; private set; }
    private VisualElement[] _renderTextureArr;
    private VisualElement[] _mainPanelArr = new VisualElement[2];
    private Label _jobLabel;
    private Label _nameLabel;
    private Label _levelLabel;
    private ProgressBar _expProgressBar;
    private int _currentCompanionIndex;
    private Button _statusButton;
    private Button _promoteButton;
    private Button[] _switchButton = new Button[2];
    //Status
    private VisualElement[] _passiveSlotArr;
    private Label _companionEffectLabel;
    //Promote
    [SerializeField] CompanionPromoteInfoUI _companionPromoteInfoUI;
    private readonly VisualElement[] _promoteSlotArr = new VisualElement[5];
    private Button _infoButton;
    private Label cloverLabel;
    private CompanionPromoteData _companionPromoteData;
    private readonly bool[] _isLockEffectArr = new bool[5];
    private int _currentActiveEffectIndex;
    private Button _changeButton;
    private Label _cloverPriceLabel;
    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
        root = GetComponent<UIDocument>().rootVisualElement;
        Button exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(evt => OnExitButtonClick());
        
        VisualElement renderTextureparent = root.Q<VisualElement>("RenderTextureParent");
        _renderTextureArr = new VisualElement[renderTextureparent.childCount];
        for (int i = 0; i < renderTextureparent.childCount; i++)
        {
            _renderTextureArr[i] = renderTextureparent.ElementAt(i);
        }
        _mainPanelArr[0] = root.Q<VisualElement>("StatusPanel");
        _mainPanelArr[1] = root.Q<VisualElement>("PromotePanel");
        _jobLabel = root.Q<Label>("JobLabel");
        _nameLabel = root.Q<Label>("NameLabel");
        _levelLabel = root.Q<Label>("LevelLabel");
        _companionEffectLabel = root.Q<Label>("CompanionEffectLabel");
        _expProgressBar = root.Q<ProgressBar>("ExpProgressBar");

        _switchButton[0] = root.Q<Button>("LeftSwitchButton");
        _switchButton[1] = root.Q<Button>("RightSwitchButton");
        _switchButton[0].RegisterCallback<ClickEvent>(evt=>OnSwitchButtonClick(0));
        _switchButton[1].RegisterCallback<ClickEvent>(evt=>OnSwitchButtonClick(1));

        InitCategoriButton();
        InitStatusPanel();
        InitPromotePanel();
    }
    private void Start()
    {
        _companionManager = CompanionManager.instance;
        BattleBroker.OnCompanionExpSet += OnCompanionExpSet;
        PlayerBroker.OnSkillLevelSet += OnSkillLevelSet;
        _companionPromoteData = CompanionManager.instance.companionPromoteData;
    }
    private void OnSwitchButtonClick(int buttonIndex)
    {
        int newCompanionIndex;
        if (buttonIndex == 0)
        {
            newCompanionIndex = _currentCompanionIndex - 1;
            if (newCompanionIndex == -1)
                newCompanionIndex = 2;
        }
        else
        {
            newCompanionIndex = _currentCompanionIndex + 1;
            if (newCompanionIndex == 3)
                newCompanionIndex = 0;
        }
        SwitchCompanion(newCompanionIndex);
    }
    private void InitPromotePanel()
    {
        _infoButton = _mainPanelArr[1].Q<Button>("InfoButton");
        _infoButton.RegisterCallback<ClickEvent>(evt=>_companionPromoteInfoUI.ActiveUI());
        cloverLabel = _mainPanelArr[1].Q<Label>("CloverLabel");
        BattleBroker.OnCloverSet += SetCloverLabel;
        SetCloverLabel();
        VisualElement promoteEffectSlotParent = root.Q<VisualElement>("PromoteEffectSlotParent");
        for (int i = 0; i < _promoteSlotArr.Length; i++)
        {
            int index = i;
            _promoteSlotArr[i] = promoteEffectSlotParent.ElementAt(i);
            _promoteSlotArr[i].Q<Button>("EachEffectChangeButton").RegisterCallback<ClickEvent>(evt=>OnClickPrmoteEffectSlot(index));
            Label disableLabel = _promoteSlotArr[i].Q<Label>("DisableLabel");
            int requireTechNum = 0;
            switch (i)
            {
                case 2:
                    requireTechNum = 1;
                    break;
                case 3:
                    requireTechNum = 2;
                    break;
                case 4:
                    requireTechNum = 3;
                    break;
            }
            disableLabel.text = $"{requireTechNum}차 전직 시 해금";
        }
        _changeButton = _mainPanelArr[1].Q<Button>("ChangeButton");
        _cloverPriceLabel = _mainPanelArr[1].Q<Label>("CloverPriceLabel");
        _changeButton.RegisterCallback<ClickEvent>(evt=>CompanionPromoteEffectChange());
        PlayerBroker.OnCompanionPromoteEffectSet += OnCompanionPromoteEffectSet;
    }
    private void OnClickPrmoteEffectSlot(int index)
    {
        _isLockEffectArr[index] = !_isLockEffectArr[index];
        _promoteSlotArr[index].Q<VisualElement>("LockPanel").style.display = _isLockEffectArr[index] ? DisplayStyle.Flex : DisplayStyle.None;
        int trueCount = _isLockEffectArr.Count(item => item);
        if (trueCount == _currentActiveEffectIndex)
        {
            _changeButton.style.display = DisplayStyle.None;
        }
        else
        {
            _changeButton.style.display = DisplayStyle.Flex;
            _cloverPriceLabel.text = (CompanionManager.PROMOTE_EFFECT_CHANGE_PRICE * (1 + _isLockEffectArr.Count(item=>item))).ToString();
        }
    }
    private void CompanionPromoteEffectChange()
    {
        int price = CompanionManager.PROMOTE_EFFECT_CHANGE_PRICE * (1 + _isLockEffectArr.Count(item => item));
        if (_gameData.clover < price)
            return;
        for (int i = 0; i < _gameData.companionPromoteTech[_currentCompanionIndex].Max() + 2; i++)
        {
            if (!_isLockEffectArr[i])
                SetEachPromoteEffect(i);
        }
        _gameData.clover -= price;
        BattleBroker.OnCloverSet();
        NetworkBroker.SaveServerData();
    }

    private void SetEachPromoteEffect(int effectIndex)
    {
        int statusTypeIndex = Random.Range(0, 10);
        StatusType statusType;
        switch (statusTypeIndex)
        {
            default:
                statusType = StatusType.Power;
                break;
            case 1:
                statusType = StatusType.CriticalDamage;
                break;
            case 2:
                statusType = StatusType.MaxHp;
                break;
            case 3:
                statusType = StatusType.HpRecover;
                break;
            case 4:
                statusType = StatusType.MaxMp;
                break;
            case 5:
                statusType = StatusType.MpRecover;
                break;
            case 6:
                statusType = StatusType.GoldAscend;
                break;
            case 7:
                statusType = StatusType.Resist;
                break;
            case 8:
                statusType = StatusType.Penetration;
                break;
            case 9:
                statusType = StatusType.ExpAscend;
                break;
        }

        Rarity rarity = (Rarity)UtilityManager.AllocateProbability(_companionPromoteData.probabilityInRarity);
        (StatusType, Rarity) newValue = (statusType, rarity);
        _gameData.companionPromoteEffect[_currentCompanionIndex][effectIndex] = (statusType, rarity);
        PlayerBroker.OnCompanionPromoteEffectSet( _currentCompanionIndex, effectIndex, newValue);
    }

    private void OnCompanionPromoteEffectSet(int companionIndex, int effectIndex, (StatusType, Rarity)? nullableTuple)
    {
        if (companionIndex != _currentCompanionIndex)
            return;
        SetPromoteEffectLabel(nullableTuple, effectIndex);
    }
    private void SetCloverLabel()
    {
        cloverLabel.text = _gameData.clover.ToString("N0");
    }

    private void OnSkillLevelSet(string skillUid, int skillLevel)
    {
        SkillData[] skillArr = _companionManager.companionArr[_currentCompanionIndex].companionStatus.companionSkillArr;
        SkillData skillData = null;
        int skillIndex = -1;
        for (int i = 0; i < skillArr.Length; i++)
        {
            SkillData x = skillArr[i];
            if (x.uid == skillUid)
            {
               skillData = SkillManager.instance.GetSkillData(skillUid);
                skillIndex = i;
            }
        }
        if (skillData == null)
            return;
        //업 한 이후
        _gameData.skillLevel[skillData.uid] = skillLevel;
        _passiveSlotArr[skillIndex].Q<Label>("SkillLevelLabel").text = $"Lv.{skillLevel}";
        if (skillLevel == PriceManager.MAXCOMPANIONSKILLLEVEL)
        {
            _passiveSlotArr[skillIndex].Q<Button>().style.display = DisplayStyle.None;
            _passiveSlotArr[skillIndex].Q<VisualElement>("MaxLevelLabel").style.display = DisplayStyle.Flex;
        }
        else
        {
            _passiveSlotArr[skillIndex].Q<Button>().style.display = DisplayStyle.Flex;
            _passiveSlotArr[skillIndex].Q<VisualElement>("MaxLevelLabel").style.display = DisplayStyle.None;
            PriceInfo.CompanionSkillPrice afterPrice = PriceManager.instance.GetRequireCompanionSkill_CloverFragment(_currentCompanionIndex, skillIndex, skillLevel + 1);
            Label cloverLabel = _passiveSlotArr[skillIndex].Q<Label>("CloverLabel");
            cloverLabel.text = afterPrice.clover.ToString();
            Label fragmentLabel = _passiveSlotArr[skillIndex].Q<Label>("FragmentLabel");
            fragmentLabel.text = afterPrice.fragment.ToString();
        }
        
        NetworkBroker.SaveServerData();
    }
    private void OnPassiveButtonClick(int skillIndex)
    {
        string uid = _companionManager.companionArr[_currentCompanionIndex].companionStatus.companionSkillArr[skillIndex].uid;
        if (!_gameData.skillLevel.TryGetValue(uid, out int currentLevel))
        {
            currentLevel = 0;
        }
        PriceInfo.CompanionSkillPrice beforePrice = PriceManager.instance.GetRequireCompanionSkill_CloverFragment(_currentCompanionIndex, skillIndex, currentLevel+1);
        if (!_gameData.skillFragment.ContainsKey(beforePrice.fragmentRarity))
        {
            _gameData.skillFragment[beforePrice.fragmentRarity] = 0;
        }
        if (beforePrice.clover > _gameData.clover || beforePrice.fragment > _gameData.skillFragment[beforePrice.fragmentRarity])
        {
            Debug.Log("재화 부족");
            return;
        }
        if (!_gameData.skillFragment.ContainsKey(beforePrice.fragmentRarity))
        {
            _gameData.skillFragment[beforePrice.fragmentRarity] = 0;
        }
        _gameData.clover -= beforePrice.clover;
        _gameData.skillFragment[beforePrice.fragmentRarity] -= beforePrice.fragment;
        _gameData.skillLevel[uid] = ++currentLevel;
        PlayerBroker.OnSkillLevelSet(uid, currentLevel);
        BattleBroker.OnCompanionExpSet(_currentCompanionIndex);
    }
    private void OnCompanionExpSet(int companionIndex)
    {
        (int, int) levelExp = CompanionManager.instance.GetCompanionLevelExp(companionIndex);
        _levelLabel.text = $"Lv.{levelExp.Item1}";
        _expProgressBar.value = levelExp.Item2 / (float)CompanionManager.EXPINTERVAL;
        _expProgressBar.title = $"{levelExp.Item2}/{CompanionManager.EXPINTERVAL}";
        var companion = CompanionManager.instance.companionArr[_currentCompanionIndex];
        //_companionEffectLabel.text = SkillManager.instance.GetParsedComplexExplain(companion.companionStatus.companionEffect, levelExp.Item1);
    }
    
    private void OnExitButtonClick()
    {
        UIBroker.InactiveCurrentUI?.Invoke();
    }
    private void InitStatusPanel()
    {
        VisualElement passiveParent = root.Q<VisualElement>("PassiveParent");
        _passiveSlotArr = new VisualElement[passiveParent.childCount];
        for (int i = 0; i < _passiveSlotArr.Length; i++)
        {
            int skillIndex = i;
            VisualElement passiveSlot = passiveParent.ElementAt(i);
            _passiveSlotArr[skillIndex] = passiveSlot;
            passiveSlot.Q<Button>().RegisterCallback<ClickEvent>(evt => OnPassiveButtonClick(skillIndex));
        }

    }
    public void ActiveUI(int companionIndex)
    {
        UIBroker.InactiveCurrentUI += RefreshRenderLayer;
        root.style.display = DisplayStyle.Flex;
        SwitchCompanion(companionIndex);
        ShowCategori(true);
    }
    private void SwitchCompanion(int companionIndex)
    {
        _currentCompanionIndex = companionIndex;
        CompanionStatus companionStatus = CompanionManager.instance.companionArr[_currentCompanionIndex].companionStatus;

        UIBroker.ActiveTranslucent(root, true);
        for (int i = 0; i < _renderTextureArr.Length; i++)
        {
            if (_currentCompanionIndex == i)
            {
                _renderTextureArr[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                _renderTextureArr[i].style.display = DisplayStyle.None;
            }
        }
        //_jobLabel.text = companionStatus.companionJob;
        _nameLabel.text = companionStatus.companionName;
        StatusSet();
        PromoteSet();
    }
    private void StatusSet()
    {
        for (int i = 0; i < _passiveSlotArr.Length; i++)
        {
            int skillIndex = i;
            VisualElement passiveSlot = _passiveSlotArr[i];
            SkillData skillData = _companionManager.companionArr[_currentCompanionIndex].companionStatus.companionSkillArr[skillIndex];
            VisualElement iconSprite = passiveSlot.Q<VisualElement>("IconSprite");
            Label nameLabel = passiveSlot.Q<Label>("NameLabel");
            nameLabel.text = skillData.skillName;
            Label effectLabel = passiveSlot.Q<Label>("EffectLabel");
            effectLabel.text = skillData.simple;
            Label skillLevelLabel = passiveSlot.Q<Label>("SkillLevelLabel");
            if (!_gameData.skillLevel.TryGetValue(skillData.uid, out int currentLevel))
            {
                currentLevel = 0;
            }
            skillLevelLabel.text = $"Lv.{currentLevel}";
            if (currentLevel == PriceManager.MAXCOMPANIONSKILLLEVEL)
            {
                passiveSlot.Q<Button>().style.display = DisplayStyle.None;
                passiveSlot.Q<Label>("MaxLevelLabel").style.display = DisplayStyle.Flex;
            }
            else
            {
                passiveSlot.Q<Button>().style.display = DisplayStyle.Flex;
                passiveSlot.Q<Label>("MaxLevelLabel").style.display = DisplayStyle.None;
                PriceInfo.CompanionSkillPrice price = PriceManager.instance.GetRequireCompanionSkill_CloverFragment(_currentCompanionIndex, skillIndex, currentLevel + 1);
                Label cloverLabel = passiveSlot.Q<Label>("CloverLabel");
                cloverLabel.text = price.clover.ToString();
                Label fragmentLabel = passiveSlot.Q<Label>("FragmentLabel");
                VisualElement fragmentSprite = passiveSlot.Q<VisualElement>("FragmentSprite");
                fragmentSprite.style.backgroundImage = new(PriceManager.instance.fragmentSprites[(int)price.fragmentRarity]);
                fragmentLabel.text = price.fragment.ToString();
            }
            iconSprite.style.backgroundImage = new(skillData.iconSprite);
        }
        OnCompanionExpSet(_currentCompanionIndex);
        UIBroker.SwitchRenderTargetLayer(new string[] { "RenderTexture_0", $"RenderTexture_{_currentCompanionIndex + 1}" });
    }
    private void PromoteSet()
    {
        Dictionary<int, (StatusType, Rarity)> companionPromoteDict = _gameData.companionPromoteEffect[_currentCompanionIndex];
        int[] companionJobDegree = _gameData.companionPromoteTech[_currentCompanionIndex];
        _currentActiveEffectIndex = companionJobDegree.Max() +2;
        for (int i = 0; i < _promoteSlotArr.Length; i++)
        {
            VisualElement ablePanel = _promoteSlotArr[i].Q<VisualElement>("AblePanel");
            VisualElement disablePanel = _promoteSlotArr[i].Q<VisualElement>("DisablePanel");

            if (_currentActiveEffectIndex <= i)
            {
                ablePanel.style.display = DisplayStyle.None;
                disablePanel.style.display = DisplayStyle.Flex;
                continue;
            }
            ablePanel.style.display = DisplayStyle.Flex;
            disablePanel.style.display = DisplayStyle.None;
            
            if (companionPromoteDict.TryGetValue(i, out (StatusType, Rarity) tuple))
            {
                SetPromoteEffectLabel(tuple, i);
            }
            else
            {
                Label effectLabel = ablePanel.Q<Label>("EffectLabel");
                effectLabel.text = string.Empty;
            }
            _isLockEffectArr[i] = false;
            ablePanel.Q<VisualElement>("LockPanel").style.display = DisplayStyle.None;
            _cloverPriceLabel.text = CompanionManager.PROMOTE_EFFECT_CHANGE_PRICE.ToString();
        }
    }
    private void SetPromoteEffectLabel((StatusType, Rarity)? nullableTuple, int effectIndex)
    {
        Label effectLabel = _promoteSlotArr[effectIndex].Q<Label>("EffectLabel");
        if (nullableTuple == null)
        {
            effectLabel.text = string.Empty;
            return;
        }
        (StatusType, Rarity) tuple = nullableTuple.Value;
        float effectValue = CompanionManager.instance.GetCompanionPromoteValue(tuple.Item1, tuple.Item2);
        
        effectLabel.text = CompanionManager.instance.GetCompanionPromoteText(tuple.Item1, effectValue);
        effectLabel.style.color = PriceManager.instance.rarityColor[(int)tuple.Item2];
    }

    private void RefreshRenderLayer()
    {
        UIBroker.SwitchRenderTargetLayer(new string[] { "RenderTexture_0", "RenderTexture_1", "RenderTexture_2", "RenderTexture_3" });
        UIBroker.InactiveCurrentUI -= RefreshRenderLayer;
    }

    private void InitCategoriButton()
    {
        _statusButton = root.Q<Button>("StatusButton");
        _promoteButton = root.Q<Button>("PromoteButton");
        _statusButton.RegisterCallback<ClickEvent>(evt =>
        {
            ShowCategori(true);
        });
        _promoteButton.RegisterCallback<ClickEvent>(evt =>
        {
            ShowCategori(false);
        });
    }
    void ShowCategori(bool isStatus)
    {
        _statusButton.Q<VisualElement>("SelectedPanel").style.display = isStatus ? DisplayStyle.Flex : DisplayStyle.None;
        _promoteButton.Q<VisualElement>("SelectedPanel").style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;
        _mainPanelArr[0].style.display = isStatus ? DisplayStyle.Flex : DisplayStyle.None;
        _mainPanelArr[1].style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;
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
