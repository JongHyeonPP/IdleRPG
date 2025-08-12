using UnityEngine.UIElements;
using UnityEngine;
using System;

public class StageSelectController : LVItemController
{
    private GameData _gameData;

    // �����ۺ� UI ���� ĳ��
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

    public override void BindItem(VisualElement element, int index)
    {
        if (_gameData == null)
            _gameData = StartBroker.GetGameData();

        if (_gameData == null)
        {
            Debug.LogError("GameData is null.");
            return;
        }

        IListViewItem item = draggableLV.items[index];
        StageInfo stageInfo = item as StageInfo;
        if (stageInfo == null)
        {
            Debug.LogError("StageInfo cast failed.");
            return;
        }

        // ĳ�� �غ�
        ItemCache cache = element.userData as ItemCache;
        if (cache == null)
        {
            cache = new ItemCache
            {
                infoButton = element.Q<Button>("InfoButton"),
                moveButton = element.Q<Button>("MoveButton"),
                stageLabel = element.Q<Label>("StageLabel"),
                titleLabel = element.Q<Label>("TitleLabel"),
                infoLabel = element.Q<Label>("InfoLabel"),
                lockGroup = element.Q<VisualElement>("LockGroup"),
                selectBorder = element.Q<VisualElement>("SelectBorder"),
            };
            element.userData = cache;
        }

        // �ؽ�Ʈ ���ε�
        cache.titleLabel.text = stageInfo.stageName;

        // ����/�� ���� ���ε�
        BindOpenState(cache, stageInfo);

        // ���� ǥ��
        SetSelected(cache.selectBorder, _gameData.currentStageNum == stageInfo.stageNum);

        // �̺�Ʈ ����(����ε� ���: ���� ���� �� ���)
        if (cache.moveButton != null)
        {
            cache.moveButton.UnregisterCallback<ClickEvent>(OnMoveButtonClick);
            cache.moveButton.userData = stageInfo.stageNum;
            cache.moveButton.RegisterCallback<ClickEvent>(OnMoveButtonClick);
        }

        if (cache.infoButton != null)
        {
            cache.infoButton.UnregisterCallback<ClickEvent>(OnInfoButtonClick);
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

    // ��ư Ŭ�� �̺�Ʈ �ڵ鷯
    private void OnMoveButtonClick(ClickEvent evt)
    {
        var button = evt.currentTarget as Button; // ��ϵ� ��ư �ڽ�
        if (button?.userData is int stageNum)
        {
            _gameData.currentStageNum = stageNum;
            Debug.Log("Move To Stage : " + stageNum);
            BattleBroker.OnStageChange();
            NetworkBroker.SaveServerData();
            UIBroker.InactiveCurrentUI?.Invoke();
        }
    }

    private void OnInfoButtonClick(ClickEvent evt)
    {
        var button = evt.currentTarget as Button;
        if (button?.userData is int stageNum)
        {
            BattleBroker.ActiveStageInfoUI(stageNum);
        }
    }
}
