using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;

namespace Verification
{
    public interface IRemoteConfigService
    {
        Dictionary<string, string> GetFormulas(IExecutionContext context, IGameApiClient gameApiClient, FormulaType formulaType);
        Dictionary<string, int> GetStageInfo(int stageNum);
    }

    public class RemoteConfigService : IRemoteConfigService
    {
        

        private readonly ILogger<RemoteConfigService> _logger;

        public RemoteConfigService(ILogger<RemoteConfigService> logger)
        {
            _logger = logger;
        }

        public Dictionary<string, string> GetFormulas(IExecutionContext context, IGameApiClient gameApiClient, FormulaType formulaType)
        {
            string FormulaKey = formulaType.ToString();
            try
            {
                // Remote Config에서 REINFORCE_FORMULAS 키 가져오기
                var result = gameApiClient.RemoteConfigSettings.AssignSettingsGetAsync(
                    context,
                    context.AccessToken,
                    context.ProjectId,
                    context.EnvironmentId,
                    null,
                    new List<string> { FormulaKey }
                );

                var settings = result.Result.Data.Configs.Settings;

                if (!settings.ContainsKey(FormulaKey))
                {
                    _logger.LogError($"Remote Config에 '{FormulaKey}' 키가 없습니다.");
                    throw new Exception($"Remote Config에 '{FormulaKey}' 키가 없습니다.");
                }

                var json = settings[FormulaKey].ToString();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch (ApiException e)
            {
                _logger.LogError($"강화 공식 로딩 실패: {e.Message}");
                throw new Exception($"강화 공식 로딩 실패: {e.Message}");
            }
        }

        public Dictionary<string, int> GetStageInfo(int stageNum)
        {
            throw new NotImplementedException();
        }
    }
}
