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
    public static Action<string, int> OnSkillLevelSet;//Skill Id, Skill Level
    public static Action<Rarity, int> OnFragmentSet;//Skill Id, Skill Level
    //��� ���� ����
    public static Action OnGoldSet;
    public static Action OnLevelExpSet;
    public static Action OnDiaSet;
    public static Action OnEmeraldSet;
    //�޴� UI ����
    public static Action<int> OnMenuUIChange;
}