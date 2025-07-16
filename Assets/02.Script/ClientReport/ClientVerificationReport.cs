using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

public class ClientVerificationReport
{
    public ClientVerificationReport(int value, Resource resource)
    {
        Resource = resource.ToString();
        Value = value;
    }

    [JsonProperty("resource")]
    public string Resource { get; set; }
    [JsonProperty("value")]
    public int Value { get; set; }
}