using System;
using UnityEngine.UIElements;

public static class UIBroker
{
    //�ֱ� Ȱ��ȭ �� Translucent�� ������ UI�� ��Ȱ��ȭ
    public static Action InactiveCurrentUI;
    //�������� ���� ��� Ȱ��ȭ, (ActiveUI, bool?Display:Visibility)
    public static Action<VisualElement, bool> ActiveTranslucent;
    //��ų ���� ���� ����
    public static Action InactiveSkillEquip;
}