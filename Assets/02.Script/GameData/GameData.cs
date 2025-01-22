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
    public BigInteger exp;
    public Dictionary<string, int> skillLevel = new();
    [JsonConverter(typeof(EnumDictConverter<Rarity>))]
    public Dictionary<Rarity, int> skillFragment = new ();
    //Weapon
    public Dictionary<int, int> weaponCount = new();
    [JsonConverter(typeof(EnumDictConverter<StatusType>))]
    public Dictionary<StatusType, int> statLevel_Gold = new();
    [JsonConverter(typeof(EnumDictConverter<StatusType>))]
    public Dictionary<StatusType, int> statLevel_StatPoint = new();
    public string weaponId;
    public string[] equipedSkillArr = new string[10];
    public int currentStageNum;
    public int maxStageNum;
}
