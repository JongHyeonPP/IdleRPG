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
    //ExpDrop�� ����� �� 
    public static Action OnDiaGain;
    //ExpDrop�� ����� �� 
    public static Action OnEmeraldGain;
    //�̸� �ٲ��� ��
    public static Action OnSetName;
    //������ ���� ��
    public static Action OnLevelUp;
}