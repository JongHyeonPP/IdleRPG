using EnumCollection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TotalStatusUI : MonoBehaviour
{
    public VisualElement root { private set; get; }
    //Status
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
            UIBroker.InactiveCurrentUI?.Invoke();
            CostumeManager.Instance.UpdateGameAppearanceData();

            NetworkBroker.SaveServerData(); // 필요없을시 삭제 // 삐용
        });

        InitEquipSlot();
        PlayerBroker.OnEquipWeapon += OnEquipWeapon;
        PlayerBroker.OnSetName += SetName;
        PlayerBroker.OnLevelExpSet += SetLevel;
    }

    private void Start()
    {
        PlayerController controller = (PlayerController)PlayerBroker.GetPlayerController();
        _status = (PlayerStatus)controller.GetStatus();

        AppearancePanelInit();
    }

    #region Equip
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
    #endregion
    #region Status
    private void StatusPanelInit()
    {
        //EquipPanel
        VisualElement weaponSlotRoot = root.Q<VisualElement>("WeaponSlot");
        VisualElement accessoriesSlotRoot = root.Q<VisualElement>("AccessoriesSlot");
        VisualElement area = root.Q<VisualElement>("PlayerSpriteArea");
        _levelLabel = area.Q<Label>("LevelLabel");
        _nameLabel = area.Q<Label>("NameLabel");

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
    public void ActiveTotalStatusUI()
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
        //SetContent();
    }
    private void SetContent()
    {
        _setDict[StatusType.Power].text = _status.Power.ToString("N0");
        _setDict[StatusType.MaxHp].text = _status.MaxHp.ToString("N0");
        _setDict[StatusType.HpRecover].text = _status.HpRecover.ToString("N0");
        _setDict[StatusType.Critical].text = _status.Critical.ToString("F1") + '%';
        _setDict[StatusType.CriticalDamage].text = _status.CriticalDamage.ToString("F1") + '%';
        _setDict[StatusType.MaxMp].text = _status.MaxMp.ToString("N0");
        _setDict[StatusType.MpRecover].text = _status.MpRecover.ToString("N0");
        _setDict[StatusType.Resist].text = _status.Resist.ToString("N0");
        _setDict[StatusType.Penetration].text = _status.Penetration.ToString("N0");
        _setDict[StatusType.GoldAscend].text = _status.GoldAscend.ToString("F1") + '%';
        _setDict[StatusType.ExpAscend].text = _status.ExpAscend.ToString("F1") + '%';
    }
    #endregion

    #region 코스튬
    // 코스튬
    // 코스튬 정보 패널 설정 메서드
    private void SetupCostumeInfoPanel()
    {
        // 정보 패널 컨테이너 찾기
        _costumeInfoPanel = root.Q<VisualElement>("CostumeInfoPanel");
        if (_costumeInfoPanel == null)
        {
            Debug.LogError("CostumeInfoPanel을 찾을 수 없습니다.");
            return;
        }

        // 라벨과 버튼 참조 가져오기
        _costumeInfoName = _costumeInfoPanel.Q<Label>("NameLabel");
        _costumeInfoDescription = _costumeInfoPanel.Q<Label>("DescriptionLabel");
        _costumeInfoEquipButton = _costumeInfoPanel.Q<Button>("EquipButton");
        _costumeInfoIcon = _costumeInfoPanel.Q<VisualElement>("IconSprite");

        if (_costumeInfoName == null) Debug.LogError("NameLabel을 찾을 수 없습니다.");
        if (_costumeInfoDescription == null) Debug.LogError("EffectLabel을 찾을 수 없습니다.");
        if (_costumeInfoEquipButton == null) Debug.LogError("EquipButton을 찾을 수 없습니다.");
        if (_costumeInfoIcon == null) Debug.LogError("InfoIconSprite를 찾을 수 없습니다.");

        // 컬러 설정
        ColorUtility.TryParseHtmlString("#FF9C31", out _costumeBtnOn);
        ColorUtility.TryParseHtmlString("#4B4B4B", out _costumeBtnOff);
        ColorUtility.TryParseHtmlString("#B58525", out _costumeBtnFilterOn);
        ColorUtility.TryParseHtmlString("#956035", out _costumeBtnFilterOff);
        ColorUtility.TryParseHtmlString("#D23113", out _costumeBtnEquip);

        // 버튼 클릭 이벤트 연결
        if (_costumeInfoEquipButton != null)
        {
            _costumeInfoEquipButton.RegisterCallback<ClickEvent>(evt => {
                if (_selectedCostume != null)
                {
                    EquipCostume(_selectedCostume);
                    UpdateCostumeInfoPanel(_selectedCostume); // 정보 패널 갱신
                    UpdateEquipButtonText(); // 버튼 텍스트 업데이트

                    // UI 갱신
                    ScrollView costumeScrollView = root.Q<ScrollView>("CostumeScrollView");
                    if (costumeScrollView != null) RefreshCostumeSlots(costumeScrollView);
                }
            });
        }
    }

    /// <summary>
    /// 외형 패널 초기화
    /// </summary>
    private void AppearancePanelInit()
    {
        // CostumeManager 참조
        CostumeManager costumeManager = CostumeManager.Instance;
        if (costumeManager == null)
        {
            //Debug.LogError("CostumeManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // UI 컨트롤 참조
        ScrollView costumeScrollView = root.Q<ScrollView>("CostumeScrollView");
        if (costumeScrollView == null)
        {
            //Debug.LogError("CostumeScrollView를 찾을 수 없습니다.");
            return;
        }

        // 코스튬 정보 패널 초기화
        ClearCostumeInfo();

        // 필터 버튼 초기화
        InitCostumeFilterButtons();

        // 초기 코스튬 슬롯 생성 (모든 코스튬 표시)
        RefreshCostumeSlots(costumeScrollView);
    }

    /// <summary>
    /// 코스튬 슬롯 새로고침
    /// </summary>

    private void RefreshCostumeSlots(ScrollView costumeScrollView)
    {
        if (costumeScrollView == null || _costumeSlotAsset == null) return;

        CostumeManager costumeManager = CostumeManager.Instance;
        if (costumeManager == null) return;

        // 테스트 데이터 (실제로는 게임 데이터에서 가져옴)
        List<string> ownedCostumes = costumeManager.GetOwnedCostumes();

        // 모든 코스튬 데이터 가져오기
        var allCostumeDatas = costumeManager.AllCostumeDatas;
        if (allCostumeDatas == null || allCostumeDatas.Length == 0) return;

        // 필터링 적용 (현재 필터 타입에 맞게 코스튬 목록 필터링)
        int filterTypeIndex = (int)_currentFilterType;
        var filteredCostumes = costumeManager.GetCostumesByFilterType(filterTypeIndex);

        costumeScrollView.Clear();

    
        List<VisualElement> containers = new List<VisualElement>();
        int itemsPerRow = 5;
        int totalContainersNeeded = Mathf.CeilToInt((float)filteredCostumes.Count / itemsPerRow);
        for (int i = 0; i < totalContainersNeeded; i++) // 최대 6개의 컨테이너 (0~5)
        {
            VisualElement container = _costumeContainerAsset.CloneTree();
            costumeScrollView.Add(container);
            if (container != null)
            {
                containers.Add(container);
                // 모든 컨테이너 내용을 초기화
                container.Clear();
                container.style.display = DisplayStyle.None;
                container.style.flexDirection = FlexDirection.Row;
                container.style.flexWrap = Wrap.Wrap;
            }
        }

/*        VisualElement defaultContainer = root.Q<VisualElement>("CostumeContainerTemplate");
        //  ScrollView costumeScrollView = root.Q<ScrollView>("CostumeScrollView");

        if (defaultContainer == null || costumeScrollView == null) return;

        // 기존 컨테이너 제거
        costumeScrollView.Clear();

        List<VisualElement> containers = new List<VisualElement>();
        int itemsPerRow = 5;
        int totalContainersNeeded = Mathf.CeilToInt((float)filteredCostumes.Count / itemsPerRow);

        for (int i = 0; i < totalContainersNeeded; i++)
        {
            VisualElement container = _costumeContainerAsset.CloneTree();
            container.name = $"CostumeContainer{5 + i}";
            container.style.display = DisplayStyle.Flex;

            costumeScrollView.Add(container);
            containers.Add(container);
        }
*/
        if (containers.Count == 0) return;

        // 필터링된 코스튬이 없는 경우 표시할 메시지
        if (filteredCostumes.Count == 0)
        {
            VisualElement noItemContainer = containers[0];
            noItemContainer.style.display = DisplayStyle.Flex;

            // "아이템 없음" 메시지 추가
            Label noItemLabel = new Label($"{GetFilterTypeName()} 코스튬이 없습니다.");
            noItemLabel.style.fontSize = 16;
            noItemLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            noItemLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            noItemLabel.style.paddingTop = 20;
            noItemLabel.style.paddingBottom = 20;

            noItemContainer.Add(noItemLabel);
            return;
        }

        // 코스튬 데이터 5개 단위 그룹화
       // int itemsPerRow = 5;
       // int totalContainersNeeded = Mathf.CeilToInt((float)filteredCostumes.Count / itemsPerRow);

        // 필요한 컨테이너가 충분한지 확인
        if (totalContainersNeeded > containers.Count)
        {
            Debug.LogWarning($"[TotalStatus:Costume] 추가필요! 코스튬 컨테이너 필요갯수 : {totalContainersNeeded}개, 현재 갯수:{containers.Count}개");
        }

        // 각 컨테이너 코스튬 슬롯 배치
        for (int i = 0; i < filteredCostumes.Count; i++)
        {
            int containerIndex = i / itemsPerRow;
            if (containerIndex >= containers.Count) // 사용 가능 컨테이너보다 코스튬이 있는 경우
                break;

            VisualElement currentContainer = containers[containerIndex];
            currentContainer.style.display = DisplayStyle.Flex;      // 컨테이너 표시

            CostumeItem costume = filteredCostumes[i];

            // UXML로 만든 슬롯 복제
            VisualElement costumeSlot = _costumeSlotAsset.CloneTree();

            // 보유 여부에 따른 스타일 적용
            bool isOwned = ownedCostumes.Contains(costume.Uid);

            // 장착 여부에 따른 스타일 적용
            bool isEquipped = costumeManager.IsEquipped(costume.Uid);
            VisualElement background = costumeSlot.Q<VisualElement>("IconFrame");


            if (background != null)
            {
                Sprite frameSpriteOn;
                Sprite frameSpriteOff;
                Sprite frameEquipped;

#if UNITY_EDITOR
                // 프레임 스프라이트 로드 (에디터 모드에서만 작동)
                frameSpriteOn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Costume/frame_violet.png");
                frameSpriteOff = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Costume/frame_black.png");
                frameEquipped = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Costume/frame_red.png"); // 착용 중인 프레임
#endif
#if UNITY_ANDROID
                // 프레임 스프라이트 로드 (런타임/빌드 모두 작동)
                frameSpriteOn = Resources.Load<Sprite>("Costume/frame_violet");
                frameSpriteOff = Resources.Load<Sprite>("Costume/frame_black");
                frameEquipped = Resources.Load<Sprite>("Costume/frame_red"); // 착용 중인 프레임
#endif
                // 착용 중이면 다른 프레임 적용
                if (isOwned && isEquipped)
                {
                    background.style.backgroundImage = new StyleBackground(frameEquipped);
                }
                else
                {
                    background.style.backgroundImage = new StyleBackground(isOwned ? frameSpriteOn : frameSpriteOff);
                }
            }

            // 아이콘 설정
            VisualElement icon = costumeSlot.Q<VisualElement>("IconSprite");
            if (icon != null)
            {
                // 아이콘 텍스처 설정
                Texture2D iconTexture = isOwned ?
                    costume.IconTexture :
                    (costume.IconXTexture != null ? costume.IconXTexture : costume.IconTexture);

                if (iconTexture != null)
                {
                    icon.style.backgroundImage = new StyleBackground(iconTexture);
                    icon.style.unityBackgroundImageTintColor = isOwned ? costume.IconColor : Color.black;
                    icon.style.opacity = isOwned ? 1f : 0.7f;
                }
            }

            // 이름 설정
            Label nameLabel = costumeSlot.Q<Label>("CostumeName");
            if (nameLabel != null)
            {
                nameLabel.text = costume.Name;

                // 착용 중이면 이름 색상 변경
                if (isOwned && isEquipped)
                {
                    nameLabel.style.color = new Color(1f, 0.6f, 0.2f); // 주황색
                }
            }

            // 클릭 이벤트 등록
            CostumeItem costumeCopy = costume;
            costumeSlot.RegisterCallback<ClickEvent>(evt =>
            {
                ShowCostumeInfo(costumeCopy, costumeSlot);
            });

            // 현재 컨테이너에 슬롯 추가
            currentContainer.Add(costumeSlot);
        }
    }

    // 주기적으로 업데이트
    public void UpdateCostumeUI()
    {
        ScrollView costumeScrollView = root.Q<ScrollView>("CostumeScrollView");
        if (costumeScrollView != null)
        {
            RefreshCostumeSlots(costumeScrollView);
        }
    }

    private void ClearCostumeInfo()
    {
        _selectedCostume = null;

        _costumeInfoName.text = "";
        _costumeInfoDescription.text = "";
        _costumeInfoIcon.style.backgroundImage = null;
        _costumeInfoEquipButton.style.unityBackgroundImageTintColor = _costumeBtnOff;
        _costumeInfoEquipButton.text = "";
    }

    // 코스튬 정보 표시 메서드 (클릭 시 호출)
    private void ShowCostumeInfo(CostumeItem costume, VisualElement slotElement = null)
    {
        if (costume == null) return;

        if (_currentSelectedSlot != null)
        {
            UpdateSlotSelectionStyle(_currentSelectedSlot, false);
        }

        // 새로 선택된 슬롯의 테두리 스타일 설정
        if (slotElement != null)
        {
            UpdateSlotSelectionStyle(slotElement, true);
            _currentSelectedSlot = slotElement; // 현재 선택된 슬롯 참조 저장
        }

        _selectedCostume = costume;

        // 코스튬 정보 패널 업데이트
        UpdateCostumeInfoPanel(costume);
    }

    private void UpdateSlotSelectionStyle(VisualElement slotElement, bool isSelected)
    {
        if (slotElement == null) return;
        VisualElement iconFrame = slotElement.Q<VisualElement>("IconFrame");

        // 테두리 설정
        iconFrame.style.borderTopWidth = isSelected ? 6 : 0;
        iconFrame.style.borderRightWidth = isSelected ? 6 : 0;
        iconFrame.style.borderBottomWidth = isSelected ? 6 : 0;
        iconFrame.style.borderLeftWidth = isSelected ? 6 : 0;

    }

    // 코스튬 정보 패널 업데이트 메서드
    private void UpdateCostumeInfoPanel(CostumeItem costume)
    {
        if (costume == null || _costumeInfoPanel == null) return;

        // 이름 설정
        if (_costumeInfoName != null)
            _costumeInfoName.text = costume.Name;

        // 코스튬 아이콘 설정
        if (_costumeInfoIcon != null && costume.IconTexture != null)
        {
            _costumeInfoIcon.style.backgroundImage = new StyleBackground(costume.IconTexture);
            bool isOwned = CostumeManager.Instance.IsOwned(_selectedCostume.Uid);
            _costumeInfoIcon.style.unityBackgroundImageTintColor = isOwned ? costume.IconColor : Color.black;
            _costumeInfoIcon.style.opacity = isOwned ? 1f : 0.7f;
        }

        // 효과/설명 설정
        if (_costumeInfoDescription != null)
        {
            // 설명이 15글자 이상이면 자르고 ... 추가 // 예외처리
            string truncatedDescription = costume.Description;
            if (truncatedDescription.Length > 15)
            {
                truncatedDescription = truncatedDescription.Substring(0, 15) + "...";
            }

            // 코스튬 설명 설정 (타입, 효과 등)
            _costumeInfoDescription.text = $"{costume.CostumeType} 타입 코스튬\n{truncatedDescription}";
        }

        // 패널 표시
        _costumeInfoPanel.style.display = DisplayStyle.Flex;

        // 장착 버튼 텍스트 업데이트
        UpdateEquipButtonText();
    }

    // 장착 버튼 텍스트 업데이트
    private void UpdateEquipButtonText()
    {
        if (_selectedCostume == null || _costumeInfoEquipButton == null) return;

        CostumeManager costumeManager = CostumeManager.Instance;
        bool isOwned = costumeManager.IsOwned(_selectedCostume.Uid);
        if (isOwned)
        {
            bool isEquipped = costumeManager.IsEquipped(_selectedCostume.Uid);

            _costumeInfoEquipButton.style.unityBackgroundImageTintColor = isEquipped ? _costumeBtnEquip : _costumeBtnOn;
            _costumeInfoEquipButton.text = isEquipped ? "해제하기" : "착용하기";
        }
        else
        {
            _costumeInfoEquipButton.style.unityBackgroundImageTintColor = _costumeBtnOff;
            _costumeInfoEquipButton.text = "갖고싶다너";
        }

    }


    // 코스튬 장착 기능 
    private void EquipCostume(CostumeItem costume)
    {
        if (costume == null) return;

        CostumeManager costumeManager = CostumeManager.Instance;
        if (costumeManager == null) return;

        string uid = costume.Uid;
        if (!costumeManager.IsOwned(uid)) return;

        bool isCurrentlyEquipped = costumeManager.IsEquipped(uid);

        // 이미 장착 중이면 해제, 아니면 장착
        if (isCurrentlyEquipped)
        {
            // 장착된 코스튬 해제
            costumeManager.UnequipCostume(uid);
        }
        else
        {
            // 특정 부위에만 장착 (CostumePart로 구분)
            costumeManager.EquipPartCostume(uid, costume.CostumeType);
        }

        // 변경 사항 저장 
        //costumeManager.SaveCostumeChanges();
    }

    #region 코스튬 필터링
    /// <summary>
    /// 코스튬 필터 타입
    /// </summary>
    public enum CostumeFilterType
    {
        All,     // 전체
        Top,     // 상의
        Bottom,  // 하의 (하의+신발)
        Face,    // 외모
        Etc      // 기타
    }

    /// <summary>
    /// 코스튬 필터 버튼 초기화
    /// </summary>
    private void InitCostumeFilterButtons()
    {
        // 필터 버튼 참조 가져오기 (UI의 버튼 이름에 맞게 수정)
        _filterAllButton = root.Q<Button>("AllButton");      // 전체
        _filterTopButton = root.Q<Button>("TopButton");      // 상의
        _filterBottomButton = root.Q<Button>("BottomButton"); // 하의
        _filterFaceButton = root.Q<Button>("FaceButton");    // 외모
        _filterEtcButton = root.Q<Button>("EtcButton");      // 기타

        // 버튼 클릭 이벤트 등록
        if (_filterAllButton != null)
        {
            _filterAllButton.RegisterCallback<ClickEvent>(evt => SetCostumeFilter(CostumeFilterType.All));
            UpdateFilterButtonStyle(_filterAllButton, true); // 초기 선택
        }

        if (_filterTopButton != null)
        {
            _filterTopButton.RegisterCallback<ClickEvent>(evt => SetCostumeFilter(CostumeFilterType.Top));
            UpdateFilterButtonStyle(_filterTopButton, false);
        }

        if (_filterBottomButton != null)
        {
            _filterBottomButton.RegisterCallback<ClickEvent>(evt => SetCostumeFilter(CostumeFilterType.Bottom));
            UpdateFilterButtonStyle(_filterBottomButton, false);
        }

        if (_filterFaceButton != null)
        {
            _filterFaceButton.RegisterCallback<ClickEvent>(evt => SetCostumeFilter(CostumeFilterType.Face));
            UpdateFilterButtonStyle(_filterFaceButton, false);
        }

        if (_filterEtcButton != null)
        {
            _filterEtcButton.RegisterCallback<ClickEvent>(evt => SetCostumeFilter(CostumeFilterType.Etc));
            UpdateFilterButtonStyle(_filterEtcButton, false);
        }
    }

    /// <summary>
    /// 코스튬 필터 설정
    /// </summary>
    private void SetCostumeFilter(CostumeFilterType filterType)
    {
        // 이미 같은 필터가 선택되어 있으면 무시
        if (_currentFilterType == filterType) return;

        // 이전 필터 버튼 스타일 초기화
        UpdateAllFilterButtonStyles(false);

        // 새 필터 타입 설정
        _currentFilterType = filterType;

        // 현재 선택된 버튼 스타일 업데이트
        Button selectedButton = null;

        switch (filterType)
        {
            case CostumeFilterType.All:
                selectedButton = _filterAllButton;
                break;
            case CostumeFilterType.Top:
                selectedButton = _filterTopButton;
                break;
            case CostumeFilterType.Bottom:
                selectedButton = _filterBottomButton;
                break;
            case CostumeFilterType.Face:
                selectedButton = _filterFaceButton;
                break;
            case CostumeFilterType.Etc:
                selectedButton = _filterEtcButton;
                break;
        }

        if (selectedButton != null)
        {
            UpdateFilterButtonStyle(selectedButton, true);
        }

        // 코스튬 목록 새로고침
        ScrollView costumeScrollView = root.Q<ScrollView>("CostumeScrollView");
        if (costumeScrollView != null)
        {
            RefreshCostumeSlots(costumeScrollView);
        }
    }

    /// <summary>
    /// 모든 필터 버튼 스타일 업데이트
    /// </summary>
    /// <param name="isSelected">선택 여부</param>
    private void UpdateAllFilterButtonStyles(bool isSelected)
    {
        UpdateFilterButtonStyle(_filterAllButton, isSelected);
        UpdateFilterButtonStyle(_filterTopButton, isSelected);
        UpdateFilterButtonStyle(_filterBottomButton, isSelected);
        UpdateFilterButtonStyle(_filterFaceButton, isSelected);
        UpdateFilterButtonStyle(_filterEtcButton, isSelected);
    }

    /// <summary>
    /// 필터 버튼 스타일 업데이트
    /// </summary>
    private void UpdateFilterButtonStyle(Button button, bool isSelected)
    {
        if (button == null) return;

        button.style.unityBackgroundImageTintColor = isSelected ? _costumeBtnFilterOn : _costumeBtnFilterOff;
    }

    /// <summary>
    /// 필터링된 코스튬 목록 가져오기
    /// </summary>
    private IEnumerable<CostumeItem> GetFilteredCostumes(IEnumerable<CostumeItem> allCostumes)
    {
        if (allCostumes == null) return new List<CostumeItem>();

        switch (_currentFilterType)
        {
            case CostumeFilterType.All:
                // 모든 코스튬 표시
                return allCostumes;

            case CostumeFilterType.Top:
                // 상의 코스튬만 필터링
                return allCostumes.Where(costume => costume.CostumeType == CostumePart.Top);

            case CostumeFilterType.Bottom:
                // 하의와 신발 코스튬 필터링
                return allCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Bottom ||
                    costume.CostumeType == CostumePart.Shoes);

            case CostumeFilterType.Face:
                // 외모 관련 코스튬 필터링 (헤어, 얼굴, 헬멧 등)
                return allCostumes.Where(costume =>
                    costume.CostumeType == CostumePart.Hair ||
                    costume.CostumeType == CostumePart.Face ||
                    costume.CostumeType == CostumePart.Helmet);

            case CostumeFilterType.Etc:
                // 기타 코스튬 필터링 (위 카테고리에 속하지 않는 모든 것)
                return allCostumes.Where(costume =>
                    costume.CostumeType != CostumePart.Top &&
                    costume.CostumeType != CostumePart.Bottom &&
                    costume.CostumeType != CostumePart.Shoes &&
                    costume.CostumeType != CostumePart.Hair &&
                    costume.CostumeType != CostumePart.Face &&
                    costume.CostumeType != CostumePart.Helmet);

            default:
                return allCostumes;
        }
    }

    /// <summary>
    /// 필터링 이름으로 반환
    /// </summary>
    private string GetFilterTypeName()
    {
        switch (_currentFilterType)
        {
            case CostumeFilterType.All:
                return "전체";
            case CostumeFilterType.Top:
                return "상의";
            case CostumeFilterType.Bottom:
                return "하의";
            case CostumeFilterType.Face:
                return "외모";
            case CostumeFilterType.Etc:
                return "기타";
            default:
                return "코스튬";
        }
    }
    #endregion
    #endregion
}