using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionPromoteInfoUI : MonoBehaviour, IGeneralUI
{
    public VisualElement root {get;private set;}
    private Button exitButton;
    private VisualElement background;
    private CompanionPromoteData _companionPromoteData;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(evt=>OnExitButtonClick());
        background = root.Q<VisualElement>("Background");
        background.RegisterCallback<ClickEvent>(evt=> root.style.display = DisplayStyle.None);
    }
    void Start()
    {
        InitLabelSets();
        InitProbabilityLabel();
    }

    private void InitProbabilityLabel()
    {
        VisualElement probabilityLabelSet = root.Q<VisualElement>("ProbabilityLabelSet");

        Label commonLabel = probabilityLabelSet.Q<Label>("CommonLabel");
        Label uncommonLabel = probabilityLabelSet.Q<Label>("UncommonLabel");
        Label rareLabel = probabilityLabelSet.Q<Label>("RareLabel");
        Label uniqueLabel = probabilityLabelSet.Q<Label>("UniqueLabel");
        Label legendaryLabel = probabilityLabelSet.Q<Label>("LegendaryLabel");
        Label mythicLabel = probabilityLabelSet.Q<Label>("MythicLabel");

        commonLabel.style.color = PriceManager.instance.rarityColor[0];
        commonLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[0] * 100f)}%";

        uncommonLabel.style.color = PriceManager.instance.rarityColor[1];
        uncommonLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[1] * 100f)}%";

        rareLabel.style.color = PriceManager.instance.rarityColor[2];
        rareLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[2] * 100f)}%";

        uniqueLabel.style.color = PriceManager.instance.rarityColor[3];
        uniqueLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[3] * 100f)}%";

        legendaryLabel.style.color = PriceManager.instance.rarityColor[4];
        legendaryLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[4] * 100f)}%";

        mythicLabel.style.color = PriceManager.instance.rarityColor[5];
        mythicLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[5] * 100f)}%";
    }

    private void InitLabelSets()
    {
        _companionPromoteData = CompanionManager.instance.companionPromoteData;
        SetLabelSet(root.Q<VisualElement>("PowerLabelSet"), _companionPromoteData.power, "추가 공격력(%)");
        SetLabelSet(root.Q<VisualElement>("CriticalDamageLabelSet"), _companionPromoteData.criticalDamage, "치명타 데미지(%)");
        SetLabelSet(root.Q<VisualElement>("MaxHpLabelSet"), _companionPromoteData.maxHp, "추가 체력(%)");
        SetLabelSet(root.Q<VisualElement>("HpRecoverLabelSet"), _companionPromoteData.hpRecover, "추가 체력 회복량(%)");
        SetLabelSet(root.Q<VisualElement>("MaxMpLabelSet"), _companionPromoteData.maxMp, "추가 마나(%)");
        SetLabelSet(root.Q<VisualElement>("MpRecoverLabelSet"), _companionPromoteData.mpRecover, "추가 마나 회복량(%)");
        SetLabelSet(root.Q<VisualElement>("GoldAscendLabelSet"), _companionPromoteData.goldAscend, "추가 골드(%)");
        SetLabelSet(root.Q<VisualElement>("ResistLabelSet"), _companionPromoteData.resist, "저항력", false);
        SetLabelSet(root.Q<VisualElement>("PenetrationLabelSet"), _companionPromoteData.penetration, "관통력", false);
        SetLabelSet(root.Q<VisualElement>("ExpAscendLabelSet"), _companionPromoteData.expAscend, "추가 경험치");
    }

    private void SetLabelSet(VisualElement labelSet, float[] statusArr, string categoriStr, bool isPercent = true)
    {
        labelSet.Q<Label>("CategoriLabel").text = categoriStr;

        labelSet.Q<Label>("CommonLabel").text = (isPercent ? statusArr[0] * 100f : statusArr[0]).ToString();
        labelSet.Q<Label>("CommonLabel").style.color = PriceManager.instance.rarityColor[0];

        labelSet.Q<Label>("UncommonLabel").text = (isPercent ? statusArr[1] * 100f : statusArr[1]).ToString();
        labelSet.Q<Label>("UncommonLabel").style.color = PriceManager.instance.rarityColor[1];

        labelSet.Q<Label>("RareLabel").text = (isPercent ? statusArr[2] * 100f : statusArr[2]).ToString();
        labelSet.Q<Label>("RareLabel").style.color = PriceManager.instance.rarityColor[2];

        labelSet.Q<Label>("UniqueLabel").text = (isPercent ? statusArr[3] * 100f : statusArr[3]).ToString();
        labelSet.Q<Label>("UniqueLabel").style.color = PriceManager.instance.rarityColor[3];

        labelSet.Q<Label>("LegendaryLabel").text = (isPercent ? statusArr[4] * 100f : statusArr[4]).ToString();
        labelSet.Q<Label>("LegendaryLabel").style.color = PriceManager.instance.rarityColor[4];

        labelSet.Q<Label>("MythicLabel").text = (isPercent ? statusArr[5] * 100f : statusArr[5]).ToString();
        labelSet.Q<Label>("MythicLabel").style.color = PriceManager.instance.rarityColor[5];
    }

    public void ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
    }
    private void OnExitButtonClick()
    {
        root.style.display = DisplayStyle.None;
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
