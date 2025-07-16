using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
public class ClientReinforceReport
{
    public ClientReinforceReport(int value, StatusType statusType, bool isByGold)
    {
        Value = value;
        StatusType = statusType.ToString();
        SourceType = isByGold ? "Gold" : "StatusPoint";
    }
    [JsonProperty("value")]
    public int Value { get; set; }
    [JsonProperty("status-type")]
    public string StatusType { get; set; }
    [JsonProperty("source-type")]
    public string SourceType  { get; set; }
}