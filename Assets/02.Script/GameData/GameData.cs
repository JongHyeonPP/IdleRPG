using System;
using System.Collections.Generic;
using System.Numerics;
using EnumCollection;
using Newtonsoft.Json;
using UnityEngine;
[Serializable]
public class GameData
{
    public BigInteger gold;
    public int level;
    public int dia;//유료 재화 - 뽑기
    public int clover;//유료 재화 - 강화
    public BigInteger exp;
    public int statPoint;
    public Dictionary<string, int> skillLevel = new();
    [JsonConverter(typeof(Struct_Struct_DictConverter<Rarity,int>))]
    public Dictionary<Rarity, int> skillFragment = new();
    //Weapon
    public Dictionary<string, int> weaponCount = new();
    public Dictionary<string, int> weaponLevel = new();
    //Player Promote
    [JsonConverter(typeof(Struct_StructTuple_DictConverter<int, StatusType, int>))]
    public Dictionary<int, (StatusType, int)> stat_Promote = new();
    //Status
    [JsonConverter(typeof(Struct_Struct_DictConverter<StatusType, int>))]
    public Dictionary<StatusType, int> statLevel_Gold = new();
    [JsonConverter(typeof(Struct_Struct_DictConverter<StatusType,int>))]
    public Dictionary<StatusType, int> statLevel_StatPoint = new();
    public string playerWeaponId;
    public string[] companionWeaponIdArr = new string[3];
    public string[] equipedSkillArr = new string[5];
    [JsonProperty("currentStageNum")]
    public int currentStageNum = 1;//내가 어느 스테이지에 위치해있냐
    [JsonProperty("maxStageNum")]
    public int maxStageNum = 0;//내가 어디까지 뚫었냐
    public string userName;
    //Array Index : 몇 번째 동료인지, Dictionary Key : 몇 번째 효과인지, Dictionary Value : Tuple.item1 스테이터스와 Tuple.item2 레어리티
    [JsonConverter(typeof(Struct_StructTuple_DictConverter<int, StatusType, Rarity>))]
    public Dictionary<int, (StatusType, Rarity)> playerPromoteEffect = new();
    [JsonConverter(typeof(Struct_StructTuple_DictArrConverter<int, StatusType, Rarity>))]
    public Dictionary<int, (StatusType, Rarity)>[] companionPromoteEffect = new Dictionary<int, (StatusType, Rarity)>[3]
    {
        new(), new(), new()
    };
    //'몇 번째 동료가', '몇 번째 테크트리를' 달성했는지에 대한 int값 ex) 동료 1이 왼쪽(0) 테크트리 두 개 밀었음 => companionJobDegree[1][0] = 2
    public int[][] companionPromoteTech = new int[3][]//동료는 세 명
    {
        //테크트리는 0과 1
        new int[2],
        new int[2],
        new int[2]
    };
    [JsonConverter(typeof(TupleArr_Converter<int, int>))]
    public (int, int)[] currentCompanionPromoteTech = new (int, int)[3];

    // Costume 코스튬
    public List<string> equipedCostumes = new();                                 // 장착한 코스튬들
    public List<string> ownedCostumes = new();                                   // 가지고 있는 코스튬 갯수 관리 // string: 코스튬 uid
}