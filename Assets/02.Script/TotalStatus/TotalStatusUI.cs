using EnumCollection;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public partial class TotalStatusUI : MonoBehaviour
{
    public VisualElement root { private set; get; }

    // Status
    private Dictionary<StatusType, Label> _setDict;
    private PlayerStatus _status;
    private Label _levelLabel;
    private Label _nameLabel;
    private VisualElement _playerWeaponSlot;
    private VisualElement[] _companionWeaponSlot = new VisualElement[3];

    // 코스튬
    [SerializeField] VisualTreeAsset _costumeSlotAsset;         // 코스튬 복제 에셋
    [SerializeField] VisualTreeAsset _costumeContainerAsset;    // 코스튬 컨테이너
    private VisualElement _costumeInfoPanel;                    // 코스튬 패널
    private ScrollView _costumeSV;                              // 코스튬 스크롤뷰
    private Label _costumeInfoName;                             // 코스튬 정보 이름 라벨
    private Label _costumeInfoDescription;                      // 코스튬 정보 설명 라벨
    private Button _costumeInfoEquipButton;                     // 코스튬 정보 장착 버튼
    private VisualElement _costumeInfoIcon;                     // 코스튬 정보 아이콘
    private CostumeItem _selectedCostume;                       // 코스튬 현재 선택된 코스튬 아이템
    private VisualElement _currentSelectedSlot = null;          // 코스튬 현재 선택된 슬롯

    // 코스튬 필터
    private CostumeFilterType _currentFilterType = CostumeFilterType.All;
    private Button _filterAllButton;     // 전체
    private Button _filterTopButton;     // 상의
    private Button _filterBottomButton;  // 하의
    private Button _filterFaceButton;    // 외모
    private Button _filterEtcButton;     // 기타

    // 코스튬 색상
    Color _costumeBtnOn;
    Color _costumeBtnOff;
    Color _costumeBtnEquip;
    Color _costumeBtnFilterOn;
    Color _costumeBtnFilterOff;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        CategoriButtonInit();
        StatusPanelInit();
        SetupCostumeInfoPanel(); // 코스튬

        Button exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(click =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            UIBroker.InactiveCurrentUI?.Invoke();
            CostumeManager.Instance.UpdateGameAppearanceData();

            NetworkBroker.SaveServerData(); // 필요없을시 삭제 // 삐용
            ParticleFxManager.Instance.Stop("CostumeEffect");
        });

        InitEquipSlot();
        PlayerBroker.OnEquipWeapon += OnEquipWeapon;
        PlayerBroker.OnSetName += SetName;
        PlayerBroker.OnLevelExpSet += SetLevel;
    }

    private void Start()
    {
        PlayerController controller = (PlayerController)BattleBroker.GetPlayerController();
        _status = (PlayerStatus)controller.GetStatus();
        AppearancePanelInit();
    }

    private void InitEquipSlot()
    {
        _playerWeaponSlot = root.Q<VisualElement>("PlayerWeaponSlot");
        _playerWeaponSlot.Q<Label>("CategoriLabel").text = "플레이어 무기";
        for (int i = 0; i < 3; i++)
        {
            _companionWeaponSlot[i] = root.Q<VisualElement>($"CompanionWeaponSlot_{i}");
            _companionWeaponSlot[i].Q<Label>("CategoriLabel").text = $"동료 무기 {i + 1}";
        }
    }

    private void OnEquipWeapon(object obj, WeaponType weaponType)
    {
        WeaponData weaponData = (WeaponData)obj;
        VisualElement currentWeaponSlot = null;
        switch (weaponType)
        {
            case WeaponType.Melee:
                currentWeaponSlot = _playerWeaponSlot;
                break;
            case WeaponType.Bow:
                currentWeaponSlot = _companionWeaponSlot[0];
                break;
            case WeaponType.Shield:
                currentWeaponSlot = _companionWeaponSlot[1];
                break;
            case WeaponType.Staff:
                currentWeaponSlot = _companionWeaponSlot[2];
                break;
        }
        VisualElement equipIcon = currentWeaponSlot.Q<VisualElement>("EquipIcon");
        Label nameLabel = currentWeaponSlot.Q<Label>("NameLabel");
        if (weaponData == null)
        {
            equipIcon.style.backgroundImage = null;
            nameLabel.text = "없음";
        }
        else
        {
            WeaponManager.instance.SetWeaponIconToVe(weaponData, equipIcon);
            nameLabel.text = weaponData.name;
        }

    }
}