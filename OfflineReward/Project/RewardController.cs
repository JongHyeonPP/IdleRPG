using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace OfflineReward
{
    public class RewardController
    {
        private ILogger<RewardController> _logger;
        public RewardController(ILogger<RewardController> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction("CheckOfflineReward")]
        public async Task<RewardResult> CheckOfflineReward(
            string playerId,
            IExecutionContext context,
            IGameApiClient gameApiClient,
            IRemoteConfigService RemoteConfigService)
        {
            

            // Cloud Save에서 마지막 확인 시간 가져오기
            ApiResponse<GetItemsResponse> lastVerificationResponse = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.ServiceToken, context.ProjectId, playerId, new() { "OfflineRewardInfo" });

            string offlineRewardJson = lastVerificationResponse.Data.Results.Count > 0
                ? lastVerificationResponse.Data.Results[0].Value.ToString()
                : "{}";

            Dictionary<string, string> offlineRewardDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(offlineRewardJson)
                ?? new Dictionary<string, string>();

            Dictionary<string, string> offlineRewardInfoConfig = RemoteConfigService.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "OFFLINE_REWARD_INFO");
            int requireOfflineSecond = int.Parse(offlineRewardInfoConfig["RequireOfflineSecond"]);
            int maxAccumulatedTime = int.Parse(offlineRewardInfoConfig["MaxAccumulatedTime"]);

            // lastChecked 값이 존재하는 경우 처리
            if (offlineRewardDict.TryGetValue("lastChecked", out string lastCheckedStr))
            {
                RewardResult rewardResult = new();
                DateTime lastVerificationTime = DateTime.Parse(lastCheckedStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
                DateTime now = DateTime.UtcNow;
                double diff = (now - lastVerificationTime).TotalSeconds;
                double accumulatedTime;
                if (offlineRewardDict.ContainsKey("AccumulatedTime"))
                {
                    accumulatedTime = double.Parse(offlineRewardDict["AccumulatedTime"]) + diff;
                }
                else
                {
                    accumulatedTime = diff;
                }
                offlineRewardDict["AccumulatedTime"] = Math.Min(accumulatedTime, maxAccumulatedTime).ToString("F2");

                string modifiedRewardJson = JsonConvert.SerializeObject(offlineRewardDict);

                await gameApiClient.CloudSaveData.SetItemAsync(
                    context, context.ServiceToken, context.ProjectId, playerId,
                    new SetItemBody("OfflineRewardInfo", modifiedRewardJson));
                if (accumulatedTime >= requireOfflineSecond)
                {
                    int maxStageNum = await GetMaxStageNum(playerId, context, gameApiClient);
                    DataTable dataTable = new();
                    string expFormula = offlineRewardInfoConfig["Exp"];
                    int expValue = Convert.ToInt32(dataTable.Compute(expFormula.Replace("{second}", accumulatedTime.ToString()).Replace("{maxStageNum}", maxStageNum.ToString()), null));
                    string goldFormula = offlineRewardInfoConfig["Gold"];
                    int goldValue = Convert.ToInt32(dataTable.Compute(goldFormula.Replace("{second}", accumulatedTime.ToString()).Replace("{maxStageNum}", maxStageNum.ToString()), null));
                    string diaFormula = offlineRewardInfoConfig["Dia"];
                    int diaValue = Convert.ToInt32(dataTable.Compute(diaFormula.Replace("{second}", accumulatedTime.ToString()).Replace("{maxStageNum}", maxStageNum.ToString()), null));
                    string cloverFormula = offlineRewardInfoConfig["Clover"];
                    int cloverValue = Convert.ToInt32(dataTable.Compute(cloverFormula.Replace("{second}", accumulatedTime.ToString()).Replace("{maxStageNum}", maxStageNum.ToString()), null));
                    rewardResult.Gold = goldValue;
                    rewardResult.Exp = expValue;
                    rewardResult.Dia = diaValue;
                    rewardResult.Clover = cloverValue;
                    return rewardResult;
                }
            }
            return null;
        }
        private async Task<int> GetMaxStageNum(string playerId,
            IExecutionContext context,
            IGameApiClient gameApiClient)
        {
            ApiResponse<GetItemsResponse> gameDataResponse = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.ServiceToken, context.ProjectId, playerId, new() { "GameData" });
            if (gameDataResponse.Data.Results.Count > 0)
            {
                string gameDataJson = gameDataResponse.Data.Results[0].Value.ToString();
                int maxStageNum = JsonConvert.DeserializeObject<GameData_MaxStageNum>(gameDataJson).maxStageNum;
                return maxStageNum;
            }
            else
            {

                return 0;
            }

        }

    }
}