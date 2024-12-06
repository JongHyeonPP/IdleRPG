using UnityEngine.UIElements;
using UnityEngine;
using System;
public class StageSelectController : LVItemController
{
    //������ �������� ui ������ �����Ѵ�.
    public override void BindItem(VisualElement element, int index)
    {
        IListViewItem item = draggableLV.items[index];
        StageInfo stageInfo = item as StageInfo;
        int stageNum = stageInfo.stageNum;
        //VisualElement ��������
        Label stageLabel = element.Q<Label>("StageLabel");
        Label titleLabel = element.Q<Label>("TitleLabel");
        Label infoLabel = element.Q<Label>("InfoLabel");
        VisualElement lockGroup = element.Q<VisualElement>("LockGroup");
        Button moveButton = element.Q<Button>("MoveButton");
        //VisualElement ����
        titleLabel.text = stageInfo.stageName;
        if (GameManager.instance.gameData.maxStageNum >= stageNum)//���µ� �����������
        {
            stageLabel.style.display = infoLabel.style.display = moveButton.style.display = DisplayStyle.Flex;
            lockGroup.style.display = DisplayStyle.None;
            stageLabel.text = $"STAGE {stageInfo.stageNum}";
            infoLabel.text = stageInfo.GetDropInfo();
        }
        else//���� �����������
        {
            stageLabel.style.display = infoLabel.style.display = moveButton.style.display = DisplayStyle.None;
            lockGroup.style.display = DisplayStyle.Flex;
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
            Debug.Log("Move To Stage" + index);
            BattleBroker.OnStageChange(index);
        }
    }
    public override ILVItem GetLVItem()
    {
        return new StageSelectItem();
    }
}
