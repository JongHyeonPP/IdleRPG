using Microsoft.Extensions.DependencyInjection;
using OfflineTimer;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

public class CloudCodeSetup : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        config.Dependencies.AddSingleton<ITimerSystem, TimerSystem>();
        config.Dependencies.AddSingleton(GameApiClient.Create());
        config.Dependencies.AddSingleton(PushClient.Create());
    }
}