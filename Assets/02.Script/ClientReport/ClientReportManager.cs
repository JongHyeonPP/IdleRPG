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

    private List<ClientVerificationReport> _clientGoldReportList = new();
    private List<ClientReinforceReport> _clientReinforceReportList = new();
    public bool isAcquireOfflineReward = false;
    private GameData _gameData;
   

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        NetworkBroker.SetResourceReport += SetResourceReport;
        NetworkBroker.SetReinforceReport += SetReinforceReport;
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

    private void SetResourceReport(int value, Resource resource)
    {
        var newGoldReport = new ClientVerificationReport(value, resource);
        _clientGoldReportList.Add(newGoldReport);
    }

    private void SetReinforceReport(int value, StatusType type, bool isByGold)
    {
        var newReinforceReport = new ClientReinforceReport(value, type, isByGold);
        _clientReinforceReportList.Add(newReinforceReport);
    }

    [ContextMenu("VerificationReport")]
    private async void VerificationReport()
    {
        string serializedGoldReport = JsonConvert.SerializeObject(_clientGoldReportList);
        string serializedReinforceReport = JsonConvert.SerializeObject(_clientReinforceReportList);
        string serializedGameData = JsonConvert.SerializeObject(_gameData);

        Dictionary<string, object> args = new()
        {
            { "serializedGoldReport", serializedGoldReport },
            { "serializedReinforceReport", serializedReinforceReport },
            { "isAcquireOfflineReward", isAcquireOfflineReward},
            { "playerId", AuthenticationService.Instance.PlayerId }
        };
        isAcquireOfflineReward = false;
        _clientGoldReportList.Clear();
        _clientReinforceReportList.Clear();

        ReportResult result = await CloudCodeService.Instance.CallModuleEndpointAsync<ReportResult>(
            "ClientVerification",
            "VerificationReport",
            args
        );
        if (result.invalidCount >= 3)
        {
            StartBroker.OnDetectInvalidAct();
        }
        Debug.Log("서버에 저장됐음.");
        //switch (result)
        //{
        //    case 0:
        //        Debug.LogError("Verification Fail!");
        //        break;
        //    case -1:
        //        Debug.LogError("Verification Fail!");
        //        break;
        //    case 1:
        //        Debug.Log("Verification Success!");
        //        break;
        //}
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
