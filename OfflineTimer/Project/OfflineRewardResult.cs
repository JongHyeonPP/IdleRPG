using Newtonsoft.Json;

namespace OfflineTimer
{
    public class OfflineRewardResult
    {
        [JsonProperty("offlineTime")]
        public double OfflineTime { get; set; }
    }
}
