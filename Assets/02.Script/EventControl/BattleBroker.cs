using System;
using UnityEngine;
using EnumCollection;
public static class BattleBroker
{
    //적이 죽었을 때 아이템 드랍. <적 위치>
    public static Action<Vector3> OnEnemyDead;
    //GoldDrop과 닿았을 때
    public static Action OnGoldGain;
    //ExpDrop과 닿았을 때 
    public static Action OnExpGain;
    //ExpDrop과 닿았을 때 
    public static Action OnDiaGain;
    //ExpDrop과 닿았을 때 
    public static Action OnEmeraldGain;
    //이름 바꿨을 때
    public static Action OnSetName;
    //레벨업 됐을 때
    public static Action OnLevelUp;
    //능력치 적용
    public static Action<StatusType, int> OnStatusChange;
    //무기장착
    public static Action<object> OnEquipWeapon;
    //스테이지 변경
    public static Action<int> OnStageChange;
    //일반 스테이지 입장
    public static Action OnStageEnter;
    //보스 스테이지 입장
    public static Action OnBossEnter;
    //플레이어의 콜라이더를 얻는다
    public static Func<Collider2D> GetPlayerCollider;
    //플레이어가 죽었을 때
    public static Action OnPlayerDead;
    //UI변경
    public static Action<int> OnUIChange;
}