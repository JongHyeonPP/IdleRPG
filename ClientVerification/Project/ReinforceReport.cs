using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Verification
{
    // 클라이언트가 서버에 전송하는 골드 보고서
    public class ReinforceReport
    {
        public ReinforceReport(int value, string statusType, string resourceType)
        {
            Value = value;
            StatusType = statusType;
            SourceType = resourceType;
        }
        [JsonProperty("value")]
        public int Value { get; set; }
        [JsonProperty("status-type")]
        public string StatusType { get; set; }
        [JsonProperty("source-type")]
        public string SourceType { get; set; }
    }
}
