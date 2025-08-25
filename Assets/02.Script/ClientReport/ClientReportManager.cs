using EnumCollection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using UnityEngine;

public class ClientReportManager : MonoBehaviour
{
    public static ClientReportManager instance;

    private readonly float _verificationInterval = 5f;
    private float _verificationElapsed;

    private List<ClientResourceReport> _clientResourceReportList = new();
    private Dictionary<string, int> _clientSpendReportDict = new();
    public bool isAcquireOfflineReward = false;
    private GameData _gameData;

    private CancellationTokenSource _cts;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        NetworkBroker.QueueResourceReport += QueueResourceReport;
        NetworkBroker.QueueSpendReport += SetSpendReport;
        NetworkBroker.OnOfflineReward += () => isAcquireOfflineReward = true;
        NetworkBroker.StageClearVerification += StageClearVerificationAsync;
        NetworkBroker.SaveServerData += ForceVerificationNow;

        _gameData = StartBroker.GetGameData();

        _cts = new CancellationTokenSource();
        _ = VerificationLoopAsync(_cts.Token);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
    }

    private async void StageClearVerificationAsync()
    {
        await CloudCodeService.Instance.CallModuleEndpointAsync(
            "ClientVerification",
            "StageClearVerification"
        );
    }

    private void QueueResourceReport(int value,string id, Resource resource, Source source)
    {
        var newResourceReport = new ClientResourceReport(value, id, resource, source);
        _clientResourceReportList.Add(newResourceReport);
    }

    private void SetSpendReport(SpendType type, string additional, int amount)
    {
        string key = $"{type}_{additional}";
        if (_clientSpendReportDict.ContainsKey(key))
        {
            _clientSpendReportDict[key] += amount;
        }
        else
        {
            _clientSpendReportDict.Add(key, amount);
        }
    }

    [ContextMenu("SendTotalReport")]
    private async void SendTotalReport()
    {
        string serializedResourceReport = JsonConvert.SerializeObject(_clientResourceReportList);
        string serializedSpendReport = JsonConvert.SerializeObject(_clientSpendReportDict);
        string serializedGameData = JsonConvert.SerializeObject(_gameData);

        Dictionary<string, object> args = new()
        {
            { "serializedResourceReport", serializedResourceReport },
            { "serializedSpendReport", serializedSpendReport },
            { "serializedGameData", serializedGameData },
            { "isAcquireOfflineReward", isAcquireOfflineReward },
            { "playerId", AuthenticationService.Instance.PlayerId }
        };

        isAcquireOfflineReward = false;
        _clientResourceReportList.Clear();
        _clientSpendReportDict.Clear();
        

        ReportResult result = await CloudCodeService.Instance.CallModuleEndpointAsync<ReportResult>(
            "ClientVerification",
            "VerificationReport",
            args
        );

        if (result.invalidCount >= 3)
        {
            StartBroker.OnDetectInvalidAct();
        }

        if (result.isVerificationSuccess)
            Debug.Log("서버에 저장됐음.");
        else
            Debug.Log($"서버에 저장 실패.. {result.failureFactor}");
    }

    private async Task VerificationLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1000, token);
            _verificationElapsed += 1f;

            if (_verificationElapsed >= _verificationInterval)
            {
                _verificationElapsed = 0f;
                SendTotalReport();
            }
        }
    }

    public void ResetVerificationInterval()
    {
        _verificationElapsed = 0f;
    }

    public async void ForceVerificationNow()
    {
        _verificationElapsed = 0f;
        SendTotalReport();
    }
}
