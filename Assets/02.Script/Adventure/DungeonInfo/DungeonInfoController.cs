using UnityEngine.UIElements;
using UnityEngine;
using System;

/// <summary>
/// 던전 스테이지 리스트(FlexibleListView) 아이템을 바인딩/선택하는 컨트롤러.
/// - 슬롯의 잠금/해금 상태 표시
/// - 자동 선택(첫 오픈 슬롯) 및 사용자 선택 처리
/// - 선택 변경 시 DungeonInfoUI에 콜백 전달
/// </summary>
public class DungeonInfoController : MonoBehaviour, LVItemController
{
    private GameData _gameData;

    // FlexibleListView에서 주입되는 리스트뷰 참조
    public FlexibleListView draggableLV { get; set; }

    // 현재 선택된 슬롯의 루트 VisualElement (선택 테두리 토글용)
    public VisualElement selectedElement { get; private set; }

    // 사용자가 직접 선택했는지 여부(자동 선택과 구분)
    private bool _userSelected;

    // 자동 선택으로 잡아둔 스테이지 인덱스(챕터 내 인덱스). -1은 선택 전
    private int _autoSelectedStageIdx = -1;

    // 선택 상태/설명 UI 갱신을 위한 상위 UI 컨트롤러
    private DungeonInfoUI _dungeonInfoUI;

    private void Start()
    {
        _dungeonInfoUI = GetComponent<DungeonInfoUI>();
        _userSelected = false;
        _autoSelectedStageIdx = -1;
        selectedElement = null;
    }

    /// <summary>
    /// 현재 선택된 StageInfo (없으면 null)
    /// </summary>
    public StageInfo SelectedStageInfo
    {
        get
        {
            if (selectedElement == null) return null;
            var cache = selectedElement.userData as ItemCache;
            return cache?.stageInfo;
        }
    }

    /// <summary>
    /// 슬롯 요소에 캐싱해둘 UI/데이터 묶음
    /// </summary>
    private class ItemCache
    {
        public Label stageLabel;
        public Label titleLabel;
        public VisualElement lockGroup;
        public VisualElement selectBorder;

        public StageInfo stageInfo;   // 데이터 원본

        public bool isOpen;           // 해금 여부
        public int stageIdx;          // 챕터 내 스테이지 인덱스(0부터)
        public int chapterIdx;        // 챕터 인덱스
        public int index;             // 리스트 내 인덱스(스크롤 정렬용)
    }

    /// <summary>
    /// LVItemController 구현: 리스트 아이템 바인딩
    /// </summary>
    public void BindItem(VisualElement element, int index)
    {
        // GameData 지연 로드
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
            Debug.LogError("Cast failed");
            return;
        }

        int chapterIdx = stageInfo.adventrueInfo.adventureIndex_0;
        int stageIdx = stageInfo.adventrueInfo.adventureIndex_1;

        // 템플릿 내 UI 요소 캐싱
        ItemCache cache = new ItemCache
        {
            stageLabel = element.Q<Label>("StageLabel"),
            titleLabel = element.Q<Label>("TitleLabel"),
            lockGroup = element.Q<VisualElement>("LockGroup"),
            selectBorder = element.Q<VisualElement>("SelectBorder"),

            stageInfo = stageInfo,
            stageIdx = stageIdx,
            chapterIdx = chapterIdx,
            index = index
        };
        element.userData = cache;

        // 타이틀
        if (cache.titleLabel != null)
            cache.titleLabel.text = stageInfo.stageName;

        // 잠금/해금 상태 UI 반영
        BindOpenState(cache, stageInfo);

        // 클릭 콜백 (중복 등록 방지 후 재등록)
        element.UnregisterCallback<ClickEvent>(OnElementClick);
        element.RegisterCallback<ClickEvent>(OnElementClick);

        // 이미 선택된 항목인지 여부
        bool isSelected = selectedElement != null
                          && ReferenceEquals((selectedElement.userData as ItemCache)?.stageInfo, stageInfo);

