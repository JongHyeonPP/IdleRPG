using UnityEngine;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using Unity.Services.Core.Environments;
using System.Threading.Tasks;
using System;
using EnumCollection;
using System.Collections.Generic;
using Newtonsoft.Json;

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
