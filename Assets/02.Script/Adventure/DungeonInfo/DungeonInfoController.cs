using UnityEngine.UIElements;
using UnityEngine;
using System;

public class DungeonInfoController : MonoBehaviour, LVItemController
{
    private GameData _gameData;

    public FlexibleListView draggableLV { get; set; }

    // 현재 선택된 슬롯의 element를 보관
    public VisualElement selectedElement { get; private set; }

    // 외부 버튼에서 바로 쓰기 편하도록 제공
    public StageInfo SelectedStageInfo
    {
        get
        {
            if (selectedElement == null) return null;
            var cache = selectedElement.userData as ItemCache;
            return cache?.stageInfo;
        }
    }

    // 인터페이스 구현
    private class ItemCache
    {
        //public Button infoButton;
        //public Button moveButton;
        public Label stageLabel;
        public Label titleLabel;
        //public Label infoLabel;
        public VisualElement lockGroup;
        public VisualElement selectBorder;

        // element에서 바로 item을 얻기 위해 저장
        public StageInfo stageInfo;
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
            Debug.LogError("Cast failed");
            return;
        }

        ItemCache cache = new ItemCache
        {
            //infoButton = element.Q<Button>("InfoButton"),
            //moveButton = element.Q<Button>("MoveButton"),
            stageLabel = element.Q<Label>("StageLabel"),
            titleLabel = element.Q<Label>("TitleLabel"),
            //infoLabel = element.Q<Label>("InfoLabel"),
            lockGroup = element.Q<VisualElement>("LockGroup"),
            selectBorder = element.Q<VisualElement>("SelectBorder"),
            stageInfo = stageInfo
        };
        element.userData = cache;

        //cache.titleLabel.text = stageInfo.stageName;
        BindOpenState(cache, stageInfo);
        //SetSelected(cache.selectBorder, _gameData.currentStageNum == stageInfo.stageNum);

        //cache.moveButton?.UnregisterCallback<ClickEvent>(OnMoveButtonClick);
        //if (cache.moveButton != null)
        //{
        //    cache.moveButton.userData = stageInfo.stageNum;
        //    cache.moveButton.RegisterCallback<ClickEvent>(OnMoveButtonClick);
        //}

        //cache.infoButton?.UnregisterCallback<ClickEvent>(OnInfoButtonClick);
        //if (cache.infoButton != null)
        //{
        //    cache.infoButton.userData = stageInfo.stageNum;
        //    cache.infoButton.RegisterCallback<ClickEvent>(OnInfoButtonClick);
        //}

        element.UnregisterCallback<ClickEvent>(OnElementClick);
        element.RegisterCallback<ClickEvent>(OnElementClick);

        // 리스트 재바인딩 시 선택 유지
        // selectedElement가 있고, 그 element가 가진 StageInfo와 동일 레퍼런스면 border를 켠다
        if (cache.selectBorder != null)
        {
            bool isSelected = selectedElement != null
                              && ReferenceEquals((selectedElement.userData as ItemCache)?.stageInfo, stageInfo);

            cache.selectBorder.style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;

            // pool 재사용으로 같은 아이템이 다시 그려질 때 selectedElement를 갱신
            if (isSelected)
                selectedElement = element;
        }
    }

    private void OnElementClick(ClickEvent evt)
    {
        var element = evt.currentTarget as VisualElement;
        if (element == null) return;

        var cache = element.userData as ItemCache;
        if (cache == null) return;

        // 이전 선택 border 끄기
        if (selectedElement != null && selectedElement != element)
        {
            var prev = selectedElement.userData as ItemCache;
            if (prev?.selectBorder != null)
                prev.selectBorder.style.display = DisplayStyle.None;
        }

        // 이번 항목 border 켜기
        if (cache.selectBorder != null)
            cache.selectBorder.style.display = DisplayStyle.Flex;

        // 선택 element 보관
        selectedElement = element;

        // 필요하다면 선택 변경 알림 이벤트 발행 지점
        // UIBroker.DungeonStageSelected?.Invoke(cache.stageInfo.stageNum);
    }

    private void BindOpenState(ItemCache cache, StageInfo stageInfo)
    {
        //bool isOpen = _gameData.maxStageNum >= stageNum;

        if (true)
        {
            SetVisible(cache.stageLabel, true);
            //SetVisible(cache.infoButton, true);
            //SetVisible(cache.infoLabel, true);
            //SetVisible(cache.moveButton, true);
            SetVisible(cache.lockGroup, false);

            cache.stageLabel.text = $"STAGE {stageInfo.adventrueInfo.adventureIndex_1 + 1}";
            //cache.infoLabel.text = stageInfo.GetDropInfo();
        }
        else
        {
            SetVisible(cache.stageLabel, false);
            //SetVisible(cache.infoButton, false);
            //SetVisible(cache.infoLabel, false);
            //SetVisible(cache.moveButton, false);
            SetVisible(cache.lockGroup, true);
        }
    }

    //private void SetSelected(VisualElement selectBorder, bool selected)
    //{
    //    if (selectBorder == null) return;
    //    selectBorder.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
    //}

    private void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    //private void OnMoveButtonClick(ClickEvent evt)
    //{
    //    var button = evt.currentTarget as Button;
    //    if (button?.userData is int stageNum)
    //    {
    //        _gameData.currentStageNum = stageNum;
    //        Debug.Log("Move To Stage " + stageNum);
    //        BattleBroker.OnStageChange();
    //        NetworkBroker.SaveServerData();
    //        UIBroker.InactiveCurrentUI?.Invoke();
    //    }
    //}

    //private void OnInfoButtonClick(ClickEvent evt)
    //{
    //    var button = evt.currentTarget as Button;
    //    if (button?.userData is int stageNum)
    //        BattleBroker.ActiveStageInfoUI(stageNum);
    //}
}
