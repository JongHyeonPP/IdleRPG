using Newtonsoft.Json;

namespace ClientVerification
{
    public class ServerGameData
    {
        [JsonProperty("currentStageNum")]
        public int currentStageNum { get; set; }
        [JsonProperty("maxStageNum")]
        public int maxStageNum { get; set; }
        [JsonProperty("invalidCount")]
        public int invalidCount { get; set; }
    }
}
