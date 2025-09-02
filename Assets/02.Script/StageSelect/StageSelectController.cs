using UnityEngine.UIElements;
using UnityEngine;
using System;

public class StageSelectController : MonoBehaviour, LVItemController
{
    private GameData _gameData;

    public FlexibleListView draggableLV { get; set; }

    // 현재 선택된 슬롯의 element
    private VisualElement selectedElement;

    // 아이템별 UI 참조 캐시
    private class ItemCache
    {
        public Button infoButton;
        public Button moveButton;
        public Label stageLabel;
        public Label titleLabel;
        public Label infoLabel;
        public VisualElement lockGroup;
        public VisualElement selectBorder;

        // 선택 및 상태 판정용
        public StageInfo stageInfo;
        public int stageNum;
        public bool isOpen;
        public int index;
    }

    // 인터페이스 구현
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
            Debug.LogError("StageInfo cast failed");
            return;
        }

        int stageNum = stageInfo.stageNum;

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
        element.userData = cache;

        cache.titleLabel.text = stageInfo.stageName;
        BindOpenState(cache, stageInfo);

        // 버튼 콜백 갱신
        cache.moveButton?.UnregisterCallback<ClickEvent>(OnMoveButtonClick);
        if (cache.moveButton != null)
        {
            cache.moveButton.userData = stageNum;
            cache.moveButton.RegisterCallback<ClickEvent>(OnMoveButtonClick);
        }

        cache.infoButton?.UnregisterCallback<ClickEvent>(OnInfoButtonClick);
        if (cache.infoButton != null)
        {
            cache.infoButton.userData = stageNum;
            cache.infoButton.RegisterCallback<ClickEvent>(OnInfoButtonClick);
        }

        // 요소 클릭으로도 선택되게 처리
        element.UnregisterCallback<ClickEvent>(OnElementClick);
        element.RegisterCallback<ClickEvent>(OnElementClick);

        // 선택 표시
        bool isSelected = _gameData.currentStageNum == stageNum;
        SetSelected(cache.selectBorder, isSelected);

        // 처음 바인딩 시 현재 선택된 스테이지를 기억하고 스크롤도 맞춤
        if (isSelected)
        {
            selectedElement = element;
            // 레이아웃 후 스크롤
            draggableLV?.ScrollToIndex(cache.index);
        }
    }

    private void OnElementClick(ClickEvent evt)
    {
        // 스크롤 드래그 중이거나 방금 끝난 경우 클릭 무시
        if (draggableLV != null && draggableLV.ShouldBlockClick())
        {
            evt.StopImmediatePropagation();
            return;
        }

        var element = evt.currentTarget as VisualElement;
        if (element == null) return;

        var cache = element.userData as ItemCache;
        if (cache == null) return;

        // 잠겨 있으면 선택 불가
        if (!cache.isOpen) return;

        // 이전 선택 border 끄기
        if (selectedElement != null && selectedElement != element)
        {
            var prev = selectedElement.userData as ItemCache;
            if (prev?.selectBorder != null)
                prev.selectBorder.style.display = DisplayStyle.None;
        }

        // 이번 항목 선택
        SetSelected(cache.selectBorder, true);
        selectedElement = element;

        // 현재 선택 스테이지 갱신
        _gameData.currentStageNum = cache.stageNum;

        // 선택 아이템으로 스크롤
        draggableLV?.ScrollToIndex(cache.index);
    }

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
            cache.infoLabel.text = stageInfo.GetDropInfo();
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

    private void SetSelected(VisualElement selectBorder, bool selected)
    {
        if (selectBorder == null) return;
        selectBorder.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

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

    private void OnInfoButtonClick(ClickEvent evt)
    {
        var button = evt.currentTarget as Button;
        if (button?.userData is int stageNum)
            BattleBroker.ActiveStageInfoUI(stageNum);
    }
}
