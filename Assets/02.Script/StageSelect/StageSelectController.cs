using UnityEngine.UIElements;
using UnityEngine;
using System;
public class StageSelectController : LVItemController
{
    //가져온 아이템의 ui 구조를 설정한다.
    public override void BindItem(VisualElement element, int index)
    {
        IListViewItem item = draggableLV.items[index];
        StageInfo stageInfo = item as StageInfo;
        int stageNum = stageInfo.stageNum;
        //VisualElement 가져오기
        Label stageLabel = element.Q<Label>("StageLabel");
        Label titleLabel = element.Q<Label>("TitleLabel");
        Label infoLabel = element.Q<Label>("InfoLabel");
        VisualElement lockGroup = element.Q<VisualElement>("LockGroup");
        Button moveButton = element.Q<Button>("MoveButton");
        //VisualElement 설정
        titleLabel.text = stageInfo.stageName;
        if (GameManager.instance.gameData.maxStageNum >= stageNum)//오픈된 스테이지라면
        {
            stageLabel.style.display = infoLabel.style.display = moveButton.style.display = DisplayStyle.Flex;
            lockGroup.style.display = DisplayStyle.None;
            stageLabel.text = $"STAGE {stageInfo.stageNum}";
            infoLabel.text = stageInfo.GetDropInfo();
        }
        else//닫힌 스테이지라면
        {
            stageLabel.style.display = infoLabel.style.display = moveButton.style.display = DisplayStyle.None;
            lockGroup.style.display = DisplayStyle.Flex;
        }
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
            BattleBroker.OnStageChange(index);
        }
    }
    public override ILVItem GetLVItem()
    {
        return new StageSelectItem();
    }
}
