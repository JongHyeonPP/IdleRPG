using EnumCollection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponUI : MonoBehaviour, IBattleUI
{
    //VIsualElement
    public VisualElement root { get; private set; }
    private Button[] buttons;//�÷��̾� ����, ���� ����, ���� ����
    private Button currentButton;//���õ��ִ� ��ư
    private Dictionary<Button, VisualElement> _connectDict;//�ش� ��ư�� ����� VisualElement
    [SerializeField] DraggableScrollView _playerScrollView;
    [SerializeField] DraggableScrollView _companionScrollView;
    //Other UI
    [SerializeField] private WeaponInfoUI _weaponInfoUI;
    [SerializeField] private WeaponBookUI _weaponBookUI;
    //Data
    private Dictionary<string, int> _weaponCount;
    private Dictionary<string, int> _weaponLevel;
    //VisualTreeAsset
    [SerializeField] VisualTreeAsset weaponSlotAsset;//���� ���� ����
    [SerializeField] VisualTreeAsset rarityLineAsset;//Rarity�� ���� �ڸ��� �������� �Ǽ�
    //ButtonColor
    private readonly Color inactiveColor = new(0f, 0.36f, 0.51f);
    private readonly Color activeColor = new(0.04f, 0.24f, 0.32f);
    //uid�� ���� Slot
    private Dictionary<string, VisualElement> _slotDict = new();
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        
        var gameData = StartBroker.GetGameData();
        _weaponCount = gameData.weaponCount;
        _weaponLevel = gameData.weaponLevel;
        _weaponInfoUI.gameObject.SetActive(true);
        foreach (Rarity rarity in (Rarity[])Enum.GetValues(typeof(Rarity)))
        {
            CreateWeaponSlot(rarity);
        }
        PlayerBroker.OnWeaponLevelSet += OnWeaponLevelSet;
        PlayerBroker.OnWeaponCountSet += OnWeaponCountSet;
    }
    private void Start()
    {
        root.Q<VisualElement>("ParentPanel").Add(_weaponBookUI.root);//_weaponBookUI.root�� �Ҵ�� ���� <- Start
        _weaponBookUI.root.style.height = _weaponBookUI.root.ElementAt(0).style.height = Length.Percent(100);
        InitUI();
        SwitchScrollView(0);
    }
    private void SwitchScrollView(int buttonIndex)
    {
        if (currentButton != null)
        {
            currentButton.style.backgroundColor = inactiveColor;
            _connectDict[currentButton].style.display = DisplayStyle.None;
        }
        currentButton = buttons[buttonIndex];
        currentButton.style.backgroundColor = activeColor;
        _connectDict[currentButton].style.display = DisplayStyle.Flex;
    }
    #region Init
    private void InitUI()
    {
        
        VisualElement buttonPanel = root.Q<VisualElement>("ButtonPanel");//��ư �θ�
        VisualElement parentPanel = root.Q<VisualElement>("ParentPanel");//��ư�� ���� ��Ÿ�� VisualElement���� �θ�
        int numOfButtons = buttonPanel.childCount;
        buttons = new Button[numOfButtons];
        _connectDict = new();
        for (int i = 0; i < numOfButtons; i++)
        {
            int index = i;//�������� ��� i�� ���� ����ȭ
            buttons[index] = (Button)buttonPanel.ElementAt(i);
            buttons[index].RegisterCallback<ClickEvent>(evt => SwitchScrollView(index));
            _connectDict.Add(buttons[index], parentPanel.ElementAt(i));
        }
        buttons[0] = root.Q<Button>("PlayerButton");
        buttons[1] = root.Q<Button>("CompanionButton");

        buttons[0].text = "���ΰ�";
        buttons[1].text = "����";
        _connectDict[buttons[0]].style.display = DisplayStyle.Flex;
        _connectDict[buttons[1]].style.display = DisplayStyle.None;
        _connectDict[buttons[2]].style.display = DisplayStyle.None;

    }
    private void CreateWeaponSlot(Rarity rarity)
    {
        if (rarity != Rarity.Common)
        {
            TemplateContainer playerLine = rarityLineAsset.CloneTree();
            _playerScrollView.scrollView.Add(playerLine);
            TemplateContainer companionLine = rarityLineAsset.CloneTree();
            _companionScrollView.scrollView.Add(companionLine);
        }
        VisualElement playerContainer = GetContainer();
        VisualElement companionContainer = GetContainer();
        List<WeaponData> dataList = WeaponManager.instance.GetWeaponDataByRarity(rarity);
        for (int index = 0; index < dataList.Count; index++)
        {
            WeaponData weaponData = dataList[index];
            if (weaponData.WeaponType == WeaponType.Melee)
            {
                playerContainer.Add(GetSlot(weaponData));
            }
            else
            {
                companionContainer.Add(GetSlot(weaponData));
            }
        }
        _playerScrollView.scrollView.Add(playerContainer);
        _companionScrollView.scrollView.Add(companionContainer);
    }

    private VisualElement GetContainer()
    {
        VisualElement playerSlotContainer = new();
        playerSlotContainer.style.width = Length.Percent(100);
        playerSlotContainer.style.height = Length.Auto();
        playerSlotContainer.style.flexDirection = FlexDirection.Row;
        playerSlotContainer.style.flexWrap = Wrap.Wrap;
        return playerSlotContainer;
    }

    private VisualElement GetSlot( WeaponData weaponData)
    {
        string weaponId = weaponData.UID;
        int weaponCount = _weaponCount.ContainsKey(weaponId) ? _weaponCount[weaponId] : 0;

        TemplateContainer weaponSlot = weaponSlotAsset.CloneTree();//���� ����
        _slotDict.Add(weaponId, weaponSlot);//Dictionary�� ���� - ������ ���� ���濡 �����ϱ� ����
        VisualElement weaponIcon = weaponSlot.Q<VisualElement>("WeaponIcon");
        VisualElement weaponBackground = weaponSlot.Q<VisualElement>("WeaponBackground");
        //LevelLabel
        int weaponLevel = _weaponLevel.ContainsKey(weaponId) ? _weaponLevel[weaponId] : 0;
        Label levelLabel = weaponSlot.Q<Label>("LevelLabel");
        levelLabel.text = weaponLevel > 0 ? $"+{weaponLevel}" : string.Empty;
        //CountProgressBar
        ProgressBar countProgressBar = weaponSlot.Q<ProgressBar>();
        if (weaponLevel == PriceManager.MAXWEAPONLEVEL)
        {
            countProgressBar.title = "Max Level";
            countProgressBar.value = 1;
        }
        else
        {
            int price = PriceManager.instance.GetRequireWeaponCount(weaponData.WeaponRarity, weaponLevel);
            countProgressBar.title = $"{weaponCount}/{price}";
            countProgressBar.value = weaponCount / (float)price;
        }
        //Icon
        weaponIcon.style.backgroundImage = new StyleBackground(weaponData.WeaponSprite.texture);
        WeaponManager.instance.SetIconScale(weaponData, weaponIcon);
        if (weaponCount > 0)
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
            weaponIcon.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
        }

        weaponSlot.RegisterCallback<ClickEvent>(evt => OnClickSlot(weaponData));

        return weaponSlot;
    }

    private void OnClickSlot(WeaponData weaponData)
    {
        bool isPlayerWeapon = weaponData.WeaponType == WeaponType.Melee;
        DraggableScrollView currentScrollView = isPlayerWeapon?_playerScrollView:_companionScrollView;
        if (currentScrollView._isDragging)
            return;
        _weaponInfoUI.ShowWeaponInfo(weaponData);
    }

    #region UIChange
    private void OnEnable()
    {
        BattleBroker.OnMenuUIChange += HandleUIChange;
    }

    private void OnDisable()
    {
        BattleBroker.OnMenuUIChange -= HandleUIChange;
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
        //ų������ ���ⰳ�� �ʱ�ȭ
    }
    #endregion
    #endregion
    private void OnWeaponLevelSet(string weaponUid, int level)
    {
        WeaponData weaponData = WeaponManager.instance.weaponDict[weaponUid];
        VisualElement slot = _slotDict[weaponData.UID];
        Label levelLabel = slot.Q<Label>("LevelLabel");
        levelLabel.text = $"+{level}";

        ProgressBar countProgressBar = slot.Q<ProgressBar>("CountProgressBar");
        if (!_weaponCount.TryGetValue(weaponData.UID, out int count))
        {
            count = 0;
        }
        if (level == PriceManager.MAXWEAPONLEVEL)
        {
            countProgressBar.title = "Max Level";
            countProgressBar.value = 1;
        }
        else
        {
            int price = PriceManager.instance.GetRequireWeaponCount(weaponData.WeaponRarity, level);
            countProgressBar.title = $"{count}/{price}";
            countProgressBar.value = count / (float)price;
        }
        
    }
    private void OnWeaponCountSet(object weaponDataObj, int count)
    {
        WeaponData weaponData = (WeaponData)weaponDataObj;
        VisualElement slot = _slotDict[weaponData.UID];
        ProgressBar countProgressBar = slot.Q<ProgressBar>("CountProgressBar");
        int level = _weaponLevel[weaponData.UID];
        int price = PriceManager.instance.GetRequireWeaponCount(weaponData.WeaponRarity, level);
        countProgressBar.title = $"{count}/{price}";
        countProgressBar.value = count/(float)price;
    }

    public void ActivateBattleMode()
    {
        
    }

    public void DeactivateBattleMode()
    {
        
    }
}
