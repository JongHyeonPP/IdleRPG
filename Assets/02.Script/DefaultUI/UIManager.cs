using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private IGeneralUI[] _uiArr;

    void Awake()
    {
        instance = this;

        _uiArr = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
        .OfType<IGeneralUI>()
        .ToArray();

        BattleBroker.SwitchToStory += OnStory;
        BattleBroker.SwitchToBattle += OnBattle;
        BattleBroker.SwitchToBoss += OnBoss;
        BattleBroker.SwitchToCompanionBattle += (arg0, arg1) => OnBoss();
        BattleBroker.SwitchToAdventure += (arg0, arg1) => OnBoss();
        BattleBroker.SwitchToDungeon += (arg0, arg1) => OnBoss();
    }
    void Start()
    {
        UIBroker.ChangeMenu?.Invoke(0);
    }

    private void OnBattle()
    {
        foreach(var x in _uiArr)
            x.OnBattle();
    }

    private void OnStory(int storyNum)
    {
        foreach (var x in _uiArr)
            x.OnStory();
    }
    private void OnBoss()
    {
        foreach (var x in _uiArr)
            x.OnBoss();
    }
}
