using System;
using UnityEngine;

public static class StartBroker
{
    public static Action OnDetectDuplicateLogin;//�ߺ� �α��� Ž������ �� �߻���ų ��������Ʈ
    public static Action OnDataLoadComplete;//�ʿ��� �������� �ε尡 ������ Battle�� �Ѿ�� ���� ��������Ʈ
    public static Action OnAuthenticationComplete;//���� ������ ������ ���� ������ �������� �����͸� �ε��ϱ� ���� ��������Ʈ
}
