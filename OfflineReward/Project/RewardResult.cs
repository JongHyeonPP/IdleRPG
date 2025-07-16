using Newtonsoft.Json;

namespace OfflineReward
{
    public class RewardResult
    {
        [JsonProperty("exp")]
        public int Exp { get; set; }
        [JsonProperty("gold")]
        public int Gold { get; set; }
        [JsonProperty("dia")]
        public int Dia { get; set; }
        [JsonProperty("Clover")]
        public int Clover { get; set; }
    }
}
