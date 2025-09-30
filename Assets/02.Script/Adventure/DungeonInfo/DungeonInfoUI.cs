using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using EnumCollection;

/// <summary>
/// 던전 정보 UI 컨트롤러.
/// - 리스트뷰(FlexibleListView)에 던전 스테이지 아이템 바인딩
/// - 슬롯 선택 시 보스 이미지/권장레벨/보상 표시
/// - 시작 버튼으로 실제 전투로 전환
/// </summary>
public class DungeonInfoUI : MonoBehaviour, IGeneralUI
{
    // UI 루트 (UIDocument의 rootVisualElement)
    public VisualElement root { get; private set; }

    // 활성/비활성 표시 패널 및 텍스트/이미지 참조
    private VisualElement _activePanel;
    private Label _stateLabel;
    private Label _recommendLabel;
    private VisualElement _rewardIcon;
    private Label _rewardLabel;
    private VisualElement _bossImage;
    private VisualElement _regionImage;
    private Label _regionLabel;
    private Label _titleLabel;

    // 데이터/컨트롤러 참조
    private GameData _gameData;
    private FlexibleListView _fListView;
    private DungeonInfoController _dungeonInfoController;

    // 현재 선택된 스테이지(슬롯 클릭 시 갱신)
    private StageInfo _currentStageInfo;

    private void Awake()
    {
        // 기본 참조 획득
        root = GetComponent<UIDocument>().rootVisualElement;
        _gameData = StartBroker.GetGameData();
        _fListView = GetComponent<FlexibleListView>();
        _dungeonInfoController = GetComponent<DungeonInfoController>();

        // 버튼 콜백 등록
        root.Q<Button>("StartButton").RegisterCallback<ClickEvent>(_ => OnStartButtonClick());
        root.Q<Button>("ExitButton").RegisterCallback<ClickEvent>(_ => UIBroker.InactiveCurrentUI());

        // UI 요소 캐싱
        _activePanel = root.Q<VisualElement>("ActivePanel");
        _recommendLabel = root.Q<Label>("RecommendLabel");
        _stateLabel = root.Q<Label>("StateLabel");
        _bossImage = root.Q<VisualElement>("BossImage");
        _rewardIcon = root.Q<VisualElement>("RewardIcon");
        _rewardLabel = root.Q<Label>("RewardLabel");
        _regionImage = root.Q<VisualElement>("RegionImage");
        _regionLabel = root.Q<Label>("RegionLabel");
        _titleLabel = root.Q<Label>("TitleLabel");
    }

    /// <summary>
    /// 시작 버튼: 선택된 스테이지로 던전 전투 진입.
    /// </summary>
    private void OnStartButtonClick()
    {
        StageInfo stageInfo = _dungeonInfoController.SelectedStageInfo;
        if (stageInfo == null)
        {
            UIBroker.ShowPopUpInBattle("스테이지를 선택하세요");
            return;
        }

        int fee = StageInfoManager.instance.adventureEntranceFee;

        // UI 닫고, 비용 체크. (UI를 닫은 뒤 부족 안내 팝업을 띄우는 플로우)
        
        if (_gameData.scroll < fee)
        {
            UIBroker.ShowPopUpInBattle("입장 비용이 부족합니다.");
            return;
        }
        UIBroker.InactiveCurrentUI();
        // 전투 전환 연출 및 씬 상태 변경
        UIBroker.ChangeMenu(0);
        UIBroker.FadeInOut(0f, 0.5f, 2f);
        _gameData.scroll -= fee;
        // 현재 선택된 스테이지의 어드벤처 인덱스 정보를 사용해 던전 진입
        BattleBroker.SwitchToDungeon(
            _currentStageInfo.adventrueInfo.adventureIndex_0,
            _currentStageInfo.adventrueInfo.adventureIndex_1
        );
    }

    // IGeneralUI: 전투 진입 시 UI 숨김
    public void OnBattle()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnStory() { }
    public void OnBoss() { }

    /// <summary>
    /// 던전 UI 활성화.
    /// - 리스트 선택 상태 초기화
    /// - index(던전 카테고리)에 해당하는 StageInfo 배열을 리스트뷰에 바인딩
    /// </summary>
    public void ActiveUI(int index, DungeonSlot dungeonSlot)
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);

        // 새로 열 때 이전 선택 상태 리셋
        _dungeonInfoController.ResetSelection();

        // 해당 던전 카테고리의 스테이지 목록을 가져와 리스트뷰 세팅
        List<IListViewItem> items = StageInfoManager.instance
            .GetDungeonStageInfo(index)
            .Select(item => (IListViewItem)item)
            .ToList();

        _fListView.ChangeItems(items);
    }

    /// <summary>
    /// 입장 가능/불가 상태에 따른 패널/라벨 표시 전환.
    /// </summary>
    public void SetState(bool isActive)
    {
        _activePanel.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        _stateLabel.style.display = isActive ? DisplayStyle.None : DisplayStyle.Flex;
    }

    /// <summary>
    /// 슬롯 클릭 시(선택 변경) 호출.
    /// - 보스 스프라이트, 권장 레벨, 보상 아이콘/수량 UI 갱신
    /// </summary>
    public void OnClickedSlot(StageInfo stageInfo)
    {
        _currentStageInfo = stageInfo;

        // 권장 레벨
        _recommendLabel.text = stageInfo.recommendLevel.ToString();

        // 보스 이미지는 프리팹에서 SpriteRenderer의 스프라이트를 끌어다 사용
        _bossImage.style.backgroundImage = new(
            _currentStageInfo.boss.prefab.GetComponentInChildren<SpriteRenderer>().sprite
        );
        // 이미지 오프셋/스케일은 StageInfo의 AdventureInfo에 정의된 파라미터 사용
        _bossImage.style.left = _currentStageInfo.adventrueInfo.imageLeft;
        _bossImage.style.scale = new Vector2(_currentStageInfo.adventrueInfo.imageScale,
                                             _currentStageInfo.adventrueInfo.imageScale);

        // 보상 아이콘/수량
        StageInfo.AdventureInfo adventureStageInfo = stageInfo.adventrueInfo;
        DungeonReward dungeonInfo = StageInfoManager.instance.GetDungeonReward(
            adventureStageInfo.adventureIndex_0,
            adventureStageInfo.adventureIndex_1
        );
        Sprite sprite;
        if (dungeonInfo.resource == Resource.Fragment)
        {
            sprite = PlayerBroker.GetFragmentSprite(dungeonInfo.rarity.Value);
        }
        else
        {
            sprite = PlayerBroker.GetResourceSprite(dungeonInfo.resource);
        }
        _rewardIcon.style.backgroundImage = new(sprite);
        _rewardLabel.text = dungeonInfo.amount.ToString("N0");
        var regionInfo = StageInfoManager.instance.GetRegionInfo((int)stageInfo.background);
        Sprite regionSprite = regionInfo.regionSprite;
        _regionImage.style.backgroundImage = new(regionSprite);
        _regionLabel.text = regionInfo.regionName;
        _titleLabel.text = stageInfo.stageName;
    }
}
