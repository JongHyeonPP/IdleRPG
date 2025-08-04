using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace OfflineTimer
{
    public class TimerController
    {
        private readonly ILogger<TimerController> _logger;

        public TimerController(ILogger<TimerController> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction("CheckOfflineReward")]
        public async Task<OfflineRewardResult> CheckOfflineReward(
            string playerId,
            IExecutionContext context,
            IGameApiClient gameApiClient,
            ITimerSystem remoteConfigService)
        {

            var rewardData = await LoadOfflineRewardInfo(context, gameApiClient, playerId);

            if (!rewardData.TryGetValue("lastChecked", out var lastCheckedStr))
            {
                return null;
            }

            var config = remoteConfigService.GetRemoteConfig<Dictionary<string, string>>(context, gameApiClient, "OFFLINE_REWARD_INFO");

            if (!config.TryGetValue("RequireOfflineSecond", out var requireStr) ||
                !config.TryGetValue("MaxAccumulatedTime", out var maxStr))
            {
                _logger.LogError("Missing required Remote Config values.");
                return null;
            }

            if (!int.TryParse(requireStr, out var requireSeconds) ||
                !int.TryParse(maxStr, out var maxSeconds))
            {
                _logger.LogError("Failed to parse RequireOfflineSecond or MaxAccumulatedTime.");
                return null;
            }

            DateTime lastChecked;
            try
            {
                lastChecked = DateTime.Parse(lastCheckedStr, null, DateTimeStyles.RoundtripKind);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Invalid lastChecked format: {lastCheckedStr}, error: {ex.Message}");
                return null;
            }

            double elapsedSeconds = (DateTime.UtcNow - lastChecked).TotalSeconds;
            double accumulatedTime = rewardData.TryGetValue("AccumulatedTime", out var accStr) && double.TryParse(accStr, out var accVal)
                ? accVal + elapsedSeconds
                : elapsedSeconds;

            accumulatedTime = Math.Min(accumulatedTime, maxSeconds);
            rewardData["AccumulatedTime"] = accumulatedTime.ToString("F2");
            rewardData["lastChecked"] = DateTime.UtcNow.ToString("o");

            await SaveOfflineRewardInfo(context, gameApiClient, playerId, rewardData);

            if (accumulatedTime >= requireSeconds)
            {
                return new OfflineRewardResult { OfflineTime = accumulatedTime };
            }
            return null;
        }

        private async Task<Dictionary<string, string>> LoadOfflineRewardInfo(
            IExecutionContext ctx, IGameApiClient api, string pid)
        {
            var response = await api.CloudSaveData.GetItemsAsync(
                ctx, ctx.ServiceToken, ctx.ProjectId, pid, new() { "OfflineRewardInfo" });

            string json = (response.Data.Results.Count > 0 && response.Data.Results[0].Value != null)
                ? response.Data.Results[0].Value.ToString()
                : "{}";

            //_logger.LogDebug($"Loaded OfflineRewardInfo: {json}");

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new();
        }

        private async Task SaveOfflineRewardInfo(
            IExecutionContext ctx, IGameApiClient api, string pid, Dictionary<string, string> data)
        {
            string json = JsonConvert.SerializeObject(data);
            //_logger.LogDebug($"Saving OfflineRewardInfo: {json}");

            await api.CloudSaveData.SetItemAsync(
                ctx, ctx.ServiceToken, ctx.ProjectId, pid, new SetItemBody("OfflineRewardInfo", json));
        }
    }
}
