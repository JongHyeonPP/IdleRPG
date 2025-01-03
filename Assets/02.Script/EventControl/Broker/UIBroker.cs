using System;
using UnityEngine.UIElements;

public static class UIBroker
{
    //최근 활성화 된 Translucent를 동반한 UI를 비활성화
    public static Action InactiveCurrentUI;
    //반투명한 검은 배경 활성화, 배경을 누를 시에 비활성화 할 VisualElement를 매개변수로 제시한다.
    public static Action<VisualElement, bool> ActiveTranslucent;
}