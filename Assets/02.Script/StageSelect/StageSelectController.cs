using UnityEngine.UIElements;
using UnityEngine;
using System;
public class StageSelectController : LVItemController
{
    private GameData _gameData;
    //������ �������� ui ������ �����Ѵ�.
    public override void BindItem(VisualElement element, int index)
    {
        _gameData = StartBroker.GetGameData();
        IListViewItem item = draggableLV.items[index];
        StageInfo stageInfo = item as StageInfo;
        int stageNum = stageInfo.stageNum;
        //VisualElement ��������
        VisualElement bottomImage = element.Q<VisualElement>("BottomImage");
        Label stageLabel = element.Q<Label>("StageLabel");
        Label titleLabel = element.Q<Label>("TitleLabel");
        Label infoLabel = element.Q<Label>("InfoLabel");
        VisualElement lockGroup = element.Q<VisualElement>("LockGroup");
        VisualElement selectBorder = element.Q<VisualElement>("SelectBorder");
        Button moveButton = element.Q<Button>("MoveButton");
        //VisualElement ����
        titleLabel.text = stageInfo.stageName;
        if (_gameData.maxStageNum >= stageNum)//���µ� �����������
        {
            stageLabel.style.display =bottomImage.style.display =infoLabel.style.display = moveButton.style.display = DisplayStyle.Flex;
            lockGroup.style.display = DisplayStyle.None;
            stageLabel.text = $"STAGE {stageInfo.stageNum}";
            infoLabel.text = stageInfo.GetDropInfo();
        }
        else//���� �����������
        {
            stageLabel.style.display =bottomImage.style.display =infoLabel.style.display = moveButton.style.display = DisplayStyle.None;
            lockGroup.style.display = DisplayStyle.Flex;
        }
        if (_gameData.currentStageNum == stageNum)
        {
            selectBorder.style.display = DisplayStyle.Flex;
        }
        else
        {
            selectBorder.style.display = DisplayStyle.None;
        }
        // ���� �̺�Ʈ ����
        moveButton.UnregisterCallback<ClickEvent>(OnMoveButtonClick);

        // ��ư�� �ε����� userData�� ����

        moveButton.userData = stageInfo.stageNum;

        // Ŭ�� �̺�Ʈ ���
        moveButton.RegisterCallback<ClickEvent>(OnMoveButtonClick);
    }
    // ��ư Ŭ�� �̺�Ʈ �ڵ鷯
    private void OnMoveButtonClick(ClickEvent evt)
    {
        // Ŭ���� ��ư�� userData�� ���� �ε��� �������� 
        
        var button = evt.target as Button;
        if (button?.userData is int index)
        {
            _gameData.currentStageNum = index;
            Debug.Log("Move To Stage" + index);
            BattleBroker.OnStageChange();
            NetworkBroker.SaveServerData();
            UIBroker.InactiveCurrentUI?.Invoke();
        }
    }
}
