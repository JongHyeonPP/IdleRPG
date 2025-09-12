using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonInfoUI : MonoBehaviour, IGeneralUI
{
    public VisualElement root { get; private set; }
    private VisualElement _activePanel;
    private Label _stateLabel;
    private Label _recommendLabel;

    private GameData _gameData;
    private FlexibleListView _fListView;
    private DungeonInfoController _dungeonInfoController;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _gameData = StartBroker.GetGameData();
        _fListView = GetComponent<FlexibleListView>();
        _dungeonInfoController = GetComponent<DungeonInfoController>();

        root.Q<Button>("StartButton").RegisterCallback<ClickEvent>(_ => OnStartButtonClick());
        root.Q<Button>("ExitButton").RegisterCallback<ClickEvent>(_ => UIBroker.InactiveCurrentUI());

        _activePanel = root.Q<VisualElement>("ActivePanel");
        _recommendLabel = root.Q<Label>("RecommendLabel");
        _stateLabel = root.Q<Label>("StateLabel");
    }

    private void OnStartButtonClick()
    {
        StageInfo stageInfo = _dungeonInfoController.SelectedStageInfo;
        if (stageInfo == null)
        {
            UIBroker.ShowPopUpInBattle("스테이지를 선택하세요");
            return;
        }
        Debug.Log(stageInfo.name);
        // 여기서 실제 시작 로직 연결하면 됨
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

    public void ActiveUI(int index, DungeonSlot dungeonSlot)
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);

        // 리스트 채우기 전에 선택 상태 초기화
        _dungeonInfoController.ResetSelection();

        List<IListViewItem> items = StageInfoManager.instance
            .GetDungeonStageInfo(index)
            .Select(item => (IListViewItem)item)
            .ToList();

        _fListView.ChangeItems(items);
    }

    public void SetState(bool isActive)
    {
        _activePanel.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        _stateLabel.style.display = isActive ? DisplayStyle.None : DisplayStyle.Flex;
    }

    // DungeonInfoController가 선택될 때마다 호출
    public void OnClickedSlot(StageInfo stageInfo)
    {
        _recommendLabel.text = stageInfo.recommendLevel.ToString();
      
    }
}
