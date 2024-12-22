using System;
using System.Collections.Generic;
using EnumCollection;
using Newtonsoft.Json;
[Serializable]
public class GameData
{
    public int gold;
    public int level;
    public int exp;
    public Dictionary<string, int> skillLevel = new();
    public Dictionary<int, int> weaponNum = new();
    [JsonConverter(typeof(StatusTypeDictionaryConverter))]
    public Dictionary<StatusType, int> statLevel_Gold = new();
    [JsonConverter(typeof(StatusTypeDictionaryConverter))]
    public Dictionary<StatusType, int> statLevel_StatPoint = new();
    public int currentStageNum;
    public int maxStageNum;
}
