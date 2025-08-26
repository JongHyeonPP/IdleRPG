using ClientVerification.Etc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ClientVerification.Verification
{
    public class StatPointVerifier : IDataVerifier
    {
        private readonly GameData clientData;

        public StatPointVerifier(GameData clientData)
        {
            this.clientData = clientData;
        }

        public bool Verify(out string failReason)
        {
            failReason = "";

            if (clientData == null)
            {
                failReason = BuildFail(
                    code: "StatPoint.ClientData.Null",
                    message: "Client data is null",
                    extra: new { }
                );
                return false;
            }

            if (clientData.statLevel_StatPoint == null)
            {
                failReason = BuildFail(
                    code: "StatPoint.Map.Null",
                    message: "Stat point map is null",
                    extra: new { }
                );
                return false;
            }

            if (clientData.level < 1)
            {
                failReason = BuildFail(
                    code: "StatPoint.Level.Invalid",
                    message: "Player level must be at least 1",
                    extra: new { level = clientData.level }
                );
                return false;
            }

            // 음수 분배 방지
            var negatives = clientData.statLevel_StatPoint
                .Where(kv => kv.Value < 0)
                .Select(kv => kv.Key.ToString())
                .ToList();

            if (negatives.Count > 0)
            {
                failReason = BuildFail(
                    code: "StatPoint.Negative.Allocation",
                    message: "Negative stat point allocation detected",
                    extra: new { stats = negatives }
                );
                return false;
            }

            var used = clientData.statLevel_StatPoint.Values.Sum();
            var available = clientData.level - 1;

            if (used > available)
            {
                failReason = BuildFail(
                    code: "StatPoint.UsedExceedsAvailable",
                    message: "Used stat points exceed available points",
                    extra: new { used, available, level = clientData.level }
                );
                return false;
            }

            return true;
        }

        private static string BuildFail(string code, string message, object extra)
        {
            var payload = new { code, message, extra };
            return JsonConvert.SerializeObject(payload);
        }
    }
}
