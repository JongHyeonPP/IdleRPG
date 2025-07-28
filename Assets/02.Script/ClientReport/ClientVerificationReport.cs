using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

public class ClientVerificationReport
{
    public ClientVerificationReport(int value, Resource resource, Source source)
    {
        Resource = resource.ToString();
        Value = value;
        Source = source.ToString();
    }

    [JsonProperty("resource")]
    public string Resource { get; set; }
    [JsonProperty("source")]
    public string Source { get; set; }
    [JsonProperty("value")]
    public int Value { get; set; }
}