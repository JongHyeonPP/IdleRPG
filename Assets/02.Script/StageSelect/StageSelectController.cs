using UnityEngine.UIElements;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "StageSelectController", menuName = "ScriptableObjects/ListView/StageSelectController")]
public class StageSelectController : LVItemController
{
    public override void BindItem(VisualElement element, int index)
    {
        IListViewItem item = draggableLV.items[index];
        // �������� ���� ����
        StageInfo stageInfo = item as StageInfo;
        element.Q<Label>("StageLabel").text = $"STAGE {stageInfo.stageNum}";
        element.Q<Label>("TitleLabel").text = stageInfo.stageName;
        element.Q<Label>("InfoLabel").text = stageInfo.GetDropInfo();

        // MoveButton ��������
        Button moveButton = element.Q<Button>("MoveButton");

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
            BattleBroker.OnMainStageChange(index);
        }
    }
    public override ILVItem GetLVItem()
    {
        return new StageSelectItem();
    }
}
