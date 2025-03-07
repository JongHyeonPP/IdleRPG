using EnumCollection;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerBroker
{
    //PlayerController에서 능력치 얻어옴 - PlayerController
    public static Func<object> GetPlayerController;//PlayerController
    //골드로 올린 스탯 적용
    public static Action<StatusType, int> OnGoldStatusLevelSet;
    //스탯 포인트로 올린 스탯 적용
    public static Action<StatusType, int> OnStatPointStatusLevelSet;
    //무기장착
    public static Action<object, WeaponType> OnEquipWeapon;//WeaponData가 null이어서 기본 무기 처리할 때도 WeaponType 정보는 필요함... TotalStatusUI
    //무기 레벨 변경
    public static Action<string, int> OnWeaponLevelSet;
    //무기 개수 변경
    public static Action<string, int> OnWeaponCountSet;
    //플레이어 HP 변경 시 비율 전달
    public static Action<float> OnPlayerHpChanged;
    //플레이어 스킬 시전 시 MP의 비율 전달
    public static Action<float> OnPlayerMpChanged;
    //플레이어가 죽었을 때
    public static Action OnPlayerDead;
    //이름 바꿨을 때
    public static Action<string> OnSetName;
    //스킬업 했을 때, <스킬 ID, 장착 인덱스>
    public static Action<string, int> OnSkillChanged;
    //스킬 레벨 바꼈을 때
    public static Action<string, int> OnSkillLevelSet;//Skill Id, Skill Level
    //스킬 파편 개수 바꼈을 때
    public static Action<Rarity, int> OnFragmentSet;//Skill Id, Skill Level
}