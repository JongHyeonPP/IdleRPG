using EnumCollection;
using Newtonsoft.Json;
using UnityEngine;

public class GachaInfo
{
    [JsonProperty("gachaType")]
    public GachaType GachaType { private set; get; }
    [JsonProperty("gachaNum")]
    public int GachaNum { private set; get; }
    public GachaInfo(GachaType gachaType, int gachaNum)
    {
        GachaType = gachaType;
        GachaNum = gachaNum;
    }
}
