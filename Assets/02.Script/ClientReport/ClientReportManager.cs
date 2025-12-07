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
    private Task _verificationTask;

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

        // Story ↔ Battle 전환 시 루프 제어
        BattleBroker.SwitchToStory += (_,_) => PauseVerificationLoop();
        BattleBroker.SwitchToBattle += () => ResumeVerificationLoop();

        _gameData = StartBroker.GetGameData();
    }

    private void OnDestroy()
    {
        PauseVerificationLoop();
    }

    /* ========================
       스토리 전환 시 일시정지
       ======================== */
    private void PauseVerificationLoop()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        _verificationTask = null;
        Debug.Log("VerificationLoop 일시정지됨.");
    }

    /* ========================
       배틀 복귀 시 다시 재개
       ======================== */
    private void ResumeVerificationLoop()
    {
        if (_cts != null)
            return;

        _cts = new CancellationTokenSource();
        _verificationTask = VerificationLoopAsync(_cts.Token);

        Debug.Log("VerificationLoop 다시 시작됨.");
    }

    private async void StageClearVerificationAsync()
    {
        await CloudCodeService.Instance.CallModuleEndpointAsync(
            "ClientVerification",
            "StageClearVerification"
        );
    }

    private void QueueResourceReport(int value, string id, Resource resource, Source source)
    {
        var newResourceReport = new ClientResourceReport(value, id, resource, source);
        _clientResourceReportList.Add(newResourceReport);
    }

    private void SetSpendReport(SpendType type, string additional, int amount)
    {
        string key = $"{type}_{additional}";
        if (_clientSpendReportDict.ContainsKey(key))
            _clientSpendReportDict[key] += amount;
        else
            _clientSpendReportDict.Add(key, amount);
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

        if (result.isVerificationSuccess)
            Debug.Log("서버에 저장 완료.");
        else
            Debug.Log($"서버 저장 실패: {result.failureFactor}");
    }

    private async Task VerificationLoopAsync(CancellationToken token)
    {
        try
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
        catch (TaskCanceledException)
        {
            // 정상적인 취소
        }
    }

    public void ResetVerificationInterval()
    {
        _verificationElapsed = 0f;
    }

    public void ForceVerificationNow()
    {
        _verificationElapsed = 0f;
        SendTotalReport();
    }
}
