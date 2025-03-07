using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillAcquireUI : MonoBehaviour
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
        BattleBroker.OnLevelExpSet += OnLevelExpSet;
        PlayerBroker.OnSkillLevelSet += OnSkillLevelSet;
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
        if (!skillData.isPlayerSkill)
            return;
        VisualElement targetSlot = _acquireSlotDict.Where(item => item.Key.SkillData.uid == skillId).FirstOrDefault().Value;
        bool isActiveSkill = skillData.isActiveSkill;
        VisualElement panel_1 = targetSlot.Q<VisualElement>(isActiveSkill?"ActivePanel_1": "PassivePanel_1");
        if (skillLevel == 0)
        {
            panel_1.style.visibility = Visibility.Visible;
        }
        else
        {
            panel_1.style.visibility = Visibility.Hidden;
        }
        
    }

    private void OnLevelExpSet()
    {
        int level = _gameData.level;
        foreach (KeyValuePair<SkillAcquireInfo, VisualElement> kvp in _acquireSlotDict)
        {
            if (kvp.Key.acquireLevel > level)//배우지 못하는 경우
            {
                kvp.Value.Q<VisualElement>("LockPanel").style.display = DisplayStyle.Flex;
            }
            else//배울 수 있는 경우
            {
                kvp.Value.Q<VisualElement>("LockPanel").style.display = DisplayStyle.None;
            }
        }
        CheckUnacquiredExist();
    }


    private void SetEntireSlots()
    {
        for(int i =0;i<slotParentPanel.childCount;i++)
        {
            VisualElement slot = slotParentPanel.ElementAt(i);
            SkillAcquireInfo info = SkillManager.instance.GetInfo(i);
            SetSkillAcquireSlot(slot, info);
        }
    }
    private void OnSlotClicked(SkillData skillData, VisualElement iconVe)
    {
        if (_draggableScrollView._isDragging)
            return;
        Dictionary<string, int> skillLevel = _gameData.skillLevel;
        iconVe.style.visibility = Visibility.Hidden;
        if (!skillLevel.ContainsKey(skillData.name) || skillLevel[skillData.name]==0)
           _gameData.skillLevel[skillData.name] = 1;
        PlayerBroker.OnSkillLevelSet(skillData.name, 1);
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
            if (!_gameData.skillLevel.ContainsKey(info.SkillData.uid)||_gameData.skillLevel[info.SkillData.uid]==0)
            {
                _skillUI.acquireNoticeDot.StartNotice();
                UIBroker.OnMenuUINotice(2, true);
                return;
            }
        }
        _skillUI.acquireNoticeDot.StopNotice();
        UIBroker.OnMenuUINotice(2, false);
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
}