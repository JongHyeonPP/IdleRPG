using System;
using UnityEngine;
using EnumCollection;
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
    //�ɷ�ġ ����
    public static Action<StatusType, int> OnStatusChange;
    //��������
    public static Action<object> OnEquipWeapon;
    //�������� ����
    public static Action<int> OnStageChange;
    //�Ϲ� �������� ����
    public static Action OnStageEnter;
    //���� �������� ����
    public static Action OnBossEnter;
    //�÷��̾��� �ݶ��̴��� ��´�
    public static Func<Collider2D> GetPlayerCollider;
    //�÷��̾ �׾��� ��
    public static Action OnPlayerDead;
    //UI����
    public static Action<int> OnUIChange;
}