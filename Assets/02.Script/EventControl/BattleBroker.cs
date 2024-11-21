using System;
using UnityEngine;
public static class BattleBroker
{
    //���� �׾��� �� ������ ���. <�� ��ġ>
    public static Action<Vector3> OnEnemyDead;
    //GoldDrop�� ����� ��
    public static Action OnGoldGain;
    //ExpDrop�� ����� �� 
    public static Action OnExpGain;
    //UI�� ��� ���� <��ü ��差>
    public static Action<int> ApplyGoldToUi;
    //UI�� ����ġ ���� <����ġ ����>
    public static Action<float> ApplyExpToUi;
}