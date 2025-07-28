using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace ClientVerification.Verification
{
    public class CloudCodeSetup : ICloudCodeSetup
    {
        public void Setup(ICloudCodeConfig config)
        {
            config.Dependencies.AddSingleton<IVerificationSystem, VerificationSystem>();
            config.Dependencies.AddSingleton(GameApiClient.Create());
            config.Dependencies.AddSingleton(PushClient.Create());
        }
    }
}
