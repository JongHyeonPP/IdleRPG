using Newtonsoft.Json;
namespace OfflineReward;
public class OfflineRewardResult
{
    [JsonProperty("offlineTime")]
    public double OfflineTime { get; set; }
}