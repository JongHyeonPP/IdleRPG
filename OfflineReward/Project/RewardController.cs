using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
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
        public static string levelUpRequireExp { private set; get; }

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
                accumulatedTime = Math.Min(accumulatedTime, maxAccumulatedTime);
                offlineRewardDict["AccumulatedTime"] = accumulatedTime.ToString("F2");
                string modifiedRewardJson = JsonConvert.SerializeObject(offlineRewardDict);
                _logger.LogDebug(modifiedRewardJson);
                await gameApiClient.CloudSaveData.SetItemAsync(
                    context, context.ServiceToken, context.ProjectId, playerId,
                    new SetItemBody("OfflineRewardInfo", modifiedRewardJson));
                if (accumulatedTime >= requireOfflineSecond)
                {
                    rewardResult.OfflineTime = accumulatedTime;
                    return rewardResult;
                }
            }
            return null;
        }

        [CloudCodeFunction("AcquireOfflineReward")]
        public async void AcquireOfflineReward(
    string playerId,
    IExecutionContext context,
    IGameApiClient gameApiClient,
    IRemoteConfigService RemoteConfigService)
        {
            //ApiResponse<GetItemsResponse> gameDataResponse = await gameApiClient.CloudSaveData.GetItemsAsync(
            //    context, context.ServiceToken, context.ProjectId, playerId, new() { "GameData" });
            //string gameDataJson = gameDataResponse.Data.Results[0].Value.ToString();
            //int maxStageNum = JsonConvert.DeserializeObject<GameData_MaxStageNum>(gameDataJson).maxStageNum;
            //levelUpRequireExp = RemoteConfigService.GetRemoteConfig<string>(context, gameApiClient, "LEVEL_UP_REQUIRE_EXP");
            //ApiResponse<GetItemsResponse> lastVerificationResponse = await gameApiClient.CloudSaveData.GetItemsAsync(
            //    context, context.ServiceToken, context.ProjectId, playerId, new() { "OfflineRewardInfo" });

            //string offlineRewardJson = lastVerificationResponse.Data.Results.Count > 0
            //    ? lastVerificationResponse.Data.Results[0].Value.ToString()
            //    : "{}";

            //Dictionary<string, string> offlineRewardDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(offlineRewardJson)
            //    ?? new Dictionary<string, string>();

            //Dictionary<string, string> offlineRewardInfoConfig = RemoteConfigService.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "OFFLINE_REWARD_INFO");

            //double accumulatedTime;
            //accumulatedTime = double.Parse(offlineRewardDict["AccumulatedTime"]);
            //DataTable dataTable = new();
            //string goldFormula = offlineRewardInfoConfig["Gold"];
            //BigInteger goldValue = Convert.ToInt64(dataTable.Compute(goldFormula.Replace("{second}", accumulatedTime.ToString()).Replace("{maxStageNum}", maxStageNum.ToString()), null)) / 60;
            //string expFormula = offlineRewardInfoConfig["Exp"];
            //BigInteger expValue = Convert.ToInt64(dataTable.Compute(expFormula.Replace("{second}", accumulatedTime.ToString()).Replace("{maxStageNum}", maxStageNum.ToString()), null))/60;
            //string diaFormula = offlineRewardInfoConfig["Dia"];
            //int diaValue = Convert.ToInt32(dataTable.Compute(diaFormula.Replace("{second}", accumulatedTime.ToString()).Replace("{maxStageNum}", maxStageNum.ToString()), null))/60;
            //string cloverFormula = offlineRewardInfoConfig["Clover"];
            //int cloverValue = Convert.ToInt32(dataTable.Compute(cloverFormula.Replace("{second}", accumulatedTime.ToString()).Replace("{maxStageNum}", maxStageNum.ToString()), null))/60;


            //string serializedGameData = gameDataJson;
            //Dictionary<string, object> updates = new()
            //{
            //    { "gold", goldValue },
            //    { "exp", expValue },
            //    { "dia", diaValue },
            //    { "Clover", cloverValue }
            //};




            //string modifiedGameData = JsonModifier.AddToFieldValues(serializedGameData, updates, _logger);
            //_logger.LogDebug("GameData : " +modifiedGameData);
            //await gameApiClient.CloudSaveData.SetItemAsync(
            //    context, context.ServiceToken, context.ProjectId, playerId,
            //    new SetItemBody("GameData", modifiedGameData)
            //);
        }
    }
}