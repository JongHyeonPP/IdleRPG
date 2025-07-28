using EnumCollection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using UnityEngine;

public class ClientReportManager : MonoBehaviour
{
    public static ClientReportManager instance;

    private readonly float _verificationInterval = 5f;
    private float _verificationElapsed;

    private List<ClientVerificationReport> _clientVerificationReportList = new();
    public bool isAcquireOfflineReward = false;
    private GameData _gameData;
   

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        NetworkBroker.SetResourceReport += SetResourceReport;
        NetworkBroker.OnOfflineReward += () => isAcquireOfflineReward = true;
        NetworkBroker.StageClearVerification += StageClearVerificationAsync;
        NetworkBroker.SaveServerData += ForceVerificationNow;

        _gameData = StartBroker.GetGameData();

        _ = VerificationLoopAsync(); // 코루틴 없이 실행
    }

    private async void StageClearVerificationAsync()
    {
        await CloudCodeService.Instance.CallModuleEndpointAsync(
            "ClientVerification",
            "StageClearVerification"
        );
    }

    private void SetResourceReport(int value, Resource resource, Source source)
    {
        var newGoldReport = new ClientVerificationReport(value, resource, source);
        _clientVerificationReportList.Add(newGoldReport);
    }


    [ContextMenu("VerificationReport")]
    private async void VerificationReport()
    {
        string serializedVerificationReport = JsonConvert.SerializeObject(_clientVerificationReportList);
        string serializedGameData = JsonConvert.SerializeObject(_gameData);

        Dictionary<string, object> args = new()
        {
            { "serializedGoldReport", serializedVerificationReport },
            { "serializedGameData", serializedGameData },
            { "isAcquireOfflineReward", isAcquireOfflineReward},
            { "playerId", AuthenticationService.Instance.PlayerId }
        };
        isAcquireOfflineReward = false;
        _clientVerificationReportList.Clear();

        ReportResult result = await CloudCodeService.Instance.CallModuleEndpointAsync<ReportResult>(
            "ClientVerification",
            "VerificationReport",
            args
        );
        if (result.invalidCount >= 3)
        {
            StartBroker.OnDetectInvalidAct();
        }
        switch (result.isVerificationSuccess)
        {
            case false:
                Debug.Log("서버에 저장 실패.");
                break;
            case true:
                Debug.Log("서버에 저장됐음.");
                break;
        }
    }

    private async Task VerificationLoopAsync()
    {
        while (true)
        {
            await Task.Delay(1000); // 1초 대기 (Time.timeScale에 영향 받지 않음)
            _verificationElapsed += 1f;

            if (_verificationElapsed >= _verificationInterval)
            {
                _verificationElapsed = 0f;

                VerificationReport();
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

        VerificationReport();
    }
}
