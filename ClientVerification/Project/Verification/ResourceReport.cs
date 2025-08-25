using ClientVerification.Etc;
using Newtonsoft.Json;

namespace ClientVerification.Verification
{
    public class ResourceReport
    {
        [JsonProperty("resource")]
        public Resource Resource { get; set; }
        [JsonProperty("source")]
        public Source Source { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
