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
    public static Action OnStatPointSet;
    public static Action OnLevelExpSet;
    public static Action OnDiaSet;
    public static Action OnCloverSet;
    public static Action OnMaxStageSet;
    public static Func<bool> IsCanAttack;

    public static Action<int> SwitchToStory;
    public static Action SwitchBattle;
    //����
    public static Action<object> StartCompanionAttack;
    public static Action StopCompanionAttack;
    public static Action<int> OnCompanionExpSet;
    //�±�
    public static Action<Rank> ChallengeRank;
}