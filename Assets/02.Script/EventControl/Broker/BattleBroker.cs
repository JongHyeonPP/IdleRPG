using EnumCollection;
using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
public static class BattleBroker
{
    //���� �׾��� �� ������ ���. <�� ��ġ>
    public static Action<Vector3> OnEnemyDead;
    //�������� ����
    public static Action<int> OnStageChange;
    //���� �������� �ð� �ʰ� ��
    public static Action OnBossTimeLimit;
    //���� �������� Ŭ���� ��
    public static Action OnBossClear;
    //BattleManager�� �ִ� BattleType�� ��´�
    public static Func<BattleType> GetBattleType;
    //���� HP ���� �� ���� ����
    public static Action<float> OnBossHpChanged;

    public static Action OnGoldSet;
    public static Action OnStatPointSet;
    public static Action OnLevelExpSet;
    public static Action OnDiaSet;
    public static Action OnCloverSet;
    public static Action OnMaxStageSet;
    public static Action<int> RefreshStageSelectUI;
    public static Func<bool> IsCanAttack;

    public static Action<int> SwitchToStory;
    public static Action SwitchToBattle;
    public static Action SwitchToBoss;
    public static Action<int, (int,int)> SwitchToCompanionBattle;
    //����
    public static Action<int> ControllCompanionMove;//0 : ����, 1 : ������, 2 : ����
    public static Action<int> OnCompanionExpSet;
    //�±�
    public static Action<Rank> ChallengeRank;

    public static Func<BigInteger> GetNeedExp;
    public static Action<int> OnExpByDrop;
    public static Action<int> OnGoldByDrop;

    public static Func<DropType, int> GetStageRewardValue;

    public static Action SetCameraExpand;
    public static Action SetCameraShrink;

    public static Action<Vector3, string> ShowDamageText;//Screen Pos

}