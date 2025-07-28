using System;
using System.Collections.Generic;
using System.Numerics;
using EnumCollection;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

[Serializable]
public class GameData
{
    #region 재화 (Gold, Exp, Dia, Clover, Scroll)
    public BigInteger gold;
    public BigInteger exp;
    public int level;
    public int dia;     // 유료 재화 - 뽑기
    public int clover;  // 유료 재화 - 강화
    public int scroll;  // 던전 입장권
    #endregion

    #region 능력치 및 스탯
    public int statPoint;

    [JsonConverter(typeof(Struct_Struct_DictConverter<StatusType, int>))]
    public Dictionary<StatusType, int> statLevel_Gold = new();

    [JsonConverter(typeof(Struct_Struct_DictConverter<StatusType, int>))]
    public Dictionary<StatusType, int> statLevel_StatPoint = new();

    [JsonConverter(typeof(Struct_StructTuple_DictConverter<int, StatusType, int>))]
    public Dictionary<int, (StatusType, int)> stat_Promote = new();
    #endregion

    #region 스킬 관련
    public Dictionary<string, int> skillLevel = new();

    [JsonConverter(typeof(Struct_Struct_DictConverter<Rarity, int>))]
    public Dictionary<Rarity, int> skillFragment = new();

    public string[] equipedSkillArr = new string[5];
    #endregion

    #region 무기 관련
    public Dictionary<string, int> weaponCount = new();
    public Dictionary<string, int> weaponLevel = new();
    public string playerWeaponId;
    public string[] companionWeaponIdArr = new string[3];
    #endregion

    #region 스테이지 진행 정보
    public int currentStageNum;  // 현재 위치한 스테이지
    public int maxStageNum;      // 최대 클리어한 스테이지
    #endregion

    #region 플레이어 정보
    public string userName;
    public int invalidCount;
    #endregion

    #region 승급 효과 관련 (Player & Companion)

    [JsonConverter(typeof(Struct_StructTuple_DictConverter<int, StatusType, Rarity>))]
    public Dictionary<int, (StatusType, Rarity)> playerPromoteEffect = new();

    [JsonConverter(typeof(Struct_StructTuple_DictArrConverter<int, StatusType, Rarity>))]
    public Dictionary<int, (StatusType, Rarity)>[] companionPromoteEffect = new Dictionary<int, (StatusType, Rarity)>[3]
    {
        new(), new(), new()
    };

    public int[][] companionPromoteTech = new int[3][]
    {
        new int[2],
        new int[2],
        new int[2]
    };

    [JsonConverter(typeof(TupleArr_Converter<int, int>))]
    public (int, int)[] currentCompanionPromoteTech = new (int, int)[3];
    #endregion

    #region 코스튬
    public List<string> equipedCostumes = new();    // 장착한 코스튬
    public List<string> ownedCostumes = new();      // 보유한 코스튬
    #endregion

    public int[] adventureProgess = new int[9];//모험 진행도
}
