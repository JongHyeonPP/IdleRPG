using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public partial class TotalStatusUI
{
    private void StatusPanelInit()
    {
        VisualElement area = root.Q<VisualElement>("PlayerSpriteArea");
        _levelLabel = area.Q<Label>("LevelLabel");
        _nameLabel = area.Q<Label>("NameLabel");

        SetLevel();
        SetName(StartBroker.GetGameData().userName);

        VisualElement setVertical = root.Q<VisualElement>("SetVertical");
        _setDict = Enum.GetValues(typeof(StatusType))
            .Cast<StatusType>()
            .ToDictionary(
                statusType => statusType,
                statusType => setVertical.Q<VisualElement>($"{statusType}Set").Q<Label>("ValueText")
            );

        foreach (StatusType statusType in Enum.GetValues(typeof(StatusType)))
        {
            Label typeLabel = setVertical.Q<VisualElement>($"{statusType}Set").Q<Label>("StatusTypeText");
            switch (statusType)
            {
                case StatusType.MaxHp:
                    typeLabel.text = "체력"; break;
                case StatusType.Power:
                    typeLabel.text = "공격력"; break;
                case StatusType.HpRecover:
                    typeLabel.text = "체력 회복"; break;
                case StatusType.Critical:
                    typeLabel.text = "치명타 확률"; break;
                case StatusType.CriticalDamage:
                    typeLabel.text = "치명타 피해량"; break;
                case StatusType.GoldAscend:
                    typeLabel.text = "골드 추가 획득"; break;
                case StatusType.ExpAscend:
                    typeLabel.text = "경험치 추가 획득"; break;
                case StatusType.MaxMp:
                    typeLabel.text = "마나"; break;
                case StatusType.MpRecover:
                    typeLabel.text = "마나 회복"; break;
                case StatusType.AttBuff:
                    typeLabel.text = "공격력 증가량"; break;
                case StatusType.DefBuff:
                    typeLabel.text = "방어력 증가량"; break;
            }
        }
    }

    private void CategoriButtonInit()
    {
        Button statusButton = root.Q<Button>("StatusButton");
        Button appearanceButton = root.Q<Button>("AppearanceButton");
        statusButton.RegisterCallback<ClickEvent>(evt =>
        {
            ShowStatusOrAppearance(true);
            ClearCostumeInfo();
        });
        appearanceButton.RegisterCallback<ClickEvent>(evt =>
        {
            ShowStatusOrAppearance(false);
        });
        ShowStatusOrAppearance(true);

        void ShowStatusOrAppearance(bool isStatus)
        {
            statusButton.Q<VisualElement>("SelectedPanel").style.display = isStatus ? DisplayStyle.Flex : DisplayStyle.None;
            appearanceButton.Q<VisualElement>("SelectedPanel").style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;
            root.Q<VisualElement>("StatusPanel").style.display = isStatus ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<VisualElement>("AppearancePanel").style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;

            UpdateCostumeUI();
        }
    }

    private void SetName(string name)
    {
        _nameLabel.text = name;
    }

    private void SetLevel()
    {
        _levelLabel.text = $"Lv. {StartBroker.GetGameData().level}";
    }

    public void ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        SetContent();
    }

    private void SetContent()
    {
        var playerController = (PlayerController)BattleBroker.GetPlayerController();
        _setDict[StatusType.Power].text = _status.Power.ToString("N0");
        _setDict[StatusType.MaxHp].text = _status.MaxHp.ToString("N0");
        _setDict[StatusType.HpRecover].text = _status.HpRecover.ToString("N0");
        _setDict[StatusType.Critical].text = _status.Critical.ToString("F1") + '%';
        _setDict[StatusType.CriticalDamage].text = (_status.CriticalDamage * 100).ToString("F0") + '%';
        _setDict[StatusType.MaxMp].text = _status.MaxMp.ToString("N0");
        _setDict[StatusType.MpRecover].text = _status.MpRecover.ToString("N0");
        _setDict[StatusType.AttBuff].text = '+' + playerController.GetPWValue(SkillType.AttBuff).ToString("N0") + '%';
        _setDict[StatusType.DefBuff].text = '+' + playerController.GetPWValue(SkillType.Durability).ToString("N0") + '%';
        _setDict[StatusType.GoldAscend].text = (_status.GoldAscend * 100f).ToString("F1") + '%';
        _setDict[StatusType.ExpAscend].text = (_status.ExpAscend * 100f).ToString("F1") + '%';
    }
}