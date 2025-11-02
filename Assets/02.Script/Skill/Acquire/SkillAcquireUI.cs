using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillAcquireUI : MonoBehaviour, IGeneralUI
{
    private GameData _gameData;
    public VisualElement root { get; private set; }
    private VisualElement slotParentPanel;
    private DraggableScrollView _draggableScrollView;
    private Dictionary<SkillAcquireInfo, VisualElement> _acquireSlotDict = new();
    [SerializeField] private SkillUI _skillUI;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        slotParentPanel = root.Q<VisualElement>("SlotParentPanel");
        _draggableScrollView = GetComponent<DraggableScrollView>();

        PlayerBroker.OnLevelExpSet += OnLevelExpSet;
        PlayerBroker.OnSkillLevelSet += OnSkillLevelSet;
        PlayerBroker.OnSkillLevelSet += (str, num) => OnLevelExpSet();

        _gameData = StartBroker.GetGameData();
    }

    private void Start()
    {
        root.style.display = DisplayStyle.None;
        SetEntireSlots();
        Button exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(evt => InactiveUI());
        CheckUnacquiredExist();
    }

    private void OnSkillLevelSet(string skillId, int skillLevel)
    {
        foreach (var kvp in _acquireSlotDict)
        {
            SkillAcquireInfo info = kvp.Key;
            VisualElement slot = kvp.Value;

            // 액티브 확인
            if (info.activeSkill != null && info.activeSkill.name == skillId)
            {
                VisualElement activePanel = slot.Q<VisualElement>("ActivePanel_1");
                if (activePanel != null)
                    activePanel.style.visibility = (skillLevel == 0) ? Visibility.Visible : Visibility.Hidden;
            }

            // 패시브 확인
            if (info.passiveSkill != null && info.passiveSkill.name == skillId)
            {
                VisualElement passivePanel = slot.Q<VisualElement>("PassivePanel_1");
                if (passivePanel != null)
                    passivePanel.style.visibility = (skillLevel == 0) ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }

    private void OnLevelExpSet()
    {
        int level = _gameData.level;
        foreach (var kvp in _acquireSlotDict)
        {
            var lockPanel = kvp.Value.Q<VisualElement>("LockPanel");
            if (lockPanel == null) continue;
            lockPanel.style.display = (kvp.Key.acquireLevel > level) ? DisplayStyle.Flex : DisplayStyle.None;
        }
        CheckUnacquiredExist();
    }

    private void SetEntireSlots()
    {
        for (int i = 0; i < slotParentPanel.childCount; i++)
        {
            VisualElement slot = slotParentPanel.ElementAt(i);
            SkillAcquireInfo info = SkillManager.instance.GetInfo(i);
            if (info == null) continue;

            SetSkillAcquireSlot(slot, info);
        }
    }

    private void OnSlotClicked(SkillData skillData, VisualElement iconVe)
    {
        if (_draggableScrollView._isDragging) return;
        if (skillData == null || iconVe == null) return;

        iconVe.style.visibility = Visibility.Hidden;

        string key = skillData.name;
        if (!_gameData.skillLevel.ContainsKey(key) || _gameData.skillLevel[key] == 0)
        {
            _gameData.skillLevel[key] = 1;
            PlayerBroker.OnSkillLevelSet(key, 1);
        }
    }

    private void SetSkillAcquireSlot(VisualElement slot, SkillAcquireInfo info)
    {
        _acquireSlotDict[info] = slot;

        Label levelLabel = slot.Q<Label>("LevelLabel");
        VisualElement lockPanel = slot.Q<VisualElement>("LockPanel");
        VisualElement activePanel = slot.Q<VisualElement>("ActivePanel_1");
        VisualElement passivePanel = slot.Q<VisualElement>("PassivePanel_1");

        // 레벨 제한
        lockPanel.style.display = (_gameData.level >= info.acquireLevel) ? DisplayStyle.None : DisplayStyle.Flex;

        // 액티브 세팅
        if (info.activeSkill != null)
        {
            VisualElement iconVe = activePanel.Q<VisualElement>("SkillIcon");
            iconVe.style.backgroundImage = new StyleBackground(info.activeSkill.iconSprite);
            iconVe.RegisterCallback<ClickEvent>(evt => OnSlotClicked(info.activeSkill, activePanel));

            bool learned = _gameData.skillLevel.ContainsKey(info.activeSkill.name) &&
                           _gameData.skillLevel[info.activeSkill.name] != 0;

            activePanel.style.visibility = learned ? Visibility.Hidden : Visibility.Visible;
        }
        else
        {
            activePanel.style.visibility = Visibility.Hidden;
        }

        // 패시브 세팅
        if (info.passiveSkill != null)
        {
            VisualElement iconVe = passivePanel.Q<VisualElement>("SkillIcon");
            iconVe.style.backgroundImage = new StyleBackground(info.passiveSkill.iconSprite);
            iconVe.RegisterCallback<ClickEvent>(evt => OnSlotClicked(info.passiveSkill, passivePanel));

            bool learned = _gameData.skillLevel.ContainsKey(info.passiveSkill.name) &&
                           _gameData.skillLevel[info.passiveSkill.name] != 0;

            passivePanel.style.visibility = learned ? Visibility.Hidden : Visibility.Visible;
        }
        else
        {
            passivePanel.style.visibility = Visibility.Hidden;
        }

        levelLabel.text = info.acquireLevel.ToString();
        levelLabel.style.fontSize = levelLabel.text.Length >= 3 ? 30 : 40;
    }

    public void CheckUnacquiredExist()
    {
        foreach (var kvp in _acquireSlotDict)
        {
            SkillAcquireInfo info = kvp.Key;

            if (info.acquireLevel > _gameData.level) continue;

            bool unacquiredActive = info.activeSkill != null &&
                                    (!_gameData.skillLevel.ContainsKey(info.activeSkill.name) ||
                                     _gameData.skillLevel[info.activeSkill.name] == 0);

            bool unacquiredPassive = info.passiveSkill != null &&
                                     (!_gameData.skillLevel.ContainsKey(info.passiveSkill.name) ||
                                      _gameData.skillLevel[info.passiveSkill.name] == 0);

            if (unacquiredActive || unacquiredPassive)
            {
                UIBroker.OnMenuUINotice(2, true);
                _skillUI.skillAcquireNotice.StartNotice();
                return;
            }
        }

        UIBroker.OnMenuUINotice(2, false);
        _skillUI.skillAcquireNotice.StopNotice();
    }

    private VisualElement FindSlotForLevel(int acquireLevel)
    {
        for (int i = 0; i < slotParentPanel.childCount; i++)
        {
            VisualElement slot = slotParentPanel.ElementAt(i);
            Label levelLabel = slot.Q<Label>("LevelLabel");
            if (levelLabel != null && int.TryParse(levelLabel.text, out int level))
                if (level == acquireLevel)
                    return slot;
        }
        return null;
    }

    public void ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
    }

    public void InactiveUI()
    {
        UIBroker.InactiveCurrentUI();
    }

    public void OnBattle() => root.style.display = DisplayStyle.None;
    public void OnStory() => root.style.display = DisplayStyle.None;
    public void OnBoss() { }
}
