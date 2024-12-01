using UnityEngine.UIElements;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "StageSelectController", menuName = "ScriptableObjects/ListView/StageSelectController")]
public class StageSelectController : LVItemController
{
    public override void BindItem(VisualElement element, int index)
    {
        IListViewItem item = draggableLV.items[index];
        // 스테이지 정보 설정
        StageInfo stageInfo = item as StageInfo;
        element.Q<Label>("StageLabel").text = $"STAGE {stageInfo.stageNum}";
        element.Q<Label>("TitleLabel").text = stageInfo.stageName;
        element.Q<Label>("InfoLabel").text = stageInfo.GetDropInfo();

        // MoveButton 가져오기
        Button moveButton = element.Q<Button>("MoveButton");

        // 기존 이벤트 제거
        moveButton.UnregisterCallback<ClickEvent>(OnMoveButtonClick);

        // 버튼에 인덱스를 userData로 저장

        moveButton.userData = stageInfo.stageNum;

        // 클릭 이벤트 등록
        moveButton.RegisterCallback<ClickEvent>(OnMoveButtonClick);
    }


    // 버튼 클릭 이벤트 핸들러
    private void OnMoveButtonClick(ClickEvent evt)
    {
        // 클릭된 버튼의 userData를 통해 인덱스 가져오기
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
