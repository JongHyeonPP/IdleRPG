using EnumCollection;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillInfoUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
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
    private Label currencyLabel;
    private ProgressBar levelProgressBar;
    //이벤트를 위한 필드
    private Button upgradeButton;  
    private Button equipButton;  
    private void Awake()
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
        currencyLabel = root.Q<Label>("CurrencyLabel");
        levelProgressBar = root.Q<ProgressBar>("LevelProgressBar");
        upgradeButton = root.Q<Button>("UpgradeButton");
        equipButton = root.Q<Button>("EquipButton");
        //EventSet
        upgradeButton.RegisterCallback<ClickEvent>(click => OnUpgradeButtonClick());
        equipButton.RegisterCallback<ClickEvent>(click => OnEquipButtonClick());
    }
    public void DataInfoSet(SkillData skillData)
    {
        iconVe.style.backgroundImage = new(skillData.icon);
        if (!StartBroker.GetGameData().skillLevel.TryGetValue(skillData.name, out int level))
        {
            level = 0;
        }
        levelLabel.text = $"Lv.{level}";
        titleLabel.text = skillData.skillName;
        rarityLabel.text = $"[{skillData.rarity}]";
        simpleLabel.text = skillData.simple;
        complexLabel.text = skillData.complex;
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
        currencyLabel.text = PriceManager.instance.GetRequireEmerald_Skill(skillData.rarity, level).ToString();
    }
    internal void ActiveUI(SkillData skillData)
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        DataInfoSet(skillData);
    }
    public void OnUpgradeButtonClick()
    {
        Debug.Log("UC");
    }
    public void OnEquipButtonClick()
    {
        Debug.Log("EC");
    }
}