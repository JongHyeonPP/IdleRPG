using UnityEngine;
using Unity.Services.RemoteConfig;


public class RemoteConfigManager : MonoBehaviour
{
    public struct userAttributes { }
    public struct appAttributes { }

    private void Awake()
    {
        StartBroker.OnAuthenticationComplete += OnAuthenticationComplete;
    }

    private void OnAuthenticationComplete()
    {
        RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());
    }
}
