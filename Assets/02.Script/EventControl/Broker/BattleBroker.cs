using EnumCollection;
using System;
using UnityEngine;
public static class BattleBroker
{
    //적이 죽었을 때 아이템 드랍. <적 위치>
    public static Action<Vector3> OnEnemyDead;
    //스테이지 변경
    public static Action<int> OnStageChange;
    //일반 스테이지 입장
    public static Action OnStageEnter;
    //보스 스테이지 입장
    public static Action OnBossEnter;
    //보스 스테이지 시간 초과 시
    public static Action OnBossTimeLimit;
    //보스 스테이지 클리어 시
    public static Action OnBossClear;
    //BattleManager에 있는 BattleType을 얻는다
    public static Func<BattleType> GetBattleType;
    //보스 HP 변경 시 비율 전달
    public static Action<float> OnBossHpChanged;
    //전투에서 무엇을 얼마나 얻었는지, 왼쪽에 줄줄이 띄우기 위함
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
    //동료
    public static Action<object> StartCompanionAttack;
    public static Action StopCompanionAttack;
    public static Action<int> OnCompanionExpSet;
    //승급
    public static Action<Rank> ChallengeRank;
}