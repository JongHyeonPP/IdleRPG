using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace DataSystem
{
    public class AccountResetController
    {
        private readonly ILogger<AccountResetController> _logger;

        public AccountResetController(ILogger<AccountResetController> logger)
        {
            _logger = logger;
        }

        // 클라이언트에서 호출할 함수
        [CloudCodeFunction("ResetAccount")]
        public async Task<bool> ResetAccount(
            IExecutionContext context,
            IGameApiClient gameApiClient)
        {
            try
            {
                // 저장할 기본값(초기상태)
                GameData emptyGameData = new GameData
                {
                    level = 1,
                    maxStageNum = 1,
                    currentStageNum = 1,
                    lastScrollTime = DateTime.UtcNow.ToString("O")
                };
                var emptyOfflineInfo = JsonConvert.SerializeObject(new { });

                // GameData 초기화(덮어쓰기)
                await gameApiClient.CloudSaveData.SetItemAsync(
                    context,
                    context.ServiceToken,
                    context.ProjectId,
                    context.PlayerId,
                    new("GameData", emptyGameData)
                );

                // OfflineRewardInfo 초기화(덮어쓰기)
                await gameApiClient.CloudSaveData.SetItemAsync(
                    context,
                    context.ServiceToken,
                    context.ProjectId,
                    context.PlayerId,
                    new("OfflineRewardInfo", emptyOfflineInfo)
                );

                _logger.LogInformation(
                    "ResetAccount succeeded by overwrite. PlayerId {PlayerId}",
                    context.PlayerId
                );

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "ResetAccount failed. PlayerId {PlayerId}",
                    context.PlayerId
                );
                return false;
            }
        }
    }
}
