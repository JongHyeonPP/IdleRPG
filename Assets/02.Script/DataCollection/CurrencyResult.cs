using EnumCollection;
using Newtonsoft.Json;

public class CurrencyResult
{
    [JsonProperty("resource")]
    public Resource Resource;
    [JsonProperty("value")]
    public int Value;
}
