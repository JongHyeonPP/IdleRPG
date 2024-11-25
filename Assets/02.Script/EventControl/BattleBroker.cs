using System;
using UnityEngine;
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
}