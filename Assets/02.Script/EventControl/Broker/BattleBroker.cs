using EnumCollection;
using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
public static class BattleBroker
{
    public static Action<Vector3> OnEnemyDead;
    public static Action OnStageChange;
    public static Action OnBossTimeLimit;
    public static Action OnBossClear;
    public static Func<BattleType> GetBattleType;
    public static Action<float> OnBossHpChanged;

    

    public static Func<bool> IsCanAttack;

    public static Action<BattleType, int[]> SwitchToStory;
    public static Action SwitchToBattle;
    public static Action SwitchToBoss;
    public static Action<int, int> SwitchToAdventure;
    public static Action<int, int> SwitchToDungeon;
    public static Action<int, (int,int)> SwitchToCompanionBattle;
    public static Action<int> ControllCompanionMove;//0 : idle 1 :run, 2 : attack
    
    public static Action<int> SwitchToPromoteBattle;

    public static Func<BigInteger> GetNeedExp;
    public static Action<DropType, int, string> OnDrop;

    public static Action SetCameraExpand;
    public static Action SetCameraShrink;

    public static Action<Vector3, string, DamageType> ShowDamageText;//Screen Pos

    public static Func<int, int, (int, int)> GetCompanionReward;//index_0, index_1, (dia, clover)
    public static Func<int, int, (int, int)> GetAdventureReward;//index_0, index_1, (dia, clover)

    public static Func<bool> GetAdventureRetry;
    //public static Func<bool> GetDungeonRetry;

    public static Action<int> ActiveStageInfoUI;

    public static Func<int, int, object> GetDungeonReward;

	public static Func<object> GetPlayerController;//PlayerController
	public static Func<object> GetEnemyArray;//EnemyController[]
    public static Func<object> GetCompanionControllerArr;//object = CompanionController[]

    public static Action RefreshPlayerSpeed;
    public static Action<string> OnWeaponLevelChanged;

    public static Action<BigInteger> OnThornsDamage;
    public static Action<Vector3, float> OnAreaDamage;
}
