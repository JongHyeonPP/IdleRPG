using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillAcquireUI : MonoBehaviour
{
    private GameData _gameData;
    public VisualElement root { get; private set; }
    private VisualElement slotParentPanel;
    private DraggableScrollView _draggableScrollView;
    Dictionary<int, VisualElement> unlockPanelDict = new();
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        slotParentPanel = root.Q<VisualElement>("SlotParentPanel");
        _draggableScrollView = GetComponent<DraggableScrollView>();
        BattleBroker.OnLevelExpSet += OnLevelExpSet;
        _gameData = StartBroker.GetGameData();
    }

    private void OnLevelExpSet()
    {
        int level = _gameData.level;
        foreach (KeyValuePair<int, VisualElement> kvp in unlockPanelDict)
        {
            if (kvp.Key > level)//배우지 못하는 경우
            {
                kvp.Value.style.display = DisplayStyle.Flex;
            }
            else//배울 수 있는 경우
            {
                kvp.Value.style.display = DisplayStyle.None;
            }
        }
    }

    private void Start()
    {
        root.style.display = DisplayStyle.None;
        SetEntireSlots();
        Button exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(evt=>InactiveUI());
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
        iconVe.style.display = DisplayStyle.None;
        if (!skillLevel.ContainsKey(skillData.name) || skillLevel[skillData.name]==0)
           _gameData.skillLevel[skillData.name] = 1;
        PlayerBroker.OnSkillLevelSet(skillData.name, 1);
    }
    private void SetSkillAcquireSlot(VisualElement slot, SkillAcquireInfo info)
    {
        Label levelLabel = slot.Q<Label>("LevelLabel");
        VisualElement skillIcon_Player = slot.Q<VisualElement>("SkillPanel_Player_1");
        VisualElement skillIcon_Companion = slot.Q<VisualElement>("SkillPanel_Companion_1");
        VisualElement lockPanel = slot.Q<VisualElement>("LockPanel");
        unlockPanelDict.Add(info.acquireLevel, lockPanel);
        if (_gameData.level >= info.acquireLevel)
        {
            lockPanel.style.display = DisplayStyle.None;
        }
        else
        {
            lockPanel.style.display = DisplayStyle.Flex;
        }
        SetEachSlot(skillIcon_Player, info.playerSkillData);
        SetEachSlot(skillIcon_Companion, info.companionSkillData);
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
    private void SetEachSlot(VisualElement iconPanel, SkillData skillData)
    {
        Dictionary<string, int> skillLevel = _gameData.skillLevel;
        if (skillData == null)
        {
            iconPanel.style.visibility = Visibility.Hidden;
            return;
        }
        if (skillLevel.ContainsKey(skillData.uid) && skillLevel[skillData.uid] != 0)
        {
            iconPanel.style.visibility = Visibility.Hidden;
            return;
        }
        VisualElement iconVe = iconPanel.Q<VisualElement>("SkillIcon");
        if (skillData.iconSprite == null)
        {
            iconVe.style.backgroundImage =null;
        }
        else
        {
            iconVe.style.backgroundImage = new(skillData.iconSprite);
        }
        
        iconVe.RegisterCallback<ClickEvent>(evt => OnSlotClicked(skillData, iconPanel));
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