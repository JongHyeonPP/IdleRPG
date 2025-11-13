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
    [SerializeField] private VisualTreeAsset rarityLineAsset;

    private Button _acquireButton;
    private Button _activeButton;
    private Button _passiveButton;
    [SerializeField] SkillAcquireUI skillAcquireUI;

    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);

    private readonly Dictionary<Rarity, Label> fragmentLabelDict = new();

    // 추가됨 : 현재 선택된 탭 저장
    private int _currentTabIndex = -1;

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

        // 획득 버튼 : 항상 소리 남
        _acquireButton.RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            OnAcquisitionButtonClicked();
        });

        // Active 버튼 : 같은 탭이면 소리 X
        _activeButton.RegisterCallback<ClickEvent>(evt =>
        {
            if (_currentTabIndex != 0)
                SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

            _currentTabIndex = 0;
            OnActiveButtonClicked();
        });

        // Passive 버튼 : 같은 탭이면 소리 X
        _passiveButton.RegisterCallback<ClickEvent>(evt =>
        {
            if (_currentTabIndex != 1)
                SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

            _currentTabIndex = 1;
            OnPassiveButtonClicked();
        });

        SetScrollView();
    }

    private void SetScrollView()
    {
        SkillData[] skillDataArr = SkillManager.instance.playerSkillArr;
        SkillData[] activeSkills = skillDataArr.Where(item => item.isActiveSkill).ToArray();
        SkillData[] passiveSkills = skillDataArr.Where(item => !item.isActiveSkill).ToArray();

        SetEachScrollViewByRarity(activeSkills, _activeScrollView);
        SetEachScrollViewByRarity(passiveSkills, _passiveScrollView);
    }

    private void SetEachScrollViewByRarity(SkillData[] dataArr, DraggableScrollView draggableScrollview)
    {
        var ordered = dataArr.OrderBy(skill => skill.rarity).ToArray();
        var grouped = ordered.GroupBy(skill => skill.rarity);
        bool firstGroup = true;

        foreach (var group in grouped)
        {
            if (!firstGroup && rarityLineAsset != null)
            {
                TemplateContainer rarityLine = rarityLineAsset.CloneTree();
                draggableScrollview.scrollView.Add(rarityLine);
            }
            firstGroup = false;

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
                        currentSlot.style.display = DisplayStyle.None;
                    }
                }

                draggableScrollview.scrollView.Add(currentSlotSet);
            }
        }
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
            {
                SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
                _skillInfoUI.ActiveUI(skillData);
            }
        });

        _skillId_SlotDict.Add(skillData.name, currentSlot);
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
