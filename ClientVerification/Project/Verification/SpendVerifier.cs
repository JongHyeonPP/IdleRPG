using ClientVerification.Etc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ClientVerification.Verification
{
    public class SpendVerifier : IDataVerifier
    {
        private Dictionary<string, int> _reportDict;
        private GameData _serverData;
        private ILogger _logger;
        private DataTable _dataTable;
        private Dictionary<StatusType, string> _reinforePriceFormula;
        public SpendVerifier(Dictionary<string, int> reportDict, GameData serverData, ILogger logger,
            IVerificationSystem verificationSystem, Unity.Services.CloudCode.Core.IExecutionContext context, Unity.Services.CloudCode.Apis.IGameApiClient gameApiClient)
        {
            _reportDict = reportDict;
            _serverData = serverData;
            _logger = logger;
            _dataTable = new();
            _reinforePriceFormula = verificationSystem.GetRemoteConfig<Dictionary<StatusType, string>>(context, gameApiClient, "REINFORCE_PRICE_GOLD");
        }

        public bool Verify(out string failReason)
        {
            //_logger.LogDebug($"Report : {JsonConvert.SerializeObject(_reportDict)}");

            foreach (KeyValuePair<string, int> kvp in _reportDict)
            {
                string[] splitted = kvp.Key.Split('_');
                switch (splitted[0])
                {
                    case "Status":
                        bool result = StatusCase(splitted[1], kvp.Value);
                        if (!result)
                        {
                            failReason = $"Invalid Spend On {kvp.Key}";
                            return false;
                        }
                        break;
                }
            }

            failReason = "";
            return true;
        }

        private bool StatusCase(string statusTypeStr, int value)
        {
            StatusType statusType = Enum.Parse<StatusType>(statusTypeStr);

            int currentLevel = _serverData.statLevel_Gold[statusType];
            int totalCost = 0;

            for (int i = 0; i < value; i++)
            {
                int level = currentLevel + i;
                int computed = Convert.ToInt32(_dataTable.Compute(_reinforePriceFormula[statusType].Replace("{level}", level.ToString()), null));
                totalCost += computed;
            }

            if (_serverData.gold < totalCost)
                return false;

            _serverData.gold -= totalCost;

            if (_serverData.statLevel_Gold.ContainsKey(statusType))
                _serverData.statLevel_Gold[statusType] += value;
            else
                _serverData.statLevel_Gold.Add(statusType, value);

            return true;
        }

    }
}
