using System;
using System.Collections.Generic;
using EnumCollection;
using Newtonsoft.Json;
[Serializable]
public class GameData
{
    public int gold;
    public int level;
    public Dictionary<string, int> skillLevel = new();
    public Dictionary<string, int> weaponNum = new();
    [JsonConverter(typeof(StatusTypeDictionaryConverter))]
    public Dictionary<StatusType, int> statLevel_0 = new();
    [JsonConverter(typeof(StatusTypeDictionaryConverter))]
    public Dictionary<StatusType, int> statLevel_1 = new();
    public string weaponId;
}
