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
    public static Action OnStageChange;
    //���� �������� �ð� �ʰ� ��
    public static Action OnBossTimeLimit;
    //���� �������� Ŭ���� ��
    public static Action OnBossClear;
    //BattleManager�� �ִ� BattleType�� ��´�
    public static Func<BattleType> GetBattleType;
    //���� HP ���� �� ���� ����
    public static Action<float> OnBossHpChanged;

    
    public static Action<int> RefreshStageSelectUI;
    public static Func<bool> IsCanAttack;

    public static Action<int> SwitchToStory;
    public static Action SwitchToBattle;
    public static Action SwitchToBoss;
    public static Action<int, int> SwitchToAdventure;
    public static Action<int, int> SwitchToDungeon;
    public static Action<int, (int,int)> SwitchToCompanionBattle;
    //����
    public static Action<int> ControllCompanionMove;//0 : ����, 1 : ������, 2 : ����
    
    //�±�
    public static Action<Rank> ChallengeRank;

    public static Func<BigInteger> GetNeedExp;
    public static Action<DropType, int, string> OnDrop;

    public static Action SetCameraExpand;
    public static Action SetCameraShrink;

    public static Action<Vector3, string> ShowDamageText;//Screen Pos

    public static Func<int, int, (int, int)> GetCompanionReward;//index_0, index_1, (dia, clover)
    public static Func<int, int, (int, int)> GetAdventureReward;//index_0, index_1, (dia, clover)

    public static Func<bool> GetAdventureRetry;

    public static Action<int> ActiveStageInfoUI;

    public static Func<int, int, object> GetDungeonReward;
}