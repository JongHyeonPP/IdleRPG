using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using ClientVerification.Etc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;

namespace ClientVerification.Verification
{
    public class ResourceVerifier : IDataVerifier
    {
        private readonly List<ResourceReport> _reports;
        private readonly GameData _serverData;
        private readonly ILogger _logger;
        private DataTable _dataTable;
        private string _levelUpRequireExp;
        private Dictionary<string, string> _stageDropFormula;
        private Dictionary<string, object> _adventureReward;
        public ResourceVerifier(List<ResourceReport> reports, GameData serverData, ILogger logger,
            IVerificationSystem verificationSystem, Unity.Services.CloudCode.Core.IExecutionContext context, IGameApiClient gameApiClient)
        {
            _reports = reports;
            _serverData = serverData;
            _logger = logger;
            _dataTable = new();
            _stageDropFormula = verificationSystem.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "STAGE_DROP_FORMULA");
            _adventureReward = verificationSystem.GetRemoteConfig<Dictionary<string, object>>(context, gameApiClient, "ADVENTURE_REWARD");
            _levelUpRequireExp = verificationSystem.GetRemoteConfig<string>(context, gameApiClient, "LEVEL_UP_REQUIRE_EXP");
        }

        public bool Verify(out string failReason)
        {
            failReason = "";

            foreach (var report in _reports)
            {
                if (!Verify(report))
                {
                    failReason = "ResourceVerificationFailed";
                    return false;
                }

                switch (report.Resource)
                {
                    case Resource.Gold:
                        _serverData.gold += report.Value;
                        break;
                    case Resource.Exp:
                        _serverData.exp += report.Value;
                        break;
                    case Resource.Dia:
                        _serverData.dia += report.Value;
                        break;
                    case Resource.Clover:
                        _serverData.clover += report.Value;
                        break;
                    case Resource.Scroll:
                        _serverData.scroll += report.Value;
                        break;
                }
            }

            return true;
        }

        private bool Verify(ResourceReport report)
        {
            switch (report.Source)
            {
                case Source.Battle:
                    return BattleCase(report);
                case Source.Adventure:
                    return AdventureCase(report);
                case Source.Companion:
                    break;
            }


            return false;
        }

        private bool BattleCase(ResourceReport report)
        {
            string standardResource = _stageDropFormula[$"{report.Resource}Standard"].Replace("{stageNum}", _serverData.currentStageNum.ToString());

            int standardValue = Convert.ToInt32(_dataTable.Compute(standardResource, null));
            float valueRange = float.Parse(_stageDropFormula[$"{report.Resource}Range"]);
            int max = (int)Math.Ceiling(standardValue * (1 + valueRange)) + 1;

            if (report.Value >= max)
            {
                var anomaly = new Dictionary<string, object>
                        {
                            { "type", $"{report.Resource}Gain" },
                            { "expectedMax", max - 1 },
                            { $"reported{report.Resource}", report.Value },
                            { "invalidCount", _serverData.invalidCount + 1 }
                        };

                _logger.LogError($"Unexpected {report.Resource}: {System.Text.Json.JsonSerializer.Serialize(anomaly)}");
                return false;
            }
            ProcessLevelUp();
            return true;
        }

        private void ProcessLevelUp()
        {
            if (string.IsNullOrEmpty(_levelUpRequireExp)) return;

            BigInteger exp = _serverData.exp;
            int level = _serverData.level;

            while (true)
            {
                BigInteger required = BigInteger.Parse(_dataTable.Compute(
                    _levelUpRequireExp.Replace("{level}", level.ToString()), null).ToString());

                if (exp < required) break;
                exp -= required;
                level++;
            }

            _serverData.exp = exp;
            _serverData.level = level;
        }
        private bool AdventureCase(ResourceReport report)
        {
            int adventureIndex_0 = report.Value;
            int adventureIndex_1 = _serverData.adventureProgess[adventureIndex_0];

            _logger.LogDebug($"[AdventureCase Before] Dia: {_serverData.dia}, Clover: {_serverData.clover}, Progress[{adventureIndex_0}]: {_serverData.adventureProgess[adventureIndex_0]}");

            int diaIncrease = Convert.ToInt32(_adventureReward["DiaIncrease"]);
            int cloverIncrease = Convert.ToInt32(_adventureReward["CloverIncrease"]);
            int entranceFee = Convert.ToInt32(_adventureReward["EntranceFee"]);

            Dictionary<string, int> adventureDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(
                _adventureReward[$"Adventure_{adventureIndex_0}"].ToString());

            int diaValue = adventureDict["Dia"] + adventureIndex_1 * diaIncrease;
            int cloverValue = adventureDict["Clover"] + adventureIndex_1 * cloverIncrease;

            _serverData.dia += diaValue;
            _serverData.clover += cloverValue;
            _serverData.scroll -= entranceFee;
            _serverData.adventureProgess[adventureIndex_0]++;

            _logger.LogDebug($"[AdventureCase After] Dia: {_serverData.dia}, Clover: {_serverData.clover}, Progress[{adventureIndex_0}]: {_serverData.adventureProgess[adventureIndex_0]}");

            return true;
        }

    }
}
