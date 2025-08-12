using EnumCollection;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class StageInfoUI : MonoBehaviour, IGeneralUI
{
    public VisualElement root { private set; get; }
    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        BattleBroker.ActiveStageInfoUI += ActiveStageInfoUI;
        root.Q<VisualElement>("StageInfoUI").RegisterCallback<ClickEvent>((evt) => { root.style.display = DisplayStyle.None; });
    }

    private void ActiveStageInfoUI(int stageIndex)
    {
        root.style.display = DisplayStyle.Flex;
        int currentGoldValue = BattleBroker.GetDropValue(DropType.Gold);
        int currentExpValue = BattleBroker.GetDropValue(DropType.Gold);
        int currentFragmentValue = BattleBroker.GetDropValue(DropType.Fragment);
        int currenWeaponValue = BattleBroker.GetDropValue(DropType.Weapon);

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
