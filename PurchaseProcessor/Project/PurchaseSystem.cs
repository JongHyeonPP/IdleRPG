using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;

namespace Purchase
{
    public interface IPurchaseSystem
    {
        T GetRemoteConfig<T>(IExecutionContext context, IGameApiClient gameApiClient, string key);
    }
    public class PurchaseSystem : IPurchaseSystem
    {
        private readonly ILogger<PurchaseSystem> _logger;

        public PurchaseSystem(ILogger<PurchaseSystem> logger)
        {
            _logger = logger;
        }

        public T GetRemoteConfig<T>(IExecutionContext context, IGameApiClient gameApiClient, string key)
        {
            try
            {
                var result = gameApiClient.RemoteConfigSettings.AssignSettingsGetAsync(
                    context,
                    context.AccessToken,
                    context.ProjectId,
                    context.EnvironmentId,
                    null,
                    new List<string> { key }
                );

                var settings = result.Result.Data.Configs.Settings;

                if (!settings.ContainsKey(key))
                {
                    _logger.LogError($"Remote Config에 '{key}' 키가 없습니다.");
                    throw new Exception($"Remote Config에 '{key}' 키가 없습니다.");
                }

                var rawValue = settings[key].ToString();

                // 타입에 따라 처리 분기 (string이면 그대로 반환, 아니면 JSON 변환 시도)
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)rawValue;
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>(rawValue);
                }
            }
            catch (ApiException e)
            {
                _logger.LogError($"Remote Config 값 로딩 실패: {e.Message}");
                throw new Exception($"Remote Config 값 로딩 실패: {e.Message}");
            }
        }
    }

}
