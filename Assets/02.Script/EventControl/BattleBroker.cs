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
    //UI에 골드 적용 <전체 골드량>
    public static Action<int> ApplyGoldToUi;
    //UI에 경험치 적용 <경험치 비율>
    public static Action<float> ApplyExpToUi;
}