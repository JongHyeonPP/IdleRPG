using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using EnumCollection;

public class DungeonInfoUI : MonoBehaviour, IGeneralUI
{
    public VisualElement root { get; private set; }
    private VisualElement _activePanel;
    private Label _stateLabel;
    private Label _recommendLabel;
    private VisualElement _rewardIcon;
    private Label _rewardLabel;

    private GameData _gameData;
    private FlexibleListView _fListView;
    private DungeonInfoController _dungeonInfoController;

    private StageInfo _currentStageInfo;
    private VisualElement _bossImage;
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
        _bossImage = root.Q<VisualElement>("BossImage");
        _rewardIcon = root.Q<VisualElement>("RewardIcon");
        _rewardLabel = root.Q<Label>("RewardLabel");
    }

    private void OnStartButtonClick()
    {
        StageInfo stageInfo = _dungeonInfoController.SelectedStageInfo;
        if (stageInfo == null)
        {
            UIBroker.ShowPopUpInBattle("스테이지를 선택하세요");
            return;
        }
        int fee = StageInfoManager.instance.adventureEntranceFee;
        UIBroker.InactiveCurrentUI();
        if (_gameData.scroll < fee)
        {
            UIBroker.ShowPopUpInBattle("입장 비용이 부족합니다.");
            return;
        }
        UIBroker.ChangeMenu(0);
        UIBroker.FadeInOut(0f, 0.5f, 2f);
        BattleBroker.SwitchToDungeon(_currentStageInfo.adventrueInfo.adventureIndex_0, _currentStageInfo.adventrueInfo.adventureIndex_1);
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
        _currentStageInfo = stageInfo;
        _recommendLabel.text = stageInfo.recommendLevel.ToString();

        _bossImage.style.backgroundImage = new(_currentStageInfo.boss.prefab.GetComponentInChildren<SpriteRenderer>().sprite);
        _bossImage.style.left = _currentStageInfo.adventrueInfo.imageLeft;
        _bossImage.style.scale = new Vector2(_currentStageInfo.adventrueInfo.imageScale, _currentStageInfo.adventrueInfo.imageScale);

        StageInfo.AdventureInfo adventureStageInfo = stageInfo.adventrueInfo;
        DungeonReward dungeonInfo = StageInfoManager.instance.GetDungeonReward(adventureStageInfo.adventureIndex_0, adventureStageInfo.adventureIndex_1);
        var sprite = PlayerBroker.GetResourceSprite(dungeonInfo.resource);
        _rewardIcon.style.backgroundImage = new(sprite);
        _rewardLabel.text = dungeonInfo.amount.ToString("N0");
    }
}
