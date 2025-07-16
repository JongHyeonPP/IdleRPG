using Newtonsoft.Json;


    public class RewardResult
    {
        [JsonProperty("offlineTime")]
        public int OfflineTime { get; set; }
        [JsonProperty("goldAcquisition")]
        public int goldAcquisition { get; set; }
        [JsonProperty("expAcquisition")]
        public int expAcquisition { get; set; }

        [JsonProperty("exp")]
        public int Exp { get; set; }
        [JsonProperty("gold")]
        public int gold { get; set; }
        [JsonProperty("dia")]
        public int Dia { get; set; }
        [JsonProperty("Clover")]
        public int clover { get; set; }
    }

