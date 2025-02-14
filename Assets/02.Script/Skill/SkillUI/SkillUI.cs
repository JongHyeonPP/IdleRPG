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
    private Button _companionSelectButton;
    [SerializeField] SkillAcquireUI skillAcquireUI;
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
        _acquisitionButton = root.Q<Button>("AcquisitionButton");
        _playerSelectButton = root.Q<Button>("PlayerSelectButton");
        _companionSelectButton = root.Q<Button>("PartySelectButton");
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
        _companionSelectButton.RegisterCallback<ClickEvent>(evt=>OnCompanionSelectButtonClicked());
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
        _playerSelectButton.style.unityBackgroundImageTintColor =_playerSelectButton.style.color = activeColor;
        _companionSelectButton.style.unityBackgroundImageTintColor=_companionSelectButton.style.color = inactiveColor;
    }
    private void OnCompanionSelectButtonClicked()
    {
        List<IListViewItem> itemList = SkillManager.instance.GetSkillDataListAsItem(false);
        _flexibleLV.ChangeItems(itemList);
        _playerSelectButton.style.unityBackgroundImageTintColor = _playerSelectButton.style.color = inactiveColor;
        _companionSelectButton.style.unityBackgroundImageTintColor = _companionSelectButton.style.color = activeColor;
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
            root.style.display = DisplayStyle.Flex;
        else
            root.style.display = DisplayStyle.None;
    }
    #endregion
}