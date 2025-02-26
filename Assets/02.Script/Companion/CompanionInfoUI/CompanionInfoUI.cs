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
    private ProgressBar _expProgressBar;
    
    private void Awake()
    {
        _companionManager = CompanionManager.instance;
        root = GetComponent<UIDocument>().rootVisualElement;
        Button exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(evt=>OnExitButtonClick());
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
        _expProgressBar = root.Q<ProgressBar>("ExpProgressBar");
    }
    private void Start()
    {
        _gameData = StartBroker.GetGameData();
    }
    private void OnExitButtonClick()
    {
        UIBroker.InactiveCurrentUI?.Invoke();
    }

    internal void ActiveUI(int companionIndex)
    {
        var companionStatus = CompanionManager.instance.companionStatusArr[companionIndex];
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        for (int i = 0; i < _renderTextureArr.Length; i++)
        {
            if (companionIndex == i)
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
        int skillLevelSum = 0;
        for (int i = 0; i < passiveParent.childCount; i++)
        {
            VisualElement parentSlot = passiveParent.ElementAt(i);
            SkillData skillData = _companionManager.companionStatusArr[companionIndex].companionSkillArr[i];
            VisualElement iconSprite = parentSlot.Q<VisualElement>("IconSprite");
            Label nameLabel = parentSlot.Q<Label>("NameLabel");
            nameLabel.text = skillData.skillName;
            Label effectLabel = parentSlot.Q<Label>("EffectLabel");
            effectLabel.text = skillData.simple;
            Label skillLevelLabel = parentSlot.Q<Label>("SkillLevelLabel");
            if (!_gameData.skillLevel.TryGetValue(skillData.uid, out int currentLevel))
            {
                currentLevel = 0;
            }
            skillLevelLabel.text = $"Lv.{currentLevel}";
            skillLevelSum += currentLevel;
            (int, int) price = PriceManager.instance.GetRequireCompanionSkill_CloverFragment(companionIndex, i, currentLevel + 1);
            Label cloverLabel = parentSlot.Q<Label>("CloverLabel");
            cloverLabel.text = price.Item1.ToString();
            Label fragmentLabel = parentSlot.Q<Label>("FragmentLabel");
            fragmentLabel.text = price.Item2.ToString();
            iconSprite.style.backgroundImage = new(skillData.iconSprite);
        }
        int companionLevel = skillLevelSum / CompanionManager.EXPINTERVAL;
        int progressIndex = skillLevelSum % CompanionManager.EXPINTERVAL;
        _levelLabel.text = $"Lv.{companionLevel}";
        _expProgressBar.value = progressIndex / CompanionManager.EXPINTERVAL;
        _expProgressBar.title = $"{progressIndex}/{CompanionManager.EXPINTERVAL}";
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
