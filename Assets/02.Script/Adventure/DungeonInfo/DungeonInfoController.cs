using UnityEngine.UIElements;
using UnityEngine;
using System;

public class DungeonInfoController : MonoBehaviour, LVItemController
{
    private GameData _gameData;

    public FlexibleListView draggableLV { get; set; }

    public VisualElement selectedElement { get; private set; }

    private bool _userSelected;
    private int _autoSelectedStageIdx = -1;

    private DungeonInfoUI _dungeonInfoUI;

    private void Start()
    {
        _dungeonInfoUI = GetComponent<DungeonInfoUI>();
        _userSelected = false;
        _autoSelectedStageIdx = -1;
        selectedElement = null;
    }

    public StageInfo SelectedStageInfo
    {
        get
        {
            if (selectedElement == null) return null;
            var cache = selectedElement.userData as ItemCache;
            return cache?.stageInfo;
        }
    }

    private class ItemCache
    {
        public Label stageLabel;
        public Label titleLabel;
        public VisualElement lockGroup;
        public VisualElement selectBorder;

        public StageInfo stageInfo;

        public bool isOpen;
        public int stageIdx;
        public int chapterIdx;
        public int index;
    }

    public void BindItem(VisualElement element, int index)
    {
        if (_gameData == null)
            _gameData = StartBroker.GetGameData();
        if (_gameData == null)
        {
            Debug.LogError("GameData is null");
            return;
        }
        if (draggableLV == null || draggableLV.items == null || index < 0 || index >= draggableLV.items.Count)
        {
            Debug.LogError("draggableLV not ready");
            return;
        }

        IListViewItem item = draggableLV.items[index];
        StageInfo stageInfo = item as StageInfo;
        if (stageInfo == null)
        {
            Debug.LogError("Cast failed");
            return;
        }

        int chapterIdx = stageInfo.adventrueInfo.adventureIndex_0;
        int stageIdx = stageInfo.adventrueInfo.adventureIndex_1;

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

        if (cache.titleLabel != null)
            cache.titleLabel.text = stageInfo.stageName;

        BindOpenState(cache, stageInfo);

        element.UnregisterCallback<ClickEvent>(OnElementClick);
        element.RegisterCallback<ClickEvent>(OnElementClick);

        bool isSelected = selectedElement != null
                          && ReferenceEquals((selectedElement.userData as ItemCache)?.stageInfo, stageInfo);

        if (!_userSelected && cache.isOpen)
        {
            if (cache.stageIdx > _autoSelectedStageIdx)
            {
                if (selectedElement != null && selectedElement != element)
                {
                    var prev = selectedElement.userData as ItemCache;
                    if (prev?.selectBorder != null)
                        prev.selectBorder.style.display = DisplayStyle.None;
                }

                selectedElement = element;
                _autoSelectedStageIdx = cache.stageIdx;
                isSelected = true;

                _dungeonInfoUI?.SetState(true);

                draggableLV?.ScrollToIndex(cache.index);

                // 자동 선택 시에도 클릭 콜백 통지
                _dungeonInfoUI?.OnClickedSlot(cache.stageInfo);
            }
        }

        if (cache.selectBorder != null)
            cache.selectBorder.style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;

        if (isSelected)
            selectedElement = element;
    }

    private void OnElementClick(ClickEvent evt)
    {
        if (draggableLV != null && draggableLV.ShouldBlockClick())
        {
            evt.StopImmediatePropagation();
            return;
        }

        var element = evt.currentTarget as VisualElement;
        if (element == null) return;

        var cache = element.userData as ItemCache;
        if (cache == null) return;

        if (selectedElement != null && selectedElement != element)
        {
            var prev = selectedElement.userData as ItemCache;
            if (prev?.selectBorder != null)
                prev.selectBorder.style.display = DisplayStyle.None;
        }

        if (cache.selectBorder != null)
            cache.selectBorder.style.display = DisplayStyle.Flex;

        selectedElement = element;
        _userSelected = true;

        _dungeonInfoUI?.SetState(cache.isOpen);

        draggableLV?.ScrollToIndex(cache.index);

        // 유저가 선택했을 때 클릭 콜백 통지
        _dungeonInfoUI?.OnClickedSlot(cache.stageInfo);
    }

    private void BindOpenState(ItemCache cache, StageInfo stageInfo)
    {
        bool isOpen = _gameData.dungeonProgress[cache.chapterIdx] >= cache.stageIdx;
        cache.isOpen = isOpen;

        if (isOpen)
        {
            SetVisible(cache.stageLabel, true);
            SetVisible(cache.lockGroup, false);

            if (cache.stageLabel != null)
                cache.stageLabel.text = $"STAGE {cache.stageIdx + 1}";
        }
        else
        {
            SetVisible(cache.stageLabel, false);
            SetVisible(cache.lockGroup, true);
        }
    }

    private void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // 선택 초기화가 필요하면 외부에서 호출
    public void ResetSelection()
    {
        _userSelected = false;
        _autoSelectedStageIdx = -1;
        selectedElement = null;
    }
}
