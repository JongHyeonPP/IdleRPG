using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Verification
{
    public class VerificationReport
    {
        public VerificationReport(int value, string resource, string source)
        {
            Resource = resource.ToString();
            Source = source.ToString();
            Value = value;
        }
        [JsonProperty("resource")]
        public string Resource { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
