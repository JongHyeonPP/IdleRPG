using Newtonsoft.Json;
namespace ClientVerification.Verification;
public class ReportResult
{
    [JsonProperty("isVerificationSuccess")]
    public bool isVerificationSuccess { get; set; }

    [JsonProperty("failureFactor")]
    public string failureFactor { get; set; }

    [JsonProperty("invalidCount")]
    public int invalidCount { get; set; }
}
