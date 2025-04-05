using EnumCollection;
using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
public static class BattleBroker
{
    //적이 죽었을 때 아이템 드랍. <적 위치>
    public static Action<Vector3> OnEnemyDead;
    //스테이지 변경
    public static Action<int> OnStageChange;
    //보스 스테이지 시간 초과 시
    public static Action OnBossTimeLimit;
    //보스 스테이지 클리어 시
    public static Action OnBossClear;
    //BattleManager에 있는 BattleType을 얻는다
    public static Func<BattleType> GetBattleType;
    //보스 HP 변경 시 비율 전달
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
    //동료
    public static Action<int> ControllCompanionMove;//0 : 멈충, 1 : 움직임, 2 : 공격
    public static Action<int> OnCompanionExpSet;
    //승급
    public static Action<Rank> ChallengeRank;

    public static Func<BigInteger> GetNeedExp;
    public static Action<int> OnExpByDrop;
    public static Action<int> OnGoldByDrop;

    public static Func<DropType, int> GetStageRewardValue;

    public static Action SetCameraExpand;
    public static Action SetCameraShrink;

    public static Action<Vector3, string> ShowDamageText;//Screen Pos

}