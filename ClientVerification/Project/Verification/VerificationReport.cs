using Newtonsoft.Json;

namespace ClientVerification.Verification
{
    public class VerificationReport
    {
        [JsonProperty("resource")]
        public string Resource { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
