using EnumCollection;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponUI : MonoBehaviour, IMenuUI
{
    public VisualElement root { get; private set; }

    private Button[] mainButtons;
    private Dictionary<Button, VisualElement> _connectMainPanelDict = new();

    private Button[] companionButtons;
    private Dictionary<Button, VisualElement> _connectCompanionPanelDict = new();

    private VisualElement _playerPanel;
    private VisualElement _companionPanel;

    [SerializeField] DraggableScrollView _playerScrollView;
    [SerializeField] DraggableScrollView _bowScrollView;
    [SerializeField] DraggableScrollView _shieldScrollView;
    [SerializeField] DraggableScrollView _staffScrollView;

    [SerializeField] private WeaponInfoUI _weaponInfoUI;
    [SerializeField] private WeaponBookUI _weaponBookUI;

    private Dictionary<string, int> _weaponCount;
    private Dictionary<string, int> _weaponLevel;

    [SerializeField] VisualTreeAsset weaponSlotAsset;
    [SerializeField] VisualTreeAsset rarityLineAsset;

    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);

    private readonly Dictionary<string, VisualElement> _slotDict = new();

    // =====================================================================
    // Awake
    // =====================================================================
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        var gameData = StartBroker.GetGameData();
        _weaponCount = gameData.weaponCount;
        _weaponLevel = gameData.weaponLevel;

        _weaponInfoUI.gameObject.SetActive(true);

        PlayerBroker.OnWeaponLevelSet += OnWeaponLevelSet;
        PlayerBroker.OnWeaponCountSet += OnWeaponCountSet;
    }

    // =====================================================================
    // Start
    // =====================================================================
    private void Start()
    {
        foreach (Rarity rarity in (Rarity[])Enum.GetValues(typeof(Rarity)))
        {
            CreateWeaponSlot(rarity);
        }

        root.Q<VisualElement>("MainParentPanel").Add(_weaponBookUI.root);
        _weaponBookUI.root.style.height = _weaponBookUI.root.ElementAt(0).style.height = Length.Percent(100);

        InitUI();
    }

    // =====================================================================
    // Main Panel Button 클릭 시 (★ 소리 조건 처리 완료 버전)
    // =====================================================================
    private void OnClickMainButton(int buttonIndex)
    {
        var selectedButton = mainButtons[buttonIndex];

        // 이미 활성화된 패널이면 아무 동작도 하지 않음 (소리 X)
        if (_connectMainPanelDict[selectedButton].style.display == DisplayStyle.Flex)
            return;

        // 다른 버튼일 때만 사운드 출력
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

        SwitchMainPanel(buttonIndex);
    }

    private void SwitchMainPanel(int buttonIndex)
    {
        for (int i = 0; i < mainButtons.Length; i++)
        {
            var currentMainButton = mainButtons[i];
            bool isActive = (i == buttonIndex);

            _connectMainPanelDict[currentMainButton].style.display =
                isActive ? DisplayStyle.Flex : DisplayStyle.None;

            currentMainButton.style.unityBackgroundImageTintColor =
                new Color(
                    isActive ? activeColor.r : inactiveColor.r,
                    isActive ? activeColor.g : inactiveColor.g,
                    isActive ? activeColor.b : inactiveColor.b,
                    isActive ? 0.1f : 0f
                );

            currentMainButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor =
                isActive ? activeColor : inactiveColor;

            currentMainButton.Q<Label>().style.color =
                isActive ? activeColor : inactiveColor;
        }
    }

    // =====================================================================
    // Companion Panel Button 클릭 시 (★ 소리 조건 처리 완료 버전)
    // =====================================================================
    private void OnClickCompanionButton(int buttonIndex)
    {
        var selectedButton = companionButtons[buttonIndex];

        // 이미 활성화된 패널이면 아무 동작도 하지 않음 (소리 X)
        if (_connectCompanionPanelDict[selectedButton].style.display == DisplayStyle.Flex)
            return;

        // 다른 버튼일 때만 사운드 출력
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

        SwitchCompanionPanel(buttonIndex);
    }

    private void SwitchCompanionPanel(int companionIndex)
    {
        for (int i = 0; i < companionButtons.Length; i++)
        {
            var btn = companionButtons[i];
            bool isActive = (i == companionIndex);

            _connectCompanionPanelDict[btn].style.display =
                isActive ? DisplayStyle.Flex : DisplayStyle.None;

            btn.style.unityBackgroundImageTintColor =
                new Color(
                    isActive ? activeColor.r : inactiveColor.r,
                    isActive ? activeColor.g : inactiveColor.g,
                    isActive ? activeColor.b : inactiveColor.b,
                    isActive ? 0.1f : 0f
                );

            btn.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor =
                isActive ? activeColor : inactiveColor;

            btn.Q<Label>().style.color =
                isActive ? activeColor : inactiveColor;
        }
    }

    // =====================================================================
    // UI Init
    // =====================================================================
    private void InitUI()
    {
        InitMainElement();
        InitCompanionElement();
    }

    private void InitCompanionElement()
    {
        VisualElement companionButtonParent = root.Q<VisualElement>("CompanionButtonParent");
        VisualElement companionScrollViewParent = root.Q<VisualElement>("CompanionScrollViewParent");

        int numOfButtons = companionButtonParent.childCount;
        companionButtons = new Button[numOfButtons];

        for (int i = 0; i < numOfButtons; i++)
        {
            int index = i;
            companionButtons[i] = (Button)companionButtonParent.ElementAt(i);
            companionButtons[i].RegisterCallback<ClickEvent>(evt => OnClickCompanionButton(index));
            _connectCompanionPanelDict.Add(companionButtons[i], companionScrollViewParent.ElementAt(i));
        }

        SwitchCompanionPanel(0);
    }

    private void InitMainElement()
    {
        VisualElement mainButtonPanel = root.Q<VisualElement>("MainButtonPanel");
        VisualElement parentPanel = root.Q<VisualElement>("MainParentPanel");

        int numOfButtons = mainButtonPanel.childCount;
        mainButtons = new Button[numOfButtons];

        for (int i = 0; i < numOfButtons; i++)
        {
            int index = i;
            mainButtons[i] = (Button)mainButtonPanel.ElementAt(i);
            mainButtons[i].RegisterCallback<ClickEvent>(evt => OnClickMainButton(index));
            _connectMainPanelDict.Add(mainButtons[i], parentPanel.ElementAt(i));
        }

        SwitchMainPanel(0);
    }

    // =====================================================================
    // Slot 생성
    // =====================================================================
    private void CreateWeaponSlot(Rarity rarity)
    {
        if (rarity != Rarity.Common)
        {
            SetRarityLine();
        }

        VisualElement playerContainer = GetContainer();
        VisualElement bowContainer = GetContainer();
        VisualElement shieldContainer = GetContainer();
        VisualElement staffContainer = GetContainer();

        List<WeaponData> dataList = WeaponManager.instance.GetWeaponDataByRarity(rarity);

        for (int index = 0; index < dataList.Count; index++)
        {
            WeaponData weaponData = dataList[index];

            switch (weaponData.WeaponType)
            {
                case WeaponType.Melee: playerContainer.Add(GetSlot(weaponData)); break;
                case WeaponType.Bow: bowContainer.Add(GetSlot(weaponData)); break;
                case WeaponType.Shield: shieldContainer.Add(GetSlot(weaponData)); break;
                case WeaponType.Staff: staffContainer.Add(GetSlot(weaponData)); break;
            }
        }

        if (rarity == Rarity.Mythic)
        {
            playerContainer.Add(GetPadding());
            bowContainer.Add(GetPadding());
            shieldContainer.Add(GetPadding());
            staffContainer.Add(GetPadding());
        }

        _playerScrollView.scrollView.Add(playerContainer);
        _bowScrollView.scrollView.Add(bowContainer);
        _shieldScrollView.scrollView.Add(shieldContainer);
        _staffScrollView.scrollView.Add(staffContainer);
    }

    private void SetRarityLine()
    {
        TemplateContainer playerLine = rarityLineAsset.CloneTree();
        TemplateContainer bowLine = rarityLineAsset.CloneTree();
        TemplateContainer shieldLine = rarityLineAsset.CloneTree();
        TemplateContainer staffLine = rarityLineAsset.CloneTree();

        _playerScrollView.scrollView.Add(playerLine);
        _bowScrollView.scrollView.Add(bowLine);
        _shieldScrollView.scrollView.Add(shieldLine);
        _staffScrollView.scrollView.Add(staffLine);
    }

    private VisualElement GetPadding()
    {
        VisualElement padding = new();
        padding.style.width = Length.Percent(100);
        padding.style.height = 30f;
        return padding;
    }

    private VisualElement GetContainer()
    {
        VisualElement container = new();
        container.style.width = Length.Percent(110);
        container.style.height = Length.Auto();
        container.style.flexDirection = FlexDirection.Row;
        container.style.flexWrap = Wrap.Wrap;
        return container;
    }

    private VisualElement GetSlot(WeaponData weaponData)
    {
        string weaponId = weaponData.UID;
        int count = _weaponCount.ContainsKey(weaponId) ? _weaponCount[weaponId] : 0;

        TemplateContainer weaponSlot = weaponSlotAsset.CloneTree();
        _slotDict.Add(weaponId, weaponSlot);

        VisualElement weaponIcon = weaponSlot.Q<VisualElement>("WeaponIcon");
        VisualElement weaponBackground = weaponSlot.Q<VisualElement>("BackgroundPanel");

        int level = _weaponLevel.ContainsKey(weaponId) ? _weaponLevel[weaponId] : 0;
        SlotSet(weaponData, level, count);

        WeaponManager.instance.SetWeaponIconToVe(weaponData, weaponIcon);

        switch (weaponData.WeaponRarity)
        {
            case Rarity.Common: weaponBackground.style.unityBackgroundImageTintColor = Color.gray; break;
            case Rarity.Uncommon: weaponBackground.style.unityBackgroundImageTintColor = new Color(0.5f, 0.75f, 1f); break;
            case Rarity.Rare: weaponBackground.style.unityBackgroundImageTintColor = Color.magenta; break;
            case Rarity.Unique: weaponBackground.style.unityBackgroundImageTintColor = Color.green; break;
            case Rarity.Legendary: weaponBackground.style.unityBackgroundImageTintColor = Color.yellow; break;
            case Rarity.Mythic: weaponBackground.style.unityBackgroundImageTintColor = new Color(0f, 0f, 0.5f); break;
            case Rarity.Ancient: weaponBackground.style.unityBackgroundImageTintColor = Color.red; break;
            default: weaponBackground.style.unityBackgroundImageTintColor = Color.white; break;
        }

        weaponSlot.RegisterCallback<ClickEvent>(evt => OnClickSlot(weaponData));
        return weaponSlot;
    }

    // =====================================================================
    // Slot Click
    // =====================================================================
    private void OnClickSlot(WeaponData weaponData)
    {
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

        DraggableScrollView currentScrollView = null;
        switch (weaponData.WeaponType)
        {
            case WeaponType.Melee: currentScrollView = _playerScrollView; break;
            case WeaponType.Bow: currentScrollView = _bowScrollView; break;
            case WeaponType.Shield: currentScrollView = _shieldScrollView; break;
            case WeaponType.Staff: currentScrollView = _staffScrollView; break;
        }

        if (currentScrollView._isDragging)
            return;

        _weaponInfoUI.ShowWeaponInfo(weaponData);
    }

    // =====================================================================
    // Slot 업데이트
    // =====================================================================
    private void OnWeaponLevelSet(string weaponId, int level)
    {
        WeaponData weaponData = WeaponManager.instance.weaponDict[weaponId];
        int count = _weaponCount.TryGetValue(weaponData.UID, out var c) ? c : 0;
        SlotSet(weaponData, level, count);
    }

    private void OnWeaponCountSet(string weaponId, int count)
    {
        WeaponData weaponData = WeaponManager.instance.weaponDict[weaponId];
        int level = _weaponLevel.TryGetValue(weaponId, out var l) ? l : 0;
        SlotSet(weaponData, level, count);
    }

    private void SlotSet(WeaponData weaponData, int level, int count)
    {
        VisualElement slot = _slotDict[weaponData.UID];

        Label levelLabel = slot.Q<Label>("LevelLabel");
        levelLabel.text = level == 0 ? string.Empty : $"+{level}";

        ProgressBar countProgressBar = slot.Q<ProgressBar>("CountProgressBar");

        if (level == CurrencyManager.MAXWEAPONLEVEL)
        {
            countProgressBar.style.letterSpacing = 15f;
            countProgressBar.title = $"{count}/Max";
            countProgressBar.value = 1;
        }
        else
        {
            countProgressBar.style.letterSpacing = 42f;
            int price = CurrencyManager.instance.GetRequireWeaponCount(weaponData.WeaponRarity, level);
            countProgressBar.title = $"{count}/{price}";
            countProgressBar.value = count / (float)price;
        }
    }

    // =====================================================================
    // IMenuUI
    // =====================================================================
    void IMenuUI.ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
    }

    void IMenuUI.InactiveUI()
    {
        root.style.display = DisplayStyle.None;
    }
}
