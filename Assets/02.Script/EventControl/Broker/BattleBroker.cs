using EnumCollection;
using System;
using UnityEngine;
public static class BattleBroker
{
    //���� �׾��� �� ������ ���. <�� ��ġ>
    public static Action<Vector3> OnEnemyDead;
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

    public static Action OnGoldSet;
    public static Action OnLevelExpSet;
    public static Action OnDiaSet;
    public static Action OnEmeraldSet;
    public static Action OnMaxStageSet;
    //�޴� UI ����
    public static Action<int> OnMenuUIChange;
    //���� ������
    public static Action<int, int> OnWeaponLevelSet;//Weapon Id, Level
    //���� ���� ����
    public static Action<int, int> OnWeaponCountSet;//Weapon Id, Count
    public static Func<bool> IsCanAttack;

    public static Action<int> SwitchToStory;
    public static Action SwitchBattle;
}