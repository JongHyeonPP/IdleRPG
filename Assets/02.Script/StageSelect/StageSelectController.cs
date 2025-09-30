using UnityEngine.UIElements;
using UnityEngine;
using System;

/// <summary>
/// 스테이지 리스트(UI Toolkit 기반)에서 아이템을 바인딩/선택/이동 처리하는 컨트롤러.
/// - FlexibleListView에 아이템을 바인딩하여 현재 스테이지 해금 여부/정보/버튼 처리
/// - 항목 클릭 또는 Move 버튼으로 스테이지 이동 트리거
/// - 스크롤 드래그 중에는 클릭 무시(오작동 방지)
/// </summary>
public class StageSelectController : MonoBehaviour, LVItemController
{
    private GameData _gameData;

    /// <summary>
    /// 외부에서 주입되는 리스트 뷰(드래그/스크롤 지원)
    /// </summary>
    public FlexibleListView draggableLV { get; set; }

    // 현재 선택된 슬롯의 VisualElement (선택 테두리 표시를 위해 보관)
    private VisualElement selectedElement;

    /// <summary>
    /// 아이템 템플릿 내에서 자주 참조하는 UI 요소 캐시
    /// (Q() 호출 최소화 + 콜백에서 빠른 접근)
    /// </summary>
    private class ItemCache
    {
        public Button infoButton;
        public Button moveButton;
        public Label stageLabel;
        public Label titleLabel;
        public Label infoLabel;
        public VisualElement lockGroup;    // 잠금 오버레이/그룹
        public VisualElement selectBorder; // 선택 표시용 테두리

        // 데이터/상태
        public StageInfo stageInfo;
        public int stageNum;
        public bool isOpen;
        public int index;                  // 리스트 내 인덱스(스크롤 이동에 사용)
    }

    /// <summary>
    /// LVItemController 구현: 리스트 아이템 바인딩 시 호출
    /// 아이템의 텍스트/버튼/잠금 상태/선택 상태/스크롤 위치를 설정한다.
    /// </summary>
    public void BindItem(VisualElement element, int index)
    {
        // 게임 데이터 지연 로딩
        if (_gameData == null)
            _gameData = StartBroker.GetGameData();
        if (_gameData == null)
        {
            Debug.LogError("GameData is null");
            return;
        }

        // 리스트/인덱스 유효성
        if (draggableLV == null || draggableLV.items == null || index < 0 || index >= draggableLV.items.Count)
        {
            Debug.LogError("draggableLV not ready");
            return;
        }

        // 데이터 캐스팅
        IListViewItem item = draggableLV.items[index];
        StageInfo stageInfo = item as StageInfo;
        if (stageInfo == null)
        {
            Debug.LogError("StageInfo cast failed");
            return;
        }

        int stageNum = stageInfo.stageNum;

        // 템플릿 내 UI 요소 조회 + 캐시 구성
        var cache = new ItemCache
        {
            infoButton = element.Q<Button>("InfoButton"),
            moveButton = element.Q<Button>("MoveButton"),
            stageLabel = element.Q<Label>("StageLabel"),
            titleLabel = element.Q<Label>("TitleLabel"),
            infoLabel = element.Q<Label>("InfoLabel"),
            lockGroup = element.Q<VisualElement>("LockGroup"),
            selectBorder = element.Q<VisualElement>("SelectBorder"),

            stageInfo = stageInfo,
            stageNum = stageNum,
            index = index
        };
        element.userData = cache; // 이후 콜백에서 element.userData로 접근

        // 제목/해금 상태 바인딩
        cache.titleLabel.text = stageInfo.stageName;
        BindOpenState(cache, stageInfo);

        // 버튼 콜백 중복 등록 방지 후 재등록
        cache.moveButton?.UnregisterCallback<ClickEvent>(OnMoveButtonClick);
        if (cache.moveButton != null)
        {
            cache.moveButton.userData = stageNum; // 클릭 시 어떤 스테이지로 이동할지
            cache.moveButton.RegisterCallback<ClickEvent>(OnMoveButtonClick);
        }

        cache.infoButton?.UnregisterCallback<ClickEvent>(OnInfoButtonClick);
        if (cache.infoButton != null)
        {
            cache.infoButton.userData = stageNum; // 클릭 시 어떤 스테이지의 상세를 열지
            cache.infoButton.RegisterCallback<ClickEvent>(OnInfoButtonClick);
        }

        // 요소 전체 클릭으로도 선택되도록 처리
        element.UnregisterCallback<ClickEvent>(OnElementClick);
        element.RegisterCallback<ClickEvent>(OnElementClick);

        // 현재 스테이지 선택 표시
        bool isSelected = _gameData.currentStageNum == stageNum;
        SetSelected(cache.selectBorder, isSelected);

        // 최초 바인딩 시 현재 선택된 항목을 스크롤 인덱스로 맞춰준다
        if (isSelected)
        {
            selectedElement = element;
            draggableLV?.ScrollToIndex(cache.index);
        }
    }

