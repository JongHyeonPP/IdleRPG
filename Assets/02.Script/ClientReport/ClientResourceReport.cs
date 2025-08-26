using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

public class ClientResourceReport
{
    public ClientResourceReport(int value,string id, Resource resource, Source source)
    {
        Resource = resource;
        Value = value;
        Source = source;
        Id = id;
    }

    [JsonProperty("resource")]
    public Resource Resource { get; set; }
    [JsonProperty("source")]
    public Source Source { get; set; }
    [JsonProperty("value")]
    public int Value { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
}