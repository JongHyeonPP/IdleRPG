using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillUI : MonoBehaviour
{
    private GameData _gameData;
    [SerializeField] SkillInfoUI _skillInfoUI;
    public VisualElement root { get; private set; }
    [SerializeField] DraggableScrollView _activeScrollView;
    [SerializeField] DraggableScrollView _passiveScrollView;
    private readonly Dictionary<string, VisualElement> _skillId_SlotDict = new();
    private VisualElement _equipBackground;
    [SerializeField] VisualTreeAsset slotSetAsset;
    private Button _acquireButton;
    private Button _activeButton;
    private Button _passiveButton;
    [SerializeField] SkillAcquireUI skillAcquireUI;
    public NoticeDot acquireNoticeDot;
    //ButtonColor
    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);
    //Fragment
    private readonly Dictionary<Rarity, Label> fragmentLabelDict = new();
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _equipBackground = root.Q<VisualElement>("EquipBackground");
        _acquireButton = root.Q<Button>("AcquireButton");
        _activeButton = root.Q<Button>("ActiveButton");
        _passiveButton = root.Q<Button>("PassiveButton");
        skillAcquireUI.gameObject.SetActive(true);
        PlayerBroker.OnSkillLevelSet += OnSkillLevelSet;
        PlayerBroker.OnFragmentSet += OnFragmentSet;

        acquireNoticeDot = new NoticeDot(_acquireButton, this);
        _gameData = StartBroker.GetGameData();
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
        _acquireButton.RegisterCallback<ClickEvent>(evt => OnAcquisitionButtonClicked());
        _activeButton.RegisterCallback<ClickEvent>(evt => OnActiveButtonClicked());
        _passiveButton.RegisterCallback<ClickEvent>(evt => OnPassiveButtonClicked());
        SetScrollView();
    }
    private void SetScrollView()
    {
        SkillData[] skillDataArr = SkillManager.instance.playerSkillArr;
        SetEachScrollView(skillDataArr.Where(item => item.isActiveSkill).ToArray(), _activeScrollView);
        SetEachScrollView(skillDataArr.Where(item => !item.isActiveSkill).ToArray(), _passiveScrollView);
    }
    private void SetEachScrollView(SkillData[] dataArr, DraggableScrollView draggableScrollview)
    {
        int indexInSet = 0;
        VisualElement currentSlotSet = null;
        for (int i = 0; i < dataArr.Length; i++)
        {
            SkillData skillData = dataArr[i];
            if (indexInSet == 0)
            {
                currentSlotSet = slotSetAsset.CloneTree();
            }
            VisualElement currentSlot = currentSlotSet.Q<VisualElement>($"SkillData_{indexInSet}");
            SetSlot(draggableScrollview, skillData, currentSlot);
            indexInSet = (indexInSet + 1) % 4;

            if (indexInSet == 0)
            {
                draggableScrollview.scrollView.Add(currentSlotSet);
                currentSlotSet = null;
            }
        }
        if (currentSlotSet != null)
        {
            draggableScrollview.scrollView.Add(currentSlotSet);

            for (int i = indexInSet; i <= 3; i++)
            {
                VisualElement currentSlot = currentSlotSet.Q<VisualElement>($"SkillData_{i}");
                currentSlot.style.display = DisplayStyle.None;
            }
        }
        if (dataArr.Length < 9)
            draggableScrollview.scrollView.style.height = Length.Auto();
    }


    private void SetSlot(DraggableScrollView draggableScrollview, SkillData skillData, VisualElement currentSlot)
    {
        if (!_gameData.skillLevel.TryGetValue(skillData.name, out int skillLevel))
        {
            skillLevel = 0;
        }
        VisualElement unacquired = currentSlot.Q<VisualElement>("Unacquired");
        VisualElement acquired = currentSlot.Q<VisualElement>("Acquired");
        if (skillLevel == 0)
        {
            acquired.style.display = DisplayStyle.None;
            unacquired.style.display = DisplayStyle.Flex;
        }
        else if (skillLevel > 0)
        {
            acquired.style.display = DisplayStyle.Flex;
            unacquired.style.display = DisplayStyle.None;
            Label levelLabel = currentSlot.Q<Label>("LevelLabel");
            levelLabel.text = $"Lv.{skillLevel}";
        }
        VisualElement skillIcon = currentSlot.Q<VisualElement>("SkillIcon");
        skillIcon.style.backgroundImage = new(skillData.iconSprite);
        Label nameLabel = currentSlot.Q<Label>("NameLabel");
        nameLabel.text = skillData.name;
        VisualElement clickVe = currentSlot.Q<VisualElement>("ClickVe");
        clickVe.RegisterCallback<ClickEvent>(evt =>
        {
            if (!draggableScrollview._isDragging)
                _skillInfoUI.ActiveUI(skillData);
        });
        _skillId_SlotDict.Add(skillData.uid, currentSlot);
    }

    private void OnFragmentSet(Rarity rarity, int num)
    {
        fragmentLabelDict[rarity].text = num.ToString();
    }
    private void OnSkillLevelSet(string skillId, int skillLevel)
    {
        if (!_skillId_SlotDict.TryGetValue(skillId, out VisualElement currentSlot))
        {
            return;
        }
        VisualElement unacquired = currentSlot.Q<VisualElement>("Unacquired");
        VisualElement acquired = currentSlot.Q<VisualElement>("Acquired");
        if (skillLevel == 0)
        {
            acquired.style.display = DisplayStyle.None;
            unacquired.style.display = DisplayStyle.Flex;
        }
        else
        {
            acquired.style.display = DisplayStyle.Flex;
            unacquired.style.display = DisplayStyle.None;
            Label levelLabel = currentSlot.Q<Label>("LevelLabel");
            levelLabel.text = $"Lv.{skillLevel}";
        }
        
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
            iconVe.style.backgroundImage = new(PriceManager.instance.fragmentSprites[(int)rarity]);
            fragmentLabelDict.Add(rarity, numLabel);
            if (!StartBroker.GetGameData().skillFragment.TryGetValue(rarity, out int value))
            {
                value = 0;
            }
            numLabel.text = value.ToString();
        }
    }

    private void OnActiveButtonClicked()
    {
        _activeScrollView.scrollView.style.display = DisplayStyle.Flex;
        _passiveScrollView.scrollView.style.display = DisplayStyle.None;

        _activeButton.style.unityBackgroundImageTintColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.1f);
        _activeButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;
        _activeButton.Q<Label>().style.color = activeColor;
        _passiveButton.style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
        _passiveButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
        _passiveButton.Q<Label>().style.color = inactiveColor;
    }
    private void OnPassiveButtonClicked()
    {
        _activeScrollView.scrollView.style.display = DisplayStyle.None;
        _passiveScrollView.scrollView.style.display = DisplayStyle.Flex;

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
            acquireNoticeDot.SetParentToRoot();
        }
        else
            root.style.display = DisplayStyle.None;
    }
    #endregion
}