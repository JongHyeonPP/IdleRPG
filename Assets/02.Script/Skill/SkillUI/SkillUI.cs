using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillUI : MonoBehaviour, IMenuUI
{
    private GameData _gameData;
    [SerializeField] SkillInfoUI _skillInfoUI;
    public VisualElement root { get; private set; }
    [SerializeField] UIDocument notice;
    public NoticeDot skillAcquireNotice;
    [SerializeField] DraggableScrollView _activeScrollView;
    [SerializeField] DraggableScrollView _passiveScrollView;
    private readonly Dictionary<string, VisualElement> _skillId_SlotDict = new();
    private VisualElement _equipBackground;
    [SerializeField] VisualTreeAsset slotSetAsset;

    // 무기 UI처럼 rarityLineAsset 추가
    [SerializeField] private VisualTreeAsset rarityLineAsset;

    private Button _acquireButton;
    private Button _activeButton;
    private Button _passiveButton;
    [SerializeField] SkillAcquireUI skillAcquireUI;

    // ButtonColor
    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);

    // Fragment
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

        _gameData = StartBroker.GetGameData();
        skillAcquireNotice = new(notice.rootVisualElement.Q<VisualElement>("SkillAcquire"), this);
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

        // Active / Passive 분류
        SkillData[] activeSkills = skillDataArr.Where(item => item.isActiveSkill).ToArray();
        SkillData[] passiveSkills = skillDataArr.Where(item => !item.isActiveSkill).ToArray();

        // 레어리티별 ScrollView 세팅
        SetEachScrollViewByRarity(activeSkills, _activeScrollView);
        SetEachScrollViewByRarity(passiveSkills, _passiveScrollView);
    }

    private void SetEachScrollViewByRarity(SkillData[] dataArr, DraggableScrollView draggableScrollview)
    {
        // rarity 순으로 정렬
        var ordered = dataArr.OrderBy(skill => skill.rarity).ToArray();

        // 레어리티 단위로 그룹핑
        var grouped = ordered.GroupBy(skill => skill.rarity);

        bool firstGroup = true;

        foreach (var group in grouped)
        {
            // 첫 그룹 빼고는 구분선 추가
            if (!firstGroup && rarityLineAsset != null)
            {
                TemplateContainer rarityLine = rarityLineAsset.CloneTree();
                draggableScrollview.scrollView.Add(rarityLine);
            }
            firstGroup = false;

            // 이 레어리티에 속하는 스킬 리스트
            var skills = group.ToList();

            int index = 0;
            while (index < skills.Count)
            {
                VisualElement currentSlotSet = slotSetAsset.CloneTree();

                for (int i = 0; i < 4; i++)
                {
                    VisualElement currentSlot = currentSlotSet.Q<VisualElement>($"SkillData_{i}");

                    if (index < skills.Count)
                    {
                        SetSlot(draggableScrollview, skills[index], currentSlot);
                        index++;
                    }
                    else
                    {
                        // 남는 칸은 숨김
                        currentSlot.style.display = DisplayStyle.None;
                    }
                }

                draggableScrollview.scrollView.Add(currentSlotSet);
            }
        }

        if (dataArr.Length < 9)
            draggableScrollview.scrollView.style.height = Length.Auto();
    }




    private void SetSlot(DraggableScrollView draggableScrollview, SkillData skillData, VisualElement currentSlot)
    {
        if (skillData == null)
        {
            currentSlot.style.display = DisplayStyle.None;
            return;
        }

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
        else
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

    private void OnFragmentSet()
    {
        foreach (var kvp in _gameData.skillFragment)
        {
            if (!fragmentLabelDict.ContainsKey(kvp.Key))
            {
                fragmentLabelDict.Add(kvp.Key, new Label());
            }

            fragmentLabelDict[kvp.Key].text = kvp.Value.ToString();
        }
    }

    private void OnSkillLevelSet(string skillId, int skillLevel)
    {
        if (!_skillId_SlotDict.TryGetValue(skillId, out VisualElement currentSlot))
            return;

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
            if (rarity == Rarity.Ancient)
                continue;
            InitFragment(rarity);
        }

        void InitFragment(Rarity rarity)
        {
            VisualElement fragment = fragmentGrid.Q<VisualElement>($"Fragment{rarity}");
            VisualElement iconVe = fragment.Q<VisualElement>("IconVe");
            Label numLabel = fragment.Q<Label>("NumLabel");
            iconVe.style.backgroundImage = new(CurrencyManager.instance._fragmentSprites[(int)rarity]);
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
        _equipBackground.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnAcquisitionButtonClicked()
    {
        skillAcquireUI.ActiveUI();
    }

    void IMenuUI.ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
        skillAcquireNotice.root.style.display = DisplayStyle.Flex;
    }

    void IMenuUI.InactiveUI()
    {
        root.style.display = DisplayStyle.None;
        skillAcquireNotice.root.style.display = DisplayStyle.None;
    }
}
