using System;
using UnityEngine.UIElements;

public static class UIBroker
{
    //최근 활성화 된 Translucent를 동반한 UI를 비활성화
    public static Action InactiveCurrentUI;
    //반투명한 검은 배경 활성화, (ActiveUI, bool?Display:Visibility)
    public static Action<VisualElement, bool> ActiveTranslucent;
    //스킬 장착 상태 해제
    public static Action InactiveSkillEquip;
}