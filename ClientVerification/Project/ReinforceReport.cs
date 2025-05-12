using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Verification
{
    // Ŭ���̾�Ʈ�� ������ �����ϴ� ��� ����
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
