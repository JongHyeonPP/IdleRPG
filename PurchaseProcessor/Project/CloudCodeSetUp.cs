using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
namespace Purchase;

    public class CloudCodeSetup : ICloudCodeSetup
    {
        public void Setup(ICloudCodeConfig config)
        {
        config.Dependencies.AddSingleton<IPurchaseSystem, PurchaseSystem>();
        config.Dependencies.AddSingleton(GameApiClient.Create());
            config.Dependencies.AddSingleton(PushClient.Create());
        }
    }