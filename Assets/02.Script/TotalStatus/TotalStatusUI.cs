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
    private PlayerStatus _status;
    //StatusVe
    private Label _levelLabel;
    private Label _nameLabel;
    private VisualElement weaponSlot;
    private VisualElement accSlot;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        CategoriButtonInit();
        StatusPanelInit();
        AppearancePanelInit();
        Button exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(click => UIBroker.InactiveCurrentUI?.Invoke());
        InitEquipSlot();
        PlayerBroker.OnEquipWeapon += OnEquipWeapon;
    }
    private void InitEquipSlot()
    {
        weaponSlot = root.Q<VisualElement>("WeaponSlot");
        accSlot = root.Q<VisualElement>("AccSlot");
        weaponSlot.Q<Label>("CategoriLabel").text = "장착 무기";
        accSlot.Q<Label>("CategoriLabel").text = "장착 악세서리";
    }
    private void OnEquipWeapon(object obj, WeaponType weaponType)
    {
        if (weaponType != WeaponType.Melee)
            return;
        WeaponData weaponData = (WeaponData)obj;
        VisualElement equipIcon = weaponSlot.Q<VisualElement>("EquipIcon");
        if (weaponData == null)
        {
            equipIcon.style.backgroundImage = null;
        }
        else
        {
            equipIcon.style.backgroundImage = new(weaponData.WeaponSprite);
            WeaponManager.instance.SetIconScale(weaponData, equipIcon);
            weaponSlot.Q<Label>("NameLabel").text = weaponData.name;
        }
    }
    private void StatusPanelInit()
    {
        //EquipPanel
        VisualElement weaponSlotRoot = root.Q<VisualElement>("WeaponSlot");
        VisualElement accessoriesSlotRoot = root.Q<VisualElement>("AccessoriesSlot");
        VisualElement area = root.Q<VisualElement>("PlayerArea");
        _levelLabel = area.Q<Label>("LevelLabel");
        _nameLabel = area.Q<Label>("NameLabel");
        PlayerBroker.OnSetName += SetName;
        BattleBroker.OnLevelExpSet += SetLevel;
        SetLevel();
        SetName(StartBroker.GetGameData().userName);

        //ValuePanel
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
    private void AppearancePanelInit()
    {
        //주연이가 코스튬 넣어야 함.
    }
    private void CategoriButtonInit()
    {
        Button statusButton = root.Q<Button>("StatusButton");
        Button appearanceButton = root.Q<Button>("AppearanceButton");
        statusButton.RegisterCallback<ClickEvent>(evt =>
        {
            ShowStatusOrAppearance(true);
        });
        appearanceButton.RegisterCallback<ClickEvent>(evt => 
        {
            ShowStatusOrAppearance(false);
        });
        ShowStatusOrAppearance(true);


        void ShowStatusOrAppearance(bool isStatus)
        {
            statusButton.Q<VisualElement>("SelectedPanel").style.display = isStatus? DisplayStyle.Flex:DisplayStyle.None;
            appearanceButton.Q<VisualElement>("SelectedPanel").style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;
            root.Q<VisualElement>("StatusPanel").style.display = isStatus ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<VisualElement>("AppearancePanel").style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;
        }
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
        _levelLabel.text = $"Lv. {StartBroker.GetGameData().level}";
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