        // 자동 선택: 아직 사용자가 선택하지 않았고, 이 슬롯이 해금 상태라면
        // 첫 번째 해금 슬롯 하나만 선택 처리
        if (!_userSelected && cache.isOpen)
        {
            if (_autoSelectedStageIdx == -1)
            {
                // 이전 선택 해제
                if (selectedElement != null && selectedElement != element)
                {
                    var prev = selectedElement.userData as ItemCache;
                    if (prev?.selectBorder != null)
                        prev.selectBorder.style.display = DisplayStyle.None;
                }

                selectedElement = element;
                _autoSelectedStageIdx = cache.stageIdx;
                isSelected = true;

                // 시작 버튼 영역 등 활성화
                _dungeonInfoUI?.SetState(true);

                // 자동 선택된 항목으로 스크롤 정렬
                draggableLV?.ScrollToIndex(cache.index);

                // 자동 선택이라도 상세패널 갱신은 동일하게 호출
                _dungeonInfoUI?.OnClickedSlot(cache.stageInfo);
            }
        }

        // 선택 테두리 표시
        if (cache.selectBorder != null)
            cache.selectBorder.style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;

        if (isSelected)
            selectedElement = element;
    }

    /// <summary>
    /// 슬롯 클릭 시 호출: 선택 토글 + 상세 패널 갱신
    /// </summary>
    private void OnElementClick(ClickEvent evt)
    {
        // 드래그 직후 클릭 오작동 방지
        if (draggableLV != null && draggableLV.ShouldBlockClick())
        {
            evt.StopImmediatePropagation();
            return;
        }

        var element = evt.currentTarget as VisualElement;
        if (element == null) return;

        var cache = element.userData as ItemCache;
        if (cache == null) return;

        // 이전 선택 해제
        if (selectedElement != null && selectedElement != element)
        {
            var prev = selectedElement.userData as ItemCache;
            if (prev?.selectBorder != null)
                prev.selectBorder.style.display = DisplayStyle.None;
        }

        // 현재 선택 보더 표시
        if (cache.selectBorder != null)
            cache.selectBorder.style.display = DisplayStyle.Flex;

        selectedElement = element;
        _userSelected = true; // 이후로는 자동 선택 금지

        // 입장 가능/불가 UI
        _dungeonInfoUI?.SetState(cache.isOpen);

        // 선택 항목으로 스크롤 정렬
        draggableLV?.ScrollToIndex(cache.index);

        // 상세 패널 갱신
        _dungeonInfoUI?.OnClickedSlot(cache.stageInfo);
    }

    /// <summary>
    /// 해금 여부에 따라 UI 표시 전환
    /// </summary>
    private void BindOpenState(ItemCache cache, StageInfo stageInfo)
    {
        // 규칙: dungeonProgress[챕터] >= 스테이지 인덱스 → 해금
        bool isOpen = _gameData.dungeonProgress[cache.chapterIdx] >= cache.stageIdx;
        cache.isOpen = isOpen;

        if (isOpen)
        {
            SetVisible(cache.stageLabel, true);
            SetVisible(cache.lockGroup, false);
            SetVisible(cache.titleLabel, true);

            if (cache.stageLabel != null)
                cache.stageLabel.text = $"STAGE {cache.stageIdx + 1}";
        }
        else
        {
            SetVisible(cache.stageLabel, false);
            SetVisible(cache.lockGroup, true);
            SetVisible(cache.titleLabel, false);
        }
    }

    /// <summary>
    /// 공통 표시 토글
    /// </summary>
    private void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// 외부에서 리스트를 다시 채우기 전에 선택 상태를 초기화할 때 사용
    /// </summary>
    public void ResetSelection()
    {
        _userSelected = false;
        _autoSelectedStageIdx = -1;
        selectedElement = null;
    }
}
