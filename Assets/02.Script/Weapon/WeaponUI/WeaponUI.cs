using EnumCollection;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponUI : MonoBehaviour, IBattleUI
{
    //VIsualElement
    public VisualElement root { get; private set; }
    //메인 버튼
    private Button[] mainButtons;//플레이어 무기, 동료 무기, 무기 도감
    private Dictionary<Button, VisualElement> _connectMainPanelDict = new();//해당 버튼에 연결된 VisualElement
    //동료 무기 버튼
    private Button[] companionButtons;//플레이어 무기, 동료 무기, 무기 도감
    private Dictionary<Button, VisualElement> _connectCompanionPanelDict = new();//해당 버튼에 연결된 VisualElement
    //패널
    private VisualElement _playerPanel;
    private VisualElement _companionPanel;
    //스크롤뷰
    [SerializeField] DraggableScrollView _playerScrollView; 
    [SerializeField] DraggableScrollView _bowScrollView;
    [SerializeField] DraggableScrollView _shieldScrollView;
    [SerializeField] DraggableScrollView _staffScrollView;
    //Other UI
    [SerializeField] private WeaponInfoUI _weaponInfoUI;
    [SerializeField] private WeaponBookUI _weaponBookUI;
    //Data
    private Dictionary<string, int> _weaponCount;
    private Dictionary<string, int> _weaponLevel;
    //VisualTreeAsset
    [SerializeField] VisualTreeAsset weaponSlotAsset;//무기 슬롯 에셋
    [SerializeField] VisualTreeAsset rarityLineAsset;//Rarity에 따라 자리를 구분지을 실선
    //ButtonColor
    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);
    //uid에 대한 Slot
    private readonly Dictionary<string, VisualElement> _slotDict = new();
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
    private void Start()
    {
        foreach (Rarity rarity in (Rarity[])Enum.GetValues(typeof(Rarity)))
        {
            CreateWeaponSlot(rarity);
        }
        root.Q<VisualElement>("MainParentPanel").Add(_weaponBookUI.root);//_weaponBookUI.root가 할당된 이후 <- Start
        _weaponBookUI.root.style.height = _weaponBookUI.root.ElementAt(0).style.height = Length.Percent(100);
        InitUI();
        
    }
    private void SwitchMainPanel(int buttonIndex)
    {
        for (int i = 0; i < mainButtons.Length; i++)
        {
            var currentMainButton = mainButtons[i];
            if (i == buttonIndex)
            {
                _connectMainPanelDict[currentMainButton].style.display = DisplayStyle.Flex;
                currentMainButton.style.unityBackgroundImageTintColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.1f);
                currentMainButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;
                currentMainButton.Q<Label>().style.color = activeColor;
            }
            else
            {
                _connectMainPanelDict[currentMainButton].style.display = DisplayStyle.None;
                currentMainButton.style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
                currentMainButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
                currentMainButton.Q<Label>().style.color = inactiveColor;
            }
        }
    }
    private void SwitchCompanionPanel(int buttonIndex)
    {
        for (int i = 0; i < companionButtons.Length; i++)
        {
            var currentCompanionButton = companionButtons[i];
            if (i == buttonIndex)
            {
                _connectCompanionPanelDict[currentCompanionButton].style.display = DisplayStyle.Flex;
                currentCompanionButton.style.unityBackgroundImageTintColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.1f);
                currentCompanionButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;
                currentCompanionButton.Q<Label>().style.color = activeColor;
            }
            else
            {
                _connectCompanionPanelDict[currentCompanionButton].style.display = DisplayStyle.None;
                currentCompanionButton.style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
                currentCompanionButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
                currentCompanionButton.Q<Label>().style.color = inactiveColor;
            }
        }
    }
    #region Init
    private void InitUI()
    {
        InitMainElement();
        InitCompanionElement();
    }

    private void InitCompanionElement()
    {
        VisualElement companionButtonParent = root.Q<VisualElement>("CompanionButtonParent");//동료 버튼 부모
        VisualElement companionScrollViewParent = root.Q<VisualElement>("CompanionScrollViewParent");//버튼에 의해 나타날 VisualElement들의 부모
        int numOfButtons = companionButtonParent.childCount;
        companionButtons = new Button[numOfButtons];
        for (int i = 0; i < numOfButtons; i++)
        {
            int index = i;//스코프를 벗어난 i를 로컬 변수화
            companionButtons[index] = (Button)companionButtonParent.ElementAt(i);
            companionButtons[index].RegisterCallback<ClickEvent>(evt => SwitchCompanionPanel(index));
            _connectCompanionPanelDict.Add(companionButtons[index], companionScrollViewParent.ElementAt(i));
        }
        _connectCompanionPanelDict[companionButtons[0]].style.display = DisplayStyle.Flex;
        _connectCompanionPanelDict[companionButtons[1]].style.display = DisplayStyle.None;
        _connectCompanionPanelDict[companionButtons[2]].style.display = DisplayStyle.None;
        SwitchCompanionPanel(0);
    }

    private void InitMainElement()
    {
        VisualElement mainButtonPanel = root.Q<VisualElement>("MainButtonPanel");//버튼 부모
        VisualElement parentPanel = root.Q<VisualElement>("MainParentPanel");//버튼에 의해 나타날 VisualElement들의 부모
        int numOfButtons = mainButtonPanel.childCount;
        mainButtons = new Button[numOfButtons];
        for (int i = 0; i < numOfButtons; i++)
        {
            int index = i;//스코프를 벗어난 i를 로컬 변수화
            mainButtons[index] = (Button)mainButtonPanel.ElementAt(i);
            mainButtons[index].RegisterCallback<ClickEvent>(evt => SwitchMainPanel(index));
            _connectMainPanelDict.Add(mainButtons[index], parentPanel.ElementAt(i));
        }
        _connectMainPanelDict[mainButtons[0]].style.display = DisplayStyle.Flex;
        _connectMainPanelDict[mainButtons[1]].style.display = DisplayStyle.None;
        _connectMainPanelDict[mainButtons[2]].style.display = DisplayStyle.None;
        SwitchMainPanel(0);
    }

    private void CreateWeaponSlot(Rarity rarity)
    {
        if (rarity != Rarity.Common)
        {
            SetRarityLine();
        }
        //같은 등급의 무기들을 넣어놓을 VisualElement
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
                case WeaponType.Melee:
                    playerContainer.Add(GetSlot(weaponData));
                    break;
                case WeaponType.Bow:
                    bowContainer.Add(GetSlot(weaponData));
                    break;
                case WeaponType.Shield:
                    shieldContainer.Add(GetSlot(weaponData));
                    break;
                case WeaponType.Staff:
                    staffContainer.Add(GetSlot(weaponData));
                    break;
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
    void SetRarityLine()
    {
        //Rarity가 다른 무기들을 구분하는 선을 만듦
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
        VisualElement playerSlotContainer = new();
        playerSlotContainer.style.width = Length.Percent(110);//여유있는 줄바꿈을 위해
        playerSlotContainer.style.height = Length.Auto();
        playerSlotContainer.style.flexDirection = FlexDirection.Row;
        playerSlotContainer.style.flexWrap = Wrap.Wrap;
        return playerSlotContainer;
    }
    private VisualElement GetSlot(WeaponData weaponData)
    {
        string weaponId = weaponData.UID;
        int count = _weaponCount.ContainsKey(weaponId) ? _weaponCount[weaponId] : 0;

        TemplateContainer weaponSlot = weaponSlotAsset.CloneTree();//슬롯 생성
        _slotDict.Add(weaponId, weaponSlot);//Dictionary에 저장 - 개수와 레벨 변경에 대응하기 위함
        VisualElement weaponIcon = weaponSlot.Q<VisualElement>("WeaponIcon");
        VisualElement weaponBackground = weaponSlot.Q<VisualElement>("WeaponBackground");
        int level = _weaponLevel.ContainsKey(weaponId) ? _weaponLevel[weaponId] : 0;
        SlotSet(weaponData, level, count);
        //Icon
        weaponIcon.style.backgroundImage = new StyleBackground(weaponData.WeaponSprite.texture);
        WeaponManager.instance.SetIconScale(weaponData, weaponIcon);
        if (count > 0)
        {
            switch (weaponData.WeaponRarity)
            {
                case Rarity.Common:
                    weaponBackground.style.backgroundColor = new StyleColor(Color.gray);
                    break;
                case Rarity.Uncommon:
                    weaponBackground.style.backgroundColor = new StyleColor(new Color(0.5f, 0.75f, 1f));
                    break;
                case Rarity.Rare:
                    weaponBackground.style.backgroundColor = new StyleColor(Color.magenta);
                    break;
                case Rarity.Unique:
                    weaponBackground.style.backgroundColor = new StyleColor(Color.green);
                    break;
                case Rarity.Legendary:
                    weaponBackground.style.backgroundColor = new StyleColor(Color.yellow);
                    break;
                case Rarity.Mythic:
                    weaponBackground.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0.5f));
                    break;
                default:
                    weaponBackground.style.backgroundColor = new StyleColor(Color.white);
                    break;
            }
        }
        else
        {
            //weaponIcon.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
        }

        weaponSlot.RegisterCallback<ClickEvent>(evt => OnClickSlot(weaponData));

        return weaponSlot;
    }
    private void OnClickSlot(WeaponData weaponData)
    {
        bool isPlayerWeapon = weaponData.WeaponType == WeaponType.Melee;
        DraggableScrollView currentScrollView = null;
        switch (weaponData.WeaponType)
        {
            case WeaponType.Melee:
                currentScrollView = _playerScrollView;
                break;
            case WeaponType.Bow:
                currentScrollView = _bowScrollView;
                break;
            case WeaponType.Shield:
                currentScrollView = _shieldScrollView;
                break;
            case WeaponType.Staff:
                currentScrollView = _staffScrollView;
                break;
        }
        if (currentScrollView._isDragging)
            return;
        _weaponInfoUI.ShowWeaponInfo(weaponData);
    }

    #region UIChange
    private void OnEnable()
    {
        UIBroker.OnMenuUIChange += HandleUIChange;
    }

    private void OnDisable()
    {
        UIBroker.OnMenuUIChange -= HandleUIChange;
    }
    private void HandleUIChange(int uiType)
    {
        if (uiType == 1)
            ShowWeaponUI();
        else
            HideWeaponUI();
    }
    public void HideWeaponUI()
    {
        root.style.display = DisplayStyle.None;
    }
    public void ShowWeaponUI()
    {
        root.style.display = DisplayStyle.Flex;
        //킬때마다 무기개수 초기화
    }
    #endregion
    #endregion
    private void OnWeaponLevelSet(string weaponId, int level)
    {
        WeaponData weaponData = WeaponManager.instance.weaponDict[weaponId];
        if (!_weaponCount.TryGetValue(weaponData.UID, out int count))
        {
            count = 0;
        }
        SlotSet(weaponData, level, count);
    }
    private void OnWeaponCountSet(string weaponId, int count)
    {
        WeaponData weaponData = WeaponManager.instance.weaponDict[weaponId];
        if (!_weaponLevel.TryGetValue(weaponId, out int level))
        {
            level = 0;
        }
        SlotSet(weaponData, level, count);
    }
    private void SlotSet(WeaponData weaponData, int level, int count)
    {
        VisualElement slot = _slotDict[weaponData.UID];
        Label levelLabel = slot.Q<Label>("LevelLabel");
        if (level == 0)
        {
            levelLabel.text = string.Empty; 
        }
        else
        {
            levelLabel.text = $"+{level}";
        }
        

        ProgressBar countProgressBar = slot.Q<ProgressBar>("CountProgressBar");

        if (level == PriceManager.MAXWEAPONLEVEL)
        {
            countProgressBar.style.letterSpacing = 15f;
            countProgressBar.title = $"{count}/Max";
            countProgressBar.value = 1;
        }
        else
        {
            countProgressBar.style.letterSpacing = 42f;
            int price = PriceManager.instance.GetRequireWeaponCount(weaponData.WeaponRarity, level);
            countProgressBar.title = $"{count}/{price}";
            countProgressBar.value = count / (float)price;
        }
    }



    public void ActivateBattleMode()
    {
        
    }

    public void DeactivateBattleMode()
    {
        
    }
}