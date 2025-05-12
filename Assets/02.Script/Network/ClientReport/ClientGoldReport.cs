using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ClientGoldReport
{
    public ClientGoldReport(int value, Resource resource, Source source)
    {
        Resource = resource.ToString();
        Source = source.ToString();
        Value = value;
    }
    [JsonProperty("resource")]
    public string Resource { get; set; }
    [JsonProperty("source")]
    public string Source { get; set; }
    [JsonProperty("value")]
    public int Value { get; set; }
    [JsonProperty("elapsed-seconds")]
    public int ElapsedSeconds { get; set; }
}