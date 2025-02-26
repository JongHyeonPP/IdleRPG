using EnumCollection;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    [SerializeField] FlexibleListView _flexibleLV;
    private VisualElement _equipBackground;
    private Button _acquireButton;
    private Button _activeButton;
    private Button _passiveButton;
    [SerializeField] SkillAcquireUI skillAcquireUI;
    private NoticeDot _acquireNoticeDot;
    //ButtonColor
    private readonly Color inactiveColor =  new (0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);
    //Fragment
    private Dictionary<Rarity, Label> fragmentDict = new();
    [SerializeField] Sprite[] fragmentSprites;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _equipBackground = root.Q<VisualElement>("EquipBackground");
        _acquireButton = root.Q<Button>("AcquireButton");
        _activeButton = root.Q<Button>("ActiveButton");
        _passiveButton = root.Q<Button>("PassiveButton");
        skillAcquireUI.gameObject.SetActive(true);
        PlayerBroker.OnSkillLevelSet += OnSkillLevelChange;
        PlayerBroker.OnFragmentSet += OnFragmentSet;
        _acquireNoticeDot =  new NoticeDot(_acquireButton, this);
        _acquireNoticeDot.StartNotice();
    }

    private void OnFragmentSet(Rarity rarity, int num)
    {
        fragmentDict[rarity].text = num.ToString();
    }

    private void OnSkillLevelChange(string skillId, int level)
    {
        _flexibleLV.listView.Rebuild();
    }

    private void Start()
    {
        OnActiveButtonClicked();
        ToggleEquipBackground(false);
        _equipBackground.RegisterCallback<ClickEvent>(evt => {
            ToggleEquipBackground(false);
        });
        InitFragmentGrid();
        // 버튼 클릭 이벤트 등록
        _acquireButton.RegisterCallback<ClickEvent>(evt=>OnAcquisitionButtonClicked());
        _activeButton.RegisterCallback<ClickEvent>(evt=>OnActiveButtonClicked());
        _passiveButton.RegisterCallback<ClickEvent>(evt=>OnPassiveButtonClicked());
    }

    private void InitFragmentGrid()
    {
        VisualElement fragmentGrid = root.Q<VisualElement>("FragmentGrid");
        Rarity[] rarityArr = (Rarity[])Enum.GetValues(typeof(Rarity));
        foreach (Rarity rarity in rarityArr)
        {
            InitFragment(rarity);
        }
        void InitFragment(Rarity rarity)
        {
            VisualElement fragment = fragmentGrid.Q<VisualElement>($"Fragment{rarity}");
            VisualElement iconVe = fragment.Q<VisualElement>("IconVe");
            Label numLabel = fragment.Q<Label>("NumLabel");
            fragmentDict.Add(rarity, numLabel);
            if (!StartBroker.GetGameData().skillFragment.TryGetValue(rarity, out int value))
            {
                value = 0;
            }
            numLabel.text = value.ToString();
        }
    }

    private void OnActiveButtonClicked()
    {
        List<IListViewItem> itemList = SkillManager.instance.GetPlayerSkillDataListAsItem(true);
        _flexibleLV.ChangeItems(itemList);//리스트뷰에 들어갈 아이템 등록

        _activeButton.style.unityBackgroundImageTintColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.1f);
        _activeButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;
        _activeButton.Q<Label>().style.color = activeColor;
        _passiveButton.style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
        _passiveButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
        _passiveButton.Q<Label>().style.color = inactiveColor;
    }
    private void OnPassiveButtonClicked()
    {
        List<IListViewItem> itemList = SkillManager.instance.GetPlayerSkillDataListAsItem(false);
        _flexibleLV.ChangeItems(itemList);//리스트뷰에 들어갈 아이템 등록

        _passiveButton.style.unityBackgroundImageTintColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.1f);
        _passiveButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;
        _passiveButton.Q<Label>().style.color = activeColor;
        _activeButton.style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
        _activeButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
        _activeButton.Q<Label>().style.color = inactiveColor;
    }

    public void ToggleEquipBackground(bool isActive)
    {
        if (isActive)
        {
            _equipBackground.style.display = DisplayStyle.Flex;
        }
        else
        {
            _equipBackground.style.display = DisplayStyle.None;
        }
    }
    // 버튼 클릭 시 실행되는 메서드
    private void OnAcquisitionButtonClicked()
    {
        skillAcquireUI.ActiveUI();
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
        if (uiType == 2)
        {
            root.style.display = DisplayStyle.Flex;
            _acquireNoticeDot.SetParentToRoot();
        }
        else
            root.style.display = DisplayStyle.None;
    }
    #endregion
}