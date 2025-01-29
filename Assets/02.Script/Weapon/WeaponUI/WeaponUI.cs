using EnumCollection;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponUI : MonoBehaviour
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
            CreateWeaponSlot(true, rarity);
            CreateWeaponSlot(false, rarity);
        }

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
    private void CreateWeaponSlot(bool isPlayerWeapon, Rarity rarity)
    {
        ScrollView currentScrollView = (isPlayerWeapon ? _playerScrollView : _companionScrollView).scrollView;
        if (rarity != Rarity.Common)
        {
            TemplateContainer line = rarityLineAsset.CloneTree();
            currentScrollView.Add(line);
        }
        string name = isPlayerWeapon ? "Player" : "Companion";
        VisualElement slotContainer = new()
        {
            name = $"{name} - {rarity}"
        };
        slotContainer.style.width = Length.Percent(100);
        slotContainer.style.height = Length.Auto();
        slotContainer.style.flexDirection = FlexDirection.Row;
        slotContainer.style.flexWrap = Wrap.Wrap;
        List<WeaponData> dataList = WeaponManager.instance.GetClassifiedWeaponData(isPlayerWeapon, rarity);
        for (int index = 0; index < dataList.Count; index++)
        {
            WeaponData weaponData = dataList[index];
            string weaponId = weaponData.uid;
            int weaponCount = _weaponCount.ContainsKey(weaponId) ? _weaponCount[weaponId] : 0;
            
            TemplateContainer weaponSlot = weaponSlotAsset.CloneTree();//���� ����
            _slotDict.Add(weaponId, weaponSlot);//Dictionary�� ���� - ������ ���� ���濡 �����ϱ� ����
            VisualElement weaponIcon = weaponSlot.Q<VisualElement>("WeaponIcon");
            VisualElement weaponBackground = weaponSlot.Q<VisualElement>("WeaponBackground");
            //LevelLabel
            int weaponLevel = _weaponLevel.ContainsKey(weaponId) ? _weaponCount[weaponId] : 0;
            Label levelLabel = weaponSlot.Q<Label>("LevelLabel");
            levelLabel.text = weaponLevel > 0 ? $"+{weaponLevel}" : string.Empty;
            //CountProgressBar
            ProgressBar countProgressBar = weaponSlot.Q<ProgressBar>();
            int requireCount = PriceManager.instance.GetRequireWeaponCount(rarity, weaponLevel);
            countProgressBar.title = $"{weaponCount}/{requireCount}";
            countProgressBar.value = weaponCount / (float)requireCount;
            //Icon
            weaponIcon.style.backgroundImage = new StyleBackground(weaponData.weaponSprite.texture);
            WeaponManager.instance.SetIconScale(weaponData, weaponIcon);
            if (weaponCount > 0)
            {
                switch (weaponData.weaponRarity)
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

            int currentIndex = index;
            weaponSlot.RegisterCallback<ClickEvent>(evt => OnClickSlot(currentIndex, weaponData, isPlayerWeapon));

            slotContainer.Add(weaponSlot);
        }
        currentScrollView.Add(slotContainer);
    }



    private void OnClickSlot(int currentIndex, WeaponData weaponData, bool isPlayerWeapon)
    {
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
    private void OnWeaponLevelSet(string uid, int level)
    {
        VisualElement slot = _slotDict[uid];
        //var 
    }
    private void OnWeaponCountSet(int uid, int count)
    {
        
    }

}
