using System;
using UnityEngine.UIElements;

public static class UIBroker
{
    //�ֱ� Ȱ��ȭ �� Translucent�� ������ UI�� ��Ȱ��ȭ
    public static Action InactiveCurrentUI;
    //�������� ���� ��� Ȱ��ȭ, ����� ���� �ÿ� ��Ȱ��ȭ �� VisualElement�� �Ű������� �����Ѵ�.
    public static Action<VisualElement, bool> ActiveTranslucent;
}