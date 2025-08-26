using UnityEngine.UIElements;
using UnityEngine;
using System;

public class StageSelectController : MonoBehaviour, LVItemController
{
    private GameData _gameData;

    public FlexibleListView draggableLV { get; set; }

    // 인터페이스 구현
    private class ItemCache
    {
        public Button infoButton;
        public Button moveButton;
        public Label stageLabel;
        public Label titleLabel;
        public Label infoLabel;
        public VisualElement lockGroup;
        public VisualElement selectBorder;
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

            var cache = new ItemCache
            {
                infoButton = element.Q<Button>("InfoButton"),
                moveButton = element.Q<Button>("MoveButton"),
                stageLabel = element.Q<Label>("StageLabel"),
                titleLabel = element.Q<Label>("TitleLabel"),
                infoLabel = element.Q<Label>("InfoLabel"),
                lockGroup = element.Q<VisualElement>("LockGroup"),
                selectBorder = element.Q<VisualElement>("SelectBorder")
            };
            element.userData = cache;
        

        cache.titleLabel.text = stageInfo.stageName;
        BindOpenState(cache, stageInfo);
        SetSelected(cache.selectBorder, _gameData.currentStageNum == stageInfo.stageNum);

        cache.moveButton?.UnregisterCallback<ClickEvent>(OnMoveButtonClick);
        if (cache.moveButton != null)
        {
            cache.moveButton.userData = stageInfo.stageNum;
            cache.moveButton.RegisterCallback<ClickEvent>(OnMoveButtonClick);
        }

        cache.infoButton?.UnregisterCallback<ClickEvent>(OnInfoButtonClick);
        if (cache.infoButton != null)
        {
            cache.infoButton.userData = stageInfo.stageNum;
            cache.infoButton.RegisterCallback<ClickEvent>(OnInfoButtonClick);
        }
    }

    private void BindOpenState(ItemCache cache, StageInfo stageInfo)
    {
        int stageNum = stageInfo.stageNum;
        bool isOpen = _gameData.maxStageNum >= stageNum;

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
            Debug.Log("Move To Stage " + stageNum);
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
