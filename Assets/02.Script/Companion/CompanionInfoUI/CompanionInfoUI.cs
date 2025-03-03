using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionInfoUI : MonoBehaviour
{
    private CompanionManager _companionManager;
    private GameData _gameData;
    public VisualElement root { get; private set; }
    private VisualElement[] _renderTextureArr;
    private Label _jobLabel;
    private Label _nameLabel;
    private Label _levelLabel;
    private Label _companionEffectLabel;
    private ProgressBar _expProgressBar;
    private VisualElement[] _passiveSlotArr;
    private int currentCompanionIndex;
    private void Awake()
    {
        
        root = GetComponent<UIDocument>().rootVisualElement;
        Button exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(evt => OnExitButtonClick());
        CategoriButtonInit();
        VisualElement renderTextureparent = root.Q<VisualElement>("RenderTextureParent");
        _renderTextureArr = new VisualElement[renderTextureparent.childCount];
        for (int i = 0; i < renderTextureparent.childCount; i++)
        {
            _renderTextureArr[i] = renderTextureparent.ElementAt(i);
        }
        _jobLabel = root.Q<Label>("JobLabel");
        _nameLabel = root.Q<Label>("NameLabel");
        _levelLabel = root.Q<Label>("LevelLabel");
        _companionEffectLabel = root.Q<Label>("CompanionEffectLabel");
        _expProgressBar = root.Q<ProgressBar>("ExpProgressBar");
    }
    private void Start()
    {
        _companionManager = CompanionManager.instance;
        BattleBroker.OnCompanionExpSet += OnCompanionExpSet;
        _gameData = StartBroker.GetGameData();

    }

    private void OnCompanionExpSet(int companionIndex)
    {
        (int, int) levelExp = CompanionManager.instance.GetCompanionLevelExp(companionIndex);
        _levelLabel.text = $"Lv.{levelExp.Item1}";
        _expProgressBar.value = levelExp.Item2 / (float)CompanionManager.EXPINTERVAL;
        _expProgressBar.title = $"{levelExp.Item2}/{CompanionManager.EXPINTERVAL}";
        var companion = CompanionManager.instance.companionArr[currentCompanionIndex];
        _companionEffectLabel.text = SkillManager.instance.GetParsedComplexExplain(companion.companionStatus.companionEffect, levelExp.Item1);
    }

    private void OnExitButtonClick()
    {
        UIBroker.InactiveCurrentUI?.Invoke();
    }

    internal void ActiveUI(int companionIndex)
    {
        UIBroker.InactiveCurrentUI += RefreshRenderLayer;
        currentCompanionIndex = companionIndex;
        CompanionStatus companionStatus = CompanionManager.instance.companionArr[currentCompanionIndex].companionStatus;
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        for (int i = 0; i < _renderTextureArr.Length; i++)
        {
            if (currentCompanionIndex == i)
            {
                _renderTextureArr[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                _renderTextureArr[i].style.display = DisplayStyle.None;
            }
        }
        _jobLabel.text = companionStatus.companionJob;
        _nameLabel.text = companionStatus.companionName;


        VisualElement passiveParent = root.Q<VisualElement>("PassiveParent");
        _passiveSlotArr = new VisualElement[passiveParent.childCount];
        for (int i = 0; i < passiveParent.childCount; i++)
        {
            int skillIndex = i;
            VisualElement passiveSlot = passiveParent.ElementAt(i);
            _passiveSlotArr[skillIndex] = passiveSlot;
            SkillData skillData = _companionManager.companionArr[currentCompanionIndex].companionStatus.companionSkillArr[skillIndex];
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
                PriceInfo.CompanionSkillPrice price = PriceManager.instance.GetRequireCompanionSkill_CloverFragment(currentCompanionIndex, skillIndex, currentLevel + 1);
                Label cloverLabel = passiveSlot.Q<Label>("CloverLabel");
                cloverLabel.text = price.clover.ToString();
                Label fragmentLabel = passiveSlot.Q<Label>("FragmentLabel");
                VisualElement fragmentSprite = passiveSlot.Q<VisualElement>("FragmentSprite");
                fragmentSprite.style.backgroundImage = new(PriceManager.instance.fragmentSprites[(int)price.fragmentRarity]);
                fragmentLabel.text = price.fragment.ToString();
                iconSprite.style.backgroundImage = new(skillData.iconSprite);
                _passiveSlotArr[skillIndex].Q<Button>().RegisterCallback<ClickEvent>(evt => OnPassiveButtonClick(skillIndex));
            }
        }
        OnCompanionExpSet(currentCompanionIndex);
        UIBroker.SwitchRenderTargetLayer(new string[] { "RenderTexture_0", $"RenderTexture_{currentCompanionIndex + 1}" });
    }

    private void RefreshRenderLayer()
    {
        UIBroker.SwitchRenderTargetLayer(new string[] { "RenderTexture_0", "RenderTexture_1", "RenderTexture_2", "RenderTexture_3" });
        UIBroker.InactiveCurrentUI -= RefreshRenderLayer;
    }

    private void OnPassiveButtonClick(int skillIndex)
    {
        SkillData skillData = _companionManager.companionArr[currentCompanionIndex].companionStatus.companionSkillArr[skillIndex];
        if (!_gameData.skillLevel.TryGetValue(skillData.uid, out int currentLevel))
        {
            currentLevel = 0;
        }
        //업 하기 전
        PriceInfo.CompanionSkillPrice beforePrice = PriceManager.instance.GetRequireCompanionSkill_CloverFragment(currentCompanionIndex, skillIndex, currentLevel + 1);
        if (!_gameData.skillFragment.ContainsKey(beforePrice.fragmentRarity))
        {
            _gameData.skillFragment[beforePrice.fragmentRarity] = 0;
        }
        if (beforePrice.clover > _gameData.clover || beforePrice.fragment > _gameData.skillFragment[beforePrice.fragmentRarity])
        {
            Debug.Log("재화 부족");
            return;
        }
        else
        {
            _gameData.clover -= beforePrice.clover;
            _gameData.skillFragment[beforePrice.fragmentRarity] -= beforePrice.fragment;
        }
        //업 한 이후
        _gameData.skillLevel[skillData.uid] = ++currentLevel;
        _passiveSlotArr[skillIndex].Q<Label>("SkillLevelLabel").text = $"Lv.{currentLevel}";
        if (currentLevel == PriceManager.MAXCOMPANIONSKILLLEVEL)
        {
            _passiveSlotArr[skillIndex].Q<Button>().style.display = DisplayStyle.None;
            _passiveSlotArr[skillIndex].Q<VisualElement>("MaxLevelLabel").style.display = DisplayStyle.Flex;
        }
        else
        {
            _passiveSlotArr[skillIndex].Q<Button>().style.display = DisplayStyle.Flex;
            _passiveSlotArr[skillIndex].Q<VisualElement>("MaxLevelLabel").style.display = DisplayStyle.None;
            PriceInfo.CompanionSkillPrice afterPrice = PriceManager.instance.GetRequireCompanionSkill_CloverFragment(currentCompanionIndex, skillIndex, currentLevel + 1);
            Label cloverLabel = _passiveSlotArr[skillIndex].Q<Label>("CloverLabel");
            cloverLabel.text = afterPrice.clover.ToString();
            Label fragmentLabel = _passiveSlotArr[skillIndex].Q<Label>("FragmentLabel");
            fragmentLabel.text = afterPrice.fragment.ToString();
        }

        PlayerBroker.OnSkillLevelSet(skillData.uid, currentLevel);
        BattleBroker.OnCompanionExpSet(currentCompanionIndex);
        StartBroker.SaveLocal();
    }

    private void CategoriButtonInit()
    {
        Button statusButton = root.Q<Button>("StatusButton");
        Button promoteButton = root.Q<Button>("PromoteButton");
        statusButton.RegisterCallback<ClickEvent>(evt =>
        {
            ShowCategori(true);
        });
        promoteButton.RegisterCallback<ClickEvent>(evt =>
        {
            ShowCategori(false);
        });
        ShowCategori(true);

        void ShowCategori(bool isStatus)
        {
            statusButton.Q<VisualElement>("SelectedPanel").style.display = isStatus ? DisplayStyle.Flex : DisplayStyle.None;
            promoteButton.Q<VisualElement>("SelectedPanel").style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;
            root.Q<VisualElement>("StatusPanel").style.display = isStatus ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<VisualElement>("PromotePanel").style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
