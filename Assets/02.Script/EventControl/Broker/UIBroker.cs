using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class UIBroker
{
    //최근 활성화 된 Translucent를 동반한 UI를 비활성화
    public static Action InactiveCurrentUI;
    //반투명한 검은 배경 활성화, (ActiveUI, bool?Display:Visibility)
    public static Action<VisualElement, bool> ActiveTranslucent;
    //스킬 장착 상태 해제
    public static Action InactiveSkillEquip;
    //메뉴 UI 변경
    public static Action<int, bool> OnMenuUINotice;
    public static Action SetPlayerBarPosition;

    public static Action<string> ShowPopUpInBattle;

    public static Action<string[]> SwitchRenderTargetLayer;
    public static Action<bool, float> ActiveBlurredBackground;

    public static Action<int> ChangeMenu;//Index
}