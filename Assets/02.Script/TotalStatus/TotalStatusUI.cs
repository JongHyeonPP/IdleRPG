using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TotalStatusUI : MonoBehaviour
{
    public VisualElement root { private set; get; }
    private Dictionary<StatusType, Label> _setDict;
    private EquipSlot _weaponSlot;
    private EquipSlot _accessoriesSlot;
    private PlayerStatus _status;
    private Label _levelLabel;
    private Label _nameLabel;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        //Top 초기화 (Button)
        TopAreaInit();
        //Middle 초기화 (EquipSlot, PlayerSpriteArea)
        MiddleAreaInit();
        //Bottom 초기화 (StatusSet)
        BottomAreaInit();
        //ExitButton
        Button exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(click => UIBroker.InactiveCurrentUI?.Invoke());
    }

    private void BottomAreaInit()
    {
        VisualElement setVertical = root.Q<VisualElement>("SetVertical");
        _setDict = Enum.GetValues(typeof(StatusType))
            .Cast<StatusType>()
            .ToDictionary(
                statusType => statusType,
                statusType => setVertical.Q<VisualElement>($"{statusType}Set").Q<Label>("ValueText")
            );
        IEnumerable<StatusType> statusTypes
            = Enum.GetValues(typeof(StatusType))
            .Cast<StatusType>();
        foreach (StatusType statusType in statusTypes)
        {
            Label typeLabel = setVertical.Q<VisualElement>($"{statusType}Set").Q<Label>("StatusTypeText");
            switch (statusType)
            {
                case StatusType.MaxHp:
                    typeLabel.text = "체력";
                    break;
                case StatusType.Power:
                    typeLabel.text = "공격력";
                    break;
                case StatusType.HpRecover:
                    typeLabel.text = "체력 회복";
                    break;
                case StatusType.Critical:
                    typeLabel.text = "치명타 확률";
                    break;
                case StatusType.CriticalDamage:
                    typeLabel.text = "치명타 피해 증가";
                    break;
                case StatusType.Resist:
                    typeLabel.text = "저항력";
                    break;
                case StatusType.Penetration:
                    typeLabel.text = "관통력";
                    break;
                case StatusType.GoldAscend:
                    typeLabel.text = "골드 추가 획득";
                    break;
                case StatusType.ExpAscend:
                    typeLabel.text = "경험치 추가 획득";
                    break;
                case StatusType.MaxMp:
                    typeLabel.text = "마나";
                    break;
                case StatusType.MpRecover:
                    typeLabel.text = "마나 회복";
                    break;
            }
        }
    }
    private void MiddleAreaInit()
    {
        //EquipSlot
        VisualElement weaponSlotRoot = root.Q<VisualElement>("WeaponSlot");
        VisualElement accessoriesSlotRoot = root.Q<VisualElement>("AccessoriesSlot");
        _weaponSlot = new(weaponSlotRoot);
        _accessoriesSlot = new(accessoriesSlotRoot);
        //PlayerSpriteArea
        VisualElement area = root.Q<VisualElement>("PlayerArea");
        _levelLabel = area.Q<Label>("LevelLabel");
        _nameLabel = area.Q<Label>("NameLabel");
        PlayerBroker.OnSetName += SetName;
        BattleBroker.OnLevelExpSet += SetLevel;
        SetLevel();
        SetName(GameManager.instance.userName);
    }
    private void TopAreaInit()
    {
        Button statusButton = root.Q<Button>("StatusButton");
        Button appearanceButton = root.Q<Button>("StatusButton");
    }

    private void Start()
    {
        PlayerController controller = (PlayerController)PlayerBroker.GetPlayerController();
        _status = (PlayerStatus)controller.GetStatus();
    }
    private void SetName(string name)
    {
        _nameLabel.text = name;
    }
    private void SetLevel()
    {
        _levelLabel.text = $"Lv. {GameManager.instance.gameData.level}";
    }
    public void ActiveTotalStatusUI()
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        SetContent();
    }

    private void SetContent()
    {
        _setDict[StatusType.Power].text = _status.Power.ToString("N0");
        _setDict[StatusType.MaxHp].text = _status.MaxHp.ToString("N0");
        _setDict[StatusType.HpRecover].text = _status.HpRecover.ToString("N0");
        _setDict[StatusType.Critical].text = _status.Critical.ToString("F1")+'%';
        _setDict[StatusType.CriticalDamage].text = _status.CriticalDamage.ToString("F1")+'%';
        _setDict[StatusType.MaxMp].text = _status.MaxMp.ToString("N0");
        _setDict[StatusType.MpRecover].text = _status.MpRecover.ToString("N0");
        _setDict[StatusType.Resist].text = _status.Resist.ToString("N0");
        _setDict[StatusType.Penetration].text = _status.Penetration.ToString("N0");
        _setDict[StatusType.GoldAscend].text = _status.GoldAscend.ToString("F1")+'%';
        _setDict[StatusType.ExpAscend].text = _status.ExpAscend.ToString("F1")+'%';
    }
}
internal class EquipSlot
{
    private Label _nameLabel;
    private VisualElement _equipIcon;
    internal EquipSlot(VisualElement slotRoot)
    {
        _nameLabel = slotRoot.Q<Label>("NameLabel");
        _equipIcon = slotRoot.Q<VisualElement>("EquipIcon");
    }
}
