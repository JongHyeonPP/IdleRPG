using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Verification;

namespace OfflineReward
{
    public class OfflineRewardController
    {
        private ILogger<OfflineRewardController> _logger;
        public OfflineRewardController(ILogger<OfflineRewardController> logger)
        {
            _logger = logger;
        }
        [CloudCodeFunction("CheckOfflineReward")]
        public async Task<OfflineRewardResult> CheckOfflineReward(
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
                OfflineRewardResult rewardResult = new();
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
    }
}