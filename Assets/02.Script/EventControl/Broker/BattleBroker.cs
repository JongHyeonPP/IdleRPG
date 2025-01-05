using EnumCollection;
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
    //�������� ����
    public static Action<int> OnStageChange;
    //�Ϲ� �������� ����
    public static Action OnStageEnter;
    //���� �������� ����
    public static Action OnBossEnter;
    //���� �������� �ð� �ʰ� ��
    public static Action OnBossTimeLimit;
    //���� �������� Ŭ���� ��
    public static Action OnBossClear;
    //BattleManager�� �ִ� BattleType�� ��´�
    public static Func<BattleType> GetBattleType;
    //���� HP ���� �� ���� ����
    public static Action<float> OnBossHpChanged;
    //�������� ������ �󸶳� �������, ���ʿ� ������ ���� ����
    public static Action<DropType, int> OnCurrencyInBattle;
    public static Action<string, int> OnSkillLevelChange;//Skill Id, Skill Level
}