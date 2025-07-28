using System;
using System.Collections.Generic;
using System.Data;
using ClientVerification.Etc;
using Microsoft.Extensions.Logging;

namespace ClientVerification.Verification
{
    public class ResourceVerifier : IDataVerifier
    {
        private readonly List<VerificationReport> _reports;
        private readonly GameData _gameData;
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _formula;

        public ResourceVerifier(List<VerificationReport> reports, GameData gameData, ILogger logger, Dictionary<string, string> formula)
        {
            _reports = reports;
            _gameData = gameData;
            _logger = logger;
            _formula = formula;
        }

        public bool Verify(GameData clientData, out string failReason)
        {
            failReason = "";

            foreach (var report in _reports)
            {
                if (!Verify(report, _gameData.currentStageNum))
                {
                    failReason = "ResourceVerificationFailed";
                    return false;
                }

                string res = report.Resource.ToLowerInvariant();
                switch (res)
                {
                    case "gold":
                        _gameData.gold += report.Value;
                        break;
                    case "exp":
                        _gameData.exp += report.Value;
                        break;
                    case "dia":
                        _gameData.dia += report.Value;
                        break;
                    case "clover":
                        _gameData.clover += report.Value;
                        break;
                    case "scroll":
                        _gameData.scroll += report.Value;
                        break;
                }
            }

            return true;
        }

        private bool Verify(VerificationReport report, int stageNum)
        {
            var dt = new DataTable();

            string standardExpr = _formula[$"{report.Resource}Standard"]
                .Replace("{stageNum}", stageNum.ToString());

            int standardValue = Convert.ToInt32(dt.Compute(standardExpr, null));
            float valueRange = float.Parse(_formula[$"{report.Resource}Range"]);
            int max = (int)Math.Ceiling(standardValue * (1 + valueRange)) + 1;

            if (report.Value >= max)
            {
                var anomaly = new Dictionary<string, object>
                {
                    { "type", $"{report.Resource}Gain" },
                    { "expectedMax", max - 1 },
                    { $"reported{report.Resource}", report.Value },
                    { "invalidCount", _gameData.invalidCount + 1 }
                };

                _logger.LogError($"Unexpected {report.Resource}: {System.Text.Json.JsonSerializer.Serialize(anomaly)}");
                return false;
            }

            return true;
        }
    }
}
