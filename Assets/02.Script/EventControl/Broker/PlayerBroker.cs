using EnumCollection;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerBroker
{
    //PlayerController에서 능력치 얻어옴 - PlayerController
    public static Func<object> GetPlayerController;
    //능력치 적용
    public static Action<StatusType, int> OnStatusChange;
    //무기장착
    public static Action<object> OnEquipWeapon;
    //플레이어 HP 변경 시 비율 전달
    public static Action<float> OnPlayerHpChanged;
    //플레이어 스킬 시전 시 MP의 비율 전달
    public static Action<float> OnPlayerMpChanged;
    //플레이어가 죽었을 때
    public static Action OnPlayerDead;
    //이름 바꿨을 때
    public static Action<string> OnSetName;
    //레벨업 됐을 때
    public static Action<int> OnSetLevel;
}