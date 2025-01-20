using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class SkillAcquireUI : MonoBehaviour
{
    public VisualElement root { get; private set; }
    private VisualElement slotParentPanel;
    private DraggableScrollView _draggableScrollView;
    Dictionary<int, VisualElement> unlockPanelDict = new();
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        slotParentPanel = root.Q<VisualElement>("SlotParentPanel");
        _draggableScrollView = GetComponent<DraggableScrollView>();
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
        Dictionary<string, int> skillLevel = StartBroker.GetGameData().skillLevel;
        iconVe.style.display = DisplayStyle.None;
        if (!skillLevel.ContainsKey(skillData.name))
           StartBroker.GetGameData().skillLevel[skillData.name] = 1;
        BattleBroker.OnSkillLevelSet(skillData.name, 1);
    }
    private void SetSkillAcquireSlot(VisualElement slot, SkillAcquireInfo info)
    {
        Label levelLabel = slot.Q<Label>("LevelLabel");
        VisualElement skillIcon_Player = slot.Q<VisualElement>("SkillIcon_Player");
        VisualElement skillIcon_Party = slot.Q<VisualElement>("SkillIcon_Party");
        VisualElement unlockPanel = slot.Q<VisualElement>("UnlockPanel");
        unlockPanelDict.Add(info.acquireLevel, unlockPanel);
        if (StartBroker.GetGameData().level >= info.acquireLevel)
        {
            unlockPanel.style.display = DisplayStyle.None;
        }
        else
        {
            unlockPanel.style.display = DisplayStyle.Flex;
        }
        SetEachSlot(skillIcon_Player, info.playerSkillData);
        SetEachSlot(skillIcon_Party, info.partySkillData);
        levelLabel.text = info.acquireLevel.ToString();
    }
    private void SetEachSlot(VisualElement iconVe, SkillData skillData)
    {
        Dictionary<string, int> skillLevel = StartBroker.GetGameData().skillLevel;
        if (skillData&&!skillLevel.ContainsKey(skillData.name))
        {
            iconVe.style.backgroundImage = new(skillData.iconSprite);
            iconVe.RegisterCallback<ClickEvent>(evt => OnSlotClicked(skillData, iconVe));
        }
        else
        {
           iconVe.style.visibility = Visibility.Hidden;
        }
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