using EnumCollection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using UnityEngine;

public class ClientReportManager : MonoBehaviour
{
    public static ClientReportManager instance;
    private readonly float _verificationInterval = 5f;
    private float _verificationElapsed;
    //보고할 것들
    private List<ClientGoldReport> _clientGoldReportList;
    private List<ClientReinforceReport> _clientReinforceReportList;
    private GameData _gameData;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        NetworkBroker.SetGoldReport += SetResourceReport;
        NetworkBroker.SetReinforceReport += SetReinforceReport;
        NetworkBroker.StageClearVerification += StageClearVerificationAsync;
        StartCoroutine(VerificationCoroutine());
        _gameData = StartBroker.GetGameData();
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
        var newGoldReport = new ClientGoldReport(value, resource, source);
        _clientGoldReportList.Add(newGoldReport);
    }
    private void SetReinforceReport(int value, StatusType type, bool isByGold/*or StatusPoint*/)
    {
        var newReinforceReport = new ClientReinforceReport(value, type, isByGold);
        _clientReinforceReportList.Add(newReinforceReport);
    }
    [ContextMenu("VerificationReport")]
    private void VerificationReport()
    {
        _clientGoldReportList = new();
        _clientReinforceReportList = new();

        //test
        SetResourceReport(1, Resource.Gold, Source.Battle);
        SetResourceReport(1, Resource.Exp, Source.Battle);
        SetReinforceReport(2, StatusType.Power, true);
        SetReinforceReport(3, StatusType.HpRecover, false);
        //

        string serializedGoldReport = JsonConvert.SerializeObject(_clientGoldReportList);
        string serializedReinforceReport = JsonConvert.SerializeObject(_clientReinforceReportList);
        string serializedGameData = JsonConvert.SerializeObject(_gameData);

        Dictionary<string, object> args = new() 
        { 
            { "serializedGoldReport", serializedGoldReport },
            { "serializedReinforceReport", serializedReinforceReport },
            { "serializedGameData", serializedGameData},
            { "playerId",AuthenticationService.Instance.PlayerId}
        };

        CloudCodeService.Instance.CallModuleEndpointAsync(
            "ClientVerification",         // 모듈 이름
            "VerificationReport",     // 엔드포인트 이름
            args
        );
    }
    public void ResetVerificationInterval()
    {
        _verificationElapsed = 0f;
    }
    private IEnumerator VerificationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            _verificationElapsed += 1f;

            if (_verificationElapsed >= _verificationInterval)
            {
                _verificationElapsed = 0f;
                VerificationReport();
            }
        }
    }
}
