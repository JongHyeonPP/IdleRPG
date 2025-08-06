using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonInfoUI : MonoBehaviour, IGeneralUI
{
    //UI
    public VisualElement root { get; private set; }
    //Ref
    private GameData _gameData;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _gameData = StartBroker.GetGameData();
    }
    private void Start()
    { }

    public void OnBattle()
    {
    }

    public void OnStory()
    {
    }

    public void OnBoss()
    {
    }

    internal void ActiveUI(AdventureSlot adventureSlot, int index)
    {
    }
}
