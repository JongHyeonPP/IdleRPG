using Newtonsoft.Json;

namespace ClientVerification
{
    public class ServerGameData
    {
        [JsonProperty("currentStageNum")]
        public int currentStageNum { get; set; }
        [JsonProperty("maxStageNum")]
        public int maxStageNum { get; set; }
    }
}
