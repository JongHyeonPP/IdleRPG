using Newtonsoft.Json;

namespace Verification
{
    public class VerificationReport
    {
        public VerificationReport(int value, string resource, string source)
        {
            Resource = resource.ToString();
            Value = value;
        }
        [JsonProperty("resource")]
        public string Resource { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
