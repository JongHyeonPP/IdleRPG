using EnumCollection;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillInfoUI : MonoBehaviour
{
    [SerializeField]GameData _gameData;
    [SerializeField]SkillUI skillUI;
    [SerializeField]EquipedSkillUI equipedSkillUI;
    [SerializeField]SkillAcquireUI skillAcquireUI;
    public VisualElement root { get; private set; }
    private SkillData currentSkillData;//현재 선택된 스킬 데이터
    //채우기 위한 필드
    private VisualElement iconVe;
    private Label levelLabel;
    private Label titleLabel;
    private Label rarityLabel;
    private Label simpleLabel;
    private Label complexLabel;
    private Label coolTypeLabel;
    private Label coolNumLabel;
    private Label mpLabel;
    private Label fragmentLabel;
    private readonly Color _valueColor = new(1.0f, 0.5f, 0.0f);
    //소유, 비소유 분기
    private VisualElement ownPanel;
    private VisualElement disownPanel;
    //이벤트를 위한 필드
    private Button upgradeButton;  
    private Button equipButton;  
    private Button acquireButton;
    private Label maxLevelLabel;
    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
        SetUI();
    }

    private void SetUI()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        iconVe = root.Q<VisualElement>("SkillIcon");
        levelLabel = root.Q<Label>("LevelLabel");
        titleLabel = root.Q<Label>("TitleLabel");
        rarityLabel = root.Q<Label>("RarityLabel");
        simpleLabel = root.Q<Label>("SimpleLabel");
        complexLabel = root.Q<Label>("ComplexLabel");
        coolTypeLabel = root.Q<Label>("CoolTypeLabel");
        coolNumLabel = root.Q<Label>("CoolNumLabel");
        mpLabel = root.Q<Label>("MpLabel");
        fragmentLabel = root.Q<Label>("FragmentLabel");
        ownPanel = root.Q<VisualElement>("OwnPanel");
        disownPanel = root.Q<VisualElement>("DisownPanel");
        upgradeButton = root.Q<Button>("UpgradeButton");
        equipButton = root.Q<Button>("EquipButton");
        acquireButton = root.Q<Button>("AcquireButton");
        maxLevelLabel = root.Q<Label>("MaxLevelLabel");
        //EventSet
        upgradeButton.RegisterCallback<ClickEvent>(click => OnUpgradeButtonClick());
        equipButton.RegisterCallback<ClickEvent>(click => OnEquipButtonClick());
        acquireButton.RegisterCallback<ClickEvent>(click => OnAcquireButtonClick());
    }

    public void ActiveUI(SkillData skillData)
    {
        if (!_gameData.skillLevel.TryGetValue(skillData.name, out int skillLevel))
        {
            skillLevel = 0;
        }
        currentSkillData = skillData;
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        iconVe.style.backgroundImage = new(skillData.iconSprite);
        levelLabel.text = $"Lv.{skillLevel}";
        titleLabel.text = skillData.skillName;
        rarityLabel.text = $"[{skillData.rarity}]";
        simpleLabel.text = skillData.simple;
        complexLabel.text = SkillManager.instance.GetParsedComplexExplain( skillData, skillLevel, _valueColor);
        switch (skillData.skillCoolType)
        {
            case SkillCoolType.ByAtt:
                coolTypeLabel.text = "필요공격수";
                coolNumLabel.text = skillData.coolAttack.ToString();
                break;
            case SkillCoolType.ByTime:
                coolTypeLabel.text = "대기시간";
                coolNumLabel.text = skillData.cooltime.ToString("F0");
                break;
        }
        mpLabel.text = skillData.requireMp.ToString();

        if (skillLevel == 0)
        {
            ownPanel.style.display = DisplayStyle.None;
            disownPanel.style.display = DisplayStyle.Flex;
        }
        else
        {
            ownPanel.style.display = DisplayStyle.Flex;
            disownPanel.style.display = DisplayStyle.None;
            if (skillLevel == PriceManager.MAXPLAYERSKILLLEVEL)
            {
                maxLevelLabel.style.display = DisplayStyle.Flex;
                upgradeButton.style.display = DisplayStyle.None;
            }
            else
            {
                maxLevelLabel.style.display = DisplayStyle.None;
                upgradeButton.style.display = DisplayStyle.Flex;
                fragmentLabel.text = PriceManager.instance.GetRequireFragment_Skill(skillData.rarity, skillLevel + 1).ToString();
            }
        }
    }
    public void OnUpgradeButtonClick()
    {
        Rarity rarity = currentSkillData.rarity;
        string uid = currentSkillData.uid;
        int requiredFragment = PriceManager.instance.GetRequireFragment_Skill(rarity, _gameData.skillLevel[uid]+1);
        if (!_gameData.skillFragment.TryGetValue(rarity, out int ownedSkillFragement))
        {
            ownedSkillFragement = 0;
        }
        if (requiredFragment <= ownedSkillFragement)//재화가 충분할 경우
        {
            _gameData.skillFragment[rarity] -= PriceManager.instance.GetRequireFragment_Skill(rarity, _gameData.skillLevel[uid]+1);
            PlayerBroker.OnFragmentSet(rarity, _gameData.skillFragment[rarity]);
            _gameData.skillLevel[uid]++;
            if (_gameData.skillLevel[uid] == PriceManager.MAXPLAYERSKILLLEVEL)
            {
                maxLevelLabel.style.display = DisplayStyle.Flex;
                upgradeButton.style.display = DisplayStyle.None;
            }
            else
            {
                fragmentLabel.text = PriceManager.instance.GetRequireFragment_Skill(rarity, _gameData.skillLevel[uid]+1).ToString();
            }
            levelLabel.text = $"Lv.{_gameData.skillLevel[currentSkillData.uid]}";
            PlayerBroker.OnSkillLevelSet(currentSkillData.uid, _gameData.skillLevel[currentSkillData.uid]);
        }
        else//재화가 부족한 경우
        {
            Debug.Log("재화 부족");
            return;
        }
        
    }
    public void OnEquipButtonClick()
    {
        UIBroker.InactiveCurrentUI();
        skillUI.ToggleEquipBackground(true);
        equipedSkillUI.SetCurrentSkillId(currentSkillData);
    }
    public void OnAcquireButtonClick()
    {
        skillAcquireUI.ActiveUI();
        root.style.display = DisplayStyle.None;
    }
}