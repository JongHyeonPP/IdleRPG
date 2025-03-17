using System;
using System.Collections.Generic;
using System.Numerics;
using EnumCollection;
using Newtonsoft.Json;
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
    [JsonConverter(typeof(EnumDictConverter<Rarity>))]
    public Dictionary<Rarity, int> skillFragment =new();
    //Weapon
    public Dictionary<string, int> weaponCount=new();
    public Dictionary<string, int> weaponLevel=new();
    //
    [JsonConverter(typeof(EnumDictConverter<StatusType>))]
    public Dictionary<StatusType, int> statLevel_Gold=new();
    [JsonConverter(typeof(EnumDictConverter<StatusType>))]
    public Dictionary<StatusType, int> statLevel_StatPoint=new();
    public Dictionary<StatusType, float> stat_Promote = new();
    public string playerWeaponId;
    public string[] companionWeaponIdArr = new string[3];
    public string[] equipedSkillArr = new string[5];
    public int currentStageNum;//내가 어느 스테이지에 위치해있냐
    public int maxStageNum;//내가 어디까지 뚫었냐
    public string userName;
}
