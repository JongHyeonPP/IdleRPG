using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace OfflineReward;
public class OfflineRewardSetup : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        config.Dependencies.AddSingleton<IRemoteConfigService, RemoteConfigService>();
        config.Dependencies.AddSingleton(GameApiClient.Create());
        config.Dependencies.AddSingleton(PushClient.Create());
    }
}