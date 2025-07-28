using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class UIBroker
{
    //�ֱ� Ȱ��ȭ �� Translucent�� ������ UI�� ��Ȱ��ȭ
    public static Action InactiveCurrentUI;
    //�������� ���� ��� Ȱ��ȭ, (ActiveUI, bool?Display:Visibility)
    public static Action<VisualElement, bool> ActiveTranslucent;
    //��ų ���� ���� ����
    public static Action InactiveSkillEquip;
    //�޴� UI ����
    public static Action<int, bool> OnMenuUINotice;
    public static Action SetPlayerBarPosition;

    public static Action<string> ShowPopUpInBattle;

    public static Action<string[]> SwitchRenderTargetLayer;
    public static Action<bool, float> ActiveBlurredBackground;

    public static Action<int> ChangeMenu;//Index
}