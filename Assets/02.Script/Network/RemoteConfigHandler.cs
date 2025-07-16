using UnityEngine;
using Unity.Services.RemoteConfig;


public class RemoteConfigHandler : MonoBehaviour
{
    public struct userAttributes { }
    public struct appAttributes { }

    private void Awake()
    {
        StartBroker.OnAuthenticationComplete += OnAuthenticationComplete;
    }

    private void OnAuthenticationComplete()
    {
        
    }
}
