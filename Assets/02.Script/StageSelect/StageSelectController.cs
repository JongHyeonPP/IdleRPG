using UnityEngine.UIElements;
using UnityEngine;
using System;

public class StageSelectController : MonoBehaviour, LVItemController
{
    private GameData _gameData;

    public FlexibleListView draggableLV { get; set; }

    // ���� ���õ� ������ element
    private VisualElement selectedElement;

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

        // ���� �� ���� ������
        public StageInfo stageInfo;
        public int stageNum;
        public bool isOpen;
        public int index;
    }

    // �������̽� ����
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

        // ��ư �ݹ� ����
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

        // ��� Ŭ�����ε� ���õǰ� ó��
        element.UnregisterCallback<ClickEvent>(OnElementClick);
        element.RegisterCallback<ClickEvent>(OnElementClick);

        // ���� ǥ��
        bool isSelected = _gameData.currentStageNum == stageNum;
        SetSelected(cache.selectBorder, isSelected);

        // ó�� ���ε� �� ���� ���õ� ���������� ����ϰ� ��ũ�ѵ� ����
        if (isSelected)
        {
            selectedElement = element;
            // ���̾ƿ� �� ��ũ��
            draggableLV?.ScrollToIndex(cache.index);
        }
    }

    private void OnElementClick(ClickEvent evt)
    {
        // ��ũ�� �巡�� ���̰ų� ��� ���� ��� Ŭ�� ����
        if (draggableLV != null && draggableLV.ShouldBlockClick())
        {
            evt.StopImmediatePropagation();
            return;
        }

        var element = evt.currentTarget as VisualElement;
        if (element == null) return;

        var cache = element.userData as ItemCache;
        if (cache == null) return;

        // ��� ������ ���� �Ұ�
        if (!cache.isOpen) return;

        // ���� ���� border ����
        if (selectedElement != null && selectedElement != element)
        {
            var prev = selectedElement.userData as ItemCache;
            if (prev?.selectBorder != null)
                prev.selectBorder.style.display = DisplayStyle.None;
        }

        // �̹� �׸� ����
        SetSelected(cache.selectBorder, true);
        selectedElement = element;

        // ���� ���� �������� ����
        _gameData.currentStageNum = cache.stageNum;

        // ���� ���������� ��ũ��
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
