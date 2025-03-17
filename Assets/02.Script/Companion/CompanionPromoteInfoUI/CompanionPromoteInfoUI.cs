using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionPromoteInfoUI : MonoBehaviour
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

        commonLabel.style.color = CompanionManager.instance.commonColor;
        commonLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[0] * 100f)}%";

        uncommonLabel.style.color = CompanionManager.instance.uncommonColor;
        uncommonLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[1] * 100f)}%";

        rareLabel.style.color = CompanionManager.instance.rareColor;
        rareLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[2] * 100f)}%";

        uniqueLabel.style.color = CompanionManager.instance.uniqueColor;
        uniqueLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[3] * 100f)}%";

        legendaryLabel.style.color = CompanionManager.instance.legendaryColor;
        legendaryLabel.text = $"{Mathf.RoundToInt(_companionPromoteData.probabilityInRarity[4] * 100f)}%";

        mythicLabel.style.color = CompanionManager.instance.mythicColor;
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
        labelSet.Q<Label>("CommonLabel").style.color = CompanionManager.instance.commonColor;

        labelSet.Q<Label>("UncommonLabel").text = (isPercent ? statusArr[1] * 100f : statusArr[1]).ToString();
        labelSet.Q<Label>("UncommonLabel").style.color = CompanionManager.instance.uncommonColor;

        labelSet.Q<Label>("RareLabel").text = (isPercent ? statusArr[2] * 100f : statusArr[2]).ToString();
        labelSet.Q<Label>("RareLabel").style.color = CompanionManager.instance.rareColor;

        labelSet.Q<Label>("UniqueLabel").text = (isPercent ? statusArr[3] * 100f : statusArr[3]).ToString();
        labelSet.Q<Label>("UniqueLabel").style.color = CompanionManager.instance.uniqueColor;

        labelSet.Q<Label>("LegendaryLabel").text = (isPercent ? statusArr[4] * 100f : statusArr[4]).ToString();
        labelSet.Q<Label>("LegendaryLabel").style.color = CompanionManager.instance.legendaryColor;

        labelSet.Q<Label>("MythicLabel").text = (isPercent ? statusArr[5] * 100f : statusArr[5]).ToString();
        labelSet.Q<Label>("MythicLabel").style.color = CompanionManager.instance.mythicColor;
    }

    public void ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
    }
    private void OnExitButtonClick()
    {
        root.style.display = DisplayStyle.None;
    }
}
