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
    private Button _acquisitionButton;
    private Button _playerSelectButton;
    private Button _partySelectButton;
    [SerializeField] SkillAcquireUI skillAcquireUI;
    //ButtonColor
    private readonly Color inactiveColor = new(0f, 0.36f, 0.51f);
    private readonly Color activeColor = new(0.04f, 0.24f, 0.32f);
    //Fragment
    private Dictionary<Rarity, Label> fragmentDict = new();
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _equipBackground = root.Q<VisualElement>("EquipBackground");
        _acquisitionButton = root.Q<Button>("AcquisitionButton");
        _playerSelectButton = root.Q<Button>("PlayerSelectButton");
        _partySelectButton = root.Q<Button>("PartySelectButton");
        skillAcquireUI.gameObject.SetActive(true);
        PlayerBroker.OnSkillLevelSet += OnSkillLevelChange;
        PlayerBroker.OnFragmentSet += OnFragmentSet;
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
        OnPlayerSelectButtonClicked();
        ToggleEquipBackground(false);
        _equipBackground.RegisterCallback<ClickEvent>(evt => {
            ToggleEquipBackground(false);
        });
        InitFragmentGrid();
        // 버튼 클릭 이벤트 등록
        _acquisitionButton.RegisterCallback<ClickEvent>(evt=>OnAcquisitionButtonClicked());
        _playerSelectButton.RegisterCallback<ClickEvent>(evt=>OnPlayerSelectButtonClicked());
        _partySelectButton.RegisterCallback<ClickEvent>(evt=>OnPartySelectButtonClicked());
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

    private void OnPlayerSelectButtonClicked()
    {
        List<IListViewItem> itemList = SkillManager.instance.GetSkillDataListAsItem(true);
        _flexibleLV.ChangeItems(itemList);
        _playerSelectButton.style.backgroundColor = activeColor;
        _partySelectButton.style.backgroundColor = inactiveColor;
    }
    private void OnPartySelectButtonClicked()
    {
        List<IListViewItem> itemList = SkillManager.instance.GetSkillDataListAsItem(false);
        _flexibleLV.ChangeItems(itemList);
        _partySelectButton.style.backgroundColor = activeColor;
        _playerSelectButton.style.backgroundColor = inactiveColor;
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
        BattleBroker.OnMenuUIChange += HandleUIChange;
    }

    private void OnDisable()
    {
        BattleBroker.OnMenuUIChange -= HandleUIChange;
    }
    private void HandleUIChange(int uiType)
    {
        if (uiType == 2)
            ShowSkillUI();
        else
            HideSkillUI();
    }
    public void HideSkillUI()
    {
        root.style.display = DisplayStyle.None;
    }
    public void ShowSkillUI()
    {
        root.style.display = DisplayStyle.Flex;
        //킬때마다 무기개수 초기화
    }
    #endregion
}