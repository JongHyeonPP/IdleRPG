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
    Dictionary<SkillAcquireInfo, VisualElement> _acquireSlotDict = new();
    [SerializeField] SkillUI _skillUI;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        slotParentPanel = root.Q<VisualElement>("SlotParentPanel");
        _draggableScrollView = GetComponent<DraggableScrollView>();
        PlayerBroker.OnLevelExpSet += OnLevelExpSet;
        PlayerBroker.OnSkillLevelSet += OnSkillLevelSet;
        PlayerBroker.OnSkillLevelSet +=(str, num)=> OnLevelExpSet();
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
        SkillData skillData = SkillManager.instance.GetSkillData(skillId);
        if (skillData == null || !skillData.isPlayerSkill) return;

        var kvp = _acquireSlotDict.FirstOrDefault(item => item.Key.SkillData.uid == skillId);
        if (kvp.Key == null || kvp.Value == null) return; 

        VisualElement targetSlot = kvp.Value;
        VisualElement panel_1 = targetSlot.Q<VisualElement>(skillData.isActiveSkill ? "ActivePanel_1" : "PassivePanel_1");
        if (panel_1 == null) return; 

        panel_1.style.visibility = (skillLevel == 0) ? Visibility.Visible : Visibility.Hidden;
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
            if (info == null || info.SkillData == null) continue;

            if (info.SkillData.isActiveSkill)
                SetSkillAcquireSlot(slot, info);
        }

        foreach (var info in SkillManager.instance.GetAllPassiveSkills())
        {
            if (info == null || info.SkillData == null) continue;
            SetPassiveSkillInSlot(info);
        }
    }
    private void OnSlotClicked(SkillData skillData, VisualElement iconVe)
    {
        if (_draggableScrollView._isDragging) return;
        if (skillData == null || iconVe == null) return;

        iconVe.style.visibility = Visibility.Hidden;

        string key = skillData.uid; 
        if (!_gameData.skillLevel.ContainsKey(key) || _gameData.skillLevel[key] == 0)
        {
            _gameData.skillLevel[key] = 1;
            PlayerBroker.OnSkillLevelSet(key, 1);
        }
    }
    private void SetSkillAcquireSlot(VisualElement slot, SkillAcquireInfo info)
    {
        _acquireSlotDict.Add(info, slot);
        Label levelLabel = slot.Q<Label>("LevelLabel");
        VisualElement activePanel_1 = slot.Q<VisualElement>("ActivePanel_1");
        VisualElement passivePanel_1 = slot.Q<VisualElement>("PassivePanel_1");
        VisualElement lockPanel = slot.Q<VisualElement>("LockPanel");
        VisualElement panel_1 = info.SkillData.isActiveSkill ? activePanel_1 : passivePanel_1;
        VisualElement iconVe = panel_1.Q<VisualElement>("SkillIcon");
        if (_gameData.level >= info.acquireLevel)
        {
            lockPanel.style.display = DisplayStyle.None;
        }
        else
        {
            lockPanel.style.display = DisplayStyle.Flex;
        }

        (info.SkillData.isActiveSkill ? passivePanel_1 : activePanel_1).style.visibility = Visibility.Hidden;
        Dictionary<string, int> skillLevel = _gameData.skillLevel;
        var skillData = info.SkillData;

        if (skillData == null)
        {
            panel_1.style.visibility = Visibility.Hidden;
            return;
        }
        if (skillLevel.ContainsKey(skillData.uid) && skillLevel[skillData.uid] != 0)
        {
            panel_1.style.visibility = Visibility.Hidden;
        }

        iconVe.style.backgroundImage = new(skillData.iconSprite);
        iconVe.RegisterCallback<ClickEvent>(evt => OnSlotClicked(skillData, panel_1));
        levelLabel.text = info.acquireLevel.ToString();
        if (levelLabel.text.Length >= 3)
        {
            levelLabel.style.fontSize = 30;
        }
        else
        {
            levelLabel.style.fontSize = 40;
        }

    }

    public void CheckUnacquiredExist()
    {
        List<SkillAcquireInfo> keys = _acquireSlotDict.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            SkillAcquireInfo info = keys[i];
            if (info.acquireLevel > _gameData.level)
            {
                break;
            }
            if (!_gameData.skillLevel.ContainsKey(info.SkillData.uid) || _gameData.skillLevel[info.SkillData.uid] == 0)
            {
                UIBroker.OnMenuUINotice(2, true);
                _skillUI.skillAcquireNotice.StartNotice();
                return;
            }
        }
        UIBroker.OnMenuUINotice(2, false);
        _skillUI.skillAcquireNotice.StopNotice();
    }

    private void SetPassiveSkillInSlot(SkillAcquireInfo info)
    {
        VisualElement slot = FindSlotForLevel(info.acquireLevel);
        if (slot == null || info.SkillData == null) return;

        if (!_acquireSlotDict.ContainsKey(info))
            _acquireSlotDict.Add(info, slot);

        VisualElement passivePanel = slot.Q<VisualElement>("PassivePanel_1");
        if (passivePanel == null) return;

        VisualElement iconVe = passivePanel.Q<VisualElement>("SkillIcon");
        if (iconVe == null) return;

        iconVe.style.backgroundImage = new StyleBackground(info.SkillData.iconSprite);
        iconVe.RegisterCallback<ClickEvent>(evt => OnSlotClicked(info.SkillData, iconVe));

        string key = info.SkillData.uid;
        if (_gameData.skillLevel.ContainsKey(key) && _gameData.skillLevel[key] != 0)
        {
            passivePanel.style.visibility = Visibility.Hidden;
        }
        else
        {
            passivePanel.style.visibility = Visibility.Visible;
        }
    }
    private VisualElement FindSlotForLevel(int acquireLevel)
    {
        for (int i = 0; i < slotParentPanel.childCount; i++)
        {
            VisualElement slot = slotParentPanel.ElementAt(i);
            Label levelLabel = slot.Q<Label>("LevelLabel");
            if (levelLabel != null && int.TryParse(levelLabel.text, out int level))
            {
                if (level == acquireLevel)
                    return slot;
            }
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

    public void OnBattle()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {
    }
}