    /// <summary>
    /// 리스트 아이템 자신을 클릭했을 때 (드래그가 아닌 경우) 선택 처리
    /// </summary>
    private void OnElementClick(ClickEvent evt)
    {
        // 스크롤 드래그 중이거나 직후면 클릭 무시(드래그-클릭 충돌 방지)
        if (draggableLV != null && draggableLV.ShouldBlockClick())
        {
            evt.StopImmediatePropagation();
            return;
        }

        var element = evt.currentTarget as VisualElement;
        if (element == null) return;

        var cache = element.userData as ItemCache;
        if (cache == null) return;

        // 미해금(잠김) 상태는 선택 불가
        if (!cache.isOpen) return;

        // 이전 선택 항목의 테두리 숨김
        if (selectedElement != null && selectedElement != element)
        {
            var prev = selectedElement.userData as ItemCache;
            if (prev?.selectBorder != null)
                prev.selectBorder.style.display = DisplayStyle.None;
        }

        // 현재 항목 선택 + 상태 갱신
        SetSelected(cache.selectBorder, true);
        selectedElement = element;
        _gameData.currentStageNum = cache.stageNum;

        // 선택 항목으로 스크롤 정렬
        draggableLV?.ScrollToIndex(cache.index);
    }

    /// <summary>
    /// 스테이지 해금 상태에 따라 UI 표시 제어
    /// - 해금: 레이블/버튼/정보 표시
    /// - 잠김: LockGroup만 표시
    /// </summary>
    private void BindOpenState(ItemCache cache, StageInfo stageInfo)
    {
        int stageNum = stageInfo.stageNum;
        bool isOpen = _gameData.maxStageNum >= stageNum;
        cache.isOpen = isOpen;

        if (isOpen)
        {
            SetVisible(cache.stageLabel, true);
            SetVisible(cache.infoButton, true);
            SetVisible(cache.infoLabel, true);
            SetVisible(cache.moveButton, true);
            SetVisible(cache.lockGroup, false);

            cache.stageLabel.text = $"STAGE {stageNum}";
            cache.infoLabel.text = stageInfo.GetDropInfo(); // 드롭 개요(설명) 표시
        }
        else
        {
            SetVisible(cache.stageLabel, false);
            SetVisible(cache.infoButton, false);
            SetVisible(cache.infoLabel, false);
            SetVisible(cache.moveButton, false);
            SetVisible(cache.lockGroup, true);
        }
    }

    /// <summary>
    /// 선택 테두리 표시/숨김
    /// </summary>
    private void SetSelected(VisualElement selectBorder, bool selected)
    {
        if (selectBorder == null) return;
        selectBorder.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// 공통 가시성 토글
    /// </summary>
    private void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// Move 버튼: 해당 스테이지로 이동 트리거
    /// - currentStageNum 갱신 → 전투 변경 → 세이브 → 현재 UI 닫기
    /// </summary>
    private void OnMoveButtonClick(ClickEvent evt)
    {
        var button = evt.currentTarget as Button;
        if (button?.userData is int stageNum)
        {
            _gameData.currentStageNum = stageNum;
            BattleBroker.OnStageChange();
            NetworkBroker.SaveServerData();
            UIBroker.InactiveCurrentUI?.Invoke();
        }
    }

    /// <summary>
    /// Info 버튼: 해당 스테이지 상세 정보 UI 열기
    /// </summary>
    private void OnInfoButtonClick(ClickEvent evt)
    {
        var button = evt.currentTarget as Button;
        if (button?.userData is int stageNum)
            BattleBroker.ActiveStageInfoUI(stageNum);
    }
}
