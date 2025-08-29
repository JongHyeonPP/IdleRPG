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

    FlexibleListView _draggableLV;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _gameData = StartBroker.GetGameData();
        _draggableLV = GetComponent<FlexibleListView>();

    }
    private void Start()
    {
        List<IListViewItem> items = StageInfoManager.instance.GetDungeonStageInfo(0).Select(item=>(IListViewItem)item).ToList();
        _draggableLV.ChangeItems(items);
    }

    public void OnBattle()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnStory()
    {
    }

    public void OnBoss()
    {
    }

    public void ActiveUI(DungeonSlot dungeonSlot)
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
    }
}
